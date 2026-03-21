;BEGIN FRAGMENT CODE - Do not edit anything between this and the end comment
Scriptname Fragments:Quests:QF_MQAstraALT_0000080A Extends Quest Hidden

;BEGIN FRAGMENT Fragment_Stage_0000_Item_00
Function Fragment_Stage_0000_Item_00()
;BEGIN CODE
; Stage 0: Reset / auto-advance to stage 5
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 0 (reset/init)")
endif
CleanupMinRecruitQuickResolveWatcher()
MinRecruitQuickResolveArmed = false
MinRecruitQuickResolveApplied = false
MinRecruitTargetWorkshopSeen = false
DiamondCitySuppressionActive = false
NuclearOptionSuppressionActive = false
SetStage(5)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0005_Item_00
Function Fragment_Stage_0005_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 5 start")
endif

; --- Recovery branch check ---
if EnableRecoveryBranch && !ForceBypassRecovery
  if MQ302 && MQ302.GetStage() >= 10
    if DebugTrace
      Debug.Trace(self + " MQAstraALT recovery route: MQ302 stage >= 10")
    endif
    RecoveryBranchActive = true
    SetStage(95)
    return
  endif
endif

; --- Hard-fail check ---
if PlayerInstitute_Destroyed
  if PlayerInstitute_Destroyed.GetValue() >= 1.0
    if DebugTrace
      Debug.Trace(self + " MQAstraALT hard fail: Institute already destroyed")
    endif
    HardFailTriggered = true
    SetObjectiveDisplayed(5, abDisplayed = false)
    FailAllObjectives()
    Stop()
    return
  endif
endif

; --- Normal bootstrap ---
BootstrapComplete = true
SetObjectiveDisplayed(5)

; Wait for alias to fill after quest initialization
Utility.Wait(2.0)

; Block standard pickup greeting IMMEDIATELY after alias fills (before vault wait)
Actor AstraActor = Alias_Astra.GetActorReference()
if AstraActor
  if DisallowedCompanionFaction
    AstraActor.AddToFaction(DisallowedCompanionFaction)
    if DebugTrace
      Debug.Trace(self + " MQAstraALT suppressed standard greeting (early)")
    endif
  endif
endif

; Vault 111 exterior guard:
; wait until player exits the vault interior before teleporting/greeting
Actor PlayerRef = Game.GetPlayer()
while PlayerRef && PlayerRef.IsInInterior()
  Utility.Wait(1.0)
endwhile

; Move Astra AND Codsworth to the player
; Narrative: Astra tracked the vault opening, brought Codsworth as proof of trust
if AstraActor
  if DebugTrace
    Debug.Trace(self + " MQAstraALT moving Astra to player")
  endif
  AstraActor.MoveTo(Game.GetPlayer())
  AstraActor.EvaluatePackage()
endif

; Codsworth deferred — keeping bootstrap simple for now

; Scene is handled by high-priority Greeting topic (Priority 70 Hello)
; firing when player approaches Astra exterior.
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0006_Item_00
Function Fragment_Stage_0006_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 6 start (escort branch)")
endif

EscortBranchSelected = true
SetObjectiveCompleted(5)

Actor AstraActor = Alias_Astra.GetActorReference()
if AstraActor
  ; Vanilla companion pattern: SetCompanion fills the Followers quest
  ; Companion alias, which has follow/sandbox packages built in.
  ; Same as Piper, Nick, Danse — proven vanilla mechanic.
  ; Dogmeat uses a separate DogmeatCompanion slot — no conflict.
  if PlayerFaction
    AstraActor.AddToFaction(PlayerFaction)
  endif
  if DisallowedCompanionFaction
    AstraActor.RemoveFromFaction(DisallowedCompanionFaction)
  endif
  FollowersScript.GetScript().SetCompanion(AstraActor)
  AstraActor.EvaluatePackage()
  Debug.Notification("MQAstraALT: Astra is now your companion")
endif

; Stage routing handled by dialogue OnBegin/OnEnd:
; Positive → OnEnd = 9 (Red Rocket, skip sanctuary)
; Negative/Neutral/Question → OnEnd = 7 (Sanctuary workbench)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0007_Item_00
Function Fragment_Stage_0007_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 7 start")
endif

SetObjectiveCompleted(5)
SetObjectiveDisplayed(7)

