;BEGIN FRAGMENT CODE - Do not edit anything between this and the end comment
Scriptname Fragments:Quests:QF_COMAstra_00000805 Extends Quest Hidden

;BEGIN FRAGMENT Fragment_Stage_0080_Item_00
Function Fragment_Stage_0080_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 80: recruited as companion (SetCompanion)")
FollowersScript.GetScript().SetCompanion(Alias_Astra.GetActorReference())
DogmeatDiag("recruit")
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0090_Item_00
Function Fragment_Stage_0090_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 90: dismissed (DismissCompanion)")
FollowersScript.GetScript().DismissCompanion(Alias_Astra.GetActorReference())
DogmeatDiag("dismiss")
;END CODE
EndFunction
;END FRAGMENT

; Dogmeat state snapshot for log-based debugging (2026-07-13).
; v2: checks the REAL Dogmeat ref (0001D162, plugin-name independent) since
; Followers' alias proved empty. If his teammate flag is set while the
; Followers system says no dog companion is active (proven inconsistent),
; auto-repairs by clearing the flag - that stuck flag blocks his recruit
; dialogue and gives command-mode-without-following.
Function DogmeatDiag(string when)
FollowersScript fs = FollowersScript.GetScript()
Actor dm = fs.DogmeatCompanion.GetActorReference()
float dmGlobal = fs.PlayerHasActiveDogmeatCompanion.GetValue()
Debug.Trace("[ASTRALOG] DogmeatDiag(" + when + "): aliasRef=" + dm + " activeGlobal=" + dmGlobal)
Actor realDM = Game.GetForm(0x0001D162) as Actor
if realDM == None
  Debug.Trace("[ASTRALOG] DogmeatDiag(" + when + "): real DogmeatRef 0001D162 not resolvable")
else
  bool tm = realDM.IsPlayerTeammate()
  Debug.Trace("[ASTRALOG] DogmeatDiag(" + when + "): realDM=" + realDM + " teammate=" + tm + " disabled=" + realDM.IsDisabled() + " ignoringHits=" + realDM.IsIgnoringFriendlyHits())
  if tm && dmGlobal == 0.0 && dm == None
    Debug.Trace("[ASTRALOG] DogmeatDiag: INCONSISTENT - teammate flag set but Followers has no dog companion. REPAIRING: SetPlayerTeammate(false)")
    realDM.SetPlayerTeammate(false)
    realDM.EvaluatePackage()
    Debug.Trace("[ASTRALOG] DogmeatDiag: repair done, teammate now=" + realDM.IsPlayerTeammate())
  endif
endif
EndFunction

;BEGIN FRAGMENT Fragment_Stage_0110_Item_00
Function Fragment_Stage_0110_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 110: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2) ;has forcegreeted
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0120_Item_00
Function Fragment_Stage_0120_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 120: HATRED scene end - left player permanently")
kmyquest.EndSceneHatred()
Alias_Astra.GetActorReference().DisallowCompanion(SuppressDismissMessage = true)
(Alias_Astra.GetActorReference() as CompanionActorScript).SetHasLeftPlayerPermanently()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0130_Item_00
Function Fragment_Stage_0130_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 130: HATRED scene end - affinity clamped Hatred..Disdain")
kmyquest.EndSceneHatred()
(Alias_Astra.GetActorReference() as CompanionActorScript).SetAffinityBetweenThresholds(CA_T5_Hatred, CA_T4_Disdain)
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0150_Item_00
Function Fragment_Stage_0150_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 150: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2) ;has forcegreeted
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0160_Item_00
Function Fragment_Stage_0160_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 160: HATRED scene end - disallowed companion, left permanently")
kmyquest.EndSceneHatred()
FollowersScript.GetScript().DisallowCompanion(Alias_Astra.GetActorReference(), SuppressDismissMessage = true)
(Alias_Astra.GetActorReference() as CompanionActorScript).SetHasLeftPlayerPermanently()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0210_Item_00
Function Fragment_Stage_0210_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 210: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2) ;has forcegreeted
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0220_Item_00
Function Fragment_Stage_0220_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 220: DISDAIN scene end")
kmyquest.EndSceneDisdain()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0240_Item_00
Function Fragment_Stage_0240_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 240: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2) ;has forcegreeted
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0250_Item_00
Function Fragment_Stage_0250_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 250: DISDAIN scene end")
kmyquest.EndSceneDisdain()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0320_Item_00
Function Fragment_Stage_0320_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 320: ADMIRATION scene end")
kmyquest.EndSceneAdmiration()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0330_Item_00
Function Fragment_Stage_0330_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 330: ADMIRATION scene end")
kmyquest.EndSceneAdmiration()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0350_Item_00
Function Fragment_Stage_0350_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 350: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2) ;has forcegreeted
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0360_Item_00
Function Fragment_Stage_0360_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 360: NEUTRAL scene end")
kmyquest.EndSceneNeutral()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0406_Item_00
Function Fragment_Stage_0406_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 406: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2)
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0407_Item_00
Function Fragment_Stage_0407_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 407: FRIEND scene end")
kmyquest.EndSceneFriend()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0410_Item_00
Function Fragment_Stage_0410_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 410: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2)
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0420_Item_00
Function Fragment_Stage_0420_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 420: ADMIRATION scene end")
kmyquest.EndSceneAdmiration()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0440_Item_00
Function Fragment_Stage_0440_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 440: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2)
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0450_Item_00
Function Fragment_Stage_0450_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 450: INFATUATION scene end")
kmyquest.EndSceneInfatuation()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0470_Item_00
Function Fragment_Stage_0470_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 470: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2)
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0480_Item_00
Function Fragment_Stage_0480_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 480: ADMIRATION scene end")
kmyquest.EndSceneAdmiration()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0496_Item_00
Function Fragment_Stage_0496_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 496: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2)
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0497_Item_00
Function Fragment_Stage_0497_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 497: CONFIDANT scene end")
kmyquest.EndSceneConfidant()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0510_Item_00
Function Fragment_Stage_0510_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 510: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2)
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0515_Item_00
Function Fragment_Stage_0515_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 515: ROMANCE declined (not permanent), infatuation unlocked")
(Alias_Astra.GetActorRef() as CompanionActorScript).RomanceDeclined(isPermanent = false)

