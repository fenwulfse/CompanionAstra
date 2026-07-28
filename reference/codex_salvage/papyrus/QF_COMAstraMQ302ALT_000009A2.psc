;BEGIN FRAGMENT CODE - Do not edit anything between this and the end comment
Scriptname Fragments:Quests:QF_COMAstraMQ302ALT_000009A2 Extends Quest Hidden

Function MQ302AltEnsureBootstrapFromStartup()
  if IsRunning() && !GetStageDone(5)
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT startup fallback bootstrap to stage 5")
    endif
    SetStage(5)
  endif
EndFunction

Event OnInit()
  MQ302AltEnsureBootstrapFromStartup()
EndEvent

Event OnQuestInit()
  MQ302AltEnsureBootstrapFromStartup()
EndEvent

Function MQ302AltAdvanceObjective(int objectiveIndex)
  int prior = objectiveIndex - 5
  while prior >= 5
    if IsObjectiveDisplayed(prior)
      SetObjectiveCompleted(prior, true)
      prior = -1
    else
      prior -= 5
    endif
  endwhile
  SetObjectiveDisplayed(objectiveIndex, true)
EndFunction

;BEGIN FRAGMENT Fragment_Stage_0000_Item_00
Function Fragment_Stage_0000_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 0 start (bootstrap to stage 5 if needed)")
endif
if !GetStageDone(5)
  SetStage(5)
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0005_Item_00
Function Fragment_Stage_0005_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 5 start (Stage5ActionMode=" + Stage5ActionMode + ")")
endif
MQ302AltAdvanceObjective(5)

BootstrapStage5Initialized = true

; Early opening-route snapshot (observer-only, no quest mutation):
int stage5Mq101Stage = -1
int stage5Mq102Stage = -1
int stage5Mq103Stage = -1
int stage5Mq104Stage = -1
int stage5Bos100Stage = -1
int stage5Bos101Stage = -1
int stage5Rr101Stage = -1
if MQ101 && MQ101.IsRunning()
  stage5Mq101Stage = MQ101.GetStage()
endif
if MQ102 && MQ102.IsRunning()
  stage5Mq102Stage = MQ102.GetStage()
endif
if MQ103 && MQ103.IsRunning()
  stage5Mq103Stage = MQ103.GetStage()
endif
if MQ104 && MQ104.IsRunning()
  stage5Mq104Stage = MQ104.GetStage()
endif
if BoS100 && BoS100.IsRunning()
  stage5Bos100Stage = BoS100.GetStage()
endif
if BoS101 && BoS101.IsRunning()
  stage5Bos101Stage = BoS101.GetStage()
endif
if RR101 && RR101.IsRunning()
  stage5Rr101Stage = RR101.GetStage()
endif
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 5 early-funnel snapshot MQ101=" + stage5Mq101Stage + " MQ102=" + stage5Mq102Stage + " MQ103=" + stage5Mq103Stage + " MQ104=" + stage5Mq104Stage + " BoS100=" + stage5Bos100Stage + " BoS101=" + stage5Bos101Stage + " RR101=" + stage5Rr101Stage)
endif

if UseAstraAsQuestGiver
  QuestGiverMode = 1
elseif UseUnknownSurvivorQuestGiver
  QuestGiverMode = 2
else
  QuestGiverMode = 0
endif

Actor astraActor = None
if Alias_Astra
  astraActor = Alias_Astra.GetActorReference()
endif
if astraActor
  if DisallowedCompanionFaction && astraActor.IsInFaction(DisallowedCompanionFaction)
    astraActor.RemoveFromFaction(DisallowedCompanionFaction)
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 5 cleared DisallowedCompanionFaction on Astra")
    endif
  endif
  astraActor.EvaluatePackage()
endif

if Stage5ActionMode == 1
  if BootstrapScene
    BootstrapScene.Start()
  endif
elseif Stage5ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 5 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage5ActionMode == 3
  UnknownSurvivorAliasFillAttempted = true
  UnknownSurvivorAliasFillResolved = false
  ; Alias 2 fill is deferred in fragment-first mode (manual/CK or future quest script).
  if Alias_UnknownSurvivor
    if Alias_UnknownSurvivor.GetRef()
      UnknownSurvivorAliasFillResolved = true
    endif
  endif
  if BootstrapUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 5 starting BootstrapUnknownSurvivorScene")
    endif
    BootstrapUnknownSurvivorScene.Start()
  endif