if BootstrapScene
  BootstrapScene.Stop()
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0009_Item_00
Function Fragment_Stage_0009_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 9 start (red rocket - dogmeat pickup)")
endif

; Stop bootstrap scene so Astra isn't stuck in it
if BootstrapScene
  BootstrapScene.Stop()
endif

if SanctuaryWorkshopRef
  UnregisterForRemoteEvent(SanctuaryWorkshopRef, "OnWorkshopMode")
  UnregisterForRemoteEvent(SanctuaryWorkshopRef, "OnActivate")
endif
WorkshopGateArmed = false
WorkshopModeSeenStart = false
if !WorkshopUsedAtSanctuary
  WorkshopUsedAtSanctuary = true
endif

SetObjectiveCompleted(7)
SetObjectiveDisplayed(9)

if SanctuaryScene
  SanctuaryScene.Stop()
endif
SetObjectiveCompleted(8)

; Astra keeps following — SetCompanion from stage 6 handles it.
; No FollowerWait here; both Astra and Dogmeat follow to Concord.

; MQ106/Nick pattern: Dogmeat follows via SetPlayerTeammate + EvaluatePackage.
; Do NOT use SetDogmeatCompanion — it dismisses the human companion.
Actor DogmeatActor = Alias_Dogmeat.GetActorReference()
if DogmeatActor
  DogmeatActor.SetPlayerTeammate(abTeammate = true, abCanDoFavor = true)
  DogmeatActor.EvaluatePackage()
  if DebugTrace
    Debug.Trace(self + " MQAstraALT Dogmeat following via quest alias (MQ106 pattern)")
  endif
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0008_Item_00
Function Fragment_Stage_0008_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 8 start (workshop gate)")
  Debug.Notification("MQAstraALT: Stage 8 active. Use Sanctuary Workshop once.")
endif

WorkshopGateArmed = false
WorkshopModeSeenStart = false
WorkshopUsedAtSanctuary = false

SetObjectiveCompleted(7)
SetObjectiveDisplayed(8)

if SanctuaryScene
  SanctuaryScene.Stop()
endif

Actor playerRef = Game.GetPlayer()
if playerRef && WorkshopWorkbenchBase
  ObjectReference workshopRef = Game.FindClosestReferenceOfTypeFromRef(WorkshopWorkbenchBase, playerRef, 4096.0)
  if workshopRef
    SanctuaryWorkshopRef = workshopRef
    RegisterForRemoteEvent(workshopRef, "OnWorkshopMode")
    RegisterForRemoteEvent(workshopRef, "OnActivate")
    WorkshopGateArmed = true
    if DebugTrace
      Debug.Trace(self + " MQAstraALT workshop gate armed on " + workshopRef)
      Debug.Notification("MQAstraALT: Workshop gate armed.")
    endif
  else
    if DebugTrace
      Debug.Trace(self + " MQAstraALT workshop gate fallback: no nearby WorkshopWorkbench")
      Debug.Notification("MQAstraALT: Workshop not found. Advancing to stage 9.")
    endif
    SetObjectiveCompleted(8)
    SetStage(9)
  endif
else
  if DebugTrace
    Debug.Trace(self + " MQAstraALT workshop gate fallback: missing player or WorkshopWorkbenchBase")
    Debug.Notification("MQAstraALT: Workshop gate fallback. Advancing to stage 9.")
  endif
  SetObjectiveCompleted(8)
  SetStage(9)
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0010_Item_00
Function Fragment_Stage_0010_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 10 start")
endif

; Resume following after Red Rocket wait, enable trade/commands
Actor AstraActor = Alias_Astra.GetActorReference()
if AstraActor
  if DisallowedCompanionFaction
    AstraActor.RemoveFromFaction(DisallowedCompanionFaction)
  endif
  ; Resume following (was set to wait at Red Rocket stage 9)
  FollowersScript.GetScript().FollowerFollow(AstraActor)
  AstraActor.EvaluatePackage()
endif

SetObjectiveCompleted(5)
SetObjectiveCompleted(7)
SetObjectiveCompleted(8)
SetObjectiveCompleted(9)
SetObjectiveDisplayed(10)

if BootstrapScene
  BootstrapScene.Stop()
endif
if SanctuaryScene
  SanctuaryScene.Stop()
endif
if RedRocketScene
  RedRocketScene.Stop()
endif

bool codsworthConcordGatePassed = false
if MQ102 && MQ102.GetStageDone(50) == 1
  codsworthConcordGatePassed = true
