# Vanilla Companion Affinity Scene Patterns
**Research Document - 2026-02-05**

## Purpose
Document the structural patterns used by ALL vanilla Fallout 4 companions in their affinity progression scenes. Goal: Identify common blueprints to apply to Claude's affinity arc.

## Research Sources
- PIPER_BIBLE.md (E:\Gemini\GeminiCompanions\docs\PIPER_BIBLE.md)
- **fo4_dump.json** (D:\FO4Edit\dumps\fo4_dump.json) - 63MB xEdit export
- xEdit manual analysis (needed for phase/action details)

## Research Status
✅ **Companion list identified** - Found all affinity scene EditorIDs in fo4_dump.json
✅ **Piper fully documented** - Complete phase/action breakdown from PIPER_BIBLE.md
⚠️ **Other companions** - Scene names found, need xEdit for structural details
❌ **JSON parsing** - File too large (63MB) for automated extraction

**NEXT STEP:** Open xEdit manually, examine each companion's scenes, record phase counts, action indices, and loop positions.

---

## PIPER (COMPiper Quest)

### Scene Structure Overview
| Scene | EditorID | FormKey | Phases | Actions | Notes |
|-------|----------|---------|--------|---------|-------|
| Friendship | COMPiper_01_NeutralToFriendship | 162EF1 | 8 | 8 | First affinity scene |
| Admiration | COMPiper_02_FriendshipToAdmiration | 1CC87F | 6 | 6 | Second affinity scene |
| Confidant | COMPiper_02a_AdmirationToConfidant | 165A52 | 8 | 8 | Third affinity scene |
| Infatuation | COMPiper_03_AdmirationToInfatuation | 162EF2 | 14 | 14 | Romance scene |
| Disdain | COMPiper_04_NeutralToDisdain | 162EF3 | 3 | 3 | Negative path |
| Hatred | COMPiper_05_DisdainToHatred | 162EF4 | 10 | 10 | Negative path |

### Friendship Scene Details (162EF1)
**Structure:**
- **8 phases** (indices 0-7)
- **8 actions** with indices: 1, 2, 3, 4, 6, 7, 8, 9 (skips 5!)
- **Loop phases:** Loop01 at phase 2, Loop02 at phase 4, Loop03 at phase 6
- **Pattern:** PlayerDialogue → Dialog → PlayerDialogue → Dialog → PlayerDialogue → Dialog → PlayerDialogue → Dialog

**Action Breakdown:**
1. Action 1 (Phase 0): PlayerDialogue - Exchange 1
2. Action 2 (Phase 1): Dialog - NPC monologue
3. Action 3 (Phase 2): PlayerDialogue - Exchange 2 (Loop01)
4. Action 4 (Phase 3): Dialog - NPC monologue
5. Action 6 (Phase 4): PlayerDialogue - Exchange 3 (Loop02)
6. Action 7 (Phase 5): Dialog - NPC monologue
7. Action 8 (Phase 6): PlayerDialogue - Exchange 4 (Loop03)
8. Action 9 (Phase 7): Dialog - Closing/farewell

**Key Pattern:** 4 player dialogue exchanges, separated by NPC monologues

### Admiration Scene Details (1CC87F)
**Structure:**
- **6 phases** (indices 0-5)
- **6 actions** (indices need verification)
- **Loop phases:** TBD (need to verify in xEdit)
- **Pattern:** TBD

### Confidant Scene Details (165A52)
**Structure:**
- **8 phases** (indices 0-7)
- **8 actions** (indices need verification)
- **Loop phases:** TBD
- **Pattern:** Similar to Friendship? (8 phases suggests 4 exchanges)

### Infatuation Scene Details (162EF2)
**Structure:**
- **14 phases** (indices 0-13)
- **14 actions** (indices need verification)
- **Loop phases:** TBD
- **Pattern:** Likely 6-7 player exchanges based on phase count

---

## CAIT (COMCait Quest)

### Scene Structure Overview
**Scenes Found in fo4_dump.json:**
- COMCait_01_NeutralToFriendship
- COMCait_02_FriendshipToAdmiration
- COMCait_03_AdmirationToConfidant
- COMCait_04_ConfidantToInfatuation (Note: Piper uses _03_ for Infatuation)
- COMCait_05_NeutralToDisdain
- COMCait_06_DisdainToHatred

| Scene | EditorID | FormKey | Phases | Actions | Notes |
|-------|----------|---------|--------|---------|-------|
| Friendship | COMCait_01_NeutralToFriendship | ? | ? | ? | NEED xEdit |
| Admiration | COMCait_02_FriendshipToAdmiration | ? | ? | ? | NEED xEdit |
| Confidant | COMCait_03_AdmirationToConfidant | ? | ? | ? | NEED xEdit |
| Infatuation | COMCait_04_ConfidantToInfatuation | ? | ? | ? | NEED xEdit |

**TODO:** Open xEdit, find COMCait quest, extract phase/action counts

---

## PRESTON GARVEY (COMPreston Quest)

