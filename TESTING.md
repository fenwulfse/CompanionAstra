# Testing Checklist (CK)

Use this when validating a build in the Creation Kit.

## Before You Start
- Confirm you loaded the newest `CompanionAstra.esp` from:
  `E:\CompanionGeminiFeb26\CompanionAstra_LockedIDs\CompanionAstra.esp`
- Confirm audio files exist in:
  `D:\SteamLibrary\steamapps\common\Fallout 4\Data\Sound\Voice\CompanionAstra.esp\NPCFAstra`

## Pickup Scene (COMAstraPickupScene)
- Greeting 1 plays Astra voice and matches text.
- Greeting 2 plays Astra voice and matches text.
- NPC lines play, no “bouncing.”

## Neutral → Friendship (COMClaude_01_NeutralToFriendship)
- Greeting 1 and 2 play Astra voice.
- NPC lines match text.
- Player lines are close enough to text (vanilla audio).

## Friendship → Admiration (COMClaude_02_FriendshipToAdmiration)
- Greeting 1 and 2 play Astra voice.
- NPC lines match text.

## Admiration → Confidant (COMClaude_02a_AdmirationToConfidant)
- Greeting 1 and 2 play Astra voice.
- NPC lines match text.

## Confidant → Infatuation (COMClaude_03_AdmirationToInfatuation)
- Greeting 1 and 2 play Astra voice.
- NPC lines match text.

## Disdain → Hatred (COMClaude_04_NeutralToDisdain / COMClaude_05_DisdainToHatred)
- Disdain greeting 1/2 play Astra voice.
- Hatred greeting 1/2 play Astra voice.
- NPC lines match text.

## Recovery / Murder
- Recovery NPC line plays Astra voice.
- Murder NPC line plays Astra voice.

## Report Issues
Open a GitHub issue using the Bug Report template.