endif

if Stage5ShouldAutoAdvanceTo10
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 5 auto-advancing to stage 10")
  endif
  SetStage(10)
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0010_Item_00
Function Fragment_Stage_0010_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 10 start (Stage10ActionMode=" + Stage10ActionMode + ")")
endif

MQ302AltAdvanceObjective(10)
CoalitionPitchStage10Presented = true

; Story-driven companion handoff (vanilla-style state gate):
; - do this in quest flow, not via manual console staging
; - unlock COMAstra pickup by ensuring HasBeenCompanionFaction==1
if !CompanionHandoffApplied
  Actor astraHandoffActor = None
  if Alias_Astra
    astraHandoffActor = Alias_Astra.GetActorReference()
  endif
  if astraHandoffActor
    if DisallowedCompanionFaction && astraHandoffActor.IsInFaction(DisallowedCompanionFaction)
      astraHandoffActor.RemoveFromFaction(DisallowedCompanionFaction)
    endif
    if HasBeenCompanionFaction && !astraHandoffActor.IsInFaction(HasBeenCompanionFaction)
      astraHandoffActor.AddToFaction(HasBeenCompanionFaction)
      astraHandoffActor.SetFactionRank(HasBeenCompanionFaction, 1)
    endif
    if CurrentCompanionFaction && astraHandoffActor.IsInFaction(CurrentCompanionFaction)
      astraHandoffActor.RemoveFromFaction(CurrentCompanionFaction)
    endif
    astraHandoffActor.EvaluatePackage()
  endif
  if COMAstra
    if !COMAstra.IsRunning()
      COMAstra.Start()
    endif
    ; Keep stage handoff meaningful and parity-friendly with companion quest flow.
    if !COMAstra.GetStageDone(80)
      COMAstra.SetStage(80)
    endif
    if !COMAstra.GetStageDone(90)
      COMAstra.SetStage(90)
    endif
  endif
  CompanionHandoffApplied = true
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 10 applied companion handoff (HasBeen=1, Current=0)")
  endif
endif

if Stage10ActionMode == 1
  if CoalitionPitchScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 10 starting CoalitionPitchScene")
    endif
    CoalitionPitchScene.Start()
  endif
elseif Stage10ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 10 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage10ActionMode == 3
  if CoalitionPitchUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 10 starting CoalitionPitchUnknownSurvivorScene")
    endif
    CoalitionPitchUnknownSurvivorScene.Start()
  endif
endif

if Stage10ShouldWriteBranchIntent
  if CoalitionPitchAccepted
    PendingMq302AltBranchIntent = 1 ; InfoFirst (placeholder)
  elseif CoalitionPitchDeclined
    PendingMq302AltBranchIntent = 2 ; NotNow (placeholder)
  else
    PendingMq302AltBranchIntent = 0
  endif
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 10 branch intent=" + PendingMq302AltBranchIntent)
  endif
endif

; Fragment-first scope guard:
; Keep vanilla MQ302 overrides out of this shell until the dedicated MQ302ALT quest script split.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0015_Item_00
Function Fragment_Stage_0015_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 15 start (Stage15ActionMode=" + Stage15ActionMode + ")")
endif

MQ302AltAdvanceObjective(15)
InfoFirstStage15Presented = true

if Stage15ActionMode == 1
  if InfoFirstScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 15 starting InfoFirstScene")
    endif
    InfoFirstScene.Start()
  endif
elseif Stage15ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 15 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage15ActionMode == 3
  if InfoFirstUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 15 starting InfoFirstUnknownSurvivorScene")
    endif
    InfoFirstUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 15 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0020_Item_00
Function Fragment_Stage_0020_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 20 start (Stage20ActionMode=" + Stage20ActionMode + ")")
endif

MQ302AltAdvanceObjective(20)
NotNowStage20Presented = true

if Stage20ActionMode == 1
  if NotNowScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 20 starting NotNowScene")
    endif
    NotNowScene.Start()
  endif
