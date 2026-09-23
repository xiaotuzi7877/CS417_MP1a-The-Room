# MP1b Full-Credit Audit — Michael Manor Hall

This audit is intentionally conservative. Final subjective points still depend on the walkthrough video and Individual Contributions statement.

## Implemented Critical Path

| Rubric item | Evidence | Status |
|---|---|---|
| 3 Key Props | Silver Fang, Moonstone, Blood Sigil each have Collider, Rigidbody, XR Grab Interactable, and an artifact ID | Implemented |
| Solid Objects | Keys use physics; the manor floor has colliders | Implemented |
| 3 Locks | Watcher, orrery, and exit sockets each use a trigger Collider plus XR Socket Interactor | Implemented |
| 3 locks required before win | The controller rejects wrong/out-of-order artifacts and does not call the exit sequence until stage 3 | Implemented and Play-Mode tested |
| Grab Signifiers | Distinct key silhouettes, glowing materials, and staged placement/reveals | Implemented; quality is subjective |
| Escape Signifiers | Sealed exit, numbered clues, active-lock glow, and progress board | Implemented; quality is subjective |
| Lock Signifiers | Explicit Fang/Watcher, Moon/Heavens, Blood/Exit clues plus matching visual metaphors | Implemented; quality is subjective |
| Repetition and Variety | Repeated ritual insertion with three different consequences | Implemented |
| Eased State Changes | Portrait, reliquary, orrery, seal, and door use eased transitions | Implemented |

Critical-path estimate: **19–20 / 20**. The only technical grading risk is that the current Unity Starter Assets use active `Near-Far Interactor` components on both hands rather than components literally named `XRDirectInteractor`. Near grabbing works, but confirm that the instructor accepts XRI 3.5's combined Near-Far interactor as the rubric's “Direct Interactor,” or add explicit direct interactors before submission.

## Implemented Side Quests

| Side quest | Conservative points | Notes |
|---|---:|---|
| Reveals | 2 | Portrait exposes Moonstone; reliquary exposes Blood Sigil |
| Win Celebration | 2 | Door/seal state change, particles/lights, and timed message |
| Puzzle System | 2 | Recognizes the required ordered interactions and releases keys |
| Puzzle Content | 1 | Strict reading: two interaction sequences release new keys; the third opens the exit instead of releasing another key |
| Puzzle Discoverability | up to 3 | Numbered local plaques plus a current-clue board |
| Progress Scoreboard | 1 | Shows hidden keys and remaining locks |
| Puzzle Scoreboard | 1 | Shows the total number of ritual clues |

Conservative total: **32 points if Near-Far is accepted, 31 if it is not**. A more generous reading that counts all three lock sequences as three puzzles gives **33 or 32**, respectively. The 3-credit individual cap is 33; the 4-credit cap is 42.

## Remaining Full-Credit Risks and Best Fixes

1. **Add an in-world Restart UI button (1 point).** The `R` keyboard shortcut is useful for presentation but does not satisfy the rubric's UI-button wording. This is the fastest score-insurance item and would reset this room's three artifacts, locks, moving props, door, and celebration.
2. **Make the Silver Fang itself puzzle-released (Puzzle Content insurance).** Put the table Fang under a short glass case or tabletop mechanism with a discoverable interaction sequence. That creates three unambiguous key-release puzzles: reveal Fang, reveal Moonstone, reveal Blood Sigil.
3. **Verify or add explicit Direct Interactors.** Do not risk the Key Props point on terminology if the grader expects `XRDirectInteractor` specifically.
4. **Record the required collider evidence.** The walkthrough must visibly show all three keys inside their matching lock colliders before the win condition. Do not use `K` as the only video evidence.
5. **Explain subjective signifiers in the contribution statement.** Name every key/lock pair, the intended metaphor, the common ritual pattern, and how each consequence differs.

## If Enrolled for 4 Credits

The current room is not yet a safe 42/42 submission. After the fixes above, add a deliberate mix of separately demonstrable side quests—for example a scoped loss timer, collectibles with an in-world counter, several clearly non-key grabbable red herrings, or an inspection mechanic. Team-wide Start Screen and Connected Scenes can only be claimed once for the integrated game and should remain assigned to the teammates responsible for them.