kmyquest.UnlockedInfatuation()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0520_Item_00
Function Fragment_Stage_0520_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 520: ROMANCE fail, infatuation unlocked")
(Alias_Astra.GetActorRef() as CompanionActorScript).RomanceFail()

kmyquest.UnlockedInfatuation()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0522_Item_00
Function Fragment_Stage_0522_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 522: ROMANCE declined PERMANENTLY, infatuation scene end")
(Alias_Astra.GetActorRef() as CompanionActorScript).RomanceDeclined(isPermanent = true)

kmyquest.EndSceneInfatuation()

kmyquest.UnlockedInfatuation()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0525_Item_00
Function Fragment_Stage_0525_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 525: ROMANCE SUCCESS, infatuation scene end")
(Alias_Astra.GetActorRef() as CompanionActorScript).RomanceSuccess()

kmyquest.EndSceneInfatuation()

kmyquest.UnlockedInfatuation()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0550_Item_00
Function Fragment_Stage_0550_Item_00()
;BEGIN AUTOCAST TYPE affinityscenehandlerscript
Quest __temp = self as Quest
affinityscenehandlerscript kmyQuest = __temp as affinityscenehandlerscript
;END AUTOCAST
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 550: INFATUATION scene end")
kmyquest.EndSceneInfatuation()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0610_Item_00
Function Fragment_Stage_0610_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 610: wants to talk about MURDER")
Alias_Astra.TryToSetActorValue(CA_WantsToTalkMurder, 2)
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0620_Item_00
Function Fragment_Stage_0620_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 620: MURDER talk resolved")
Alias_Astra.TryToSetActorValue(CA_WantsToTalkMurder, 0) ;done wanting to talk - scene resolved
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_0630_Item_00
Function Fragment_Stage_0630_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 630: MURDER talk cleared")
Alias_Astra.TryToSetActorValue(CA_WantsToTalkMurder, 0)
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN FRAGMENT Fragment_Stage_1010_Item_00
Function Fragment_Stage_1010_Item_00()
;BEGIN CODE
Debug.Trace("[ASTRALOG] Stage 1010: wants to talk (forcegreet armed)")
Alias_Astra.TryToSetActorValue(CA_WantsToTalk, 2)
Alias_Astra.GetActorReference().EvaluatePackage()
;END CODE
EndFunction
;END FRAGMENT

;BEGIN PROPERTIES
ReferenceAlias Property Alias_Astra Auto
ActorValue Property CA_WantsToTalk Auto
ActorValue Property CA_WantsToTalkMurder Auto
GlobalVariable Property CA_T5_Hatred Auto
GlobalVariable Property CA_T4_Disdain Auto
;END PROPERTIES

;END FRAGMENT CODE - Do not edit anything between this and the begin comment
