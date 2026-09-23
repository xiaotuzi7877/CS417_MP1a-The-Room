# MP1a: The Room

A Unity 3D interactive room project created for CS417.

The project demonstrates room construction, materials, lighting, object-space and world-space transformations, object spawning, particle effects, spatial audio, orbital motion, camera teleportation, custom shaders, and OpenXR controller input mappings.

## Playable Build

Itch.io:

https://ziyangl7.itch.io/mp1a-the-room

## Features

- Enclosed medieval manor hall with a raised ceiling and a central celestial orrery
- Realtime point lighting with color switching
- Multiple wall materials with textures, normal maps, and tiling
- World Space controls canvas
- Planet and orbiting Moon
- Custom outline vertex shader
- Comet using kinematic double integration
- Object spawning system
- 18 particle systems, including four distributed action-feedback banks
- 16 spatialized 3D audio generators distributed through the hall
- Projectile motion with Trail Renderer
- Arbitrary gravitational attractor
- Stable orbital motion using calculated tangential velocity
- Camera breakout / return teleportation
- Custom six-sided skybox
- Multiple decorative 3D objects
- OpenXR / Oculus Touch controller mappings
- Keyboard fallback controls for editor and Web testing

## Controls

### Keyboard Testing Controls

| Key | Action |
|---|---|
| `Q` | Quit |
| `L` | Change room lighting |
| `B` | Break out / return to room |
| `P` | Spawn orbital object |

### VR Controller Mappings

| Controller Input | Action |
|---|---|
| Right Primary Button | Quit |
| Left Primary Button | Change room lighting |
| Right Secondary Button | Break out / return |
| Right Trigger | Spawn object |

Keyboard controls are included as testing fallbacks. The project also contains OpenXR controller bindings for Oculus Touch controllers.

## Particle Feedback

The main scene includes four banks of distributed particle and spatial-audio feedback:

- `Spawn` — triggered when an object is spawned
- `Light` — triggered when the room lighting changes
- `BreakOut` — triggered when leaving the room
- `Return` — triggered when returning to the room

Each bank contains four emitters and four 3D audio sources placed at different locations in the room. The original local spawn and light bursts remain as additional feedback.

## Orbital Motion

Spawned objects calculate their initial velocity relative to the Planet attractor.

The radial component of the initial direction is removed to create a tangential direction, and the orbital speed is calculated using:

```text
orbitalSpeed = sqrt(gravity / distance)
```

During each frame:

```text
velocity += acceleration * Time.deltaTime
position += velocity * Time.deltaTime
```

This produces stable orbital motion around the Planet.

## Project Structure

```text
Assets/
├── Audio/
├── Materials/
├── MichaelManor/
│   ├── Editor/
│   ├── Materials/
│   ├── Prefabs/
│   ├── Scripts/
│   ├── Textures/
│   └── ThirdParty/
├── Prefabs/
├── Scenes/
├── Scripts/
├── Shaders/
├── Skybox/
└── XR/
```

Main scene:

```text
Assets/Scenes/MichaelManorHall.unity
```

`MichaelManorHall` is build index 0. `SampleScene` is retained as the earlier MP1a reference scene.

## Grading Demo

See [`RUBRIC_EVIDENCE.md`](RUBRIC_EVIDENCE.md) for a short recording order and the exact scene objects that provide evidence for each implemented requirement.

## Built With

- Unity 6
- Universal Render Pipeline
- XR Interaction Toolkit
- OpenXR
- Unity Input System

## Contributor

**Ziyang Li**

CS417 — MP1a: The Room