endif

if codsworthConcordGatePassed
  Utility.Wait(0.5)
  CoalitionPitchPresented = true
  if CoalitionPitchScene
    CoalitionPitchScene.Start()
  endif
elseif DebugTrace
  Debug.Trace(self + " MQAstraALT stage 10 waiting for MQ102 stage 50 (Codsworth Concord handoff)")
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0015_Item_00
Function Fragment_Stage_0015_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 15 start")
endif
InfoFirstAccepted = true
SetObjectiveCompleted(10)
SetObjectiveDisplayed(15)
if CoalitionPitchScene
  CoalitionPitchScene.Stop()
endif
Utility.Wait(0.5)
if InfoFirstScene
  InfoFirstScene.Start()
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0020_Item_00
Function Fragment_Stage_0020_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 20 start")
endif
NotNowChosen = true
SetObjectiveCompleted(10)
SetObjectiveDisplayed(20)
if CoalitionPitchScene
  CoalitionPitchScene.Stop()
endif
Utility.Wait(0.5)
if NotNowScene
  NotNowScene.Start()
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0025_Item_00
Function Fragment_Stage_0025_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 25 start")
endif

SetObjectiveCompleted(15)
SetObjectiveCompleted(20)
SetObjectiveDisplayed(25)

if InfoFirstScene
  InfoFirstScene.Stop()
endif
if NotNowScene
  NotNowScene.Stop()
endif
Utility.Wait(0.5)
if ConvergencePrepScene
  ConvergencePrepScene.Start()
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0030_Item_00
Function Fragment_Stage_0030_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 30 start")
endif

SetObjectiveCompleted(25)
SetObjectiveDisplayed(30)

if ConvergencePrepScene
  ConvergencePrepScene.Stop()
endif

; Auto-advance to 35 (regroup) — gates on Min00/Min01 are on the greetings,
; but stage progression should not block.
Utility.Wait(0.5)
SetStage(35)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0035_Item_00
Function Fragment_Stage_0035_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 35 start")
endif

SetObjectiveCompleted(30)
SetObjectiveDisplayed(35)

if SuppressDiamondCityDuringRailroad
  DiamondCitySuppressionActive = true
  SuppressDiamondCityObjectives("Stage35")
endif

if SanctuaryRegroupScene
  SanctuaryRegroupScene.Stop()
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0040_Item_00
Function Fragment_Stage_0040_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 40 start")
endif

if !GetStageDone(35)
  SetStage(35)
endif

SetObjectiveCompleted(35)
SetObjectiveDisplayed(40)

if FirstStepTermsScene
  FirstStepTermsScene.Stop()
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0045_Item_00
Function Fragment_Stage_0045_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 45 start")
endif

SetObjectiveCompleted(40)
SetObjectiveDisplayed(45)

; Arm a low-risk First Step triage hook:
; once MinRecruit00 picks Tenpines/Oberland, touching that workshop can
; fast-resolve the Corvega campaign path and keep route momentum.
if !MinRecruitQuickResolveApplied
  ArmMinRecruitQuickResolve()
endif

if RouteDisciplineScene
  RouteDisciplineScene.Stop()
endif

; Auto-advance to 50 — keeps flow moving for testing
Utility.Wait(0.5)
SetStage(50)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0050_Item_00
Function Fragment_Stage_0050_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 50 start")
endif

if !GetStageDone(35)
  SetStage(35)
endif
if !GetStageDone(40)
  SetStage(40)
endif
if !GetStageDone(45)
  SetStage(45)
endif

if MinRecruitQuickResolveArmed
  CleanupMinRecruitQuickResolveWatcher()
  MinRecruitQuickResolveArmed = false
endif

if SuppressDiamondCityDuringRailroad
  DiamondCitySuppressionActive = true
  SuppressDiamondCityObjectives("Stage50")
endif

SetObjectiveCompleted(45)
SetObjectiveDisplayed(50)

if RailroadVectorScene
  RailroadVectorScene.Stop()
endif

; Auto-advance past vanilla RR101 gates for testing
Utility.Wait(0.5)
SetStage(55)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0055_Item_00
Function Fragment_Stage_0055_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 55 start")
endif

if SuppressDiamondCityDuringRailroad && DiamondCitySuppressionActive
  SuppressDiamondCityObjectives("Stage55")
endif

