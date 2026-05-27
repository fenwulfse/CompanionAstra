; ============================================================================
; MQAstraALTQuestScript
; Vanilla-style readable MQ quest script.
; Fragments should stay thin and forward into this script.
; ============================================================================

Scriptname MQAstraALTQuestScript Extends Quest Hidden

; --- Fragment handlers ---

Function HandleFragmentStage0000Item00()
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
CompanionAvailabilityUnlocked = false
RedRocketArrivalWatchArmed = false
Actor resetPlayerRef = Game.GetPlayer()
if resetPlayerRef
  UnregisterForRemoteEvent(resetPlayerRef, "OnLocationChange")
endif
SetStage(5)
EndFunction

Function HandleFragmentStage0005Item00()
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 5 start")
endif

; --- Recovery branch check ---
if EnableRecoveryBranch && !ForceBypassRecovery
  if ShouldUseExistingSaveMigration("Stage5")
    if DebugTrace
      Debug.Trace(self + " MQAstraALT recovery route: existing-save readiness matched")
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

; Codsworth deferred ? keeping bootstrap simple for now

; Scene is handled by high-priority Greeting topic (Priority 70 Hello)
; firing when player approaches Astra exterior.
EndFunction

Function HandleFragmentStage0006Item00()
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 6 start (Sanctuary escort branch)")
endif

EscortBranchSelected = true
SetObjectiveCompleted(5)
SetObjectiveDisplayed(9)
RedRocketArrivalWatchArmed = true

Actor AstraActor = Alias_Astra.GetActorReference()
if AstraActor
  ; Vanilla Nick/Deacon pattern:
  ; MQAstraALT owns the temporary escort beat first.
  ; COMAstra only becomes available after the post-Museum handoff.
  if PlayerFaction
    AstraActor.AddToFaction(PlayerFaction)
  endif
  AstraActor.SetPlayerTeammate(abTeammate = true, abCanDoFavor = false)
  FollowersScript.GetScript().FollowerFollow(AstraActor)
  FollowersScript.GetScript().FollowerSetDistanceMedium(AstraActor)
  AstraActor.IgnoreFriendlyHits()
  AstraActor.EvaluatePackage(abResetAI = true)
  if AstraEscortScene && !AstraEscortScene.IsPlaying()
    AstraEscortScene.Start()
  endif
  Debug.Notification("MQAstraALT: Astra temporary escort active")
endif

; Stage routing handled by dialogue OnBegin/OnEnd:
; Negative ? OnEnd = 7 (Sanctuary workbench)
EndFunction

Function HandleFragmentStage0205Item00()
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 205 start (Red Rocket travel branch)")
endif

EscortBranchSelected = true
SetObjectiveCompleted(5)
SetObjectiveDisplayed(9)
RedRocketArrivalWatchArmed = true

Actor AstraActor = Alias_Astra.GetActorReference()
if AstraActor
  if PlayerFaction
    AstraActor.AddToFaction(PlayerFaction)
  endif
  AstraActor.SetPlayerTeammate(abTeammate = true, abCanDoFavor = false)
  AstraActor.IgnoreFriendlyHits()
  AstraActor.EvaluatePackage(abResetAI = true)
  if AstraEscortScene && AstraEscortScene.IsPlaying()
    AstraEscortScene.Stop()
  endif
  if AstraTravelToRedRocketScene && !AstraTravelToRedRocketScene.IsPlaying()
    AstraTravelToRedRocketScene.Start()
  endif
  Debug.Notification("MQAstraALT: Astra leading to Red Rocket")
endif
Actor playerRef = Game.GetPlayer()
if playerRef
  RegisterForRemoteEvent(playerRef, "OnLocationChange")
endif
EndFunction

Function HandleFragmentStage0007Item00()
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 7 start")
endif

SetObjectiveCompleted(5)
SetObjectiveDisplayed(7)

