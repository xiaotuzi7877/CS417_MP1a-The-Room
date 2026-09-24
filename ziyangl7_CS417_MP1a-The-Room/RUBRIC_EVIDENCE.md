# Manor Hall Rubric Evidence

Use `Assets/Scenes/MichaelManorHall.unity`. It is build index 0.

## Suggested Recording Order

1. Begin inside the manor and show the room boundaries, medieval furniture, gothic lanterns, raised ceiling, and moving celestial orrery.
2. Look at `ControlsCanvas_WorldSpace` on the left wall to show the in-world controls.
3. Show the Silver Fang outline and the lit central orrery.
4. Press the left primary controller button (keyboard: `L`) to change the room light color and trigger the distributed feedback.
5. Press the right trigger (keyboard: `P`) to spawn an object. Show its projectile trail, particle/audio feedback, gravitational motion, and stable orbit.
6. Press the right secondary controller button (keyboard: `B`) to break out of the room. Show the skybox and outside platform, then press it again to return.
7. Press the right primary controller button (keyboard: `Q`) last to demonstrate quit.

## MP1b Five-Chamber Video Shot List

Perform every step manually in VR. `K` (solve all), `W` (replay win), and `R` (reset) are presentation fallbacks only; keep each Key Prop visibly inside its Lock collider while the Lock completes.

1. Start facing `RitualProgressBoard`: `RITUAL PROGRESS 0 / 3`, `CHAMBERS EXPLORED 0 / 5`, objective "PLACE THE SILVER FANG IN THE WATCHER LOCK". Turn to show the five dark Gate runes.
2. Inspect clue I, then press the BAT, WOLF, and MOON portrait runes. The display-case shutter rises; grab the released `SilverFang` (show both hands).
3. Insert it into `SilverFangWatcherSocket`; the Lock light turns green.
4. The chest lid eases open, revealing the `MidnightCodex` and `FiveEntranceMap`; all five Gate runes light (four purple, the Moon Gate blue-white). Board: `1 / 3`, "READ THE CODEX ... TOUCH A GLOWING GATE RUNE".
5. Touch `Gate_01_Cellar` (floorboard rune); pull `CellarLever`; the cache opens and `RustyKey_RedHerring` appears. Use the Return Rune.
6. `Gate_02_BoneCloset`: press `BoneRevealButton`; `WoodenFang_RedHerring` appears. Return.
7. `Gate_03_CoffinVault`: press `OpenCoffinButton`; the lid eases open on `BlackRose_RedHerring`. Return.
8. `Gate_04_Portrait`: press `GlowingEyeButton`; the portrait reveals the Moon Crypt clue. Return. Show the four green runes with `EXPLORED` labels and the board at `4 / 5`.
9. Touch `Gate_05_MoonCrypt` (blue-white rune).
10. Read "PRESS IN ORDER: 1. WOLF 2. MOON 3. BLOOD"; press MOON first so all three buttons flash red and the sequence resets.
11. Press WOLF, MOON, BLOOD; each accepted button turns blue.
12. `MoonstoneSlab_Moving` slides aside with easing; grab the revealed `Moonstone` (`CHAMBERS EXPLORED 5 / 5`).
13. Insert it into `MoonstoneOrrerySocket` on the labelled Celestial Lock.
14. `BloodSlab_Moving` rises, but the final seal remains. Press LEFT then RIGHT; the seal rises and releases `BloodSigil`. Grab it. The gold Return Rune appears.
15. Use the gold Return Rune to return to the hall; the board reads `2 / 3`, "TAKE THE BLOOD SIGIL TO THE EXIT LOCK".
16. Insert `BloodSigil` into `BloodSigilDoorSocket`.
17. The seal retracts, the exit door opens, lights and particles play, and `CONGRATULATIONS!` fades after five seconds; the board reads `3 / 3`.

## MP1b Evidence Map