SetObjectiveCompleted(50)
SetObjectiveDisplayed(55)

if RailroadContactScene
  RailroadContactScene.Stop()
endif
Utility.Wait(0.5)
SetStage(60)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0060_Item_00
Function Fragment_Stage_0060_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 60 start")
endif

SetObjectiveCompleted(55)
SetObjectiveDisplayed(60)

if TradecraftDebriefScene
  TradecraftDebriefScene.Stop()
endif
Utility.Wait(0.5)
SetStage(65)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0065_Item_00
Function Fragment_Stage_0065_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 65 start")
endif

SetObjectiveCompleted(60)
SetObjectiveDisplayed(65)

if InstituteAccessScene
  InstituteAccessScene.Stop()
endif
Utility.Wait(0.5)
SetStage(70)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0070_Item_00
Function Fragment_Stage_0070_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 70 start")
endif

SetObjectiveCompleted(65)
SetObjectiveDisplayed(70)

if BoSContactScene
  BoSContactScene.Stop()
endif
Utility.Wait(0.5)
SetStage(75)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0075_Item_00
Function Fragment_Stage_0075_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 75 start")
endif

SetObjectiveCompleted(70)
SetObjectiveDisplayed(75)

if SturgesTunnelScene
  SturgesTunnelScene.Stop()
endif
Utility.Wait(0.5)
SetStage(80)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0080_Item_00
Function Fragment_Stage_0080_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 80 start")
  if MQ206Min
    Debug.Trace(self + " MQAstraALT stage 80 snapshot MQ206Min stage=" + MQ206Min.GetStage())
  endif
  if MQ302Min
    Debug.Trace(self + " MQAstraALT stage 80 snapshot MQ302Min stage=" + MQ302Min.GetStage())
  endif
endif

SetObjectiveCompleted(75)
SetObjectiveDisplayed(80)

if SturgesIntelScene
  SturgesIntelScene.Stop()
endif
Utility.Wait(0.5)
SetStage(85)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0085_Item_00
Function Fragment_Stage_0085_Item_00()
;BEGIN CODE
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 85 start")
endif

SetObjectiveCompleted(80)
SetObjectiveDisplayed(85)

if CITIngressScene
  CITIngressScene.Stop()
endif

; Tunnel ingress failsafe:
; keep this as gate-only access (no holotape dependency) and hold
; vanilla Nuclear Option startup while MQAstraALT owns this branch.
NuclearOptionSuppressionActive = true
ApplyNuclearOptionSuppression("Stage85")
PrimeTunnelInstituteLink("Stage85")
PrimeInstitutionalizedScannerBypass("Stage85")

ObjectReference tunnelTerminal = Game.GetFormFromFile(0x0017E1BE, "Fallout4.esm") as ObjectReference
if tunnelTerminal
  tunnelTerminal.Lock(false)
endif

ObjectReference tunnelKeypad = Game.GetFormFromFile(0x0017E1CA, "Fallout4.esm") as ObjectReference
if tunnelKeypad
  tunnelKeypad.Lock(false)
endif

ObjectReference tunnelGate = Game.GetFormFromFile(0x00150BED, "Fallout4.esm") as ObjectReference
if tunnelGate
  tunnelGate.Lock(false)
  tunnelGate.SetOpen()
  if DebugTrace
    Debug.Trace(self + " MQAstraALT stage 85 unlocked/opened Public Works tunnel gate failsafe")
  endif
endif
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0095_Item_00
Function Fragment_Stage_0095_Item_00()
;BEGIN CODE
RecoveryBranchActive = true
SetObjectiveDisplayed(95)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0100_Item_00
Function Fragment_Stage_0100_Item_00()
;BEGIN CODE
CleanupMinRecruitQuickResolveWatcher()
MinRecruitQuickResolveArmed = false
DiamondCitySuppressionActive = false
ReleaseNuclearOptionSuppression("Stage100")
SetObjectiveDisplayed(100)
;END CODE
EndFunction
;END FRAGMENT

