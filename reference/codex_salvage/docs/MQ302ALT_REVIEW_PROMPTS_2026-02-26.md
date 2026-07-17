# COMAstraMQ302ALT Review Prompts (Player Memory + Replay Capture)

Purpose: lightweight checklist for replay/review so we can convert memory into exact hook points without guessing.

Use this while replaying or reviewing CK/xEdit/script notes. Keep entries short.

## Capture Template (per turning point)
- Location:
- Active quest/branch:
- Stage shown:
- On-screen warning/text:
- NPCs present:
- What changed immediately after:
- Did hostility lock in here? (`yes/no/unknown`)
- Candidate COMAstraMQ302ALT stage:

---

## Priority Turning Points

### 1) Bunker Hill flashpoint
Target shell stage: `35/40`

Capture:
- Which objective sent you there (`MQ205` / branch owner)
- Which factions are already present when combat starts
- Whether you can release synths without hard-locking a faction
- If firefight starts anyway, identify exact stage where containment is still possible

### 2) Mass Fusion split warning
Target shell stage: `45/50`

Capture:
- Exact warning text (Institute vs BoS hostility warning)
- Which quest currently owns objective at that moment
- Whether fast travel / faction state changes immediately on confirm

### 3) Castle conflict window
Target shell stage: `55/60`

Capture:
- Which side initiates (BoS vs Minutemen) in your route
- What quest/stage triggers the battle start
- Whether stand-down is possible in vanilla at that point

### 4) BoS/RR conflict escalation
Target shell stage: `65/70`

Capture:
- Earliest objective that clearly commits to RR-HQ/Prydwen escalation
- Which quest owns that objective (`RR201/RR303/RR304` lane)
- Whether any non-hostile branch still exists at that exact stage

### 5) CIT breach warning lane
Target shell stage: `75/80`

Capture:
- First unmistakable "breach is now inevitable" moment
- Which quest/stage announces or enables it
- Any reversible step left immediately after

### 6) Shared convergence (control panel / pre-Old-Robotics)
Target shell stage: `95`

Capture:
- Which factions are present in your route
- Exact room/marker where paths converge
- Last moment where route can be redirected before Old Robotics

### 7) MQ302 suppression checkpoint
Target shell stage: `100`

Capture:
- What exact event starts `MQ302` in your route (if it starts)
- Which precondition must be prevented so `MQ302` never starts
- What fallback state still keeps non-nuclear path viable

---

## Console Snapshot Block (optional, quick)
Use when you hit a turning point:

```txt
GetQuestRunning 0229EE
GetStage 0229EE
GetQuestRunning 0229EB
GetStage 0229EB
GetQuestRunning 06FA37
GetStage 06FA37
GetQuestRunning 0B9F9D
GetStage 0B9F9D
GetQuestRunning 0A8258
GetStage 0A8258
GetQuestRunning 10C64A
GetStage 10C64A
GetQuestRunning 10C64B
GetStage 10C64B
GetQuestRunning 10C64C
GetStage 10C64C
GetQuestRunning 215CC7
GetStage 215CC7
```

IDs:
- `0229EE` = MQ302
- `0229EB` = MQ205 (Bunker Hill)
- `06FA37` = RR102
- `0B9F9D` = BoS302
- `0A8258` = Inst302
- `10C64A` = MQ302Min
- `10C64B` = MQ302BoS
- `10C64C` = MQ302RR
- `215CC7` = MQ302Post