if BootstrapScene
  BootstrapScene.Stop()
endif
EndFunction

Function HandleFragmentStage0009Item00()
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 9 start (red rocket - dogmeat pickup)")
endif

RedRocketArrivalWatchArmed = false
Actor playerRef = Game.GetPlayer()
if playerRef
  UnregisterForRemoteEvent(playerRef, "OnLocationChange")
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

Actor AstraActor = Alias_Astra.GetActorReference()
if AstraActor
  if AstraTravelToRedRocketScene && AstraTravelToRedRocketScene.IsPlaying()
    AstraTravelToRedRocketScene.Stop()
  endif
  FollowersScript.GetScript().FollowerWait(AstraActor)
  AstraActor.EvaluatePackage(abResetAI = true)
  if RedRocketScene && !RedRocketScene.IsPlaying()
      RedRocketScene.Start()
  endif
endif

; MQ106/Nick pattern: Dogmeat follows via SetPlayerTeammate + EvaluatePackage.
; Do NOT use SetDogmeatCompanion ? it dismisses the human companion.
Actor DogmeatActor = Alias_Dogmeat.GetActorReference()
if DogmeatActor
  DogmeatActor.SetPlayerTeammate(abTeammate = true, abCanDoFavor = true)
  FollowersScript.GetScript().FollowerFollow(DogmeatActor)
  FollowersScript.GetScript().FollowerSetDistanceMedium(DogmeatActor)
  DogmeatActor.IgnoreFriendlyHits()
  DogmeatActor.EvaluatePackage(abResetAI = true)
  if DogmeatEscortScene && !DogmeatEscortScene.IsPlaying()
    DogmeatEscortScene.Start()
  endif
  if DebugTrace
    Debug.Trace(self + " MQAstraALT Dogmeat following via quest alias (MQ106 pattern)")
  endif
endif
EndFunction

Function HandleFragmentStage0008Item00()
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
EndFunction

Function HandleFragmentStage0010Item00()
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 10 start")
endif

; Resume following after Red Rocket wait.
Actor AstraActor = Alias_Astra.GetActorReference()
if AstraActor
  if DisallowedCompanionFaction
    AstraActor.RemoveFromFaction(DisallowedCompanionFaction)
  endif
  ; Resume following (was set to wait at Red Rocket stage 9)
  FollowersScript.GetScript().FollowerFollow(AstraActor)
  FollowersScript.GetScript().FollowerSetDistanceMedium(AstraActor)
  AstraActor.EvaluatePackage(abResetAI = true)
  if AstraEscortScene && AstraEscortScene.IsPlaying()
    AstraEscortScene.Stop()
  endif
  if AstraTravelToMuseumScene && !AstraTravelToMuseumScene.IsPlaying()
    AstraTravelToMuseumScene.Start()
  endif
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
EndFunction

Function HandleFragmentStage0015Item00()
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 15 start")
endif
TransitionPostMuseumEscortToCompanionAvailability("Stage15")
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
EndFunction

Function HandleFragmentStage0020Item00()
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 20 start")
endif
TransitionPostMuseumEscortToCompanionAvailability("Stage20")
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
EndFunction

Function HandleFragmentStage0025Item00()
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
EndFunction

Function HandleFragmentStage0030Item00()
if DebugTrace
  Debug.Trace(self + " MQAstraALT stage 30 start")
endif

SetObjectiveCompleted(25)
SetObjectiveDisplayed(30)

if ConvergencePrepScene
  ConvergencePrepScene.Stop()
endif

; Auto-advance to 35 (regroup) ? gates on Min00/Min01 are on the greetings,
; but stage progression should not block.
Utility.Wait(0.5)
SetStage(35)
EndFunction

Function HandleFragmentStage0035Item00()
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
EndFunction

Function HandleFragmentStage0040Item00()
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
EndFunction

Function HandleFragmentStage0045Item00()
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

