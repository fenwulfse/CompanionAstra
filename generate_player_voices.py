"""
Generate player-side voice files for the current MQAstraALT build.
Reads player topic lines from player_voice_lines.json and writes .fuz files
to both PlayerVoiceFemale01 and PlayerVoiceMale01.
"""
import asyncio
import json
import os
import struct
import subprocess
import sys
import time
import wave

import edge_tts
import miniaudio

TOOLS_ROOT = r"E:\FO4Projects\Tools"
LIP_GEN = os.path.join(TOOLS_ROOT, "LipGenerator.exe")
XWM_ENCODE = os.path.join(TOOLS_ROOT, "xwmaencode.exe")
DATA_PATH = r"E:\SteamLibrary\steamapps\common\Fallout 4\Data"
VOICE_ROOT = os.path.join(DATA_PATH, "Sound", "Voice", "MQAstraALT.esp")
PLAYER_FEMALE_DST = os.path.join(VOICE_ROOT, "PlayerVoiceFemale01")
PLAYER_MALE_DST = os.path.join(VOICE_ROOT, "PlayerVoiceMale01")
JSON_PATH = os.path.join(os.path.dirname(__file__), "player_voice_lines.json")
INTERMEDIATE_DIR = os.path.join(TOOLS_ROOT, "mqastralt_player_voice")

FEMALE_VOICE = "en-US-JennyNeural"
MALE_VOICE = "en-US-GuyNeural"


def ensure_dir(path: str) -> None:
    os.makedirs(path, exist_ok=True)


def run_tool(exe: str, args: list[str]) -> None:
    subprocess.run([exe] + args, capture_output=True, cwd=TOOLS_ROOT)


def mp3_to_wav(mp3_path: str, wav_path: str) -> None:
    decoded = miniaudio.decode_file(
        mp3_path,
        output_format=miniaudio.SampleFormat.SIGNED16,
        nchannels=1,
        sample_rate=44100,
    )
    with wave.open(wav_path, "wb") as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(44100)
        wav.writeframes(decoded.samples)


def write_fuz(lip_path: str, xwm_path: str, fuz_path: str) -> None:
    lip_data = open(lip_path, "rb").read()
    audio_data = open(xwm_path, "rb").read()
    last_error = None
    for _ in range(10):
        try:
            with open(fuz_path, "wb") as f:
                f.write(b"FUZE")
                f.write(struct.pack("<I", 1))
                f.write(struct.pack("<I", len(lip_data)))
                f.write(lip_data)
                f.write(audio_data)
            return
        except PermissionError as exc:
            last_error = exc
            time.sleep(1)
    raise last_error if last_error else RuntimeError(f"Could not write {fuz_path}")


async def synth_line(text: str, voice: str, out_mp3: str) -> None:
    last_error = None
    for _ in range(3):
        try:
            communicate = edge_tts.Communicate(text, voice=voice)
            await communicate.save(out_mp3)
            return
        except edge_tts.exceptions.NoAudioReceived as exc:
            last_error = exc
            await asyncio.sleep(1)
    raise last_error if last_error else RuntimeError("TTS failed")


def load_lines() -> list[dict]:
    with open(JSON_PATH, "r", encoding="utf-8") as f:
        return json.load(f)


def generate_one(line: dict, voice: str, dest_dir: str, label: str) -> None:
    form_id = line["FormId"]
    text = line["Text"].strip()
    ensure_dir(dest_dir)
    ensure_dir(INTERMEDIATE_DIR)

    mp3 = os.path.join(INTERMEDIATE_DIR, f"{label}_{form_id}.mp3")
    wav = os.path.join(INTERMEDIATE_DIR, f"{label}_{form_id}.wav")
    lip = os.path.join(INTERMEDIATE_DIR, f"{label}_{form_id}.lip")
    xwm = os.path.join(INTERMEDIATE_DIR, f"{label}_{form_id}.xwm")
    fuz = os.path.join(dest_dir, f"{form_id}_1.fuz")

    asyncio.run(synth_line(text, voice, mp3))
    mp3_to_wav(mp3, wav)
    run_tool(LIP_GEN, [wav, text])
    run_tool(XWM_ENCODE, [wav, xwm])
    if not os.path.exists(lip):
        raise RuntimeError(f"LIP generation failed for {form_id}")
    if not os.path.exists(xwm):
        raise RuntimeError(f"XWM generation failed for {form_id}")
    write_fuz(lip, xwm, fuz)


def main() -> int:
    ensure_dir(PLAYER_FEMALE_DST)
    ensure_dir(PLAYER_MALE_DST)
    lines = load_lines()
    start_form_id = None
    end_form_id = None
    args = sys.argv[1:]
    if "--start-formid" in args:
        idx = args.index("--start-formid")
        if idx + 1 < len(args):
            start_form_id = args[idx + 1].upper()
    if "--end-formid" in args:
        idx = args.index("--end-formid")
        if idx + 1 < len(args):
            end_form_id = args[idx + 1].upper()

    if start_form_id:
        filtered = []
        started = False
        for line in lines:
            if line["FormId"].upper() == start_form_id:
                started = True
            if started:
                filtered.append(line)
        lines = filtered

    if end_form_id:
        filtered = []
        for line in lines:
            filtered.append(line)
            if line["FormId"].upper() == end_form_id:
                break
        lines = filtered

    print(f"=== Player Voice Generator ===")
    print(f"Lines: {len(lines)}")
    print(f"Female dst: {PLAYER_FEMALE_DST}")
    print(f"Male dst:   {PLAYER_MALE_DST}")

    for idx, line in enumerate(lines, start=1):
        form_id = line["FormId"]
        text = line["Text"].strip()
        print(f"[{idx}/{len(lines)}] {form_id} {text[:60]}")
        generate_one(line, FEMALE_VOICE, PLAYER_FEMALE_DST, "pf")
        generate_one(line, MALE_VOICE, PLAYER_MALE_DST, "pm")

    print("Done.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