### Scene Structure Overview
| Scene | EditorID | FormKey | Phases | Actions | Notes |
|-------|----------|---------|--------|---------|-------|
| TBD | TBD | TBD | ? | ? | Need to research |

**TODO:** Research Preston's affinity progression scenes

---

## NICK VALENTINE (COMNick Quest)

### Scene Structure Overview
| Scene | EditorID | FormKey | Phases | Actions | Notes |
|-------|----------|---------|--------|---------|-------|
| TBD | TBD | TBD | ? | ? | Need to research |

**TODO:** Research Nick's affinity progression scenes

---

## CURIE (COMCurie Quest)

### Scene Structure Overview
**Scenes Found:**
- COMCurie_01_NeutralToFriendship
- COMCurie_02_FriendshipToAdmiration
- COMCurie_03_AdmirationToConfidant
- COMCurie_04_ConfidantToInfatuation

| Scene | EditorID | FormKey | Phases | Actions | Notes |
|-------|----------|---------|--------|---------|-------|
| Friendship | COMCurie_01_NeutralToFriendship | ? | ? | ? | NEED xEdit |
| Admiration | COMCurie_02_FriendshipToAdmiration | ? | ? | ? | NEED xEdit |
| Confidant | COMCurie_03_AdmirationToConfidant | ? | ? | ? | NEED xEdit |
| Infatuation | COMCurie_04_ConfidantToInfatuation | ? | ? | ? | NEED xEdit |

---

## COMMON PATTERNS (Preliminary)

### Phase Count Observations
- **8 phases** appears to be standard for early scenes (Friendship, Confidant)
- **6 phases** for middle scenes (Admiration)
- **14 phases** for romance scenes (Infatuation)
- Pattern suggests: **2 phases per exchange** (1 for player dialogue, 1 for NPC response/transition)

### Action Index Patterns
- **Friendship uses indices 1,2,3,4,6,7,8,9** - Why skip 5?
- Need to verify if this pattern repeats across other companions
- Hypothesis: Index 5 might be reserved for special conditions or branching

### Loop Naming
- **Loop01, Loop02, Loop03** placed at player dialogue phases
- Allows questions (PQue responses) to loop back to the same exchange
- Standard vanilla convention confirmed

### Alternating Action Types
- **PlayerDialogue → Dialog** pattern confirmed for Friendship
- PlayerDialogue: Full 4-option wheel (Pos/Neg/Neu/Que)
- Dialog: NPC monologue or transition dialogue

---

## RESEARCH METHODOLOGY

### Step 1: Load Fallout4.esm in xEdit
1. Open xEdit with Fallout4.esm
2. Navigate to Quest category
3. Search for companion quests:
   - **COMCait** - Cait's companion quest
   - **COMPreston** - Preston Garvey's companion quest
   - **COMNick** - Nick Valentine's companion quest
   - **COMCurie** - Curie's companion quest
   - **COMHancock** - Hancock's companion quest
   - **COMMacCready** - MacCready's companion quest
   - **COMDeacon** - Deacon's companion quest
   - **COMDanse** - Paladin Danse's companion quest
   - **COMPiper** - Piper's companion quest (reference/validation)

### Step 2: For Each Companion Quest
Expand the quest record and examine the **Scenes** section. For each affinity scene, record:

**Scene Metadata:**
- EditorID (e.g., "COMCait_01_NeutralToFriendship")
- FormKey (8-digit hex ID)
- Scene name/description

**Structural Data:**
- **Phase count** - Count entries in Phases array
- **Action count** - Count entries in Actions array
- **Action indices** - List the Index field for each action (e.g., 1,2,3,4,6,7,8,9)
- **Loop phases** - Which phase indices have Loop01, Loop02, Loop03 names?

**Action Pattern:**
- Action Type sequence (PlayerDialogue vs Dialog)
- How many PlayerDialogue actions? (= number of player exchanges)
- How many Dialog actions? (= number of NPC monologues)

### Step 3: Create Comparison Table
Build a master table comparing all companions:

| Companion | Scene 1 (Friendship) | Scene 2 (Admiration) | Scene 3 (Confidant) | Scene 4 (Romance) |
|-----------|---------------------|----------------------|---------------------|-------------------|
| Piper     | 8ph/8act (1,2,3,4,6,7,8,9) | 6ph/6act | 8ph/8act | 14ph/14act |
| Cait      | ?ph/?act | ?ph/?act | ?ph/?act | ?ph/?act |
| Preston   | ?ph/?act | ?ph/?act | ?ph/?act | ?ph/?act |
| Nick      | ?ph/?act | ?ph/?act | ?ph/?act | ?ph/?act |
| Curie     | ?ph/?act | ?ph/?act | ?ph/?act | ?ph/?act |

### Step 4: Identify Common Patterns
Look for:
- **Standard phase counts** - Do all companions use 8/6/8/14 or similar?
- **Action index patterns** - Does everyone skip index 5 in 8-action scenes?
- **Loop placement** - Are loops always at player dialogue phases?
- **Exchange counts** - How many player choices per scene?

