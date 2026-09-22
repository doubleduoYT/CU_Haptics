# CU Haptics

**CU Haptics** is the haptic feedback system used by the Casualties: Unknown mobile port. It turns gameplay events into short vibration patterns so actions feel different instead of using one generic vibration.

## What it adds

The system includes dedicated haptic patterns for weapons, melee hits, damage, landing, explosions, earthquakes, radiation, hazards, minigames, medical interactions, UI feedback and death events.

It also supports intensity control, per-event cooldowns and priority handling so small feedback does not interrupt stronger effects.

On Android, the runtime can use the phone vibrator or compatible controller vibration hardware when available.

## How it works

Gameplay code calls the matching `HGHaptics` method when an event happens. For example:

- `HGHaptics.Gunshot(...)` for firearm recoil
- `HGHaptics.MeleeHit(...)` for melee impacts
- `HGHaptics.WorldExplosion(...)` for nearby explosions
- `HGHaptics.Earthquake(...)` for world shaking
- `HGHaptics.BandageWrap(...)`, `SyringeInject(...)` and other medical/minigame interactions

Each call is converted into a timed amplitude pattern, then routed to the active haptic output.

## Source

The complete runtime implementation is available here:

- [HGHaptics.cs](Source/HGHaptics.cs)

This repository contains the haptics module itself. The game-side event calls are integration points in the C:U mobile source and are intentionally kept separate from the runtime implementation.

## Download

Use the latest release for the packaged C:U mobile source mod. GitHub also provides the repository source code automatically with every release.

## Compatibility

Designed for the Casualties: Unknown mobile port and Unity's Android runtime. The module uses Unity APIs and is intended to be compiled as part of the game project.
