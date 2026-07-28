# CK Infatuation Branch Test Checklist

Use this to validate the four `COMAstra` infatuation end outcomes in one session.

## Preconditions
- Build and run the generator from `CompanionAstra_LockedIDs`.
- Confirm latest plugin is loaded: `CompanionAstra.esp`.
- Confirm you can trigger the infatuation scene greeting (`CA_WantsToTalk` path).

## Test Method
1. Get to the **final infatuation player-choice exchange** (last response wheel in `COMClaude_03_AdmirationToInfatuation`).
2. Make a **hard save** right before selecting the final option.
3. Pick one option, finish scene, then run `sqs COMAstra` in console.
4. Confirm expected stage is set.
5. Reload the hard save and repeat for the next option.

## Expected Stage Mapping (Final Choice)
- `Optimized` -> stage `525` (romance success)
- `Solid` -> stage `515` (declined temporary)
- `Stay Practical` -> stage `522` (declined permanent)
- `Eternal?` -> stage `520` (romance fail)

## Post-Outcome Checks
- After stage `525`, romance-complete idle greeting should become available.
- After stages `515`, `520`, `522`, romance-complete idle greeting should **not** be available.
- No invalid conditions should appear on greeting records in CK.

## Regression Spot-Checks
- Friendship scene completion still lands on stage `407`.
- Admiration scene completion still lands on stage `420`.
- Confidant scene completion still lands on stage `497`.
