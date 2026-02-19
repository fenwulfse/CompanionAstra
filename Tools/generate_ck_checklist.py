import csv
import os
import sys

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(SCRIPT_DIR)
DUMP = os.path.join(SCRIPT_DIR, "FO4Edit", "Astra_DialogueDump.csv")
OUT = os.path.join(SCRIPT_DIR, "CK_Checklist_Astra.txt")

if not os.path.exists(DUMP):
    print(f"Missing dump: {DUMP}")
    sys.exit(1)

# Focus on scenes that matter for CK verification
TARGET_TOPICS = {
    "COMClaudeGreetings": "Greetings",
    "COMClaude_01_NeutralToFriendship": "Neutral->Friendship",
    "COMClaude_02_FriendshipToAdmiration": "Friendship->Admiration",
    "COMClaude_02a_AdmirationToConfidant": "Admiration->Confidant",
    "COMClaude_03_AdmirationToInfatuation": "Confidant->Infatuation",
    "COMClaude_04_NeutralToDisdain": "Disdain",
    "COMClaude_05_DisdainToHatred": "Hatred",
    "COMClaude_10_RepeatAdmirationToInfatuation": "Recovery",
    "COMClaudeMurderScene": "Murder",
}

rows = []
with open(DUMP, newline="", encoding="utf-8") as f:
    reader = csv.DictReader(f)
    for r in reader:
        topic = r.get("Topic", "")
        text = r.get("Text", "")
        formid = r.get("FormID", "")
        for key in TARGET_TOPICS:
            if key in topic:
                rows.append((TARGET_TOPICS[key], formid, topic, text))
                break

with open(OUT, "w", encoding="utf-8") as out:
    out.write("CK CHECKLIST (Astra)\n")
    out.write("Generated from xEdit dump. Verify these lines in CK.\n\n")
    current = None
    for section, formid, topic, text in sorted(rows, key=lambda x: (x[0], x[1])):
        if section != current:
            out.write(f"\n=== {section} ===\n")
            current = section
        out.write(f"{formid} | {topic} | {text}\n")

print(f"Wrote checklist: {OUT}")
