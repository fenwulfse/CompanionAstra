# Dialogue Polish Notes — v2 (2026-03-13)

## 2026-06-07 Nuka-World Bark Pass 3

### Astra Ambient Pass 3
- Expanded `COMAstraIdles` from 35 to 65 total barks.
- New structure adds 30 Nuka-World lines:
  - 8 park-wide barks gated by `DLC04NukaWorldLocation`.
  - 1 Transit Center bark.
  - 1 Gauntlet bark.
  - 4 Nuka-Town USA/Fizztop/gang atmosphere barks.
  - 3 Galactic Zone barks.
  - 3 Kiddie Kingdom barks.
  - 3 Safari Adventure barks.
  - 2 Dry Rock Gulch barks.
  - 2 Bottling Plant barks.
  - 2 Power Plant barks.
  - 1 Nukacade bark.
- Tone target: Astra should sound amused, analytical, and lightly alarmed by the park's cheerful violence without becoming generic raider commentary.
- Because the location gates reference `DLCNukaWorld.esm`, the plugin now requires Nuka-World as well as Far Harbor.

## 2026-06-07 Far Harbor Bark Pass 2

### Astra Ambient Pass 2
- Expanded `COMAstraIdles` from 5 to 35 total barks.
- New structure:
  - 15 general exploration barks with no location gate.
  - 5 Commonwealth/resupply barks gated by `CommonwealthLocation`.
  - 9 Far Harbor island barks gated by `DLC03FarHarborWorldLocation`.
  - 2 Far Harbor town barks gated by `DLC03FarHarborSettlementLocation`.
  - 2 Acadia barks gated by `DLC03AcadiaLocation`.
  - 2 Nucleus barks gated by `DLC03NucleusLocation`.
- This is the first real context-aware bark pass and intentionally targets the user's current play loop: Far Harbor questing with Commonwealth ammo/resupply breaks.
- Because the location gates reference `DLCCoast.esm`, the plugin now requires Far Harbor.

### Backlog: Combat Weapon Awareness
- User requested future exploration of Astra changing weapons based on combat context: ranged weapon at distance, melee weapon up close.
- Keep this separate from dialogue polish. It should be treated as an AI/package/equipment-system project and tested carefully because companion inventory/equip behavior can be fragile.
- Possible future approach: inspect vanilla companion combat packages and equip-item behavior first, then prototype a tiny controlled script or package layer before touching Astra's normal follower setup.

## 2026-06-03 Bark Field Pass

### Vanilla Bark Audit
- Ran a dedicated companion bark audit across base-game companions, DLC companions/robots, and live Astra.
- Audit result: vanilla companions use large `CompanionActorScript.IdleTopic` pools, plus separate Hello, `CA_Event_*`, combat/damage, gift, and talk-greeting lanes.
- Key lesson: ambient travel barks must stand alone while walking; command-wheel greetings and relationship/status lines should stay in talk scenes unless rewritten to work without a player prompt.
- Report saved at `docs/ASTRA_BARK_AUDIT_2026-06-03.md`.

### Astra Ambient Pass 1
- Replaced the five `COMAstraIdles` lines that sounded like command-wheel check-ins with neutral exploration barks.
- Regenerated matching voice files for `00020363`-`00020367`.
- Next bark work should expand the pool to 20-30 ambient lines, then add condition-specific location/story barks and combat barks as separate passes.

## 2026-05-28 Field Notes

### Voice / Mapping Polish
- User reported that some exchange/trade-adjacent dialogue is using a robotic voice in places where it should not, including lines where characters speak to Andrew.
- 2026-05-28 playtest note: the "thoughts"/Y-button player line is sometimes using a robotic voice for the player response. Audit player response FormIDs and `PlayerVoiceFemale01` / `PlayerVoiceMale01` file placement before deeper dialogue polish.
- Treat this as a voice-folder and voice-type audit item: verify response FormIDs, `Sound\Voice\MQAstraALT.esp\...` folder placement, player voice folders, and any Robot voice folders copied into the package.
- New gift handoff INFO `0203A0` has text, VMAD behavior, and a generated FUZ voice file.

### Companion Gift Handoff
- Astra now uses a guarded talk greeting for `HasItemForPlayer == 1`: "I found a small supply cache. It's yours."
- `CompanionGivePlayerItemInfoScript` is attached to the gift INFO so the item handoff clears the pending state.
- Ordinary talk greetings are gated by `HasItemForPlayer == 0` so the gift line has a clean lane.
- Astra's base `HasItemForPlayer` actor value is now seeded to `1` to provide one starter gift and kick the vanilla repeating timer after the handoff.

