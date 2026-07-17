"""
CompanionClaude rewrite voice generator.
Regenerates FUZ audio only for the lines rewritten by Program.cs
(rewritten_npc_lines.json / rewritten_player_lines.json).
Pipeline: edge-tts (MP3) -> miniaudio (WAV) -> LipGenerator (LIP) -> xwmaencode (XWM) -> FUZ.
Writes only into this project's Data staging folder.
"""
import asyncio
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


async def generate(form_id, text, voice_name, dst_dir, tag):
    base = os.path.join(INTERMEDIATE_DIR, f"{tag}_{form_id}")
    mp3, wav = base + ".mp3", base + ".wav"
    lip, xwm = base + ".lip", base + ".xwm"
    fuz = os.path.join(dst_dir, f"{form_id}_1.fuz")

    speak = text.replace("--", ",")  # em-dash placeholder reads badly in TTS
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
    subprocess.run([LIP_GEN, wav, speak], capture_output=True, cwd=TOOLS_ROOT)
    subprocess.run([XWM_ENCODE, wav, xwm], capture_output=True, cwd=TOOLS_ROOT)
    if not os.path.exists(lip):
        return None, "LIP generation failed"
    if not os.path.exists(xwm):
        return None, "XWM encoding failed"
    write_fuz(lip, xwm, fuz)
    with open(fuz, 'rb') as f:
        header = f.read(5)
    ok = len(header) >= 5 and header[4] == 0x01
    return os.path.getsize(fuz), "OK" if ok else f"BAD FORMAT (byte4=0x{header[4]:02X})"


async def main():
    os.makedirs(INTERMEDIATE_DIR, exist_ok=True)
    total = ok = 0
    for json_name, targets in JOBS:
        with open(os.path.join(HERE, json_name), encoding="utf-8") as f:
            lines = json.load(f)
        for folder, voice in targets:
            dst = os.path.join(VOICE_ROOT, folder)
            os.makedirs(dst, exist_ok=True)
            print(f"--- {folder} ({voice}): {len(lines)} lines ---")
            for row in lines:
                total += 1
                size, status = await generate(row["FormId"], row["Text"], voice, dst, folder)
                print(f"  {row['FormId']}: {status}" + (f" ({size:,} bytes)" if size else ""))
                if status == "OK":
                    ok += 1
    print(f"\n=== {ok}/{total} voice files generated ===")


if __name__ == "__main__":
    asyncio.run(main())
