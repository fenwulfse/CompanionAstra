# AI Baton Protocol

This is the short rule for switching leadership between Claude, Codex, Gemini,
Grok, or another collaborator without overwriting one another's work.

## The Rule

There is one active build. The AI taking the baton copies the latest active
build forward, adds to it in a new workspace, and leaves a complete handoff for
the next AI.

## Outgoing AI

1. Finish or clearly mark unfinished work.
2. Preserve the exact deployed plugin before changing anything else.
3. Record the Git commit, plugin SHA-256, deployment location, test result, known
   problems, and rollback path in a dated handoff note.
4. Push the source and handoff note so the next AI can inspect them.

Use `docs/AI_HANDOFF_TEMPLATE.md` so every handoff records the same essentials.

## Incoming AI

1. Read the newest handoff and verify the deployed plugin against its hash.
2. Use that exact build and commit as the new base, even when an older personal
   fork exists.
3. Create a new dated branch/worktree and a separate backup before editing.
4. Add to the inherited work. Do not regenerate from an older source tree or
   silently remove another collaborator's changes.
5. Produce a new standalone handoff package when passing the baton again.

## Fallout 4 Safety

- Load only one collaborator's standalone plugin build at a time.
- Keep AI-specific ownership in branches, folders, package names, and handoff
  notes. Do not casually rename an ESP; plugin identity and FormIDs are recorded
  in saves.
- A deliberate ESP rename or separate character plugin requires a proper rebuild
  and an appropriate clean test save, not a Windows file rename.
- The currently deployed plugin is the runtime truth. Reconcile it before any
  generator is allowed to overwrite it.

In plain language: Claude hands Codex the latest build; Codex copies it forward
and adds work; Codex hands that result back; Claude copies that result forward.
The project advances in one line instead of several forks writing over each
other.
