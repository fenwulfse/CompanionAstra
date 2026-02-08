# Handover - 2026-02-07 (Astra)

## Current Status
- Voice generation works with **FO4 `.fuz` layout** (FUZE + version=1 + lip size + lip + audio).
- Windows built-in **SAPI TTS** used to generate WAVs locally (no paid services).
- Pickup scene structured to **match vanilla** (3 actors, 6 phases, 5 actions, Stage 80).
- Companion + Dogmeat **phase conditions** added for pickup scene.
- Companion renamed to **Astra**.
  - Quest EditorID: `COMAstra`
  - NPC name: `Astra`
  - ESP output: `CompanionAstra.esp`
  - Voice path: `Data\Sound\Voice\CompanionAstra.esp\NPCFPiper`

## Key Files
- `E:\CompanionGeminiFeb26\CompanionClaude\Program.cs`
  - Pickup scene Astra text (greetings + responses).
  - TTS generation and FO4 `.fuz` packing for pickup NPC lines.
- `E:\CompanionGeminiFeb26\CompanionGemini_v14_Synthesis\FuzPacker.cs`
  - Updated to FO4 `.fuz` layout.
- `E:\CompanionGeminiFeb26\docs\ASTRA_VOICE_PIPELINE_2026-02-07.md`
  - Pipeline details.
- `E:\CompanionGeminiFeb26\docs\CLAUDE_HANDOVER_2026-02-06.md`
  - Added 2026-02-07 update.

## Known Constraints
- CK preview “bounce” is normal; not a failure.
- CK can appear unchanged if old ESP is loaded; verify by quest EditorID (`COMAstra`) and pickup greeting text.

## Next Task (Requested)
- Locate historical docs/code under `E:\Gemini` to see how Claude/Gemini recreated Neutral→Friendship scene to match Piper.
- Work backward from most recent dates.

