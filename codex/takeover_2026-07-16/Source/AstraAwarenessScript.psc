Scriptname AstraAwarenessScript extends Quest

; Persistent, vanilla-Papyrus awareness layer for Astra. It records shared
; travel and combat, then uses custom dialogue keywords for selective comments.

ReferenceAlias Property AstraAlias Auto Const Mandatory
FormList Property VisitedLocations Auto Const Mandatory

Keyword Property InteriorEntryKeyword Auto Const Mandatory
Keyword Property ReturnLocationKeyword Auto Const Mandatory
Keyword Property LongInteriorKeyword Auto Const Mandatory
Keyword Property CombatResolvedKeyword Auto Const Mandatory
Keyword Property CloseCallKeyword Auto Const Mandatory
Keyword Property MilestoneOneKeyword Auto Const Mandatory
Keyword Property MilestoneFiveKeyword Auto Const Mandatory
Keyword Property MilestoneTenKeyword Auto Const Mandatory
Keyword Property MilestoneTwentyFiveKeyword Auto Const Mandatory
Keyword Property MilestoneFiftyKeyword Auto Const Mandatory

Bool Property DebugTrace = True Auto Const
Float Property CommentCooldownDays = 0.003 Auto Const
Float Property MaximumSpeakingDistance = 3000.0 Auto Const

Int Property LocationsEntered Auto
Int Property InteriorEntries Auto
Int Property ReturnVisits Auto
Int Property CombatsStarted Auto
Int Property CombatsCompleted Auto
Int Property CloseCalls Auto
Int Property LongInteriorComments Auto

Actor PlayerRef
Actor AstraRef
Location PendingLocation
Bool PendingLocationWasReturn
Bool CombatActive
Float CombatStartedAt
Float CombatStartHealth
Float LastSpokenGameTime

Int EntryTimer = 7101
Int LongInteriorTimer = 7102
Int CombatResolveTimer = 7103

Event OnInit()
    InitializeAwareness("init")
EndEvent

Event OnQuestInit()
    InitializeAwareness("quest-init")
EndEvent

Event Actor.OnPlayerLoadGame(Actor akSender)
    InitializeAwareness("load")
EndEvent

Function InitializeAwareness(String reason)
    PlayerRef = Game.GetPlayer()
    AstraRef = AstraAlias.GetActorRef()

    if PlayerRef
        RegisterForRemoteEvent(PlayerRef, "OnLocationChange")
        RegisterForRemoteEvent(PlayerRef, "OnCombatStateChanged")
        RegisterForRemoteEvent(PlayerRef, "OnPlayerLoadGame")

        Location currentLocation = PlayerRef.GetCurrentLocation()
        if currentLocation && !VisitedLocations.HasForm(currentLocation)
            VisitedLocations.AddForm(currentLocation)
        endif
    endif

    if DebugTrace
        Debug.Trace("[ASTRA_BRAIN] READY reason=" + reason + " astra=" + AstraRef + " locations=" + LocationsEntered + " combats=" + CombatsCompleted + " returns=" + ReturnVisits)
    endif
EndFunction

Bool Function IsActiveCompanion()
    if !PlayerRef
        PlayerRef = Game.GetPlayer()
    endif
    if !AstraRef
        AstraRef = AstraAlias.GetActorRef()
    endif
    return PlayerRef && AstraRef && AstraRef.IsPlayerTeammate()
EndFunction

String Function GetSpeakBlockReason()
    if !IsActiveCompanion()
        return "not-active-companion"
    endif
    if !AstraRef.Is3DLoaded()
        return "astra-3d-not-loaded"
    endif
    if AstraRef.IsInCombat()
        return "astra-in-combat"
    endif
    if AstraRef.IsTalking()
        return "astra-already-talking"
    endif
    if AstraRef.IsInScene()
        return "astra-in-scene"
    endif
    if AstraRef.IsInDialogueWithPlayer()
        return "player-dialogue-active"
    endif
    if PlayerRef.GetDistance(AstraRef) > MaximumSpeakingDistance
        return "astra-too-far"
    endif
    return ""
EndFunction

Bool Function CanSpeak()
    return GetSpeakBlockReason() == ""
EndFunction

Bool Function TrySpeak(Keyword akKeyword, String reason, Bool ignoreCooldown = false)
    if !akKeyword
        if DebugTrace
            Debug.Trace("[ASTRA_BRAIN] SPEAK-SKIP reason=" + reason + " blocked=missing-keyword")
        endif
        return false
    endif

    String blocked = GetSpeakBlockReason()
    if blocked != ""
        if DebugTrace
            Debug.Trace("[ASTRA_BRAIN] SPEAK-SKIP reason=" + reason + " blocked=" + blocked)
        endif
        return false
    endif

    Float now = Utility.GetCurrentGameTime()
    if LastSpokenGameTime > now
        LastSpokenGameTime = 0.0
    endif
    if !ignoreCooldown && LastSpokenGameTime > 0.0 && (now - LastSpokenGameTime) < CommentCooldownDays
        if DebugTrace
            Debug.Trace("[ASTRA_BRAIN] SPEAK-COOLDOWN reason=" + reason + " elapsedDays=" + (now - LastSpokenGameTime))
        endif
        return false
    endif

    AstraRef.SayCustom(akKeyword)
    LastSpokenGameTime = now
    if DebugTrace
        Debug.Trace("[ASTRA_BRAIN] SPEAK reason=" + reason + " keyword=" + akKeyword)
    endif
    return true
EndFunction

