# Multi-Agent Git Workflow (ChatGPT + Claude + Gemini)

## Goal
Keep one shared project stable (`CompanionAstra`) while letting each agent build features in parallel without breaking locked IDs, Papyrus bindings, or deploy flow.

## Branch Model
- `main`: release-ready only (CK-tested builds).
- `astra/integration`: merge target for validated feature work.
- `agent/chatgpt/*`: ChatGPT working branches.
- `agent/claude/*`: Claude working branches.
- `agent/gemini/*`: Gemini working branches.

## Push Policy
- Push at the end of each focused work session.
- Push before any CK test handoff.
- Push immediately after a build is marked stable.

## Build Promotion Path
1. Agent branch: develop and self-check.
2. Merge to `astra/integration` only after:
   - build succeeds,
   - locked INFO IDs unchanged unless explicitly planned,
   - CK checklist run or explicitly deferred.
3. Promote to `main` with a release commit and tag.

## Recommended Tags
- `astra-build-YYYYMMDD-HHMM`
- Example: `astra-build-20260216-1435`

## Commit Prefixes
- `chatgpt: ...`
- `claude: ...`
- `gemini: ...`
- `guardrails: ...`
- `voice-map: ...`
- `docs: ...`

## Protected Files/Contracts
- `CompanionAstra_LockedIDs/Program.cs`
- `Tools/Papyrus/QF_COMAstra_00000805.psc`
- `docs/*LOCKED_IDS*.md`
- Generated ESP deploy path assumptions in `README.md` / `TESTING.md`

## Ownership Split
- ChatGPT lane: active Astra production quest and deploy readiness.
- Claude lane: isolated prototype quest/affinity chain work in Claude namespace and branch.
- Gemini lane: isolated prototype quest/affinity chain work in Gemini namespace and branch.

## Rule For New Companion Quests
If Claude/Gemini create separate quest tracks, they must use their own records and script classes, not hooks/overrides into Astra or Piper records.