## Changes from v1 (2026-03-12)

### Stages 5-10 — Polished
- Red Rocket monologue shortened: just the dog intro, no tactical dump
- Concord Approach differentiated from Red Rocket: "You can hear the gunfire from here"
- Coalition Pitch rewritten: Astra's loneliness first appears ("empty room while the world burned")
- Player responses given personality: "Come on, boy. Let's move." instead of "We grab the dog and move."
- Removed "Good." as response opener across all scenes

### Stages 15-25 — Polished
- Info-First: "We build a map before we build a war" kept; added Astra's loneliness thread
- Not-Now: completely rewritten with vulnerability ("I've waited two hundred years — I can wait a little longer")
- Convergence Prep (Shaun reveal): MAJOR rewrite — emotional weight added
  - Astra admits she should have told you sooner
  - Player can be angry, shocked, or pragmatic
  - "Exit vector" replaced with actual emotion

### Stages 35-85 — COMPLETE REWRITE (New Route)
**Old route**: Minutemen → Railroad (Freedom Trail) → Tradecraft → BoS → Sturges tunnel → Institute
**New route**: Minutemen → Brotherhood (Cambridge) → Deacon encounter → Institute

| Stage | Old Scene | New Scene |
|-------|-----------|-----------|
| 35 | Sanctuary Regroup (Railroad pitch) | Sanctuary Regroup (Cambridge pitch) |
| 40 | First Step Terms (bland) | First Step Terms (personality) |
| 45 | Route Discipline (Corvega triage) | Cambridge Approach (Brotherhood distress signal) |
| 50 | Railroad Vector (ops briefing) | BoS Contact (Danse debrief) |
| 55 | Railroad Contact (Old North Church) | Deacon Encounter (Railroad spy) |
| 60 | Tradecraft Debrief (operational) | Institute Prep (nervous Astra) |
| 65 | Institute Access (protocol lock) | The Descent (emotional peak) |
| 70 | BoS Contact (Cambridge) | Inside the Institute (awe + anger) |
| 75 | Sturges Tunnel (technical) | Father's Truth (Shaun = Father) |
| 80 | Sturges Intel (packet debrief) | The Choice (what to do now) |
| 85 | CIT Ingress (approach brief) | Emergence (leaving, gratitude) |

## Patterns Fixed
1. **"Good." opener** — eliminated from all response openings
2. **Military briefing monotone** — replaced with character moments, emotional beats
3. **Generic player responses** — "Come on, boy", "You knew this whole time?", "Are you afraid?"
4. **Astra's loneliness** — woven throughout from stage 10 onward
5. **Emotional variety** — Shaun reveal (25) hits different from Cambridge approach (45) from Father reveal (75)
6. **Repetitive info** — Red Rocket and Concord no longer repeat Gristle/raider info

## Character Arc (Positive Path)
- **Stages 5-10**: Clinical, urgent, mission-focused Astra
- **Stage 10 (Coalition Pitch)**: First crack — "empty room while the world burned"
- **Stage 15**: "I've been collecting signals for two centuries — now I have someone to act on them"
- **Stage 20**: "I'll be here. I'm not going anywhere. Haven't for a very long time."
- **Stage 25**: Guilt about withholding Shaun info — "I should have said it sooner"
- **Stage 40**: "Two hundred years of watching people struggle alone... it changes your priorities"
- **Stage 45**: Teases Danse's secret — shows she knows more than she shares
- **Stage 50**: "Destroy what you don't understand isn't a philosophy — it's fear with better weapons"
- **Stage 55**: Respects the Railroad — "dangerous, thankless work, and they do it anyway"
- **Stage 60**: Admits to being nervous — "I think the human word is 'nervous'"
- **Stage 65**: Emotional peak — "you're the first person who treated me like I mattered"
- **Stage 70**: Conflicted — "Part of me wants to understand them. Part of me wants to burn it all down"
- **Stage 75**: Devastated by Father reveal — "I genuinely don't know"
- **Stage 80**: Most vulnerable — "The only world where I matter is the one where someone remembers that I helped"
- **Stage 85**: Gratitude — "For not leaving me behind. I've been alone a very long time."

## Still TODO
- [ ] Stay-in-place during dialogue (SetRestrained or FollowerWait)
- [ ] Camera actions for greetings (research Mutagen implementation)
- [ ] Voice generation for all 100+ lines (generate_voices.py updated, needs to run)
- [ ] Test full chain end-to-end with new dialogue
- [ ] PSC: wire Deacon encounter to trigger RR101 stage 1050 for Railroad protection