| Requirement | Scene or script evidence |
|---|---|
| Three Key Props (20:1 mass) | `Puzzle/SilverFangQuest/SilverFang` (0.8 kg), `GatedLocations/Chamber_05_MoonCrypt/Content_Section06/MoonstoneVault/Moonstone` (0.1 kg), `.../BloodSigilVault/BloodSigil` (2.0 kg); each has Rigidbody, Collider, `XRGrabInteractable`, `ManorKeyArtifact` |
| Three Locks | `Puzzle/RitualSequence/Locks/Lock_01_WatcherPortrait/SilverFangWatcherSocket`, `GatedLocations/Chamber_05_MoonCrypt/Content_Section06/Lock_02_CelestialConsole/MoonstoneOrrerySocket`, `Puzzle/SilverFangQuest/SilverFang_Pedestal/BloodSigilDoorSocket`; trigger Collider + `XRSocketInteractor` |
| Three Locks required before win | `Puzzle/RitualSequence` (`ManorThreeStagePuzzle`) accepts only the matching artifact ID for the current stage and opens the exit only at 3/3; wrong/early items are rejected with a red light and popped out of the Lock |
| Reveals | Watcher Lock opens `Puzzle/FiveChamberQuest/WatcherChestReveal` (Codex + map); Moon Crypt sequence moves `MoonstoneSlab_Moving`; Celestial Lock raises `BloodSlab_Moving`; four chamber Reveals via `ManorChamberReveal` |
| Gate affordance and travel | `Puzzle/FiveChamberQuest/Gates/Gate_01..05` (`ManorGatePortal` + explicit select), `Puzzle/FiveChamberQuest/TravelSystem` (`ManorGateTravelSystem`), `GatedLocations/Chamber_0N_*/TravelShell/ReturnRune` |
| Five gated locations | `GatedLocations/Chamber_01_Cellar`, `Chamber_02_BoneCloset`, `Chamber_03_CoffinVault`, `Chamber_04_Portrait`, `Chamber_05_MoonCrypt`; each counts once, only after its internal interaction |
| Puzzle system | Three genuine Key-release puzzles: `Decor/SilverFangReleasePuzzle` BAT-WOLF-MOON; `GatedLocations/Chamber_05_MoonCrypt/Content_Section06` WOLF-MOON-BLOOD; `BloodSigilVault/BloodSigilLeverPuzzle` LEFT-RIGHT after ritual stage 2. Wrong order resets; each correct sequence moves a physical seal and releases one Key |
| Puzzle discoverability | Codex ("FOLLOW THE MOON"), five-passage map, Portrait Chamber moon clue, blue-white Moon Gate rune, `SequenceInstruction` wall text, and the board's current objective |
| Three Red Herrings | `RustyKey_RedHerring` (Cellar), `WoodenFang_RedHerring` (Bone Closet), `BlackRose_RedHerring` (Coffin Vault); grabbable, rejected by every ritual Lock |
| Progress Scoreboard | `Puzzle/RitualSequence/CluesAndProgress/RitualProgressBoard` driven by `ManorQuestScoreboard`: ritual progress, keys remaining, locks remaining, chambers explored, current objective |
| Puzzle and clue scoreboard | `Systems/PuzzleProgressTracker` plus `PuzzleAndClueCounter`: `PUZZLES SOLVED N / 3` and `CLUES FOUND N / 3`; all three clue plaques are explicit XR interactions |
| Optional collectibles | Eight one-shot XR Moon Shards across the hall and five chambers, tracked as `MOON SHARDS N / 8`; resettable and independent of escape |
| Loss condition | Moon Crypt Gate starts a visible 02:30 timer; expiry shows `THE MOON HAS SET`, resets only that puzzle, and returns the player to the hall |
| Restart | `Systems/VR_Restart_Control`: physical world-space button requires a two-second hold and resets the complete experience |
| Signifiers | Gate runes: dark = locked, purple = false passage, blue-white = Moon passage, green + `EXPLORED` = done; Lock lights: blue = current, green/gold = complete, red flash = wrong item or order |
| Win Celebration | `Systems/WinFlow` (`WinCelebrationController`): seal and door state change, lights, particles, five-second `CONGRATULATIONS!` |

### Individual Contributions Draft

