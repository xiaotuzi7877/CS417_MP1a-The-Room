# MP1a Manor Hall Rubric Evidence

Use `Assets/Scenes/MichaelManorHall.unity`. It is build index 0.

## Suggested Recording Order

1. Begin inside the manor and show the room boundaries, medieval furniture, gothic lanterns, raised ceiling, and moving celestial orrery.
2. Look at `ControlsCanvas_WorldSpace` on the left wall to show the in-world controls.
3. Show the Silver Fang outline and the lit central orrery.
4. Press the left primary controller button (keyboard: `L`) to change the room light color and trigger the distributed feedback.
5. Press the right trigger (keyboard: `P`) to spawn an object. Show its projectile trail, particle/audio feedback, gravitational motion, and stable orbit.
6. Press the right secondary controller button (keyboard: `B`) to break out of the room. Show the skybox and outside platform, then press it again to return.
7. Press the right primary controller button (keyboard: `Q`) last to demonstrate quit.

## Evidence Map

| Requirement | Scene or script evidence |
|---|---|
| Enclosed themed 3D room | `Architecture`, `Furniture`, `Decor`, `Lighting`, `ExitDoor` |
| World Space Canvas | `Systems/Rubric_Presentation/ControlsCanvas_WorldSpace` |
| Point light | `Systems/Rubric_Presentation/CeilingPointLight_Rubric` |
| Custom outline shader | Silver Fang renderer plus `Assets/Shaders/Outline.shader` |
| Rainbow/color-changing light | `LightSwitch`; left primary controller button or `L` |
| Quit input | `QuitGame`; right primary controller button or `Q` |
| Camera breakout and return | `BreakOut`; right secondary controller button or `B` |
| Object shooter/spawner | `ObjectSpawner`; right trigger or `P` |
| Projectile trail | Spawned `SpawnBall` and its `TrailRenderer` |
| Kinematic double integration | `KinematicIntegrate` on the spawned ball |
| Arbitrary attractor | `AttractedTo` targets the orrery Sun |
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
