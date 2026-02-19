# Team Collaboration Protocol (Astra)

Date: 2026-02-19

## Goal
Enable ChatGPT, Claude, Gemini, Copilot, and human testers to contribute without breaking Astra production stability.

## Non-Negotiable Boundaries
- ChatGPT owns `CompanionAstra` production mechanics and release promotion.
- Claude works in Claude-owned records/scripts and folders only.
- Gemini works in Gemini-owned records/scripts and folders only.
- No agent edits another agent's companion records without explicit coordinator approval.
- Deploys to Fallout 4 `Data` are serialized: one deploy owner at a time.

## Write Scope Rules
- Repository writes are allowed for docs/tools/source tied to assigned lane.
- Steam `Data` writes are deploy-only and must be logged in `CHANGELOG.md`.
- Deletions require explicit confirmation when not clearly temporary/build artifacts.

## Branch and Merge Rules
- `main`: release-ready only.
- `astra/integration`: staging for validated merges.
- `agent/chatgpt/*`, `agent/claude/*`, `agent/gemini/*`: active work.
- Merge gate:
  - build passes,
  - locked ID contract preserved unless planned migration,
  - changelog entry added,
  - test evidence attached or explicitly deferred.

## Required Handoff Block (All Agents)
Every handoff must include:
- Exact file list changed.
- Behavior change summary (gameplay-visible and data-level).
- Deployment fingerprint:
  - ESP SHA256 + size + timestamp,
  - PEX/PSC SHA256 for `QF_COMAstra_00000805`,
  - voice counts (`total`, `NPCFAstra`).
- Known risks and next test requested.

## Human Tester Program
- Use GitHub issue template: `.github/ISSUE_TEMPLATE/playtest-run.md`.
- For each test request, attach a packet from:
  - `Tools/create_playtest_packet.ps1`
- Minimum tester return data:
  - save context (new game or long save),
  - exact repro steps,
  - observed/expected behavior,
  - screenshot or short clip when possible,
  - whether exit-to-desktop required force close.

## Copilot and External Contributor Use
- Copilot can assist code drafting and review in PRs.
- External contributors should use `help wanted` + `playtest` labeled issues.
- No direct pushes to `main`; require PR review and checklist.

## Fast Incident Protocol (When Build Regresses)
1. Freeze new feature edits.
2. Record current hash and failing symptom.
3. Restore last known-good packet/checkpoint.
4. Apply one targeted fix only.
5. Re-test and re-fingerprint before any additional change.