elseif Stage20ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 20 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage20ActionMode == 3
  if NotNowUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 20 starting NotNowUnknownSurvivorScene")
    endif
    NotNowUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 20 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0025_Item_00
Function Fragment_Stage_0025_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 25 start (Stage25ActionMode=" + Stage25ActionMode + ")")
endif

MQ302AltAdvanceObjective(25)
ConvergencePrepStage25Presented = true

if Stage25ActionMode == 1
  if ConvergencePrepScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 25 starting ConvergencePrepScene")
    endif
    ConvergencePrepScene.Start()
  endif
elseif Stage25ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 25 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage25ActionMode == 3
  if ConvergencePrepUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 25 starting ConvergencePrepUnknownSurvivorScene")
    endif
    ConvergencePrepUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 25 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0030_Item_00
Function Fragment_Stage_0030_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 30 start (Stage30ActionMode=" + Stage30ActionMode + ")")
endif

MQ302AltAdvanceObjective(30)
ConvergenceCommitStage30Presented = true

if Stage30ActionMode == 1
  if ConvergenceCommitScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 30 starting ConvergenceCommitScene")
    endif
    ConvergenceCommitScene.Start()
  endif
elseif Stage30ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 30 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage30ActionMode == 3
  if ConvergenceCommitUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 30 starting ConvergenceCommitUnknownSurvivorScene")
    endif
    ConvergenceCommitUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 30 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0035_Item_00
Function Fragment_Stage_0035_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 35 start (Stage35ActionMode=" + Stage35ActionMode + ")")
endif

; Stage 35 = Bunker Hill intercept window (observer-first).
; Fail-safe: if MQ206 lane already committed, jump to mass-fusion hook stage.
if (MQ206 && MQ206.IsRunning()) || (MQ206BoS && MQ206BoS.IsRunning()) || (MQ206RR && MQ206RR.IsRunning()) || (MQ206Min && MQ206Min.IsRunning())
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 35 skip: MQ206 branch already running; forwarding to stage " + HookStageMassFusionSplit)
  endif
  if HookStageMassFusionSplit > 0
    SetStage(HookStageMassFusionSplit)
  else
    SetStage(45)
  endif
  return
endif

int stage35Mq205Stage = -1
int stage35Inst302Stage = -1
int stage35Rr102Stage = -1
int stage35Bos302Stage = -1
if MQ205 && MQ205.IsRunning()
  stage35Mq205Stage = MQ205.GetStage()
endif
if Inst302 && Inst302.IsRunning()
  stage35Inst302Stage = Inst302.GetStage()
endif
if RR102 && RR102.IsRunning()
  stage35Rr102Stage = RR102.GetStage()
endif
if BoS302 && BoS302.IsRunning()
  stage35Bos302Stage = BoS302.GetStage()
endif
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 35 snapshot MQ205=" + stage35Mq205Stage + " Inst302=" + stage35Inst302Stage + " RR102=" + stage35Rr102Stage + " BoS302=" + stage35Bos302Stage)
endif

; Pre-commit window guard (design target: MQ205 >=40 and <100).
if stage35Mq205Stage != -1 && (stage35Mq205Stage < 40 || stage35Mq205Stage >= 100)
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 35 outside MQ205 pre-commit window; skipping scene start")
  endif
  return
endif

MQ302AltAdvanceObjective(35)
DeEscalationProbeStage35Presented = true

if Stage35ActionMode == 1
  if DeEscalationProbeScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 35 starting DeEscalationProbeScene")
    endif
    DeEscalationProbeScene.Start()
  endif
elseif Stage35ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 35 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage35ActionMode == 3
  if DeEscalationProbeUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 35 starting DeEscalationProbeUnknownSurvivorScene")
    endif
    DeEscalationProbeUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 35 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0040_Item_00
Function Fragment_Stage_0040_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 40 start (Stage40ActionMode=" + Stage40ActionMode + ")")
endif

; Stage 40 = containment follow-up; valid only if stage 35 fired.
if !DeEscalationProbeStage35Presented
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 40 skip: stage 35 was never presented")
  endif
  return
endif