; Auto-advance to 50 ? keeps flow moving for testing
Utility.Wait(0.5)
SetStage(50)
EndFunction

Function HandleFragmentStage0050Item00()
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
EndFunction

Function HandleFragmentStage0055Item00()
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
EndFunction

Function HandleFragmentStage0060Item00()
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
EndFunction

Function HandleFragmentStage0065Item00()
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
EndFunction

Function HandleFragmentStage0070Item00()
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
EndFunction

Function HandleFragmentStage0075Item00()
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
EndFunction

Function HandleFragmentStage0080Item00()
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
EndFunction

Function HandleFragmentStage0085Item00()
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
EndFunction

Function HandleFragmentStage0095Item00()
RecoveryBranchActive = true
ApplyLateSaveCompanionMigration("Stage95")
EndFunction

Function HandleFragmentStage0096Item00()
RecoveryBranchActive = true
ApplyLateSaveCompanionMigration("Stage96")
EndFunction

Function HandleFragmentStage0100Item00()
CleanupMinRecruitQuickResolveWatcher()
MinRecruitQuickResolveArmed = false
DiamondCitySuppressionActive = false
ReleaseNuclearOptionSuppression("Stage100")
SetObjectiveDisplayed(100)
EndFunction

Function HandleFragmentStage0200Item00()
; Trade: open Astra's inventory for the player
Actor AstraActor = Alias_Astra.GetActorReference()
if AstraActor
  AstraActor.OpenInventory(true)
endif
EndFunction

Function TransitionPostMuseumEscortToCompanionAvailability(String sourceTag)
if CompanionAvailabilityUnlocked
  return
endif

Actor AstraActor = Alias_Astra.GetActorReference()
if AstraActor
  if AstraEscortScene && AstraEscortScene.IsPlaying()
    AstraEscortScene.Stop()
  endif
  if AstraTravelToMuseumScene && AstraTravelToMuseumScene.IsPlaying()
    AstraTravelToMuseumScene.Stop()
  endif
  if DogmeatEscortScene && DogmeatEscortScene.IsPlaying()
    DogmeatEscortScene.Stop()
  endif
  FollowersScript.GetScript().FollowerWait(AstraActor)
  AstraActor.SetPlayerTeammate(abTeammate = false)
  if DisallowedCompanionFaction
    AstraActor.RemoveFromFaction(DisallowedCompanionFaction)
  endif
  AstraActor.EvaluatePackage()
endif

if !COMAstra.IsRunning()
  COMAstra.Start()
endif
if AstraActor
  AstraActor.SetAvailableToBeCompanion()
endif
CompanionAvailabilityUnlocked = true

if DebugTrace
  Debug.Trace(self + " MQAstraALT post-Museum handoff via " + sourceTag + " removed pickup block and unlocked COMAstra availability")
endif
EndFunction

Function ApplyLateSaveCompanionMigration(String sourceTag)
TraceExistingSaveReadiness(sourceTag)

Actor AstraActor = Alias_Astra.GetActorReference()
if !AstraActor
  if DebugTrace
    Debug.Trace(self + " MQAstraALT late-save migration skipped (" + sourceTag + "): Astra alias is empty")
  endif
  return
endif

DisableStartupRouteForExistingSave(sourceTag)

if DisallowedCompanionFaction
  AstraActor.RemoveFromFaction(DisallowedCompanionFaction)
endif

if COMAstra && !COMAstra.IsRunning()
  COMAstra.Start()
endif

Actor PlayerRef = Game.GetPlayer()
if PlayerRef
  AstraActor.MoveTo(PlayerRef)
endif

AstraActor.SetPlayerTeammate(abTeammate = false)
AstraActor.SetAvailableToBeCompanion()
CompanionAvailabilityUnlocked = true
RepairMissedAffinityForcegreet(AstraActor, sourceTag)
AstraActor.EvaluatePackage()