Event ObjectReference.OnWorkshopMode(ObjectReference akSender, bool aStart)
bool handledSanctuaryGate = false
if WorkshopGateArmed && akSender == SanctuaryWorkshopRef
  if aStart
    WorkshopModeSeenStart = true
    if DebugTrace
      Debug.Trace(self + " MQAstraALT workshop mode entered at Sanctuary")
    endif
    handledSanctuaryGate = true
  elseif WorkshopModeSeenStart
    WorkshopUsedAtSanctuary = true
    WorkshopGateArmed = false
    WorkshopModeSeenStart = false
    UnregisterForRemoteEvent(akSender, "OnWorkshopMode")
    UnregisterForRemoteEvent(akSender, "OnActivate")
    if DebugTrace
      Debug.Trace(self + " MQAstraALT workshop mode exit detected; advancing to stage 9")
      Debug.Notification("MQAstraALT: Workshop mode exit detected. Stage 9.")
    endif
    SetObjectiveCompleted(8)
    SetStage(9)
    handledSanctuaryGate = true
  endif
endif

if handledSanctuaryGate
  return
endif

if MinRecruitQuickResolveArmed && !MinRecruitQuickResolveApplied && akSender == MinRecruitWorkshopRef
  if aStart
    MinRecruitTargetWorkshopSeen = true
    if DebugTrace
      Debug.Trace(self + " MQAstraALT MinRecruit workshop mode entered")
    endif
  elseif MinRecruitTargetWorkshopSeen
    MinRecruitTargetWorkshopSeen = false
    ApplyMinRecruitQuickResolve("WorkshopModeExit")
  endif
endif
EndEvent

Event ObjectReference.OnActivate(ObjectReference akSender, ObjectReference akActionRef)
Actor playerRef = Game.GetPlayer()

if WorkshopGateArmed && akSender == SanctuaryWorkshopRef && akActionRef == playerRef
  WorkshopUsedAtSanctuary = true
  WorkshopGateArmed = false
  WorkshopModeSeenStart = false
  UnregisterForRemoteEvent(akSender, "OnActivate")
  UnregisterForRemoteEvent(akSender, "OnWorkshopMode")

  if DebugTrace
    Debug.Trace(self + " MQAstraALT workshop activate detected; advancing to stage 9")
    Debug.Notification("MQAstraALT: Workshop activated. Stage 9.")
  endif

  SetObjectiveCompleted(8)
  if !GetStageDone(9)
    SetStage(9)
  endif
  return
endif

if MinRecruitQuickResolveArmed && !MinRecruitQuickResolveApplied && akSender == MinRecruitWorkshopRef && akActionRef == playerRef
  ApplyMinRecruitQuickResolve("Activate")
endif
EndEvent

Event Actor.OnLocationChange(Actor akSender, Location akOldLoc, Location akNewLoc)
if akSender != Game.GetPlayer()
  return
endif

if SuppressNuclearOptionDuringTunnel && GetStageDone(85) && !GetStageDone(100)
  if !NuclearOptionSuppressionActive
    NuclearOptionSuppressionActive = true
  endif
  ApplyNuclearOptionSuppression("LocationChange")
  PrimeTunnelInstituteLink("LocationChange")
  PrimeInstitutionalizedScannerBypass("LocationChange")
endif

if DiamondCitySuppressionActive && SuppressDiamondCityDuringRailroad
  SuppressDiamondCityObjectives("LocationChange")
endif

if MinRecruitQuickResolveArmed && !MinRecruitQuickResolveApplied
  if DebugTrace
    Debug.Trace(self + " MQAstraALT player location changed; refreshing MinRecruit quick-resolve watcher")
  endif
  RefreshMinRecruitQuickResolveBinding()
endif
EndEvent

Function ArmMinRecruitQuickResolve()
if MinRecruitQuickResolveApplied
  return
endif
MinRecruitQuickResolveArmed = true
MinRecruitTargetWorkshopSeen = false
Actor playerRef = Game.GetPlayer()
if playerRef
  RegisterForRemoteEvent(playerRef, "OnLocationChange")
endif
if DebugTrace
  Debug.Trace(self + " MQAstraALT MinRecruit quick-resolve arm requested")
endif
RefreshMinRecruitQuickResolveBinding()
EndFunction

Function CleanupMinRecruitQuickResolveWatcher()
Actor playerRef = Game.GetPlayer()
if playerRef
  UnregisterForRemoteEvent(playerRef, "OnLocationChange")
endif
if MinRecruitWorkshopRef
  UnregisterForRemoteEvent(MinRecruitWorkshopRef, "OnActivate")
  UnregisterForRemoteEvent(MinRecruitWorkshopRef, "OnWorkshopMode")
  MinRecruitWorkshopRef = None
endif
MinRecruitTargetWorkshopSeen = false
EndFunction