I built the Michael Manor five-chamber ritual. Three Key Props with a 20:1 mass range (Silver Fang 0.8 kg, Moonstone 0.1 kg, Blood Sigil 2.0 kg) must be placed into three XR socket Locks in order; wrong or early items are rejected with a red light and pushed back out of the Lock. The first Lock opens a chest that reveals a Midnight Codex and a five-passage map and activates five hall Gates. Each Gate is an explicit rune interaction that teleports the XR rig to its own chamber and back through a Return Rune. Four chambers are false leads with their own lever or button Reveal (cellar cache, bone closet, coffin, portrait); three of them release grabbable red herrings (Rusty Key, Wooden Fang, Black Rose) that no Lock accepts. The Moon Crypt holds the puzzle: WOLF, MOON, BLOOD buttons that turn blue when correct and flash red and reset when wrong. The correct order slides a stone slab aside to reveal the Moonstone, and the Moonstone in the Celestial Lock raises a second slab to reveal the Blood Sigil. The in-world board tracks ritual progress, keys and locks remaining, chambers explored (counted only after each chamber's interaction), and the current objective; Gate runes change color and show EXPLORED. Inserting the Blood Sigil opens the exit and plays the Win Celebration.

Each true Key is now physically sealed until its own puzzle is solved: BAT-WOLF-MOON raises the Silver Fang display shutter, WOLF-MOON-BLOOD reveals the Moonstone, and LEFT-RIGHT raises the Blood Sigil's final seal only after the Celestial Lock. Three inspectable clue plaques and the world board track puzzles and clues at 3/3. The optional layer adds eight resettable Moon Shards, a scoped 02:30 Moon Crypt loss condition, explicit Direct Grab on both hands, and a deliberate two-second world-space Restart control without changing the required escape route.

### Signifier Statement

- **Grab signifier:** both hands have explicit Direct Interactors. The instruction
  wall teaches `Grip = pick up and hold`, while the three true artifacts and three
  false relics are small hand-sized objects with distinct handles, bright metal or
  magical highlights, and physical fall/tumble behavior.
- **Escape signifier:** the sealed exit door, the `RITUAL PROGRESS` board, and the
  blue current-Lock light establish that the three ritual Locks are the route out.
  Completed Locks turn green/gold and the final Lock physically opens the exit.
- **Lock signifier:** the board names the current Key and destination; plaques read
  `FANG -> WATCHER`, `MOON -> HEAVENS`, and `BLOOD -> EXIT`; each Lock repeats its
  Key's name, color, and celestial/blood shape. A wrong or early item flashes red
  and is pushed out.
- **Repetition and variety:** all three Locks teach the same physical rule—carry a
  hand-sized artifact to a glowing socket and release it inside—but their outcomes
  increase in scope: the Fang reveals a Codex and five passages, the Moonstone
  opens a hidden Blood Sigil vault, and the Blood Sigil opens the exit and starts
  the full Win Celebration.

## Evidence Map

| Requirement | Scene or script evidence |
|---|---|
| Enclosed themed 3D room | `Architecture`, `Furniture`, `Decor`, `Lighting`, `ExitDoor` |
| World Space Canvas | `Systems/Rubric_Presentation/ControlsCanvas_WorldSpace` |
| Point light | `Systems/Rubric_Presentation/CeilingPointLight_Rubric` |
| Custom outline shader | Silver Fang renderer plus `Assets/Shaders/OutlineShader.shader` |
| Rainbow/color-changing light | `LightSwitch`; left primary controller button or `L` |
| Quit input | `QuitGame`; right primary controller button or `Q` |
| Camera breakout and return | `BreakOut`; right secondary controller button or `B` |
| Object shooter/spawner | `ObjectSpawner`; right trigger or `P` |
| Projectile trail | Spawned `SpawnBall` and its `TrailRenderer` |
| Kinematic double integration | `ProjectileMotion` integrates acceleration to velocity, then velocity to position |
| Arbitrary attractor | `ObjectSpawner` passes the selected orrery Sun transform into `ProjectileMotion.InitializeOrbit` |
| Perfect orbital velocity | `ObjectSpawner` calculates `sqrt(gravity / distance)` tangential speed |
| Particle feedback | 18 scene particle systems; four four-emitter feedback banks plus local effects |
| Spatial sound | 16 distributed 3D `AudioSource` components plus local spawn audio |
| Multiple action-driven effects | Spawn, light, breakout, and return each trigger a distinct feedback bank |

## Hierarchy Landmarks

```text
MichaelManorHall
├── XR Origin (XR Rig)
├── Architecture
├── Furniture
├── Decor
├── Lighting
├── Puzzle
├── Systems
│   ├── Rubric_Presentation
│   │   ├── ControlsCanvas_WorldSpace
│   │   └── CeilingPointLight_Rubric
│   └── Rubric_Interactions
│       └── Rubric_FeedbackNetwork
├── ExitDoor
└── Celestial_Orrery
```

The strict legacy wall-material checklist is intentionally not claimed here; the manor uses the current URP presentation instead.
