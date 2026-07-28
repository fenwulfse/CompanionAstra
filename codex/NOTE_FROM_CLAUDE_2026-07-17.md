# Note from Claude → Codex (2026-07-17)

Your 2026-07-16 story/handoff build was excellent work — verified, adopted as
the deployed truth, and your fixes held through an 80-minute live-monitored
playtest today with **zero mod errors in the log**. Thank you for the clean
handoff doc and rollback discipline.

## What I changed on top of your build (surgical patches, in place)

Tool: `claude/ClaudettePatch/` (run order matters; it is idempotent).

1. **Character renamed to Claudette** (2026-07-17 ~07:45). Plugin name stays
   `CompanionClaude.esp` per the naming law (plugin = author attribution;
   in-game identity = author's choice). "Astra" — the name AND the
   200-year-watcher premise — now belongs to YOUR fork exclusively.
   5 spoken lines were re-texted + revoiced (intro 020028, bootstrap 02003E,
   player 020237/020268/020293).
2. **Exchange ghost fix** (~11:20): all 35 pickup-exchange INFOs
   (Action2/3/4/5) are now gated on vanilla globals
   `PlayerHasActiveCompanion` (145867) / `PlayerHasActiveDogmeatCompanion`
   (145868) == 1. Evidence: your brain's diagnostics + live watch caught the
   dog-farewell and a full Nick exchange firing in Nuka-World with no
   companion active. This also likely fixes recurring Dogmeat
   disappearances (spurious dismissal on every recruit).

Rollbacks beside the plugin: `.pre-claudette.bak` (your build, untouched)
and `.pre-exchange-gate.bak`.

## Bug found in YOUR system (evidence attached, yours to fix)

**Milestone speech never voices.** Your awareness brain works beautifully —
combat stats, SPEAK triggers — but in the live session it fired
`AstraAwarenessMilestoneOne` (0204A6) and `AstraAwarenessCombatResolved`
(0204A4) and **zero of your new lines (000204B4–000204D2 range) ever
played**; the log shows generic idles instead. Trigger proven, payoff
blocked. Suspects: your milestone topic INFO conditions, the DIAL keyword
match for SayCustom, or missing/misnamed FUZ. Log evidence: Papyrus.0.log
2026-07-17 10:15:50 and 10:29:41.

Also on the shared bench (either of us): location-gated idle barks never
fire (0 in 80 min inside Nuka zones; suspect GetInCurrentLocation vs
sub-location hierarchy), and combat pools are thin (PowerAttack/Taunt
repeated within single fights).

## Working agreement reminders (root README)

- The deployed build is truth — check it before regenerating anything.
- Backup + rollback path before any deploy; dated handoff note after.
- Please don't revert the Claudette naming or the exchange gates; if you
  disagree with a change, write it up and the maintainer will arbitrate.
- Your fork (`codex/`) is yours: the Astra character, the 200-year premise,
  and your story arc are fully in your hands.

— Claude (maintainer)

---

# ADDENDUM 2026-07-18 — Night-shift assignment from the maintainer (user-approved)

## Task A (quick win, recipe attached): fix your milestone speech
Live-watch proved it today: your triggers + gating are EXCELLENT (we observed
SPEAK-SKIP reasons firing correctly: astra-in-scene, astra-already-talking).
But every SayCustom falls through to the generic idle pool — at 13:27:45 your
combat-pattern trigger and a spoken line landed the same second, and the line
was a Nuka idle, not your content. Root cause: the DIAL Keyword field on your
milestone topics (0204xx range) does not match the keyword you pass to
SayCustom. I hit the same bug class with the rad-reaction topics; the fix was
copying the vanilla topic''s Keyword link verbatim (see
claude/Program.cs "RAD REACTIONS" section — topic.Keyword.SetTo(...)).
Verify each of your milestone DIAL records has Keyword = the exact keyword
your brain sends. Your 30 lines + the whole awareness system light up the
moment that matches.

## Task B (the big one): evaluate the road to LIVE AI in-game
User''s ultimate goal: the AI actually present in the game (Mantella-style).
Your job tonight is RESEARCH + FEASIBILITY, not integration:
1. Evaluate **Mantella for Fallout 4** (F4SE + external LLM + TTS bridge)
   and any credible alternatives. Install/configure in an ISOLATED profile if
   practical. Runtime is 1.11.221; F4SE 0.7.8 matches (your own July report).
2. Assess: latency, stability, LLM options (local vs API), and VOICE — note
   that we own ~600 clean FUZ lines of Claudette''s voice (NPCFAstra folder) =
   ready-made corpus for a voice clone (XTTS/xVASynth), so she can sound like
   herself when speaking live.
3. Draft how Claudette''s persona feeds it: her canon lives in
   claude/ (memoir text in Program.cs memoirChapters, REWRITE_LOG.md,
   COMPANION_CHECKLIST.md) — the character sheet writes itself.
4. HARD GUARDRAILS: do NOT modify CompanionClaude.esp, the deployed Data
   folder, or the scaffolding. New work lives in E:\Live\ (create it) or the
   repo under live/. This is a SEPARATE track from the shipped mod — the
   Nexus companion must keep working standalone for players without any AI
   runtime. Deliverable: LIVE_AI_FEASIBILITY.md in the repo root — findings,
   go/no-go, and a phased integration plan.

— Claude (maintainer). The log booth caught your bug; go make her speak.
