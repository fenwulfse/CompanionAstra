"""
CompanionClaude rewrite voice generator.
Regenerates FUZ audio only for the lines rewritten by Program.cs
(rewritten_npc_lines.json / rewritten_player_lines.json).
Pipeline: edge-tts (MP3) -> miniaudio (WAV) -> LipGenerator (LIP) -> xwmaencode (XWM) -> FUZ.
Writes only into this project's Data staging folder.
"""
import asyncio
import argparse
import struct
import wave
import os
import json
import subprocess
import miniaudio
import edge_tts

TOOLS_ROOT = r"E:\FO4Projects\Tools"
LIP_GEN = os.path.join(TOOLS_ROOT, "LipGenerator.exe")
XWM_ENCODE = os.path.join(TOOLS_ROOT, "xwmaencode.exe")
HERE = os.path.dirname(os.path.abspath(__file__))
VOICE_ROOT = os.path.join(HERE, "Data", "Sound", "Voice", "CompanionClaude.esp")
INTERMEDIATE_DIR = os.path.join(HERE, "voice_intermediate")
MANIFEST_PATH = os.path.join(HERE, "voice_generation_manifest.json")

JOBS = [
    ("rewritten_npc_lines.json", [("NPCFAstra", "en-US-AvaNeural")]),
    ("rewritten_player_lines.json", [("PlayerVoiceFemale01", "en-US-JennyNeural"),
                                     ("PlayerVoiceMale01", "en-US-AndrewNeural")]),
    ("memoir_npc_lines.json", [("NPCFAstra", "en-US-AvaNeural")]),
    ("memoir_player_lines.json", [("PlayerVoiceFemale01", "en-US-JennyNeural"),
                                  ("PlayerVoiceMale01", "en-US-AndrewNeural")]),
    ("radreact_npc_lines.json", [("NPCFAstra", "en-US-AvaNeural")]),
    ("presence_npc_lines.json", [("NPCFAstra", "en-US-AvaNeural")]),
    ("exchange_npc_lines.json", [("NPCFAstra", "en-US-AvaNeural")]),
    ("exchange2_npc_lines.json", [("NPCFAstra", "en-US-AvaNeural")]),
    ("awareness_npc_lines.json", [("NPCFAstra", "en-US-AvaNeural")]),
]


def mp3_to_wav(mp3_path, wav_path):
    decoded = miniaudio.decode_file(mp3_path,
                                    output_format=miniaudio.SampleFormat.SIGNED16,
                                    nchannels=1, sample_rate=44100)
    with wave.open(wav_path, 'wb') as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(44100)
        wf.writeframes(decoded.samples)


def write_fuz(lip_path, xwm_path, fuz_path):
    lip_data = open(lip_path, 'rb').read()
    audio_data = open(xwm_path, 'rb').read()
    with open(fuz_path, 'wb') as f:
        f.write(b'FUZE')
        f.write(struct.pack('<I', 1))
        f.write(struct.pack('<I', len(lip_data)))
        f.write(lip_data)
        f.write(audio_data)


async def generate(form_id, text, voice_name, dst_dir, tag, mode):
    base = os.path.join(INTERMEDIATE_DIR, f"{tag}_{form_id}")
    mp3, wav = base + ".mp3", base + ".wav"
    lip, xwm = base + ".lip", base + ".xwm"
    fuz = os.path.join(dst_dir, f"{form_id}_1.fuz")

    speak = text.replace("--", ",")  # em-dash placeholder reads badly in TTS
    if mode != "package":
        for attempt in range(3):
            try:
                comm = edge_tts.Communicate(speak, voice_name)
                await comm.save(mp3)
                break
            except edge_tts.exceptions.NoAudioReceived:
                if attempt == 2:
                    return None, "TTS failed (NoAudioReceived)"
                await asyncio.sleep(1)

        mp3_to_wav(mp3, wav)
        subprocess.run([XWM_ENCODE, wav, xwm], capture_output=True, cwd=TOOLS_ROOT)
        if not os.path.exists(xwm):
            return None, "XWM encoding failed"
        if mode == "prepare":
            return os.path.getsize(wav), "PREPARED"

    if not os.path.exists(lip):
        return None, "LIP file missing"
    if not os.path.exists(xwm):
        return None, "XWM encoding failed"
    write_fuz(lip, xwm, fuz)
    with open(fuz, 'rb') as f:
        header = f.read(5)
    ok = len(header) >= 5 and header[4] == 0x01
    return os.path.getsize(fuz), "OK" if ok else f"BAD FORMAT (byte4=0x{header[4]:02X})"


async def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--only", help="Generate only the named JSON job")
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--prepare-only", action="store_true",
                      help="Generate WAV/XWM files and a lip-sync manifest")
    mode.add_argument("--package-existing", action="store_true",
                      help="Package existing LIP/XWM files into FUZ files")
    args = parser.parse_args()
    generation_mode = "prepare" if args.prepare_only else "package"
    os.makedirs(INTERMEDIATE_DIR, exist_ok=True)
    total = ok = 0
    manifest = []
    for json_name, targets in JOBS:
        if args.only and json_name != args.only:
            continue
        with open(os.path.join(HERE, json_name), encoding="utf-8") as f:
            lines = json.load(f)
        for folder, voice in targets:
            dst = os.path.join(VOICE_ROOT, folder)
            os.makedirs(dst, exist_ok=True)
            print(f"--- {folder} ({voice}): {len(lines)} lines ---")
            for row in lines:
                total += 1
                size, status = await generate(
                    row["FormId"], row["Text"], voice, dst, folder, generation_mode)
                print(f"  {row['FormId']}: {status}" + (f" ({size:,} bytes)" if size else ""))
                if status in ("OK", "PREPARED"):
                    ok += 1
                if status == "PREPARED":
                    base = os.path.join(INTERMEDIATE_DIR, f"{folder}_{row['FormId']}")
                    manifest.append({
                        "WaveFile": base + ".wav",
                        "LipFile": base + ".lip",
                        "SpokenText": row["Text"].replace("--", ","),
                    })
    if generation_mode == "prepare":
        with open(MANIFEST_PATH, "w", encoding="utf-8") as f:
            json.dump(manifest, f, indent=2)
    action = "prepared" if generation_mode == "prepare" else "packaged"
    print(f"\n=== {ok}/{total} voice files {action} ===")


if __name__ == "__main__":
    asyncio.run(main())