if DebugTrace
  Debug.Trace(self + " MQAstraALT late-save companion migration applied via " + sourceTag + "; stopping startup route quest to suppress Red Rocket/Concord greetings")
endif
Stop()
EndFunction

Bool Function ShouldUseExistingSaveMigration(String sourceTag)
if MQ302 && MQ302.GetStage() >= 10
  return true
endif

if MQ207 && MQ207.GetStage() > 0
  return true
endif

if MQ206Min && MQ206Min.GetStage() > 0
  return true
endif

if MQ302Min && MQ302Min.GetStage() > 0
  return true
endif

if MQ103 && MQ103.GetStage() > 0
  return true
endif

if MQ102 && MQ102.GetStage() >= 50
  return true
endif

if MinRecruit00 && MinRecruit00.GetStage() > 0
  return true
endif

return false
EndFunction

Function DisableStartupRouteForExistingSave(String sourceTag)
CleanupMinRecruitQuickResolveWatcher()
MinRecruitQuickResolveArmed = false
MinRecruitQuickResolveApplied = false
MinRecruitTargetWorkshopSeen = false
DiamondCitySuppressionActive = false
NuclearOptionSuppressionActive = false
RedRocketArrivalWatchArmed = false
BootstrapComplete = true
EscortBranchSelected = true

Actor PlayerRef = Game.GetPlayer()
if PlayerRef
  UnregisterForRemoteEvent(PlayerRef, "OnLocationChange")
endif

StopStartupScene(BootstrapScene)
StopStartupScene(SanctuaryScene)
StopStartupScene(RedRocketScene)
StopStartupScene(AstraEscortScene)
StopStartupScene(AstraTravelToRedRocketScene)
StopStartupScene(AstraTravelToMuseumScene)
StopStartupScene(DogmeatEscortScene)
StopStartupScene(CoalitionPitchScene)
StopStartupScene(InfoFirstScene)
StopStartupScene(NotNowScene)
StopStartupScene(ConvergencePrepScene)
StopStartupScene(SanctuaryRegroupScene)
StopStartupScene(FirstStepTermsScene)
StopStartupScene(RouteDisciplineScene)
StopStartupScene(RailroadVectorScene)
StopStartupScene(RailroadContactScene)
StopStartupScene(TradecraftDebriefScene)
StopStartupScene(InstituteAccessScene)
StopStartupScene(BoSContactScene)
StopStartupScene(SturgesTunnelScene)
StopStartupScene(SturgesIntelScene)
StopStartupScene(CITIngressScene)

HideStartupObjectives()

if DebugTrace
  Debug.Trace(self + " MQAstraALT existing-save migration disabled startup route via " + sourceTag)
endif
EndFunction

Function StopStartupScene(Scene targetScene)
if targetScene && targetScene.IsPlaying()
  targetScene.Stop()
endif
EndFunction

Function HideStartupObjectives()
SetObjectiveDisplayed(5, abDisplayed = false)
SetObjectiveDisplayed(7, abDisplayed = false)
SetObjectiveDisplayed(8, abDisplayed = false)
SetObjectiveDisplayed(9, abDisplayed = false)
SetObjectiveDisplayed(10, abDisplayed = false)
SetObjectiveDisplayed(15, abDisplayed = false)
SetObjectiveDisplayed(20, abDisplayed = false)
SetObjectiveDisplayed(25, abDisplayed = false)
SetObjectiveDisplayed(30, abDisplayed = false)
SetObjectiveDisplayed(35, abDisplayed = false)
SetObjectiveDisplayed(40, abDisplayed = false)
SetObjectiveDisplayed(45, abDisplayed = false)
SetObjectiveDisplayed(50, abDisplayed = false)
SetObjectiveDisplayed(55, abDisplayed = false)
SetObjectiveDisplayed(60, abDisplayed = false)
SetObjectiveDisplayed(65, abDisplayed = false)
SetObjectiveDisplayed(70, abDisplayed = false)
SetObjectiveDisplayed(75, abDisplayed = false)
SetObjectiveDisplayed(80, abDisplayed = false)
SetObjectiveDisplayed(85, abDisplayed = false)
SetObjectiveDisplayed(95, abDisplayed = false)
SetObjectiveDisplayed(100, abDisplayed = false)
EndFunction

