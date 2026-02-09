# Changelog

## 2026-02-08
- Locked Neutral -> Friendship INFO IDs to stable range `0000F200` - `0000F223`.
- Softened Neutral -> Friendship player negative response to "Maybe later."
- Rebuilt LockedIDs generator with fixed INFO IDs.
- Updated greeting stability doc with locked ranges and corrected paths.
- Added guardrail to prevent overwriting Neutral -> Friendship NPC audio unless explicitly allowed.
- Added `--enable-friendship-tts` to generate Astra TTS for Neutral -> Friendship NPC lines.
- Locked Neutral -> Friendship greetings to `0000F230` - `0000F231` and generated Astra TTS.
- Mapped Neutral -> Friendship player lines to closest vanilla player voice files (see doc for IDs).
- Updated Neutral -> Friendship player text to match selected vanilla player audio exactly.
- Locked Friendship -> Admiration INFO IDs and updated player text to match vanilla audio.
- Added admiration TTS generation flag and guardrail against overwriting admiration NPC audio.
- Added second admiration greeting and locked its INFO ID.
- Restored admiration player text to Astra-first phrasing; mapped closest vanilla player audio without changing text.
- Locked Confidant INFO IDs and added second confidant greeting.
- Added confidant TTS generation flag and guardrail against overwriting confidant NPC audio.
- Locked Infatuation INFO IDs and added second infatuation greeting.
- Added infatuation TTS generation flag and guardrail against overwriting infatuation NPC audio.
- Added infatuation locked ID map doc.
- Locked Disdain + Hatred INFO IDs, added two greetings each, and updated greeting stability doc.
- Added Disdain/Hatred player voice mapping to closest vanilla lines.
- Added Disdain/Hatred TTS generation flags.
- Added Disdain/Hatred locked ID map doc.
- Locked Recovery/Murder INFO IDs and mapped player voice to closest vanilla lines.
- Added Recovery/Murder TTS generation flags.
- Added Recovery/Murder locked ID map doc.
- Guardrails now allow COMAstra/COMClaude fragment names and alias properties.

## 2026-02-07
- Fixed Fallout 4 `.fuz` layout (FUZE + version + lip + audio).
- Added Astra pickup scene text and TTS audio generation (Windows SAPI).
- Added companion/dogmeat phase conditions to pickup scene.
- Renamed quest to `COMAstra` and companion display name to Astra.
- Documented voice pipeline and CK behavior.
- Documented Piper-exact pickup scene replica and exact topic FormKeys.
- Documented greeting voice ID stability and the required ID dump workflow.
