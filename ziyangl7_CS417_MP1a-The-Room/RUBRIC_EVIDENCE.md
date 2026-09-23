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

## MP1b Three-Lock Recording Order

The submitted walkthrough should perform the locks manually in VR. The `K`, `W`, and `R` keys are presentation fallbacks, not substitutes for showing the required XR interactions.

1. Show both controller hands and grab the `SilverFang` from its table.
2. Carry it to `SilverFangWatcherSocket`; keep the Fang visible inside the collider while the first lock completes.
3. Show `Portrait_Left_0` easing upward and the `Moonstone` becoming reachable.
4. Grab the Moonstone and place it into `MoonstoneOrrerySocket`; show the orrery response and the reliquary opening.
5. Grab the revealed `BloodSigil` and place it into `BloodSigilDoorSocket`.
6. Keep the Blood Sigil visible in the final socket while the seal retracts, the door opens, particles/lights play, and `CONGRATULATIONS!` appears.
7. Look back at `RitualProgressBoard` so the final 3/3 state is readable.

### MP1b Evidence Map

| Requirement | Scene or script evidence |
|---|---|
| Three Key Props | `SilverFang`, `Moonstone`, `BloodSigil`; each has a Rigidbody, Collider, `XRGrabInteractable`, and `ManorKeyArtifact` |
| Three Locks | `SilverFangWatcherSocket`, `MoonstoneOrrerySocket`, `BloodSigilDoorSocket`; each uses a trigger Collider and `XRSocketInteractor` |
| Three required scripts before escape | `ManorThreeStagePuzzle` enforces the ordered artifact IDs and calls the exit sequence only at 3/3 |
| Grab signifiers | Distinct hand-sized silhouettes, glowing materials, and display/reveal staging |
| Escape signifiers | Blue active-lock light, numbered plaques, blocked sealed exit, and `RitualProgressBoard` |
| Lock signifiers | `FANG -> WATCHER`, `MOON -> HEAVENS`, `BLOOD -> EXIT`, reinforced by matching imagery and colors |
| Repetition and variety | Every stage repeats the ritual insertion language; outcomes vary across moving portrait, celestial response/reliquary, and final seal/door |
| Eased state changes | Smooth portrait lift, reliquary lid rotation, orrery response, seal shrink/rotation, and door lift |
| Reveals | First lock uncovers Moonstone; second lock opens the reliquary containing Blood Sigil |
| Puzzle system | Wrong/out-of-order artifacts are rejected; each valid stage reveals the next required artifact |
| Progress/Puzzle scoreboards | In-world board shows progress, hidden keys, remaining locks, and total clues |
| Win Celebration | Door and seal change state, particles/lights play, and timed `CONGRATULATIONS!` text appears |

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
