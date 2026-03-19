# MQAstraALT Storyline Flow — v9 (2026-03-13)

## Narrative Arc
Astra is a pre-war DIA intelligence prototype (mechanical android, sister project to P.A.M.).
She's been monitoring the Commonwealth for 200 years — alone, watching, unable to act.
She intercepts the Vault 111 opening, recognizes the Sole Survivor as the variable that
changes everything, and recruits them.

## The Path: Minutemen → Brotherhood → Deacon → Institute

### Act 1: Recruitment & Concord (Stages 5-10)

**Stage 5** — Bootstrap at Vault 111
- Astra appears outside the vault: "There's an emergency in Concord"
- Short, urgent, gets the player moving
- Player choices: "Let's go" (Pos) / "Check house first" (Neg) / "Trade" (Neu) / "Who are you?" (Que)

**Stage 6** — Astra becomes companion (SetCompanion)

**Stage 7** — Sanctuary Workbench (Negative path only)
- Workshop tutorial, gear up, plan Concord

**Stage 9** — Red Rocket waypoint
- Dogmeat joins via quest alias (MQ106 pattern)
- Astra introduces the dog — charming, personal moment
- No more tactical dump here — that's for Concord Approach

**Stage 10** — Concord Approach (pre-Concord)
- "There — the Museum. You can hear the gunfire from here."
- Power armor + minigun on roof as insurance
- Differentiated from Red Rocket — focus on the assault, not the briefing

**Stage 10** — Coalition Pitch (post-Concord, gated on MQ102 stage 50)
- Astra reveals the four factions for the first time
- "I've been watching this Commonwealth tear itself apart for two hundred years"
- First hint of Astra's loneliness: "Running projections in an empty room while the world burned"
- Pos/Neu → Stage 15 (Info-First) | Neg/Que → Stage 20 (Not-Now)

### Act 2: The Big Picture (Stages 15-25)

**Stage 15** — Info-First
- "We build a map before we build a war" — kept from v8, great line
- Astra's been collecting signals for two centuries, now has someone to act on them
- All paths → Stage 25

**Stage 20** — Not-Now (player declines)
- Astra shows vulnerability: "I've waited two hundred years — I can wait a little longer"
- "Haven't for a very long time" — loneliness theme
- All paths → Stage 25

**Stage 25** — Convergence Prep (MAJOR EMOTIONAL BEAT)
- Astra reveals she knows about Shaun — "I should have said it sooner"
- "I was monitoring Vault 111 when someone opened it, killed your spouse, and took your child"
- Player can be angry ("You should have told me"), shocked ("You knew?"), or pragmatic
- Points toward the Institute as the culprit
- Mentions Diamond City detective and Railroad as leads
- All paths → Stage 30 (auto-advance to 35)

### Act 3: Alliances (Stages 35-55)

**Stage 35** — Sanctuary Regroup
- Preston is settling in — "he knows how to build something from nothing"
- Astra picks up Brotherhood distress signal from Cambridge
- Points south: "It's on the way toward Boston, toward the Institute"
- Pos → 40 (help Preston first) | Neg → 40 | Neu → 45 (skip to Cambridge) | Que → 45

**Stage 40** — First Step Terms (Preston's settlement ask)
- "Goodwill is currency out here"
- Astra shows personality: "Two hundred years of watching people struggle alone... it changes your priorities"
- Quick help, then move south
- All paths → Stage 45

**Stage 45** — Cambridge Approach
- Brotherhood distress signal getting stronger
- Astra teases Danse's secret: "he doesn't know something very important about himself"
- "We need friends with power armor"
- All paths → Stage 50

**Stage 50** — BoS Contact (after helping Danse)
- Debrief on Brotherhood intel
- "All roads lead to CIT" — Brotherhood's relay tracking confirms Institute location
- Astra reflects: "Destroy what you don't understand isn't a philosophy — it's fear with better weapons"
- All paths → Stage 55

**Stage 55** — Deacon Encounter (KEY SCENE — Railroad protection)
- A man in sunglasses appears — Railroad spy
- Astra recognizes the signal pattern: "Different face, different clothes, but the same signal pattern"
- This encounter PROTECTS the Railroad quest from failing when entering the Institute
- In-game: triggers RR101 stage 1050 (Road to Freedom complete) + starts Tradecraft
- "The Railroad has been inside the Institute. They know things about how to get in and get out alive"
- All paths → Stage 60

### Act 4: The Institute (Stages 60-85)

**Stage 60** — Institute Prep
- "Three factions, all pointing at the same place underground"
- Astra admits to being nervous: "I think the human word is 'nervous'"
- Plans the approach: tunnels, relay data, Sturges' knowledge
- All paths → Stage 65

**Stage 65** — The Descent (at CIT ruins)
- EMOTIONAL PEAK: "In two hundred years, you're the first person who treated me like I mattered"
- Astra is afraid the Institute will see her as "something to study. Or something to dismantle"
- Entering the tunnels
- All paths → Stage 70

**Stage 70** — Inside the Institute
- "Beautiful. And terrible." — Astra processes 200 years of surveillance finally seeing the real thing
- "Part of me wants to understand them. Part of me wants to burn it all down"
- Astra might learn about her own origins (DIA program, P.A.M. connection)
- All paths → Stage 75

**Stage 75** — Father's Truth (THE REVEAL)
- Father = Shaun. Your baby is sixty years old and runs the Institute.
- Astra is devastated: "I ran the data a thousand times... I never considered he might be the one running it all"
- "For the first time in two hundred years, I genuinely don't know"
- Player can comfort Astra, be angry, be pragmatic, or ask if she's okay
- All paths → Stage 80

**Stage 80** — The Choice
- Every faction wants something different
- Astra's most vulnerable line: "The only world where I matter is the one where someone remembers that I helped"
- Player chooses direction (or doesn't yet)
- All paths → Stage 85

**Stage 85** — Emergence (leaving the Institute)
- "Sunlight. I never thought I'd be so glad to see a ruined sky."
- Astra thanks the player: "For not leaving me behind. I've been alone a very long time."
- Strategic reality: "We know the truth. That makes us the most important people in the Commonwealth right now"
- All paths → Stage 100

**Stage 100** — Complete (endgame checkpoint)

## Positive Path Quick Flow
5 → 6 → 9 → 10 → 15 → 25 → 30 → 35 → 40 → 45 → 50 → 55 → 60 → 65 → 70 → 75 → 80 → 85 → 100

Skips (expected): 7/8 (Sanctuary branch), 20 (Not-Now branch)
Auto-advances (testing): 30→35, 45→50, 50→55, 55→60, 60→65, 65→70, 70→75, 75→80, 80→85

## Railroad Protection Mechanism
The key challenge: going to the Institute first normally fails the Railroad quest.
Solution: Deacon encounter at Stage 55 triggers the equivalent of:
- RR101 stage 1050 (Road to Freedom complete)
- Tradecraft started (RR102)
This protects the Railroad from being destroyed when the player enters the Institute.

## Character Voice Notes
- Astra starts clinical/urgent (stages 5-10) and gradually reveals vulnerability
- Her 200-year loneliness is the emotional through-line
- "Good." no longer opens every response — variety in openers
- Player responses have personality, not just "Confirmed" and "Let's do it"
- The Shaun reveal (stage 25) and Father reveal (stage 75) are emotional peaks
- The Descent (stage 65) is Astra's most personal moment
- The Choice (stage 80) is Astra at her most uncertain

## Console Shortcut for Testing (all positives)
```
SetStage Min00 100
; Then just talk to Astra repeatedly, hitting Positive each time
```

