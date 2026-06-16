# PONG — Modern Edition

A modern take on the classic Pong game built with MonoGame and .NET 8.

## Preview
![Gameplay screenshot](./screenshot.png)

## Features
- 1v1 local multiplayer
- 60-second timed match — most points wins
- DRAW! screen on tie
- Neon aesthetic: cyan vs magenta, dark background
- Orbitron display font for titles
- Ball trail and neon glow effects on paddles and ball
- Countdown animation before each round
- Pulse effect on timer when under 5 seconds
- Background music with smooth crossfade transitions
- Keyboard controls (W/S and Up/Down arrows)
- Touch input ready for future mobile port

## Tech Stack
- MonoGame 3.8.1 / .NET 8
- C# 12
- Platform: Windows DesktopGL (Android planned)

## Controls
| Action | Keys |
| :--- | :--- |
| Move Player 1 up | W |
| Move Player 1 down | S |
| Move Player 2 up | Up Arrow |
| Move Player 2 down | Down Arrow |
| Pause (coming soon) | ESC |

## Project Structure
```text
PongGame/
├── Core/          # GameSettings, Theme, InputManager, SceneManager, AudioManager
├── Entities/      # Ball, Paddle
├── Scenes/        # IScene, MainMenuScene, GameScene
├── UI/            # Button
└── Content/
    ├── Fonts/     # DisplayFont (Orbitron), UIFont, ScoreFont (Courier New)
    └── audio/
        ├── music/ # menu_theme.wav, gameplay_theme.wav
        └── sfx/   # (future sound effects)
```

## How to Run
Prerequisites: .NET 8 SDK, MonoGame 3.8.1

```bash
dotnet restore
dotnet run
```

> [!NOTE]
> Place `Orbitron-Bold.ttf` in `Content/Fonts/` before building (download from Google Fonts — not included due to font license).

## Audio System

### Asset Structure

All audio assets live under `Content/audio/`:

```text
Content/audio/
├── music/    # Looping background tracks (WAV → compiled as Song by MGCB)
└── sfx/      # One-shot sound effects (WAV → SoundEffect, future use)
```

### AudioManager

`Core/AudioManager.cs` is the single point of truth for music playback. It wraps
MonoGame's `MediaPlayer` / `Song` API and exposes a clean, scene-agnostic interface.

| Responsibility | Detail |
| :--- | :--- |
| Loading tracks | `AudioManager.LoadTrack(key, contentPath)` — call once at startup |
| Playing a track | `AudioManager.PlayTrack(key)` — safe to call from any `OnEnter` |
| Stopping music | `AudioManager.Stop()` — fades out and stops |
| Crossfade | Automatic: fade-out current → fade-in next (400 ms each direction) |
| Duplicate guard | Calling `PlayTrack` with the already-playing key is a no-op |
| Master volume | `AudioManager.MasterVolume` (0.0–1.0, default 1.0) |

### Scene→Track Mapping

| Scene | Track key | Asset |
| :--- | :--- | :--- |
| `MainMenuScene` | `"menu"` | `audio/music/menu_theme` |
| `GameScene` | `"gameplay"` | `audio/music/gameplay_theme` |

Scenes call `AudioManager.PlayTrack(...)` inside their `OnEnter()` override.
The `AudioManager` handles the fade-out of the previous track automatically, so
`OnExit()` does **not** need to stop music explicitly.

### Adding a New Music Track

1. Place the WAV file in `Content/audio/music/`.
2. Register it in `Content/Content.mgcb`:
   ```
   /importer:WavImporter
   /processor:SongProcessor
   /build:audio/music/your_track.wav
   ```
3. Preload it in `Game1.LoadContent`:
   ```csharp
   AudioManager.LoadTrack("mykey", "audio/music/your_track");
   ```
4. In the relevant scene's `OnEnter()`:
   ```csharp
   AudioManager.PlayTrack("mykey");
   ```

### Adding Sound Effects (Future)

Place WAV files in `Content/audio/sfx/`, register them in `Content.mgcb` with
`/processor:SoundEffectProcessor`, then load and play via `SoundEffect` directly.
A future `AudioManager.PlaySfx(key)` helper is the intended pattern.

## Roadmap
- [x] 1v1 local multiplayer
- [x] Timed match (60 seconds)
- [x] Background music with crossfade transitions
- [ ] vs AI opponent
- [ ] Pause screen
- [ ] Sound effects (paddle hit, score, game over)
- [ ] Particle effects on score
- [ ] Android support

## Author
Diego Herrera — Fullstack Developer  
GitHub: [github.com/dherrera-software](https://github.com/dherrera-software)