Function RefreshMinRecruitQuickResolveBinding()
if !MinRecruitQuickResolveArmed || MinRecruitQuickResolveApplied
  return
endif

Quest recruitQuest = ResolveMinRecruitQuestRef()
if !recruitQuest || !recruitQuest.IsRunning()
  return
endif

if recruitQuest.GetStageDone(400) == 1
  MinRecruitQuickResolveApplied = true
  MinRecruitQuickResolveArmed = false
  CleanupMinRecruitQuickResolveWatcher()
  return
endif

ReferenceAlias settlementWorkshopAlias = recruitQuest.GetAlias(9) as ReferenceAlias
ObjectReference targetWorkshop = None
if settlementWorkshopAlias
  targetWorkshop = settlementWorkshopAlias.GetRef()
endif

if targetWorkshop
  if MinRecruitWorkshopRef != targetWorkshop
    if MinRecruitWorkshopRef
      UnregisterForRemoteEvent(MinRecruitWorkshopRef, "OnActivate")
      UnregisterForRemoteEvent(MinRecruitWorkshopRef, "OnWorkshopMode")
    endif
    MinRecruitWorkshopRef = targetWorkshop
    RegisterForRemoteEvent(targetWorkshop, "OnActivate")
    RegisterForRemoteEvent(targetWorkshop, "OnWorkshopMode")
    if DebugTrace
      Debug.Trace(self + " MQAstraALT MinRecruit quick-resolve watcher armed on " + targetWorkshop)
      Debug.Notification("MQAstraALT: First Step quick-assist armed.")
    endif
  endif
elseif DebugTrace
  Debug.Trace(self + " MQAstraALT waiting for MinRecruit00 settlement workshop alias")
endif
EndFunction

Quest Function ResolveMinRecruitQuestRef()
Quest recruitQuest = MinRecruit00
if !recruitQuest
  recruitQuest = Game.GetFormFromFile(0x0011B36E, "Fallout4.esm") as Quest
  if recruitQuest
    MinRecruit00 = recruitQuest
  endif
endif
return recruitQuest
EndFunction

Quest Function ResolveMQ103QuestRef()
Quest dcQuest = MQ103
if !dcQuest
  dcQuest = Game.GetFormFromFile(0x000229E5, "Fallout4.esm") as Quest
  if dcQuest
    MQ103 = dcQuest
  endif
endif
return dcQuest
EndFunction

Quest Function ResolveMQ302QuestRef()
Quest mq302Quest = MQ302
if !mq302Quest
  mq302Quest = Game.GetFormFromFile(0x000229EE, "Fallout4.esm") as Quest
  if mq302Quest
    MQ302 = mq302Quest
  endif
endif
return mq302Quest
EndFunction

Quest Function ResolveMQ302MinQuestRef()
Quest mq302MinQuest = MQ302Min
if !mq302MinQuest
  mq302MinQuest = Game.GetFormFromFile(0x0010C64A, "Fallout4.esm") as Quest
  if mq302MinQuest
    MQ302Min = mq302MinQuest
  endif
endif
return mq302MinQuest
EndFunction

Quest Function ResolveMQ207QuestRef()
Quest mq207Quest = MQ207
if !mq207Quest
  mq207Quest = Game.GetFormFromFile(0x000229ED, "Fallout4.esm") as Quest
  if mq207Quest
    MQ207 = mq207Quest
  endif
endif
return mq207Quest
EndFunction

Function PrimeTunnelInstituteLink(String sourceTag)
Quest mq302MinQuest = ResolveMQ302MinQuestRef()
if !mq302MinQuest
  return
endif

Fragments:Quests:QF_MQ302Min_0010C64A mq302MinFrag = mq302MinQuest as Fragments:Quests:QF_MQ302Min_0010C64A
if !mq302MinFrag
  if DebugTrace
    Debug.Trace(self + " MQAstraALT tunnel prime skipped (" + sourceTag + "): MQ302Min fragment cast failed")
  endif
  return
endif

if mq302MinFrag.InstituteMMEntranceMarker && mq302MinFrag.InstituteMMEntranceMarker.IsDisabled()
  mq302MinFrag.InstituteMMEntranceMarker.EnableNoWait()
endif

if mq302MinFrag.InstituteRelayDoorRef
  mq302MinFrag.InstituteRelayDoorRef.Lock(false)
  mq302MinFrag.InstituteRelayDoorRef.SetOpen()
