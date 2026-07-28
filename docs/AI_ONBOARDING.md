# AI Contributor Onboarding (Safe + Automated)

## Current Model
- `main` is production.
- Contributors (human or AI-assisted) should use **fork + PR**.
- Do not grant direct write access to unknown bot accounts.

This repo is public, so contributors do **not** need collaborator access to open pull requests.

## One-Time Setup (Owner)
1. Keep `main` as the stable branch.
2. Use issue templates to define tasks.
3. Require all changes through PRs.
4. Use CI guardrails:
   - `.github/workflows/privacy-guard.yml`
   - `.github/workflows/locked-id-guard.yml`

## Contributor Flow (Claude / Gemini / Copilot / Humans)
1. Fork repo:
   - `gh repo fork <OWNER>/CompanionAstra --clone`
2. Create branch:
   - `git checkout -b agent/<name>/<task-slug>`
3. Make changes and commit.
4. Push branch to fork:
   - `git push -u origin agent/<name>/<task-slug>`
5. Open PR back to upstream:
   - `gh pr create --repo <OWNER>/CompanionAstra --fill`

## Optional: Trusted Collaborator Invite
Only for trusted human accounts you personally verify.

Example (write access):
`gh api -X PUT repos/<OWNER>/CompanionAstra/collaborators/<github_user> -f permission=push`

## Sanity Checks
- Active PRs:
  - `gh pr list --repo <OWNER>/CompanionAstra`
- Existing forks:
  - `gh api repos/<OWNER>/CompanionAstra/forks --paginate --jq '.[].full_name'`
