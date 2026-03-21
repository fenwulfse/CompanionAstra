# Package And Greeting Findings - 2026-03-14

Current user feedback to treat as truth:
- Astra still does not follow correctly
- MQ scenes still feel doubled because they use:
  - greeting
  - then extra dialog/camera action
  - then player dialogue

Practical rule from this point:
- if a greeting is already launching the scene, do not also spend an extra NPC line before the player wheel unless it adds real value
- for the opening MQ path, prefer:
  - greeting launcher
  - immediate player dialogue
  - or one very short scene line only if necessary

Vanilla/package findings:
- Nick-style escort is not just `SetPlayerTeammate + FollowerFollow`
- the actual pattern is:
  - quest stage starts a scene
  - scene contains `Package` action(s)
  - package action points to real package record(s)
  - package record holds the actual target/location data
- `MQ105_001_NickEscortToDiamondCity` is heavier than Astra's current travel scene:
  - 7 phases
  - 2 package actions for Nick
  - 4 interleaved travel-banter dialog actions
  - the first package action is `IgnoreForCompletion` at phase `0`
  - the long-running travel package action spans phases `1` through `6`
- Exact package links in `MQ105_001_NickEscortToDiamondCity`:
  - phase `0` package action:
    - `MQ105NickEscortPlayerWhenNearToDiamondCity`
    - `MQ105NickTraveltoDiamondCityPkg`
  - phase `1..6` package action:
    - `MQ105NickEscortPlayerWhenNearToDiamondCityAlways`
- Exact package record pattern:
  - `MQ105NickEscortPlayerWhenNearToDiamondCity`
    - template: `EscortPlayerWhenNear`
    - preferred speed: `Jog`
    - conditioned on `MQ104` stage `205`
  - `MQ105NickTraveltoDiamondCityPkg`
    - template: `Travel`
    - preferred speed: `FastWalk`
    - conditioned on `MQ104` stage `205`
  - `MQ105NickEscortPlayerWhenNearToDiamondCityAlways`
    - template: `EscortPlayerWhenNear`
    - preferred speed: `Jog`
    - no extra condition
- Astra's current travel scene is much thinner:
  - 1 phase
  - 1 package action
  - no interleaved travel phases yet
- Conclusion:
  - Astra now has a real package-scene shell
  - but it is still not Nick-parity escort behavior yet

Mutagen translation:
- `SceneAction.Type = Package`
- `SceneAction.Packages = [...]`
- `Package.Data` holds the travel target / location payload
- In `MQ105`, package actions are not empty shells; they link to named package records.
- The dump tool in this lane was updated to print `SceneAction.Packages` so future checks can see the exact package links.

External reference checked:
- CreationKit Wiki `ForceGreet (Package Template)`:
  - actor moves/waits, then walks up and initiates dialogue
  - confirms that package-driven movement before dialogue is normal
- GECK `Dialogue Package` reference:
  - actor can move to start/wait location, then move toward target and engage dialogue
  - confirms that location/trigger/target behavior belongs to the package, not just the scene shell

What was changed in `v27`:
- positive opener now uses hidden stage `205`
- stage `205` starts `MQAstraALT_AstraTravelToRedRocketScene`
- that scene points at `MQAstraALT_AstraTravelToRedRocketPkg`
- the package targets vanilla `RedRocketTruckStopMapMarker`

What is still not solved:
- whether this Red Rocket travel package is sufficient by itself
- whether Astra also needs a fuller Nick-style alias package stack for the opening route
- whether the opening scene should be simplified further to remove the extra in-scene NPC preamble entirely

Current `v28` scene cleanup:
- MQ player-dialogue scenes no longer create a separate in-scene monologue action before the wheel
- the staged Greeting is now the opening spoken line
- the scene itself starts directly on the player dialogue wheel

Saved research artifacts:
- `docs\\mq104_mq105_dialogue_dump.txt`
- `docs\\nick_package_dump.txt`