; Fail-safe: if MQ206 lane already committed, forward to stage 45.
if (MQ206 && MQ206.IsRunning()) || (MQ206BoS && MQ206BoS.IsRunning()) || (MQ206RR && MQ206RR.IsRunning()) || (MQ206Min && MQ206Min.IsRunning())
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 40 skip: MQ206 branch already running; forwarding to stage " + HookStageMassFusionSplit)
  endif
  if HookStageMassFusionSplit > 0
    SetStage(HookStageMassFusionSplit)
  else
    SetStage(45)
  endif
  return
endif

int stage40Mq205Stage = -1
int stage40Inst302Stage = -1
if MQ205 && MQ205.IsRunning()
  stage40Mq205Stage = MQ205.GetStage()
endif
if Inst302 && Inst302.IsRunning()
  stage40Inst302Stage = Inst302.GetStage()
endif
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 40 snapshot MQ205=" + stage40Mq205Stage + " Inst302=" + stage40Inst302Stage)
endif

; Containment window guard (design target: MQ205 >=80 and <100).
if stage40Mq205Stage != -1 && (stage40Mq205Stage < 80 || stage40Mq205Stage >= 100)
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 40 outside containment window; skipping scene start")
  endif
  return
endif

MQ302AltAdvanceObjective(40)
EscalationContainmentStage40Presented = true

if Stage40ActionMode == 1
  if EscalationContainmentScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 40 starting EscalationContainmentScene")
    endif
    EscalationContainmentScene.Start()
  endif
elseif Stage40ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 40 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage40ActionMode == 3
  if EscalationContainmentUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 40 starting EscalationContainmentUnknownSurvivorScene")
    endif
    EscalationContainmentUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 40 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0045_Item_00
Function Fragment_Stage_0045_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 45 start (Stage45ActionMode=" + Stage45ActionMode + ")")
endif

Stage45ObservedMq206BranchMode = 0
Stage45ObservedMq206BranchStage = -1
if MQ206 && MQ206.IsRunning()
  Stage45ObservedMq206BranchMode = 1
  Stage45ObservedMq206BranchStage = MQ206.GetStage()
elseif MQ206BoS && MQ206BoS.IsRunning()
  Stage45ObservedMq206BranchMode = 2
  Stage45ObservedMq206BranchStage = MQ206BoS.GetStage()
elseif MQ206RR && MQ206RR.IsRunning()
  Stage45ObservedMq206BranchMode = 3
  Stage45ObservedMq206BranchStage = MQ206RR.GetStage()
elseif MQ206Min && MQ206Min.IsRunning()
  Stage45ObservedMq206BranchMode = 4
  Stage45ObservedMq206BranchStage = MQ206Min.GetStage()
endif

int stage45Dn084Stage = -1
bool stage45Dn084Running = false
if DN084 && DN084.IsRunning()
  stage45Dn084Running = true
  stage45Dn084Stage = DN084.GetStage()
endif
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 45 snapshot MQ206Mode=" + Stage45ObservedMq206BranchMode + " MQ206Stage=" + Stage45ObservedMq206BranchStage + " DN084Running=" + stage45Dn084Running + " DN084Stage=" + stage45Dn084Stage)
endif

; Guard: this intercept is only valid once a MQ206 branch exists.
if Stage45ObservedMq206BranchMode == 0
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 45 skip: no MQ206 branch running")
  endif
  return
endif

; Guard: this intercept targets DN084 pre-commit only.
if !stage45Dn084Running
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 45 skip: DN084 not running")
  endif
  return
endif
if stage45Dn084Stage >= 50
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 45 too late (DN084 stage=" + stage45Dn084Stage + "); forwarding to stage 50")
  endif
  SetStage(50)
  return
endif

MQ302AltAdvanceObjective(45)
MassFusionInterceptStage45Presented = true

if Stage45ActionMode == 1
  if MassFusionInterceptScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 45 starting MassFusionInterceptScene")
    endif
    MassFusionInterceptScene.Start()
  endif
elseif Stage45ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 45 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage45ActionMode == 3
  if MassFusionInterceptUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 45 starting MassFusionInterceptUnknownSurvivorScene")
    endif
    MassFusionInterceptUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 45 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0050_Item_00
Function Fragment_Stage_0050_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 50 start (Stage50ActionMode=" + Stage50ActionMode + ")")
endif

