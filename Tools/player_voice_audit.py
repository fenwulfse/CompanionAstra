import os
import re
import sys

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(SCRIPT_DIR)
PROGRAM = os.path.join(ROOT, "CompanionAstra_LockedIDs", "Program.cs")
OUT = os.path.join(SCRIPT_DIR, "PlayerVoice_Audit.txt")

if not os.path.exists(PROGRAM):
    print(f"Missing Program.cs: {PROGRAM}")
    sys.exit(1)

player_map = []
in_block = False
with open(PROGRAM, "r", encoding="utf-8") as f:
    for line in f:
        if "var playerVoiceMap" in line:
            in_block = True
        if in_block:
            m = re.search(r"\(\s*0x([0-9A-Fa-f]+)\s*,\s*([^)]+)\)\s*,?\s*//\s*(.*)", line)
            if m:
                src = m.group(1).upper().zfill(8)
                dst = m.group(2).strip()
                comment = m.group(3).strip()
                player_map.append((src, dst, comment))
        if in_block and "};" in line:
            break

with open(OUT, "w", encoding="utf-8") as out:
    out.write("PLAYER VOICE AUDIT (from Program.cs)\n")
    out.write("Format: VanillaINFO -> OurINFORef // Comment\n\n")
    for src, dst, comment in player_map:
        out.write(f"{src} -> {dst} // {comment}\n")

print(f"Wrote audit: {OUT}")