endif

if mq302MinFrag.MQ302MinTunnelSecurityGate01
  mq302MinFrag.MQ302MinTunnelSecurityGate01.Lock(false)
  mq302MinFrag.MQ302MinTunnelSecurityGate01.SetOpen()
endif

if mq302MinFrag.ExteriorTunnelDoor
  mq302MinFrag.ExteriorTunnelDoor.Lock(false)
  mq302MinFrag.ExteriorTunnelDoor.SetOpen()
endif

if DebugTrace
  Debug.Trace(self + " MQAstraALT tunnel prime applied from " + sourceTag)
endif
EndFunction

Function PrimeInstitutionalizedScannerBypass(String sourceTag)
Quest mq207Quest = ResolveMQ207QuestRef()
if !mq207Quest || !mq207Quest.IsRunning()
  return
endif

if !mq207Quest.GetStageDone(5)
  mq207Quest.SetStage(5)
  if DebugTrace
    Debug.Trace(self + " MQAstraALT scanner bypass primed MQ207 stage 5 from " + sourceTag)
  endif
endif
EndFunction

Function ApplyNuclearOptionSuppression(String sourceTag)
if !SuppressNuclearOptionDuringTunnel || !NuclearOptionSuppressionActive
  return
endif

ObjectReference relayTriggerRef = Game.GetFormFromFile(0x001126AC, "Fallout4.esm") as ObjectReference
if relayTriggerRef && !relayTriggerRef.IsDisabled()
  relayTriggerRef.DisableNoWait()
  if DebugTrace
    Debug.Trace(self + " MQAstraALT nuclear suppression disabled relay trigger from " + sourceTag)
  endif
endif

Quest mq302MinQuest = ResolveMQ302MinQuestRef()
if mq302MinQuest && mq302MinQuest.IsRunning()
  mq302MinQuest.Stop()
  if DebugTrace
    Debug.Trace(self + " MQAstraALT nuclear suppression stopped MQ302Min from " + sourceTag)
  endif
endif
EndFunction

Function ReleaseNuclearOptionSuppression(String sourceTag)
if !NuclearOptionSuppressionActive
  return
endif
NuclearOptionSuppressionActive = false

ObjectReference relayTriggerRef = Game.GetFormFromFile(0x001126AC, "Fallout4.esm") as ObjectReference
if relayTriggerRef && relayTriggerRef.IsDisabled()
  relayTriggerRef.EnableNoWait()
  if DebugTrace
    Debug.Trace(self + " MQAstraALT nuclear suppression released relay trigger from " + sourceTag)
  endif
endif
EndFunction

Function SuppressDiamondCityObjectives(String sourceTag)
if !SuppressDiamondCityDuringRailroad || !DiamondCitySuppressionActive
  return
endif

Quest dcQuest = ResolveMQ103QuestRef()
if !dcQuest || !dcQuest.IsRunning()
  return
endif

if dcQuest.GetStageDone(100) == 1
  return
endif

if dcQuest.GetStageDone(10) == 1 && dcQuest.GetStageDone(20) == 0
  dcQuest.SetObjectiveDisplayed(10, false)
endif
if dcQuest.GetStageDone(20) == 1 && dcQuest.GetStageDone(30) == 0
  dcQuest.SetObjectiveDisplayed(20, false)
endif
if dcQuest.GetStageDone(30) == 1 && dcQuest.GetStageDone(100) == 0
  dcQuest.SetObjectiveDisplayed(30, false)
endif

if DebugTrace
  Debug.Trace(self + " MQAstraALT MQ103 objective suppression applied from " + sourceTag)
endif
EndFunction

Function ApplyMinRecruitQuickResolve(String sourceTag)
if !MinRecruitQuickResolveArmed || MinRecruitQuickResolveApplied
  return
endif

Quest recruitQuest = ResolveMinRecruitQuestRef()
if !recruitQuest
  if DebugTrace
    Debug.Trace(self + " MQAstraALT quick-resolve skipped (" + sourceTag + "): MinRecruit00 not found")
  endif
  return
endif

if !recruitQuest.IsRunning()
  if DebugTrace
    Debug.Trace(self + " MQAstraALT quick-resolve skipped (" + sourceTag + "): MinRecruit00 not running")
  endif
  return
endif

