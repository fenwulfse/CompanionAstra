# Companion Talk Audit — 2026-03-22

Purpose:
- document the new batch inspector for vanilla companion talk/main-quest interactions
- explain the Deacon power-armor false lead without requiring more CK screenshots

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\Run-MQAstraALT.ps1 --dump-companion-talk-audit
```

Focused command:

```powershell
powershell -ExecutionPolicy Bypass -File .\Run-MQAstraALT.ps1 --dump-companion-talk-audit --quest=COMDeaconTalk,COMDeacon
```

Generated raw output:
- `companion_talk_audit_2026-03-22.txt`

## Key Findings

1. Vanilla talk quests really do own the normal power-armor exit prompt.
- `COMPiperTalk`, `COMCaitTalk`, and `COMDeaconTalk` all contain `Exit Power Armor`.
- In all three cases, the prompt is gated by:
  - `WornHasKeyword == 1`
  - `RunOn = QuestAlias`
  - `Alias = 0`
- The NPC-side exit response uses raw flag `64` (`EndRunningScene`).

2. Cait working in-game is consistent with the vanilla data.
- `COMCaitTalk` exposes a normal power-armor exit path.
- The user tested Cait and confirmed she gets in and out correctly.

3. Deacon's problem is probably not "missing power-armor support."
- `COMDeaconTalk` also exposes a normal `Exit Power Armor` prompt with the same alias-0 `WornHasKeyword` gate.
- That means Deacon does have a vanilla talk-quest PA-exit route.

4. Deacon's main companion quest has several likely interaction stealers.
- `COMDeacon` priority is `70`.
- `COMDeaconTalk` priority is `30`.
- `COMDeacon` also has `AddIdleTopicToHello`.
- `COMDeacon` contains recall-code prompts and multiple greeting-style entries gated by:
  - `CA_WantsToTalk`
  - `CA_WantsToTalkMurder`
  - `CA_WantsToTalkRomanceRetry`
- The user's in-game symptom matched that shape exactly:
  - when Deacon was in power armor, talking to him kept surfacing recall-code content instead of the PA-exit path

## Practical Interpretation

- Astra's current power-armor exit implementation should not be judged by Deacon's behavior.
- Deacon appears to be a vanilla runtime-state edge case where his higher-priority main companion quest can steal the interaction before the talk quest gets control.
- If Deacon needs deeper study later, the next target is not his exit fragment; it is the exact `COMDeacon` greeting/info that wins when his recall-code content is pending.

## User-Test Notes From Tonight

- Astra:
  - exited power armor successfully again
  - talked normally afterward on the later run
  - earlier post-exit talk lock did not reproduce in the later test
- Deacon:
  - did not offer the expected exit path in at least one run
  - kept surfacing recall-code dialogue instead
- Cait:
  - got in and out of power armor correctly

## Guardrail

When a vanilla companion behaves oddly in power armor:
- inspect the whole talk quest first
- inspect the main companion quest second
- do not assume the fragment is the problem just because the symptom appears on the talk wheel
