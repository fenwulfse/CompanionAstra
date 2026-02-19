# Vanilla Companion Affinity Scene Patterns
**Research Document - 2026-02-05**

## Purpose
Document the structural patterns used by ALL vanilla Fallout 4 companions in their affinity progression scenes. Goal: Identify common blueprints to apply to Claude's affinity arc.

## Research Sources
- PIPER_BIBLE.md (<ARCHIVE_ROOT>\GeminiCompanions\docs\PIPER_BIBLE.md)
- **fo4_dump.json** (<FO4_DUMP_JSON>) - 63MB xEdit export
- xEdit manual analysis (needed for phase/action details)

## Research Status
✅ **Companion list identified** - Found all affinity scene EditorIDs in fo4_dump.json
✅ **Piper fully documented** - Complete phase/action breakdown from PIPER_BIBLE.md
⚠️ **Other companions** - Scene names found, need xEdit for structural details
❌ **JSON parsing** - File too large (63MB) for automated extraction

**NEXT STEP:** Open xEdit manually, examine each companion's scenes, record phase counts, action indices, and loop positions.

---

### Piper (COMPiper Quest) - UPDATED GOLD STANDARD
*Verified via InspectAllPiperAffinity tool - 2026-02-06*

| Scene | EditorID | FormKey | Phases | Actions | Action Indices (Sorted) | Loop/Special Phases |
|-------|----------|---------|--------|---------|-------------------------|----------------------|
| Friendship | COMPiper_01_NeutralToFriendship | 162EF1 | 8 | 8 | 1, 2, 3, 4, 6, 7, 8, 9 (Skips 5) | Loop01(Ph2), Loop02(Ph4), Loop03(Ph6) |
| Admiration | COMPiper_02_FriendshipToAdmiration | 1CC87F | 6 | 6 | 1, 2, 3, 4, 5, 6 | Loop01(Ph2) |
| Confidant | COMPiper_02a_AdmirationToConfidant | 165A52 | 8 | 8 | 1, 2, 3, 5, 6, 7, 8, 9 (Skips 4) | Loop01(Ph4) |
| Infatuation | COMPiper_03_AdmirationToInfatuation | 162EF2 | 14 | 14 | 1-14 | Loop01(Ph4), Romance Gate(Ph10), RomanceArray(Ph12) |
| Disdain | COMPiper_04_NeutralToDisdain | 162EF3 | 3 | 3 | 1, 2, 3 | Loop01(Ph2) |
| Hatred | COMPiper_05_DisdainToHatred | 162EF4 | 10 | 10 | 1-10 | ResolutionPhase(Ph6), PostiveEndingPhase(Ph7), NegativeEndingPhase(Ph8) |

#### Structural Blueprints (The "Golden Pattern")

**1. The 8-Phase Friendship Blueprint (Scene 01)**
- **Structure:** PD → D → PD → D → PD → D → PD → D
- **Phases:** 8 (indices 0-7)
- **Loops:** Phase 2 (Loop01), Phase 4 (Loop02), Phase 6 (Loop03)
- **Indices:** 1, 2, 3, 4, **6, 7, 8, 9** (NOTE: Vanilla deliberately skips index 5)

**2. The 6-Phase Admiration Blueprint (Scene 02)**
- **Structure:** PD → D → PD → D → PD → D
- **Phases:** 6 (indices 0-5)
- **Loops:** Phase 2 (Loop01)
- **Indices:** 1, 2, 3, 4, 5, 6 (No skips)

**3. The 8-Phase Confidant Blueprint (Scene 02a)**
- **Structure:** PD → D → PD → D → PD → D → PD → D
- **Phases:** 8 (indices 0-7)
- **Loops:** Phase 4 (Loop01)
- **Indices:** 1, 2, 3, **5, 6, 7, 8, 9** (Skips index 4)

**4. The 14-Phase Infatuation Blueprint (Romance)**
- **Structure:** Complex branching with monologues and player choices.
- **Phases:** 14 (indices 0-13)
- **Indices:** 1-14 (Non-sequential phase mapping!)
- **Verified Mapping:**
  - Action 1 (PD): Phase 0
  - Action 2-4 (D): Phases 1, 2, 3
  - Action 5 (PD): Phase 4 (Loop01)
  - Action 6 (D): Phase 5
  - Action 14 (PD): Phase 6
  - Action 11 (D): Phase 7
  - Action 12 (PD): Phase 8
  - Action 13 (D): Phase 9
  - Action 7 (PD): Phase 10 (Romance Gate)
  - Action 8 (D): Phase 11
  - Action 9 (PD): Phase 12 (RomanceArray)
  - Action 10 (D): Phase 13
- **Highlights:** Romance Gate at Phase 10 (Player choice to romance), RomanceArray at Phase 12.

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

## CROSS-COMPANION PATTERN VALIDATION
*Verified via multi-companion scan - 2026-02-06*

### Friendship Scenes (Scene 01)
- **Piper:** 8 Phases, 8 Actions (Skips index 5)
- **Curie:** 8 Phases, 8 Actions (Sequential)
- **Preston:** 10 Phases, 10 Actions (Sequential)
- **Cait:** 6 Phases, 6 Actions (Sequential)
- **CONCLUSION:** 8 phases is the "Golden Mean". Piper's "skip index 5" is a unique quirk but safe to replicate.

### Admiration Scenes (Scene 02)
- **Piper:** 6 Phases, 6 Actions
- **Curie:** 11 Phases, 10 Actions
- **Cait:** 12 Phases, 12 Actions (Extremely complex branching)
- **CONCLUSION:** Admiration scenes are less standardized. Piper's 6-phase pattern is the most streamlined for a baseline.

### Confidant Scenes (Scene 02a/03)
- **Piper:** 8 Phases, 8 Actions
- **Cait:** 10 Phases, 10 Actions
- **CONCLUSION:** Returns to the ~8 phase pattern for deeper character development.

### Infatuation Scenes (Scene 03/04)
- **Piper:** 14 Phases, 14 Actions
- **Cait:** 12 Phases, 12 Actions
- **CONCLUSION:** 12-14 phases is the standard for romance/max affinity climaxes.

### Universal Constraints Identified:
1. **Alternating Action Types:** Almost all scenes strictly alternate between `PlayerDialogue` (Exchange) and `Dialog` (NPC Monologue/Transition).
2. **Phase Naming:** "Loop01" is the standard name for the first player dialogue phase to enable question looping.
3. **Stage Triggers:** Usually handled via `PhaseSetParentQuestStage` on the final phase (e.g., Curie/Preston/Cait set Stage 407 on their last phase).

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