MQ302AltAdvanceObjective(50)
NoEnemiesCommitStage50Presented = true

Stage50ObservedMq206BranchMode = 0
Stage50ObservedMq206BranchStage = -1
if MQ206 && MQ206.IsRunning()
  Stage50ObservedMq206BranchMode = 1
  Stage50ObservedMq206BranchStage = MQ206.GetStage()
elseif MQ206BoS && MQ206BoS.IsRunning()
  Stage50ObservedMq206BranchMode = 2
  Stage50ObservedMq206BranchStage = MQ206BoS.GetStage()
elseif MQ206RR && MQ206RR.IsRunning()
  Stage50ObservedMq206BranchMode = 3
  Stage50ObservedMq206BranchStage = MQ206RR.GetStage()
elseif MQ206Min && MQ206Min.IsRunning()
  Stage50ObservedMq206BranchMode = 4
  Stage50ObservedMq206BranchStage = MQ206Min.GetStage()
endif
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 50 MQ206 gate snapshot mode=" + Stage50ObservedMq206BranchMode + " stage=" + Stage50ObservedMq206BranchStage)
endif

if Stage50ActionMode == 1
  if NoEnemiesCommitScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 50 starting NoEnemiesCommitScene")
    endif
    NoEnemiesCommitScene.Start()
  endif
elseif Stage50ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 50 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage50ActionMode == 3
  if NoEnemiesCommitUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 50 starting NoEnemiesCommitUnknownSurvivorScene")
    endif
    NoEnemiesCommitUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 50 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0055_Item_00
Function Fragment_Stage_0055_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 55 start (Stage55ActionMode=" + Stage55ActionMode + ")")
endif

MQ302AltAdvanceObjective(55)
CastleConflictInterceptStage55Presented = true

if Stage55ActionMode == 1
  if CastleConflictInterceptScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 55 starting CastleConflictInterceptScene")
    endif
    CastleConflictInterceptScene.Start()
  endif
elseif Stage55ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 55 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage55ActionMode == 3
  if CastleConflictInterceptUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 55 starting CastleConflictInterceptUnknownSurvivorScene")
    endif
    CastleConflictInterceptUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 55 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0060_Item_00
Function Fragment_Stage_0060_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 60 start (Stage60ActionMode=" + Stage60ActionMode + ")")
endif

MQ302AltAdvanceObjective(60)
CastleStandDownStage60Presented = true

if Stage60ActionMode == 1
  if CastleStandDownScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 60 starting CastleStandDownScene")
    endif
    CastleStandDownScene.Start()
  endif
elseif Stage60ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 60 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage60ActionMode == 3
  if CastleStandDownUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 60 starting CastleStandDownUnknownSurvivorScene")
    endif
    CastleStandDownUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 60 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0065_Item_00
Function Fragment_Stage_0065_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 65 start (Stage65ActionMode=" + Stage65ActionMode + ")")
endif

MQ302AltAdvanceObjective(65)
BosRrConflictInterceptStage65Presented = true

if Stage65ActionMode == 1
  if BosRrConflictInterceptScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 65 starting BosRrConflictInterceptScene")
    endif
    BosRrConflictInterceptScene.Start()
  endif
elseif Stage65ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 65 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage65ActionMode == 3
  if BosRrConflictInterceptUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 65 starting BosRrConflictInterceptUnknownSurvivorScene")
    endif
    BosRrConflictInterceptUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 65 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0070_Item_00
Function Fragment_Stage_0070_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 70 start (Stage70ActionMode=" + Stage70ActionMode + ")")
endif

MQ302AltAdvanceObjective(70)
BosRrContainmentStage70Presented = true

if Stage70ActionMode == 1
  if BosRrContainmentScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 70 starting BosRrContainmentScene")
    endif
    BosRrContainmentScene.Start()
  endif
elseif Stage70ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 70 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage70ActionMode == 3
  if BosRrContainmentUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 70 starting BosRrContainmentUnknownSurvivorScene")
    endif
    BosRrContainmentUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 70 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0075_Item_00
Function Fragment_Stage_0075_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 75 start (Stage75ActionMode=" + Stage75ActionMode + ")")
endif

