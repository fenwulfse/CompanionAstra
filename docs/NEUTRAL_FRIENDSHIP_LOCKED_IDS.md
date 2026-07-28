# Neutral -> Friendship Locked IDs (Authoritative Map)

Date: 2026-02-08

This is the authoritative ID map for the Neutral -> Friendship scene.
These IDs are fixed in `CompanionAstra_LockedIDs/Program.cs` and must not change.

## Exchange 1 (Phase 0 PlayerDialogue)
- 0000F200 PPos Prompt: "I try" Text: "I help when I can. It's the right thing."
- 0000F201 NPos Text: "Curious. Most people don't choose the hard right."
- 0000F202 PNeg Prompt: "Later" Text: "Maybe later."
- 0000F203 NNeg Text: "Understood. I'll wait."
- 0000F204 PNeu Prompt: "Not sure" Text: "I just do what feels right."
- 0000F205 NNeu Text: "Then your instincts are good."
- 0000F206 PQue Prompt: "Why ask?" Text: "Why are you asking?"
- 0000F207 NQue Text: "I'm mapping who you are. Not just what you do."

## Exchange 2 (Phase 2 PlayerDialogue)
- 0000F208 PPos Prompt: "Good team" Text: "We make a good team."
- 0000F209 NPos Text: "Agreed. Your decisions improve our odds."
- 0000F20A PNeg Prompt: "Not sure" Text: "I'm not sure we're there yet."
- 0000F20B NNeg Text: "Then I'll keep earning it."
- 0000F20C PNeu Prompt: "Maybe" Text: "We'll see."
- 0000F20D NNeu Text: "I can work with that."
- 0000F20E PQue Prompt: "Really?" Text: "You trust me?"
- 0000F20F NQue Text: "More than I expected to."

## Exchange 3 (Phase 4 PlayerDialogue)
- 0000F210 PPos Prompt: "Trust" Text: "I trust you."
- 0000F211 NPos Text: "That's not a small thing. Thank you."
- 0000F212 PNeg Prompt: "Doubt" Text: "I still have doubts."
- 0000F213 NNeg Text: "Then I'll keep proving myself."
- 0000F214 PNeu Prompt: "Uncertain" Text: "I'm still figuring you out."
- 0000F215 NNeu Text: "Fair. I'm still figuring me out."
- 0000F216 PQue Prompt: "And you?" Text: "Do you trust me?"
- 0000F217 NQue Text: "Enough to follow you into danger."

## Exchange 4 (Phase 6 PlayerDialogue)
- 0000F218 PPos Prompt: "Friends" Text: "I consider you a friend."
- 0000F219 NPos Text: "Then I'm glad I found you."
- 0000F21A PNeg Prompt: "Professional" Text: "Let's keep this professional."
- 0000F21B NNeg Text: "Acknowledged. I'll keep my distance."
- 0000F21C PNeu Prompt: "Allies" Text: "We're allies. That's enough."
- 0000F21D NNeu Text: "Allies is a start."
- 0000F21E PQue Prompt: "Meaning?" Text: "What does that mean to you?"
- 0000F21F NQue Text: "It means I choose to stay."

## Dialog Actions (NPC Monologue)
- 0000F220 Closing Text: "I'm glad we talked. Ready to move out?"
- 0000F221 Dialog2 Text: "I've been analyzing our path together."
- 0000F222 Dialog4 Text: "Trust isn't efficient, but it's effective."
- 0000F223 Dialog7 Text: "You're not just an outcome. You're a choice."

## Greetings (Neutral -> Friendship)
- 0000F230 Greeting 1: "I've been watching your choices. Why do you help people?"
- 0000F231 Greeting 2: "You keep taking risks for strangers. What drives that?"

## Player Voice Mapping (Vanilla Player Lines)
These are the current vanilla player voice files mapped to each player line.
FormKey is the Fallout4.esm INFO used for audio.

### Exchange 1 (Phase 0 PlayerDialogue)
- 00F200 "It was the right thing to do." -> 00123053 "It was the right thing to do."
- 00F202 "Maybe later." -> 000264DE "Maybe later."
- 00F204 "I'll do what I can." -> 0005E52F "I'll do what I can."
- 00F206 "Why do you ask?" -> 000C43E6 "Why do you ask?"

### Exchange 2 (Phase 2 PlayerDialogue)
- 00F208 "We made a good team." -> 00072FFB "We made a good team."
- 00F20A "I'm not sure." -> 00064ED8 "I'm not sure."
- 00F20C "We'll see." -> 000B17AB "We'll see."
- 00F20E "Do you trust me?" -> 000B9241 "Do you trust me?"

### Exchange 3 (Phase 4 PlayerDialogue)
- 00F210 "I believe you." -> 0001DBC6 "I believe you."
- 00F212 "Doubts?" -> 00147D00 "Doubts?"
- 00F214 "I'm still figuring things out. You need to be patient with me." -> 00120957 "I'm still figuring things out. You need to be patient with me."
- 00F216 "Why don't you trust me?" -> 0006819F "Why don't you trust me?"

### Exchange 4 (Phase 6 PlayerDialogue)
- 00F218 "It's okay. I'm a friend." -> 000E98AE "It's okay. I'm a friend."
- 00F21A "Let's do this." -> 000FED78 "Let's do this."
- 00F21C "It's okay... we're friends." -> 00079484 "It's okay... we're friends."
- 00F21E "What do you mean by that?" -> 0001F745 "What do you mean by that?"
