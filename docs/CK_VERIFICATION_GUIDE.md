# CK Verification Guide (CompanionAstra)

This guide documents how to verify CompanionAstra's dialogue in the Fallout 4 Creation Kit (CK). CK is used for validation only. Do not edit or save changes to the plugin.

## Prerequisites
- Latest ESP at `<WORKSPACE_ROOT>\CompanionAstra_LockedIDs\CompanionAstra.esp`
- Voice files at `<FO4_DATA>\Sound\Voice\CompanionAstra.esp\NPCFAstra`
- Creation Kit installed and working

## 1) Open the ESP in CK
1. Launch Creation Kit.
2. `File > Data...`
3. Select `CompanionAstra.esp` and set it as the **Active File**.
4. Click `OK` and let CK load.

## 2) Find the quest
1. Open `Object Window`.
2. Navigate to `Character > Quest`.
3. Filter for `COMAstra` and open the `COMAstra` quest.

## 3) Check affinity scenes
In the quest window, open the `Scenes` tab. Filter or sort by EditorID and check these scenes:

- Friendship: `COMClaude_01_NeutralToFriendship`
- Admiration: `COMClaude_02_FriendshipToAdmiration`
- Confidant: `COMClaude_02a_AdmirationToConfidant`
- Infatuation: `COMClaude_03_AdmirationToInfatuation`
- Disdain: `COMClaude_04_NeutralToDisdain`
- Hatred: `COMClaude_05_DisdainToHatred`
- Recovery: `COMClaude_10_RepeatAdmirationToInfatuation` (Recovery scene)
- Murder: `COMClaudeMurderScene`

Open each scene and verify the dialogue lines in the Actions list.

## 4) Preview dialogue audio
1. In the Scene window, select a Dialogue action.
2. Click `Scene > Preview` (or the Preview button).
3. Confirm:
   - Astra's line plays the correct voice.
   - Player line is present and matches expected intent.
   - There is no silence where a voice line should play.

## 5) Verify INFO IDs (FormKey / INFO ID)
These IDs are the filename contract for `.fuz` files.

1. In a Scene, double-click a Dialogue action to open the Topic/Response.
2. Open the response (Topic Info).
3. Read the FormID shown in the Topic Info window.
4. For voice file mapping, record the **last 6 hex digits** of the FormID.
5. Cross-check against the locked ID docs:
   - `docs/GREETING_VOICE_ID_STABILITY.md`
   - `docs/*_LOCKED_IDS.md`

## 6) Check greeting conditions (truth table)
1. In the `COMAstra` quest, open the `Dialogue` tab.
2. Find the greeting topic (e.g., `COMClaudeGreetings`).
3. Review each response's conditions:
   - `CA_WantsToTalk` value
   - Faction checks (`HasBeenCompanionFaction`, `CurrentCompanionFaction`, `DisallowedCompanionFaction`)
4. Ensure each condition row maps to the correct scene and state.

## 7) Common issues to watch for
- **Bouncing (wrong greeting fires)**
  Usually caused by incorrect greeting conditions or `CA_WantsToTalk` state. Re-check the truth table.

- **Missing voice files (silent dialogue)**
  The `.fuz` file is missing or the INFO ID does not match the filename. Re-verify FormID and locked ID docs.

- **Phase index mismatch warnings**
  CK may warn about scene phase indices. This is a known, usually harmless warning from Mutagen-generated scenes.

## Notes
- Do not edit or save the ESP in CK.
- If you find issues, open a GitHub issue with the scene name, INFO ID, and a short description.