Function TraceExistingSaveReadiness(String sourceTag)
if DebugTrace
  Debug.Trace(self + " MQAstraALT readiness " + sourceTag + " MQ102=" + QuestStageText(MQ102) + " MQ103=" + QuestStageText(MQ103) + " MQ207=" + QuestStageText(MQ207) + " MQ206Min=" + QuestStageText(MQ206Min) + " MQ302Min=" + QuestStageText(MQ302Min) + " MQ302=" + QuestStageText(MQ302) + " MinRecruit00=" + QuestStageText(MinRecruit00))
endif
EndFunction

String Function QuestStageText(Quest targetQuest)
if targetQuest
  return "" + targetQuest.GetStage()
endif
return "None"
EndFunction

Function RepairMissedAffinityForcegreet(Actor AstraActor, String sourceTag)
ActorValue wantsToTalkAV = ResolveActorValueProperty(CA_WantsToTalk, 0x000FA86B, "CA_WantsToTalk")
ActorValue sceneToPlayAV = ResolveActorValueProperty(CA_AffinitySceneToPlay, 0x000FA875, "CA_AffinitySceneToPlay")
ActorValue currentThresholdAV = ResolveActorValueProperty(CA_CurrentThreshold, 0x000A1B81, "CA_CurrentThreshold")

if !AstraActor || !COMAstra || !wantsToTalkAV || !sceneToPlayAV || !currentThresholdAV
  if DebugTrace
    Debug.Trace(self + " MQAstraALT affinity repair skipped (" + sourceTag + "): missing actor, quest, or actor-value properties")
  endif
  return
endif

float wantsToTalk = AstraActor.GetValue(wantsToTalkAV)
float sceneToPlay = AstraActor.GetValue(sceneToPlayAV)
if wantsToTalk > 0.0 || sceneToPlay > 0.0
  if DebugTrace
    Debug.Trace(self + " MQAstraALT affinity repair skipped (" + sourceTag + "): talk already pending wants=" + wantsToTalk + " scene=" + sceneToPlay)
  endif
  return
endif

float currentThreshold = AstraActor.GetValue(currentThresholdAV)
GlobalVariable t1Infatuation = ResolveGlobalProperty(CA_T1_Infatuation, 0x0004B1C4, "CA_T1_Infatuation")
GlobalVariable t2Admiration = ResolveGlobalProperty(CA_T2_Admiration, 0x0004B1C5, "CA_T2_Admiration")
GlobalVariable tCustom1Confidant = ResolveGlobalProperty(CA_TCustom1_Confidant, 0x000F75E2, "CA_TCustom1_Confidant")
GlobalVariable tCustom2Friend = ResolveGlobalProperty(CA_TCustom2_Friend, 0x000F75E1, "CA_TCustom2_Friend")
GlobalVariable sceneInfatuation = ResolveGlobalProperty(CA_Scene_Infatuation, 0x000FA86D, "CA_Scene_Infatuation")
GlobalVariable sceneAdmiration = ResolveGlobalProperty(CA_Scene_Admiration, 0x000FA86C, "CA_Scene_Admiration")
GlobalVariable sceneConfidant = ResolveGlobalProperty(CA_Scene_Confidant, 0x00166701, "CA_Scene_Confidant")
GlobalVariable sceneFriendship = ResolveGlobalProperty(CA_Scene_Friendship, 0x00166700, "CA_Scene_Friendship")

