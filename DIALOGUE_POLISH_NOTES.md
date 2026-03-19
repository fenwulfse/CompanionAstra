# Dialogue Polish Notes — v2 (2026-03-13)

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

