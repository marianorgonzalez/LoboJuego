# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Keep this file to things that stay true as the code changes: design intent, architectural principles and workflow. Implementation details belong in the code.

## Project

Lobojuego is a 2D platformer in Unity (URP 2D). The main character shapeshifts between a **human** form and a **werewolf** form.

Code identifiers are in English. Comments and in-game/debug strings are in Spanish (rioplatense).

## Game design intent

- Each form has its own abilities and its own movement settings.
- Switching form **in midair** triggers an ability that depends on the transition:
  - wolf → human: **double jump**
  - human → wolf: **forward dash**

## Architecture principles

- **Gameplay and collision are separate.** Player logic decides intent and velocity. The mover component resolves collision and sliding, and knows nothing about players, forms or abilities.
- **The character is kinematic.** Unity's physics solver does not move it, so all character motion goes through the mover.
- **Input is sampled per frame, simulation runs per physics step.** Button presses are buffered so they are not lost between the two.
- **Movement is tuned in designer terms** (speeds, jump heights, times), and physics values are derived from those rather than set directly.

## Commands

Day-to-day work happens in the Unity Editor, using the scene in `Assets/Scenes`. There are no build scripts and no linter.

Batch-mode commands need the Editor closed for this project. Use the Editor version in `ProjectSettings/ProjectVersion.txt`, installed under `C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe`.

Compile check, with errors in the log:

```
Unity.exe -batchmode -quit -projectPath . -logFile -
```

Run a single test:

```
Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testFilter <FullTestName> -testResults results.xml
```

Test assemblies cannot reference the default `Assembly-CSharp`, so code under test needs an `.asmdef`.

## Unity workflow

- Serialized scene values override code defaults, so changing a default in a script does not affect existing objects. Check the scene for the values actually in use.
- Renaming or moving a serialized field loses its scene value unless `[FormerlySerializedAs]` is used.
- Keep `.meta` files with their assets when moving or renaming.
- Do scene wiring in the Editor rather than hand-editing `.unity` YAML.