if t1Infatuation && sceneInfatuation && currentThreshold >= t1Infatuation.GetValue()
  if COMAstra.GetStageDone(500) && !COMAstra.GetStageDone(510) && !COMAstra.GetStageDone(515) && !COMAstra.GetStageDone(520) && !COMAstra.GetStageDone(522) && !COMAstra.GetStageDone(525) && !COMAstra.GetStageDone(550)
    QueueAffinityForcegreet(AstraActor, sceneToPlayAV, wantsToTalkAV, sceneInfatuation, "infatuation", sourceTag)
    return
  endif
endif

if tCustom1Confidant && sceneConfidant && currentThreshold >= tCustom1Confidant.GetValue()
  if COMAstra.GetStageDone(495) && !COMAstra.GetStageDone(496) && !COMAstra.GetStageDone(497)
    QueueAffinityForcegreet(AstraActor, sceneToPlayAV, wantsToTalkAV, sceneConfidant, "confidant", sourceTag)
    return
  endif
endif

if t2Admiration && sceneAdmiration && currentThreshold >= t2Admiration.GetValue()
  if COMAstra.GetStageDone(400) && !COMAstra.GetStageDone(410) && !COMAstra.GetStageDone(420)
    QueueAffinityForcegreet(AstraActor, sceneToPlayAV, wantsToTalkAV, sceneAdmiration, "admiration", sourceTag)
    return
  endif
endif

if tCustom2Friend && sceneFriendship && currentThreshold >= tCustom2Friend.GetValue()
  if COMAstra.GetStageDone(405) && !COMAstra.GetStageDone(406) && !COMAstra.GetStageDone(407)
    QueueAffinityForcegreet(AstraActor, sceneToPlayAV, wantsToTalkAV, sceneFriendship, "friendship", sourceTag)
    return
  endif
endif

if DebugTrace
  Debug.Trace(self + " MQAstraALT affinity repair found no missed forcegreet via " + sourceTag + " threshold=" + currentThreshold)
endif
EndFunction

Function QueueAffinityForcegreet(Actor AstraActor, ActorValue sceneToPlayAV, ActorValue wantsToTalkAV, GlobalVariable SceneGlobal, String sceneName, String sourceTag)
float sceneValue = SceneGlobal.GetValue()
AstraActor.SetValue(sceneToPlayAV, sceneValue)
AstraActor.SetValue(wantsToTalkAV, 1.0)
AstraActor.EvaluatePackage()

if DebugTrace
  Debug.Trace(self + " MQAstraALT queued missed " + sceneName + " affinity forcegreet via " + sourceTag + " scene=" + sceneValue)
endif
EndFunction

ActorValue Function ResolveActorValueProperty(ActorValue configuredValue, int formId, String label)
if configuredValue
  return configuredValue
endif

ActorValue resolvedValue = Game.GetFormFromFile(formId, "Fallout4.esm") as ActorValue
if DebugTrace && !resolvedValue
  Debug.Trace(self + " MQAstraALT affinity repair could not resolve actor value " + label)
endif
return resolvedValue
EndFunction

GlobalVariable Function ResolveGlobalProperty(GlobalVariable configuredValue, int formId, String label)
if configuredValue
  return configuredValue
endif

GlobalVariable resolvedValue = Game.GetFormFromFile(formId, "Fallout4.esm") as GlobalVariable
if DebugTrace && !resolvedValue
  Debug.Trace(self + " MQAstraALT affinity repair could not resolve global " + label)
endif
return resolvedValue
EndFunction

; --- Shared quest events and helpers ---

























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