Event Actor.OnLocationChange(Actor akSender, Location akOldLoc, Location akNewLoc)
    if akSender != PlayerRef || !akNewLoc
        return
    endif

    LocationsEntered += 1
    PendingLocation = akNewLoc
    PendingLocationWasReturn = VisitedLocations.HasForm(akNewLoc)
    if PendingLocationWasReturn
        ReturnVisits += 1
    else
        VisitedLocations.AddForm(akNewLoc)
    endif

    Bool isInterior = PlayerRef.IsInInterior()
    if isInterior
        InteriorEntries += 1
        CancelTimer(EntryTimer)
        CancelTimer(LongInteriorTimer)
        StartTimer(12.0, EntryTimer)
        StartTimer(300.0, LongInteriorTimer)
    else
        CancelTimer(EntryTimer)
        CancelTimer(LongInteriorTimer)
    endif

    if DebugTrace
        Debug.Trace("[ASTRA_BRAIN] LOCATION count=" + LocationsEntered + " interior=" + isInterior + " return=" + PendingLocationWasReturn + " old=" + akOldLoc + " new=" + akNewLoc)
    endif
EndEvent

Event Actor.OnCombatStateChanged(Actor akSender, Actor akTarget, Int aeCombatState)
    if akSender != PlayerRef || !IsActiveCompanion()
        return
    endif

    if aeCombatState == 1 && !CombatActive
        CombatActive = true
        CombatsStarted += 1
        CombatStartedAt = Utility.GetCurrentRealTime()
        CombatStartHealth = PlayerRef.GetValuePercentage(Game.GetHealthAV())
        CancelTimer(CombatResolveTimer)
        if DebugTrace
            Debug.Trace("[ASTRA_BRAIN] COMBAT-START count=" + CombatsStarted + " target=" + akTarget + " health=" + CombatStartHealth)
        endif
    elseif aeCombatState == 0 && CombatActive
        CancelTimer(CombatResolveTimer)
        StartTimer(8.0, CombatResolveTimer)
        if DebugTrace
            Debug.Trace("[ASTRA_BRAIN] COMBAT-CLEAR-PENDING target=" + akTarget)
        endif
    endif
EndEvent

Event OnTimer(Int aiTimerID)
    if aiTimerID == EntryTimer
        HandleInteriorEntry()
    elseif aiTimerID == LongInteriorTimer
        HandleLongInterior()
    elseif aiTimerID == CombatResolveTimer
        ResolveCombat()
    endif
EndEvent

Function HandleInteriorEntry()
    if !PendingLocation || PlayerRef.GetCurrentLocation() != PendingLocation || !PlayerRef.IsInInterior()
        return
    endif
    if PendingLocationWasReturn
        TrySpeak(ReturnLocationKeyword, "return-location")
    else
        TrySpeak(InteriorEntryKeyword, "new-interior")
    endif
EndFunction

Function HandleLongInterior()
    if !PendingLocation || PlayerRef.GetCurrentLocation() != PendingLocation || !PlayerRef.IsInInterior() || !IsActiveCompanion()
        return
    endif
    if PlayerRef.GetCombatState() != 0 || AstraRef.GetCombatState() != 0
        StartTimer(45.0, LongInteriorTimer)
        if DebugTrace
            Debug.Trace("[ASTRA_BRAIN] LONG-INTERIOR delayed-for-combat location=" + PendingLocation)
        endif
        return
    endif

    if TrySpeak(LongInteriorKeyword, "long-interior")
        LongInteriorComments += 1
    endif
    StartTimer(420.0, LongInteriorTimer)
EndFunction

Function ResolveCombat()
    if !CombatActive
        return
    endif
    if PlayerRef.GetCombatState() != 0 || (AstraRef && AstraRef.GetCombatState() != 0)
        StartTimer(8.0, CombatResolveTimer)
        return
    endif

    CombatActive = false
    CombatsCompleted += 1
    Float duration = Utility.GetCurrentRealTime() - CombatStartedAt
    Float endHealth = PlayerRef.GetValuePercentage(Game.GetHealthAV())
    Keyword milestoneKeyword = None

    if CombatsCompleted == 1
        milestoneKeyword = MilestoneOneKeyword
    elseif CombatsCompleted == 5
        milestoneKeyword = MilestoneFiveKeyword
    elseif CombatsCompleted == 10
        milestoneKeyword = MilestoneTenKeyword
    elseif CombatsCompleted == 25
        milestoneKeyword = MilestoneTwentyFiveKeyword
    elseif CombatsCompleted == 50
        milestoneKeyword = MilestoneFiftyKeyword
    endif

    if endHealth < 0.40
        CloseCalls += 1
    endif

    if DebugTrace
        Debug.Trace("[ASTRA_BRAIN] COMBAT-END completed=" + CombatsCompleted + " duration=" + duration + " startHealth=" + CombatStartHealth + " endHealth=" + endHealth + " closeCalls=" + CloseCalls)
    endif

    if milestoneKeyword
        TrySpeak(milestoneKeyword, "combat-milestone-" + CombatsCompleted, true)
    elseif endHealth < 0.40
        TrySpeak(CloseCallKeyword, "combat-close-call", true)
    elseif (CombatsCompleted % 3) == 0
        TrySpeak(CombatResolvedKeyword, "combat-pattern")
    endif
EndFunction

Function PrintAwarenessStatus()
    String status = "Astra memory: " + LocationsEntered + " locations, " + ReturnVisits + " returns, " + CombatsCompleted + " fights, " + CloseCalls + " close calls."
    Debug.Notification(status)
    Debug.Trace("[ASTRA_BRAIN] STATUS " + status + " interiorEntries=" + InteriorEntries + " longInteriorComments=" + LongInteriorComments)
EndFunction

Function TestAwarenessVoice()
    TrySpeak(InteriorEntryKeyword, "manual-test", true)
EndFunction