### Step 5: Document Deviations
Note any companions that DON'T follow the pattern:
- Why might they deviate?
- Does it cause issues?
- Should we avoid those patterns?

### Step 6: Create the Golden Blueprint
Based on findings, define the "safest" pattern that works across all companions:
```
FRIENDSHIP SCENE (Scene 1):
- Phases: 8
- Actions: 8 (indices 1,2,3,4,6,7,8,9)
- Loops: Phase 2 (Loop01), Phase 4 (Loop02), Phase 6 (Loop03)
- Pattern: PD → D → PD → D → PD → D → PD → D
- Player exchanges: 4

ADMIRATION SCENE (Scene 2):
- Phases: 6 (TBD from research)
- Actions: 6 (TBD)
- Loops: TBD
- Player exchanges: 3 (estimated)

... etc.
```

### Step 7: Apply to Claude
Redesign Claude's scenes to match the golden blueprint while keeping AI theme.

## NEXT STEPS

1. **User task:** Open xEdit and research companion scenes using methodology above
2. **Document findings** in this file (VANILLA_AFFINITY_PATTERNS.md)
3. **Identify golden blueprint** from cross-companion comparison
4. **Redesign Claude's scenes:**
   - Admiration → Match vanilla 6-phase pattern
   - Confidant → Match vanilla 8-phase pattern
   - Infatuation → Match vanilla 14-phase pattern
   - Keep AI-learning-humanity dialogue theme
   - Apply vanilla structural blueprint

---

---

## CLAUDE'S CURRENT IMPLEMENTATION (For Comparison)

### Friendship Scene ✅ MATCHES VANILLA
**Status:** Architecturally replicates Piper's structure - works great!
- **Phases:** 8 (indices 0-7)
- **Actions:** 8 (indices 1,2,3,4,6,7,8,9) - matches Piper exactly
- **Loops:** Loop01 (phase 2), Loop02 (phase 4), Loop03 (phase 6)
- **Pattern:** PlayerDialogue → Dialog → PlayerDialogue → Dialog (repeating)
- **Player exchanges:** 4
- **Code location:** Program.cs lines 390-560

**Why it works so well:**
- Direct structural replication of COMPiper_01_NeutralToFriendship
- All 8 DIAL slots filled per PlayerDialogue action (Pos/Neg/Neu/Que)
- Follows vanilla action index pattern (skips 5)
- Alternating action types match vanilla

### Admiration Scene ⚠️ NEEDS VALIDATION
**Status:** Original design, needs vanilla pattern matching
- **Phases:** 6 (indices 0-5)
- **Actions:** 6 (estimated, need to count in code)
- **Loops:** Loop01, Loop02, Loop03 at phases 0, 2, 4
- **Player exchanges:** 3
- **Code location:** Program.cs lines ~600-643

**Concerns:**
- Is 6 phases correct for Admiration?
- Are loop positions correct?
- Do action indices match vanilla?
- Need to verify against Piper's COMPiper_02_FriendshipToAdmiration (1CC87F)

### Confidant Scene ⚠️ NEEDS VALIDATION
**Status:** Original design, needs vanilla pattern matching
- **Phases:** 8 (indices 0-7)
- **Actions:** 8 (estimated)
- **Loops:** Loop01, Loop02, Loop03, Loop04 at phases 0, 2, 4, 6
- **Player exchanges:** 4
- **Code location:** Program.cs lines ~645-694

**Concerns:**
- Phase count seems right (8 like Friendship)
- But are we using the right action indices?
- Are loop positions vanilla-compliant?
- Need to verify against Piper's COMPiper_02a_AdmirationToConfidant (165A52)

### Infatuation Scene ⚠️ NEEDS VALIDATION
**Status:** Original design, most complex, needs thorough validation
- **Phases:** 14 (indices 0-13)
- **Actions:** 14 (estimated)
- **Loops:** Loop01-Loop06 at phases 0, 4, 6, 8, 10, 12
- **Player exchanges:** 6
- **Code location:** Program.cs lines ~696-769

**Concerns:**
- Phase count matches Piper's romance scene count (14)
- But are loop positions correct?
- Are we using vanilla action indices?
- Loop spacing seems irregular (0, then 4, then every 2)
- Need detailed comparison with Piper's COMPiper_03_AdmirationToInfatuation (162EF2)

### Summary of Required Work
- **Friendship:** ✅ Already matches vanilla - minimal polish needed
- **Admiration:** ⚠️ Verify structure against vanilla, likely needs redesign
- **Confidant:** ⚠️ Verify structure against vanilla, likely needs redesign
- **Infatuation:** ⚠️ Most complex, definitely needs structure validation

---

## REFERENCES
- PIPER_BIBLE.md: Complete Piper scene documentation
- Current Friendship scene: Lines 390-560 in Program.cs (working example)
- Guardrail.AssertScenes(): Validates scene structure
- This document: Track vanilla patterns and guide redesign