if RedRocketArrivalWatchArmed && GetStageDone(205) && !GetStageDone(9)
  bool arrivedAtRedRocket = false
  if RedRocketTruckStopLocation && akSender.IsInLocation(RedRocketTruckStopLocation)
    arrivedAtRedRocket = true
  elseif RedRocketCenterMarker && akSender.GetDistance(RedRocketCenterMarker) <= 300.0
    arrivedAtRedRocket = true
  endif

  if arrivedAtRedRocket
    Actor astraRef = Alias_Astra.GetActorReference()
    bool astraIsNear = false
    if astraRef && RedRocketCenterMarker
       if astraRef.GetDistance(RedRocketCenterMarker) <= 256.0
          astraIsNear = true
       endif
    endif

    if astraIsNear
        RedRocketArrivalWatchArmed = false
        UnregisterForRemoteEvent(akSender, "OnLocationChange")
        if DebugTrace
          Debug.Trace(self + " MQAstraALT Red Rocket arrival watcher advancing to stage 9 (Player+Astra arrived)")
          Debug.Notification("MQAstraALT: Red Rocket reached. Stage 9.")
        endif
        SetStage(9)
        return
    else
        ; Player is here, but Astra isn't close enough yet. Start polling.
        if !RedRocketArrivalPolling
           RedRocketArrivalPolling = true
           if DebugTrace
              Debug.Trace(self + " MQAstraALT Player at Red Rocket, waiting for Astra...")
           endif
           StartTimer(1.0, 900)
        endif
    endif
  endif
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

Event OnTimer(int aiTimerID)
  if aiTimerID == 900
    if !RedRocketArrivalPolling || GetStageDone(9)
       return
    endif

    Actor astraRef = Alias_Astra.GetActorReference()
    bool astraIsNear = false
    if astraRef && RedRocketCenterMarker
       if astraRef.GetDistance(RedRocketCenterMarker) <= 256.0
          astraIsNear = true
       endif
    endif

    if astraIsNear
       RedRocketArrivalPolling = false
       RedRocketArrivalWatchArmed = false
       Actor playerRef = Game.GetPlayer()
       if playerRef
          UnregisterForRemoteEvent(playerRef, "OnLocationChange")
       endif

       if DebugTrace
          Debug.Trace(self + " MQAstraALT Astra arrived at Red Rocket. Advancing to Stage 9.")
          Debug.Notification("MQAstraALT: Astra arrived.")
       endif
       SetStage(9)
    else
       StartTimer(1.0, 900)
    endif
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



; --- Quest properties ---

; --- Aliases ---
ReferenceAlias Property Alias_Astra Auto
ReferenceAlias Property Alias_Dogmeat Auto

; --- Scene Properties ---
Scene Property BootstrapScene Auto
Scene Property SanctuaryScene Auto
Scene Property RedRocketScene Auto
Scene Property AstraEscortScene Auto
Scene Property AstraTravelToRedRocketScene Auto
Scene Property DogmeatEscortScene Auto
Scene Property AstraTravelToMuseumScene Auto
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
Quest Property COMAstra Auto

; --- Vanilla Globals ---
GlobalVariable Property PlayerInstitute_Destroyed Auto

; --- Companion Affinity Repair ---
ActorValue Property CA_WantsToTalk Auto
ActorValue Property CA_AffinitySceneToPlay Auto
ActorValue Property CA_CurrentThreshold Auto
GlobalVariable Property CA_T1_Infatuation Auto
GlobalVariable Property CA_T2_Admiration Auto
GlobalVariable Property CA_TCustom1_Confidant Auto
GlobalVariable Property CA_TCustom2_Friend Auto
GlobalVariable Property CA_Scene_Infatuation Auto
GlobalVariable Property CA_Scene_Admiration Auto
GlobalVariable Property CA_Scene_Confidant Auto
GlobalVariable Property CA_Scene_Friendship Auto

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
Bool Property CompanionAvailabilityUnlocked Auto
Bool Property RedRocketArrivalWatchArmed Auto
Bool Property RedRocketArrivalPolling Auto

; --- Runtime References ---
ObjectReference Property SanctuaryWorkshopRef Auto
ObjectReference Property MinRecruitWorkshopRef Auto
ObjectReference Property RedRocketCenterMarker Auto
Location Property RedRocketTruckStopLocation Auto