MQ302AltAdvanceObjective(75)
CitBreachWarningStage75Presented = true

if Stage75ActionMode == 1
  if CitBreachWarningScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 75 starting CitBreachWarningScene")
    endif
    CitBreachWarningScene.Start()
  endif
elseif Stage75ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 75 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage75ActionMode == 3
  if CitBreachWarningUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 75 starting CitBreachWarningUnknownSurvivorScene")
    endif
    CitBreachWarningUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 75 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0080_Item_00
Function Fragment_Stage_0080_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 80 start (Stage80ActionMode=" + Stage80ActionMode + ")")
endif

MQ302AltAdvanceObjective(80)
CitBreachContainmentStage80Presented = true

if Stage80ActionMode == 1
  if CitBreachContainmentScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 80 starting CitBreachContainmentScene")
    endif
    CitBreachContainmentScene.Start()
  endif
elseif Stage80ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 80 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage80ActionMode == 3
  if CitBreachContainmentUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 80 starting CitBreachContainmentUnknownSurvivorScene")
    endif
    CitBreachContainmentUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 80 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0085_Item_00
Function Fragment_Stage_0085_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 85 start (Stage85ActionMode=" + Stage85ActionMode + ")")
endif

MQ302AltAdvanceObjective(85)
ReactorDecisionInterceptStage85Presented = true

if Stage85ActionMode == 1
  if ReactorDecisionInterceptScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 85 starting ReactorDecisionInterceptScene")
    endif
    ReactorDecisionInterceptScene.Start()
  endif
elseif Stage85ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 85 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage85ActionMode == 3
  if ReactorDecisionInterceptUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 85 starting ReactorDecisionInterceptUnknownSurvivorScene")
    endif
    ReactorDecisionInterceptUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 85 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0090_Item_00
Function Fragment_Stage_0090_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 90 start (Stage90ActionMode=" + Stage90ActionMode + ")")
endif

MQ302AltAdvanceObjective(90)
NoDetonationCommitStage90Presented = true

if Stage90ActionMode == 1
  if NoDetonationCommitScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 90 starting NoDetonationCommitScene")
    endif
    NoDetonationCommitScene.Start()
  endif
elseif Stage90ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 90 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage90ActionMode == 3
  if NoDetonationCommitUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 90 starting NoDetonationCommitUnknownSurvivorScene")
    endif
    NoDetonationCommitUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 90 remains a local shell beat until MQ302ALT quest flow is stable and tested.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0095_Item_00
Function Fragment_Stage_0095_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 95 start (Stage95ActionMode=" + Stage95ActionMode + ")")
endif

MQ302AltAdvanceObjective(95)
PostReactorTriageStage95Presented = true

if Stage95ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 95 mode 2 (state-only placeholder; no scene start)")
  endif
elseif Stage95ActionMode == 1
  if SharedConvergenceInterceptScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 95 starting SharedConvergenceInterceptScene")
    endif
    SharedConvergenceInterceptScene.Start()
  endif
elseif Stage95ActionMode == 3
  if SharedConvergenceInterceptUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 95 starting SharedConvergenceInterceptUnknownSurvivorScene")
    endif
    SharedConvergenceInterceptUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 95 is the shared control-panel convergence intercept shell beat.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0100_Item_00
Function Fragment_Stage_0100_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 100 start (Stage100ActionMode=" + Stage100ActionMode + ")")
endif

MQ302AltAdvanceObjective(100)
Mq302HookHandoffStage100Presented = true

if Stage100ActionMode == 2
  if DebugTraceMq302AltShellStages
    Debug.Trace(self + " MQ302ALT stage 100 mode 2 (state-only placeholder; no MQ302 calls)")
  endif
elseif Stage100ActionMode == 1
  if Mq302SuppressionCheckpointScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 100 starting Mq302SuppressionCheckpointScene")
    endif
    Mq302SuppressionCheckpointScene.Start()
  endif
elseif Stage100ActionMode == 3
  if Mq302SuppressionCheckpointUnknownSurvivorScene
    if DebugTraceMq302AltShellStages
      Debug.Trace(self + " MQ302ALT stage 100 starting Mq302SuppressionCheckpointUnknownSurvivorScene")
    endif
    Mq302SuppressionCheckpointUnknownSurvivorScene.Start()
  endif
