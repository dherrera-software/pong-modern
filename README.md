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
├── Core/          # GameSettings, Theme, InputManager, SceneManager
├── Entities/      # Ball, Paddle
├── Scenes/        # IScene, MainMenuScene, GameScene
├── UI/            # Button
└── Content/
    └── Fonts/     # DisplayFont (Orbitron), UIFont, ScoreFont (Courier New)
```

## How to Run
Prerequisites: .NET 8 SDK, MonoGame 3.8.1

```bash
dotnet restore
dotnet run
```

> [!NOTE]
> Place `Orbitron-Bold.ttf` in `Content/Fonts/` before building (download from Google Fonts — not included due to font license).

## Roadmap
- [x] 1v1 local multiplayer
- [x] Timed match (60 seconds)
- [ ] vs AI opponent
- [ ] Pause screen
- [ ] Sound effects
- [ ] Particle effects on score
- [ ] Android support

## Author
Diego Herrera — Fullstack Developer  
GitHub: [github.com/dherrera-software](https://github.com/dherrera-software)
