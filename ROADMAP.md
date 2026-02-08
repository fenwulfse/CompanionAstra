# CompanionAstra Roadmap

## Phase 1: Stability (Now)
- Lock INFO IDs for all affinity scenes (done through Murder).
- Keep Astra text as source of truth (text-first).
- Map player responses to closest vanilla audio.
- Generate Astra TTS for NPC lines by scene.

## Phase 2: CK Verification
- Verify every scene in CK:
  - Greetings (no bouncing)
  - NPC lines match text
  - Player lines are closest plausible vanilla matches
- Log discrepancies in GitHub issues.

## Phase 3: Narrative Pass
- Refine Astra dialogue for cohesion across scenes.
- Avoid verbatim Piper text.
- Preserve vanilla structure.

## Phase 4: Tooling
- Improve player voice matcher.
- Add mapping output for CK verification (CSV).
- Automate summary reports after build.

## Phase 5: Release Prep
- Stable build tag.
- Documentation sweep.
- Public “How to test” guide.