if recruitQuest.GetStageDone(400) == 1
  MinRecruitQuickResolveApplied = true
  MinRecruitQuickResolveArmed = false
  CleanupMinRecruitQuickResolveWatcher()
  return
endif

if DebugTrace
  Debug.Trace(self + " MQAstraALT applying MinRecruit00 quick-resolve from " + sourceTag)
endif
recruitQuest.SetStage(400)
Utility.Wait(0.2)

if recruitQuest.GetStageDone(400) == 1
  MinRecruitQuickResolveApplied = true
  MinRecruitQuickResolveArmed = false
  CleanupMinRecruitQuickResolveWatcher()
  if DebugTrace
    Debug.Trace(self + " MQAstraALT MinRecruit00 advanced to stage 400; report to Preston")
    Debug.Notification("MQAstraALT: First Step triage complete. Report to Preston.")
  endif
else
  if DebugTrace
    Debug.Trace(self + " MQAstraALT MinRecruit00 stage 400 not confirmed yet; retry scheduled")
  endif
  RefreshMinRecruitQuickResolveBinding()
endif
EndFunction

;BEGIN FRAGMENT Fragment_Stage_0200_Item_00
Function Fragment_Stage_0200_Item_00()
;BEGIN CODE
; Trade: open Astra's inventory for the player
Actor AstraActor = Alias_Astra.GetActorReference()
if AstraActor
  AstraActor.OpenInventory(true)
endif
;END CODE
EndFunction
;END FRAGMENT

;END FRAGMENT CODE - Do not edit anything between this and the begin comment

; --- Aliases ---
ReferenceAlias Property Alias_Astra Auto
ReferenceAlias Property Alias_Dogmeat Auto

; --- Scene Properties ---
Scene Property BootstrapScene Auto
Scene Property SanctuaryScene Auto
Scene Property RedRocketScene Auto
Scene Property CoalitionPitchScene Auto
Scene Property InfoFirstScene Auto
Scene Property NotNowScene Auto
Scene Property ConvergencePrepScene Auto
Scene Property SanctuaryRegroupScene Auto
Scene Property FirstStepTermsScene Auto
Scene Property RouteDisciplineScene Auto
Scene Property RailroadVectorScene Auto
Scene Property RailroadContactScene Auto
Scene Property TradecraftDebriefScene Auto
Scene Property InstituteAccessScene Auto
Scene Property BoSContactScene Auto
Scene Property SturgesTunnelScene Auto
Scene Property SturgesIntelScene Auto
Scene Property CITIngressScene Auto

; --- Vanilla Quest References ---
Quest Property MQ302 Auto
Quest Property MQ102 Auto
Quest Property MQ103 Auto
Quest Property MinRecruit00 Auto
Quest Property MQ206Min Auto
Quest Property MQ302Min Auto
Quest Property MQ207 Auto

; --- Vanilla Globals ---
GlobalVariable Property PlayerInstitute_Destroyed Auto

; --- Vanilla Factions ---
Faction Property DisallowedCompanionFaction Auto
Faction Property PlayerFaction Auto

; --- Vanilla Base Forms ---
Form Property WorkshopWorkbenchBase Auto

; --- Config Properties ---
Bool Property DebugTrace = true Auto
Bool Property EnableRecoveryBranch = true Auto
Bool Property ForceBypassRecovery = false Auto
Bool Property SuppressDiamondCityDuringRailroad = true Auto
Bool Property SuppressNuclearOptionDuringTunnel = true Auto

; --- Runtime State ---
Bool Property BootstrapComplete Auto
Bool Property EscortBranchSelected Auto
Bool Property CoalitionPitchPresented Auto
Bool Property InfoFirstAccepted Auto
Bool Property NotNowChosen Auto
Bool Property RecoveryBranchActive Auto
Bool Property HardFailTriggered Auto
Bool Property WorkshopGateArmed Auto
Bool Property WorkshopModeSeenStart Auto
Bool Property WorkshopUsedAtSanctuary Auto
Bool Property MinRecruitQuickResolveArmed Auto
Bool Property MinRecruitQuickResolveApplied Auto
Bool Property MinRecruitTargetWorkshopSeen Auto
Bool Property DiamondCitySuppressionActive Auto
Bool Property NuclearOptionSuppressionActive Auto

; --- Runtime References ---
ObjectReference Property SanctuaryWorkshopRef Auto
ObjectReference Property MinRecruitWorkshopRef Auto