endif

; Fragment-first scope guard:
; Stage 100 is the MQ302 suppression / non-nuclear handoff checkpoint shell beat.
;END CODE
EndFunction
;END FRAGMENT

;END FRAGMENT CODE - Do not edit anything between this and the begin comment

Scene Property BootstrapScene Auto
Scene Property BootstrapUnknownSurvivorScene Auto
Scene Property CoalitionPitchScene Auto
Scene Property CoalitionPitchUnknownSurvivorScene Auto
Scene Property InfoFirstScene Auto
Scene Property InfoFirstUnknownSurvivorScene Auto
Scene Property NotNowScene Auto
Scene Property NotNowUnknownSurvivorScene Auto
Scene Property ConvergencePrepScene Auto
Scene Property ConvergencePrepUnknownSurvivorScene Auto
Scene Property ConvergenceCommitScene Auto
Scene Property ConvergenceCommitUnknownSurvivorScene Auto
Scene Property DeEscalationProbeScene Auto
Scene Property DeEscalationProbeUnknownSurvivorScene Auto
Scene Property EscalationContainmentScene Auto
Scene Property EscalationContainmentUnknownSurvivorScene Auto
Scene Property MassFusionInterceptScene Auto
Scene Property MassFusionInterceptUnknownSurvivorScene Auto
Scene Property NoEnemiesCommitScene Auto
Scene Property NoEnemiesCommitUnknownSurvivorScene Auto
Scene Property CastleConflictInterceptScene Auto
Scene Property CastleConflictInterceptUnknownSurvivorScene Auto
Scene Property CastleStandDownScene Auto
Scene Property CastleStandDownUnknownSurvivorScene Auto
Scene Property BosRrConflictInterceptScene Auto
Scene Property BosRrConflictInterceptUnknownSurvivorScene Auto
Scene Property BosRrContainmentScene Auto
Scene Property BosRrContainmentUnknownSurvivorScene Auto
Scene Property CitBreachWarningScene Auto
Scene Property CitBreachWarningUnknownSurvivorScene Auto
Scene Property CitBreachContainmentScene Auto
Scene Property CitBreachContainmentUnknownSurvivorScene Auto
Scene Property ReactorDecisionInterceptScene Auto
Scene Property ReactorDecisionInterceptUnknownSurvivorScene Auto
Scene Property NoDetonationCommitScene Auto
Scene Property NoDetonationCommitUnknownSurvivorScene Auto
Scene Property SharedConvergenceInterceptScene Auto
Scene Property SharedConvergenceInterceptUnknownSurvivorScene Auto
Scene Property Mq302SuppressionCheckpointScene Auto
Scene Property Mq302SuppressionCheckpointUnknownSurvivorScene Auto

Quest Property COMAstra Auto
ReferenceAlias Property Alias_Player Auto
ReferenceAlias Property Alias_Astra Auto
ReferenceAlias Property Alias_UnknownSurvivor Auto

Quest Property MQ302 Auto
Quest Property MQ302Min Auto
Quest Property MQ302BoS Auto
Quest Property MQ302RR Auto
Quest Property MQ302Post Auto
GlobalVariable Property MQ302Faction Auto
Topic Property BranchIntentInfoFirstTopic Auto
Topic Property BranchIntentNotNowTopic Auto
Faction Property CurrentCompanionFaction Auto
Faction Property HasBeenCompanionFaction Auto
Faction Property DisallowedCompanionFaction Auto

Bool Property UseAstraAsQuestGiver Auto
Bool Property UseUnknownSurvivorQuestGiver Auto
Bool Property DebugTraceMq302AltShellStages Auto

