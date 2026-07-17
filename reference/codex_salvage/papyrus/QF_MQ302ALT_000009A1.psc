;BEGIN FRAGMENT CODE - Do not edit anything between this and the end comment
Scriptname Fragments:Quests:QF_MQ302ALT_000009A1 Extends Quest Hidden Const

;BEGIN FRAGMENT Fragment_Stage_0005_Item_00
Function Fragment_Stage_0005_Item_00()
;BEGIN CODE
if DebugTraceMq302AltShellStages
  Debug.Trace(self + " MQ302ALT stage 5 start (Stage5ActionMode=" + Stage5ActionMode + ")")
endif

BootstrapStage5Initialized = true

if UseAstraAsQuestGiver
  QuestGiverMode = 1
elseif UseUnknownSurvivorQuestGiver
  QuestGiverMode = 2
else
  QuestGiverMode = 0
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

CoalitionPitchStage10Presented = true

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

;END FRAGMENT CODE - Do not edit anything between this and the begin comment

Scene Property BootstrapScene Auto
Scene Property BootstrapUnknownSurvivorScene Auto
Scene Property CoalitionPitchScene Auto
Scene Property CoalitionPitchUnknownSurvivorScene Auto
Scene Property InfoFirstScene Auto
Scene Property InfoFirstUnknownSurvivorScene Auto
Scene Property NotNowScene Auto
Scene Property NotNowUnknownSurvivorScene Auto

ReferenceAlias Property Alias_UnknownSurvivor Auto

Bool Property UseAstraAsQuestGiver Auto
Bool Property UseUnknownSurvivorQuestGiver Auto
Bool Property DebugTraceMq302AltShellStages Auto

Int Property Stage5ActionMode Auto
Int Property Stage10ActionMode Auto
Int Property Stage15ActionMode Auto
Int Property Stage20ActionMode Auto
Int Property PendingMq302AltBranchIntent Auto
Int Property QuestGiverMode Auto
Int Property UnknownSurvivorAliasFillMode Auto

Bool Property Stage5ShouldAutoAdvanceTo10 Auto
Bool Property Stage10ShouldWriteBranchIntent Auto
Bool Property BootstrapStage5Initialized Auto
Bool Property CoalitionPitchStage10Presented Auto
Bool Property CoalitionPitchAccepted Auto
Bool Property CoalitionPitchDeclined Auto
Bool Property InfoFirstStage15Presented Auto
Bool Property NotNowStage20Presented Auto
Bool Property UnknownSurvivorAliasFillAttempted Auto
Bool Property UnknownSurvivorAliasFillResolved Auto

; Placeholder properties exist on the VMAD shell but are intentionally unused here for now:
; COMAstra, Alias_Player, Alias_Astra, Alias_UnknownSurvivor, MQ302, MQ302Min, MQ302BoS,
; MQ302RR, MQ302Post, MQ302Faction, BranchIntentInfoFirstTopic, BranchIntentNotNowTopic