# CompanionAstra Public Release Plan

Date: 2026-03-22

## Goal

Keep `CompanionAstra` as the public project name for now while making it obvious which work came from:
- user direction
- Claude
- Codex
- Gemini
- other AI contributors

## Recommended GitHub Structure

Use one public hub repo:
- `fenwulfse/CompanionAstra`

Use that repo as the public link you share everywhere.

Inside that repo:
- `main`
  - only the current user-approved stable build
- `claude/current`
  - Claude's current working branch
- `codex/current`
  - Codex current working branch
- `gemini/current`
  - Gemini current working branch
- `integration/current`
  - optional merge-test branch when comparing or combining work

## What To Avoid

- do not force a rename from `Astra` to anything else yet
- do not overwrite Claude's current repo or branch history
- do not treat old `main` as if it is the newest state without checking branch heads first
- do not ask outside testers to guess which AI branch matters

## Current GitHub Fact To Keep In Mind

The original public repo identified from local clone:
- `https://github.com/fenwulfse/CompanionAstra.git`

Remote state checked on 2026-03-22:
- default branch `HEAD` -> `v30-followplayer`
- `main` is older than the current default branch

Implication:
- if this repo is used as the hub, the README and branch layout should be cleaned up before wide public sharing

## Public-Facing README Shape

Top of README should state:
- what CompanionAstra is
- current status
- current known-good build
- where testers should go
- where developers should go

Suggested top sections:
1. `What This Project Is`
2. `Current Status`
3. `Known Working Features`
4. `Current AI Build Lanes`
5. `How To Test`
6. `How To Report Issues`
7. `Credits`

## How To Distinguish AI Work Cleanly

Use plain labels, not vague wording:
- `Claude lane`
- `Codex lane`
- `Gemini lane`

Suggested rule:
- each AI lane gets its own branch and release notes
- stable `main` only gets merged work the user has approved

Suggested release naming:
- `claude-2026-03-22`
- `codex-2026-03-22-power-armor-exit-working`
- `gemini-2026-03-22`

## Minimum Public Docs

Add these files to the hub repo:
- `README.md`
- `docs/STATUS.md`
- `docs/TESTING.md`
- `docs/AI_BUILDS.md`
- `docs/CONTRIBUTORS.md`

`docs/AI_BUILDS.md` should explain:
- which branch is whose
- what each build currently does
- what each build is trying to solve

## Best Places To Get Technical Eyes On The Project

### 1. GitHub first

Why:
- easiest place to send one link
- easiest place for programmers to inspect code
- easiest place to separate stable vs experimental lanes

### 2. Mutagen / Synthesis community

Public sources:
- Mutagen repo: `https://github.com/Mutagen-Modding/Mutagen`
- Synthesis repo: `https://github.com/Mutagen-Modding/Synthesis`
- Spriggit docs: `https://mutagen-modding.github.io/Spriggit/`

Why they matter:
- this project is using Mutagen directly
- the right audience will understand generator workflows, record parity, git-backed plugin workflows, and collaboration structure

Recommended approach:
- do not open with promotion
- open with a technical writeup
- ask for specific feedback on:
  - record generation strategy
  - CK parity validation
  - Git + Spriggit workflow for multi-AI collaboration

### 3. Kinggath / Sim Settlements modding community

Public sources:
- Sim Settlements credits page: `https://simsettlements2.com/credits/`
- Addon Maker's Toolkit Discord invite thread: `https://simsettlements.com/site/index.php?threads/addon-makers-toolkit-discord.15723/`
- Patreon: `https://www.patreon.com/kinggath`

Why this matters:
- they already work with large Fallout 4 mod pipelines
- they understand team-based mod development, writing, quests, voice, and testing
- their builder/modder channels are more likely to give practical CK and production feedback than general social media

Recommended approach:
- ask for modding workflow advice, not endorsement
- keep the ask narrow and respectful
- do not send a giant lore dump

### 4. Fallout modding communities

Best use:
- recruit testers
- show video clips
- ask for focused bug reproduction help

Good post angle:
- "Custom Fallout 4 companion built with code generation and multi-AI collaboration; looking for testers and CK/Mutagen feedback"

## Smart Outreach Order

1. Clean hub repo
2. Publish stable README and status docs
3. Publish one known-good release
4. Post to technical communities first
5. Post to broader Fallout communities second
6. Only then widen to LinkedIn/X/Facebook

Reason:
- programmers and modders will tell you if the public presentation is confusing before you blast it widely

## Suggested First Outreach Targets

### Technical / modding
- Mutagen GitHub community
- Synthesis GitHub community
- Sim Settlements addon-maker forum/Discord path

### Broader tester recruitment
- Reddit Fallout modding communities
- Nexus page or file page later, once there is a clean public package
- X / Facebook / LinkedIn only after the hub repo is readable

## Example Short Pitch

`CompanionAstra` is a Fallout 4 companion mod being developed with code generation, CK validation, and multi-AI collaboration. We are looking for testers, Papyrus/CK modders, and Mutagen/Spriggit workflow advice. Current known-good milestone: Astra can now enter and exit power armor through companion dialogue.

## Example Technical Ask For Mutagen Community

I am building a Fallout 4 companion mod generator with Mutagen and validating the generated records against vanilla companion structures in CK. I would like feedback on the cleanest public repo layout for a generator-driven mod with multiple AI contributors, and whether Spriggit should be introduced now or only after the generator stabilizes.

## Example Ask For Kinggath / Builder Community

I am working on a Fallout 4 companion project and trying to organize it so outside modders can inspect and help test it. The code-generated plugin now has a confirmed working power armor exit dialogue path. I am looking for advice on how to present a project like this cleanly to CK-heavy modders so it is easy to review and contribute without confusion.

## Best Immediate Next Step

Before public posting, create or clean:
- one hub README
- one stable release
- one `AI_BUILDS.md`
- one short issue template for testers

That will make every later social post easier and reduce confusion immediately.