Int Property Stage5ActionMode Auto
Int Property Stage10ActionMode Auto
Int Property Stage15ActionMode Auto
Int Property Stage20ActionMode Auto
Int Property Stage25ActionMode Auto
Int Property Stage30ActionMode Auto
Int Property Stage35ActionMode Auto
Int Property Stage40ActionMode Auto
Int Property Stage45ActionMode Auto
Int Property Stage50ActionMode Auto
Int Property Stage55ActionMode Auto
Int Property Stage60ActionMode Auto
Int Property Stage65ActionMode Auto
Int Property Stage70ActionMode Auto
Int Property Stage75ActionMode Auto
Int Property Stage80ActionMode Auto
Int Property Stage85ActionMode Auto
Int Property Stage90ActionMode Auto
Int Property Stage95ActionMode Auto
Int Property Stage100ActionMode Auto
Int Property PendingMq302AltBranchIntent Auto
Int Property Stage45ObservedMq206BranchMode Auto
Int Property Stage45ObservedMq206BranchStage Auto
Int Property Stage50ObservedMq206BranchMode Auto
Int Property Stage50ObservedMq206BranchStage Auto
Int Property QuestGiverMode Auto
Int Property UnknownSurvivorAliasFillMode Auto
Int Property HookStageBunkerHillIntercept Auto
Int Property HookStageBunkerHillContainment Auto
Int Property HookStageMassFusionSplit Auto
Int Property HookStageBosRrConflict Auto
Int Property HookStageCitBreachWarning Auto
Int Property HookStageReactorNoDetonation Auto
Int Property HookStageSharedConvergence Auto
Int Property HookStageMq302SuppressionCheckpoint Auto
Int Property HookMq206BranchStartStage Auto
Int Property HookMq206BranchPrepStage Auto
Int Property HookMq206BranchHandoffStage Auto
Int Property HookStageMinutemenOnlyEscalation Auto
Int Property HookStageMinutemenOnlyCastleFallback Auto

Bool Property Stage5ShouldAutoAdvanceTo10 Auto
Bool Property Stage10ShouldWriteBranchIntent Auto
Bool Property UsePreMq302HookMap Auto
Bool Property UseMinutemenOnlyFallbackMap Auto
Bool Property BootstrapStage5Initialized Auto
Bool Property CompanionHandoffApplied Auto
Bool Property CoalitionPitchStage10Presented Auto
Bool Property CoalitionPitchAccepted Auto
Bool Property CoalitionPitchDeclined Auto
Bool Property InfoFirstStage15Presented Auto
Bool Property NotNowStage20Presented Auto
Bool Property ConvergencePrepStage25Presented Auto
Bool Property ConvergenceCommitStage30Presented Auto
Bool Property DeEscalationProbeStage35Presented Auto
Bool Property EscalationContainmentStage40Presented Auto
Bool Property MassFusionInterceptStage45Presented Auto
Bool Property NoEnemiesCommitStage50Presented Auto
Bool Property CastleConflictInterceptStage55Presented Auto
Bool Property CastleStandDownStage60Presented Auto
Bool Property BosRrConflictInterceptStage65Presented Auto
Bool Property BosRrContainmentStage70Presented Auto
Bool Property CitBreachWarningStage75Presented Auto
Bool Property CitBreachContainmentStage80Presented Auto
Bool Property ReactorDecisionInterceptStage85Presented Auto
Bool Property NoDetonationCommitStage90Presented Auto
Bool Property PostReactorTriageStage95Presented Auto
Bool Property Mq302HookHandoffStage100Presented Auto
Bool Property UnknownSurvivorAliasFillAttempted Auto
Bool Property UnknownSurvivorAliasFillResolved Auto

Quest Property MQ101 Auto
Quest Property MQ102 Auto
Quest Property MQ103 Auto
Quest Property MQ104 Auto
Quest Property BoS100 Auto
Quest Property BoS101 Auto
Quest Property RR101 Auto
Quest Property MQ205 Auto
Quest Property RR102 Auto
Quest Property BoS302 Auto
Quest Property Inst302 Auto
Quest Property DN084 Auto
Quest Property BoS303 Auto
Quest Property RR201 Auto
Quest Property RR303 Auto
Quest Property RR304 Auto
Quest Property MQ302_Min Auto
Quest Property MQ206 Auto
Quest Property MQ206BoS Auto
Quest Property MQ206RR Auto
Quest Property MQ206Min Auto
Quest Property Min301 Auto

; NOTE:
; Keep declarations synchronized with VMAD properties to avoid CK property-cleanup prompts.
; Some properties are placeholders for future MQ302ALT behavior and may be read-only in current shell stages.