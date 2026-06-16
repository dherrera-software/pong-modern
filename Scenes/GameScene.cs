using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PongGame.Core;
using PongGame.Core.Particles;
using PongGame.Core.Rendering;
using PongGame.Entities;
using PongGame.UI;
using System;

namespace PongGame.Scenes
{
    /// <summary>
    /// The main gameplay scene implementing the Pong match with countdown,
    /// scoring, and game-over states.
    /// </summary>
    public class GameScene : IScene
    {
        /// <summary>
        /// Represents the possible states of the game match.
        /// </summary>
        private enum GameState
        {
            Countdown,
            Playing,
            Scored,
            GameOver
        }

        private Paddle? _leftPaddle;
        private Paddle? _rightPaddle;
        private Ball? _ball;
        private SpriteFont? _displayFont;
        private SpriteFont? _scoreFont;
        private SpriteFont? _uiFont;
        private Texture2D? _pixel;
        private SpriteBatch? _spriteBatch;
        private RenderLayerManager? _renderer;
        private GameState _state;
        private float _stateTimer;
        private float _matchTimeRemaining;
        private const float MATCH_DURATION = 60f;
        private int _countdownStep;
        private int _lastScorer;
        private Button? _btnPlayAgain;
        private Button? _btnMenu;

        // ── Pause and Settings state ──────────────────────────────────────────
        private bool _isPaused;
        private bool _inSettings;

        // Pause Menu UI elements
        private Button? _btnResume;
        private Button? _btnSettings;
        private Button? _btnMainMenu;

        // Settings Screen UI elements
        private Slider? _sliderMaster;
        private Slider? _sliderMusic;
        private Slider? _sliderSfx;
        private Button? _btnSettingsBack;

        /// <summary>
        /// Initializes the scene. No specific state to reset for initial setup.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Loads fonts, creates pixel texture, initializes paddles, ball, and UI buttons.
        /// </summary>
        /// <param name="content">The application's content manager.</param>
        /// <param name="graphicsDevice">The active graphics device.</param>
        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _displayFont = content.Load<SpriteFont>("Fonts/DisplayFont");
            _scoreFont = content.Load<SpriteFont>("Fonts/ScoreFont");
            _uiFont = content.Load<SpriteFont>("Fonts/UIFont");

            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData([Color.White]);

            _spriteBatch = new SpriteBatch(graphicsDevice);
            _renderer = new RenderLayerManager(_spriteBatch);

            // Left paddle
            const float leftStartX = GameSettings.PADDLE_MARGIN + (GameSettings.PADDLE_WIDTH / 2f);
            _leftPaddle = new Paddle(leftStartX, 1);

            // Right paddle
            const float rightStartX = GameSettings.SCREEN_WIDTH - GameSettings.PADDLE_MARGIN - (GameSettings.PADDLE_WIDTH / 2f);
            _rightPaddle = new Paddle(rightStartX, 2);

            // Ball
            Vector2 center = new(GameSettings.SCREEN_WIDTH / 2f, GameSettings.SCREEN_HEIGHT / 2f);
            _ball = new Ball(center);

            // Play Again button: centered, Y=440, 260x56
            const int btnPlayAgainWidth = 260;
            const int btnPlayAgainHeight = 56;
            const int btnPlayAgainX = (GameSettings.SCREEN_WIDTH - btnPlayAgainWidth) / 2;
            const int btnPlayAgainY = 440;

            _btnPlayAgain = new Button(
                new Rectangle(btnPlayAgainX, btnPlayAgainY, btnPlayAgainWidth, btnPlayAgainHeight),
                "PLAY AGAIN",
                _uiFont,
                Theme.AccentP1,
                Color.White
            );

            // Menu button: centered, 70px below Play Again
            const int btnMenuWidth = 260;
            const int btnMenuHeight = 56;
            const int btnMenuX = (GameSettings.SCREEN_WIDTH - btnMenuWidth) / 2;
            const int btnMenuY = btnPlayAgainY + 70;

            _btnMenu = new Button(
                new Rectangle(btnMenuX, btnMenuY, btnMenuWidth, btnMenuHeight),
                "MENU",
                _uiFont,
                Theme.AccentP2,
                Color.White
            );

            // ── Pause Menu UI ────────────────────────────────────────────────
            const int btnPauseWidth = 260;
            const int btnPauseHeight = 56;
            const int btnPauseX = (GameSettings.SCREEN_WIDTH - btnPauseWidth) / 2;

            _btnResume = new Button(
                new Rectangle(btnPauseX, 280, btnPauseWidth, btnPauseHeight),
                "RESUME",
                _uiFont,
                Theme.AccentP1,
                Color.White
            );

            _btnSettings = new Button(
                new Rectangle(btnPauseX, 350, btnPauseWidth, btnPauseHeight),
                "SETTINGS",
                _uiFont,
                Theme.AccentP2,
                Color.White
            );

            _btnMainMenu = new Button(
                new Rectangle(btnPauseX, 420, btnPauseWidth, btnPauseHeight),
                "MAIN MENU",
                _uiFont,
                Theme.AccentP1,
                Color.White
            );

            // ── Settings UI ──────────────────────────────────────────────────
            const int sliderWidth = 400;
            const int sliderHeight = 20;
            const int sliderX = (GameSettings.SCREEN_WIDTH - sliderWidth) / 2;

            _sliderMaster = new Slider(
                new Rectangle(sliderX, 280, sliderWidth, sliderHeight),
                "MASTER VOLUME",
                _uiFont,
                Theme.AccentP1,
                AudioManager.MasterVolume
            );
            _sliderMaster.OnValueChanged += (val) => AudioManager.MasterVolume = val;

            _sliderMusic = new Slider(
                new Rectangle(sliderX, 370, sliderWidth, sliderHeight),
                "MUSIC VOLUME",
                _uiFont,
                Theme.AccentP2,
                AudioManager.MusicVolume
            );
            _sliderMusic.OnValueChanged += (val) => AudioManager.MusicVolume = val;

            _sliderSfx = new Slider(
                new Rectangle(sliderX, 460, sliderWidth, sliderHeight),
                "SFX VOLUME",
                _uiFont,
                Theme.AccentP1,
                AudioManager.SfxVolume
            );
            _sliderSfx.OnValueChanged += (val) => AudioManager.SfxVolume = val;

            const int btnBackWidth = 200;
            const int btnBackHeight = 50;
            const int btnBackX = (GameSettings.SCREEN_WIDTH - btnBackWidth) / 2;
            _btnSettingsBack = new Button(
                new Rectangle(btnBackX, 550, btnBackWidth, btnBackHeight),
                "BACK",
                _uiFont,
                Theme.AccentP2,
                Color.White
            );
        }

        /// <summary>
        /// Called when this scene becomes the active scene. Resets scores, positions, and starts countdown.
        /// Also triggers the gameplay music with a crossfade from whatever was playing before.
        /// </summary>
        public void OnEnter()
        {
            if (_leftPaddle == null || _rightPaddle == null || _ball == null)
            {
                return;
            }

            _matchTimeRemaining = MATCH_DURATION;

            _leftPaddle.Score = 0;
            _rightPaddle.Score = 0;
            _leftPaddle.ResetPosition();
            _rightPaddle.ResetPosition();

            Vector2 center = new(GameSettings.SCREEN_WIDTH / 2f, GameSettings.SCREEN_HEIGHT / 2f);
            _ball.Reset(center);

            _state = GameState.Countdown;
            _stateTimer = 0f;
            _countdownStep = 3;

            _isPaused = false;
            _inSettings = false;

            // Clear any lingering particles from previous gameplay session
            ParticleManager.Clear();

            // Start gameplay music (crossfades from any currently playing track).
            AudioManager.PlayTrack("gameplay");
        }

        /// <summary>
        /// Called when this scene is deactivated and replaced by another scene.
        /// Music transition is driven by the next scene's <see cref="AudioManager.PlayTrack"/> call.
        /// </summary>
        public void OnExit()
        {
            ParticleManager.Clear();
        }

        /// <summary>
        /// Updates the game logic based on the current game state.
        /// </summary>
        /// <param name="gameTime">Provides snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            if (TransitionManager.IsTransitioning)
            {
                return;
            }

            if (_leftPaddle == null || _rightPaddle == null || _ball == null ||
                _btnPlayAgain == null || _btnMenu == null ||
                _btnResume == null || _btnSettings == null || _btnMainMenu == null ||
                _sliderMaster == null || _sliderMusic == null || _sliderSfx == null ||
                _btnSettingsBack == null)
            {
                return;
            }

            // Check pause toggle
            if (InputManager.IsPausePressed())
            {
                _isPaused = !_isPaused;
                if (_isPaused)
                {
                    _inSettings = false;
                    _sliderMaster.Value = AudioManager.MasterVolume;
                    _sliderMusic.Value = AudioManager.MusicVolume;
                    _sliderSfx.Value = AudioManager.SfxVolume;
                }
            }

            if (_isPaused)
            {
                if (_inSettings)
                {
                    _sliderMaster.Update(gameTime);
                    _sliderMusic.Update(gameTime);
                    _sliderSfx.Update(gameTime);
                    _btnSettingsBack.Update(gameTime);

                    if (_btnSettingsBack.WasClicked)
                    {
                        _inSettings = false;
                    }
                }
                else
                {
                    _btnResume.Update(gameTime);
                    _btnSettings.Update(gameTime);
                    _btnMainMenu.Update(gameTime);

                    if (_btnResume.WasClicked)
                    {
                        _isPaused = false;
                    }
                    if (_btnSettings.WasClicked)
                    {
                        _inSettings = true;
                    }
                    if (_btnMainMenu.WasClicked)
                    {
                        TransitionManager.StartTransition("menu");
                    }
                }
                return; // Freeze gameplay simulation
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update particles
            ParticleManager.Update(gameTime);

            switch (_state)
            {
                case GameState.Countdown:
                    UpdateCountdown(deltaTime);
                    break;

                case GameState.Playing:
                    UpdatePlaying(gameTime);
                    break;

                case GameState.Scored:
                    UpdateScored(deltaTime);
                    break;

                case GameState.GameOver:
                    UpdateGameOver(gameTime);
                    break;
            }
        }

        private void UpdateCountdown(float deltaTime)
        {
            _stateTimer += deltaTime;

            if (_stateTimer >= 1.0f)
            {
                _stateTimer--;
                _countdownStep--;

                if (_countdownStep <= 0)
                {
                    _state = GameState.Playing;
                    _stateTimer = 0f;
                }
            }
        }

        private void UpdatePlaying(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _matchTimeRemaining -= deltaTime;
            if (_matchTimeRemaining < 0f)
            {
                _matchTimeRemaining = 0f;
            }

            if (_matchTimeRemaining <= 0f)
            {
                _state = GameState.GameOver;
                return;
            }

            // Move paddles
            _leftPaddle!.Update(gameTime, InputManager.IsMoveUp(1), InputManager.IsMoveDown(1));
            _rightPaddle!.Update(gameTime, InputManager.IsMoveUp(2), InputManager.IsMoveDown(2));

            // Update ball
            _ball!.Update(gameTime, _leftPaddle, _rightPaddle, out int scorer);

            if (scorer != 0)
            {
                _lastScorer = scorer;

                // Color score burst based on which player scored
                Color burstColor = scorer == 1 ? Theme.AccentP1 : Theme.AccentP2;
                ParticleManager.Emit(ParticleEmitter.Score, _ball.Position, 0f, burstColor);
                AudioManager.PlaySfx("score");

                if (scorer == 1)
                {
                    _leftPaddle.Score++;
                }
                else if (scorer == 2)
                {
                    _rightPaddle.Score++;
                }

                _stateTimer = 0f;
                _state = GameState.Scored;
            }
        }

        private void UpdateScored(float deltaTime)
        {
            _stateTimer += deltaTime;

            if (_stateTimer >= 1.5f)
            {
                // Reset ball and paddles for next round
                Vector2 center = new(GameSettings.SCREEN_WIDTH / 2f, GameSettings.SCREEN_HEIGHT / 2f);
                _ball!.Reset(center);
                _leftPaddle!.ResetPosition();
                _rightPaddle!.ResetPosition();

                _state = GameState.Countdown;
                _countdownStep = 3;
                _stateTimer = 0f;
            }
        }

        private void UpdateGameOver(GameTime gameTime)
        {
            _btnPlayAgain!.Update(gameTime);
            _btnMenu!.Update(gameTime);

            if (_btnPlayAgain.WasClicked)
            {
                OnEnter();
            }

            if (_btnMenu.WasClicked)
            {
                TransitionManager.StartTransition("menu");
            }
        }

        /// <summary>
        /// Draws all game elements and state-specific overlays.
        /// </summary>
        /// <param name="spriteBatch">The default sprite batch (not used directly, we use our local _spriteBatch).</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_spriteBatch == null || _pixel == null || _displayFont == null || _scoreFont == null || _uiFont == null ||
                _leftPaddle == null || _rightPaddle == null || _ball == null ||
                _btnPlayAgain == null || _btnMenu == null || _renderer == null ||
                _btnResume == null || _btnSettings == null || _btnMainMenu == null ||
                _sliderMaster == null || _sliderMusic == null || _sliderSfx == null ||
                _btnSettingsBack == null)
            {
                return;
            }

            // 1. Clear background
            _spriteBatch.GraphicsDevice.Clear(Theme.Background);

            _renderer.ExecuteGlowPass(sb =>
            {
                _leftPaddle.DrawGlow(sb, _pixel);
                _rightPaddle.DrawGlow(sb, _pixel);
                _ball.DrawGlow(sb, _pixel);
                ParticleManager.DrawGlow(sb);

                if (_isPaused)
                {
                    if (_inSettings)
                    {
                        _sliderMaster.DrawGlow(sb, _pixel);
                        _sliderMusic.DrawGlow(sb, _pixel);
                        _sliderSfx.DrawGlow(sb, _pixel);
                    }
                    else
                    {
                        Vector2 titleSize = _displayFont.MeasureString("GAME PAUSED");
                        float titleX = (GameSettings.SCREEN_WIDTH - titleSize.X) / 2f;
                        GlowRenderer.DrawTextGlow(sb, _displayFont, "GAME PAUSED", new Vector2(titleX, 160f), Theme.AccentP1, layers: 3);
                    }
                }
            });

            _renderer.ExecuteGameplayPass(sb =>
            {
                DrawCenterLine();
                _leftPaddle.Draw(sb, _pixel);
                _rightPaddle.Draw(sb, _pixel);
                _ball.Draw(sb, _pixel);
                ParticleManager.Draw(sb);
            });

            _renderer.ExecuteUIPass(sb =>
            {
                DrawHUD();
                // State overlays
                switch (_state)
                {
                    case GameState.Countdown: DrawCountdownOverlay(); break;
                    case GameState.Scored:    DrawScoredOverlay();    break;
                    case GameState.GameOver:  DrawGameOverOverlay();  break;
                }

                if (_isPaused)
                {
                    // Semi-transparent dark overlay
                    sb.Draw(_pixel, new Rectangle(0, 0, GameSettings.SCREEN_WIDTH, GameSettings.SCREEN_HEIGHT), new Color(0, 0, 0, 180));

                    if (_inSettings)
                    {
                        // Header "AUDIO SETTINGS"
                        Vector2 headerSize = _displayFont.MeasureString("AUDIO SETTINGS");
                        float headerX = (GameSettings.SCREEN_WIDTH - headerSize.X) / 2f;
                        sb.DrawString(_displayFont, "AUDIO SETTINGS", new Vector2(headerX, 160f), Theme.AccentP2);

                        // Draw Sliders
                        _sliderMaster.Draw(sb, _pixel);
                        _sliderMusic.Draw(sb, _pixel);
                        _sliderSfx.Draw(sb, _pixel);

                        // Draw Back Button
                        _btnSettingsBack.Draw(sb, _pixel);
                    }
                    else
                    {
                        // Header "GAME PAUSED"
                        Vector2 headerSize = _displayFont.MeasureString("GAME PAUSED");
                        float headerX = (GameSettings.SCREEN_WIDTH - headerSize.X) / 2f;
                        sb.DrawString(_displayFont, "GAME PAUSED", new Vector2(headerX, 160f), Theme.AccentP1);

                        // Draw Buttons
                        _btnResume.Draw(sb, _pixel);
                        _btnSettings.Draw(sb, _pixel);
                        _btnMainMenu.Draw(sb, _pixel);
                    }
                }
            });
        }

        private void DrawCenterLine()
        {
            const int dashHeight = GameSettings.CENTER_LINE_DASH_HEIGHT;
            const int gap = GameSettings.CENTER_LINE_GAP;
            const int lineX = (GameSettings.SCREEN_WIDTH - 3) / 2;
            Color lineColor = Theme.DimText * 0.4f;

            for (int y = 0; y < GameSettings.SCREEN_HEIGHT; y += dashHeight + gap)
            {
                int currentDashHeight = Math.Min(dashHeight, GameSettings.SCREEN_HEIGHT - y);
                if (currentDashHeight > 0)
                {
                    _spriteBatch!.Draw(_pixel, new Rectangle(lineX, y, 3, currentDashHeight), lineColor);
                }
            }
        }

        private void DrawHUD()
        {
            // Left paddle score
            string leftScoreText = _leftPaddle!.Score.ToString();
            Vector2 leftScoreSize = _scoreFont!.MeasureString(leftScoreText);
            float leftScoreX = (GameSettings.SCREEN_WIDTH / 4f) - (leftScoreSize.X / 2f);
            _spriteBatch!.DrawString(_scoreFont, leftScoreText, new Vector2(leftScoreX, 40f), Theme.AccentP1);

            // Right paddle score
            string rightScoreText = _rightPaddle!.Score.ToString();
            Vector2 rightScoreSize = _scoreFont.MeasureString(rightScoreText);
            float rightScoreX = (3f * GameSettings.SCREEN_WIDTH / 4f) - (rightScoreSize.X / 2f);
            _spriteBatch.DrawString(_scoreFont, rightScoreText, new Vector2(rightScoreX, 40f), Theme.AccentP2);

            // Timer display
            int seconds = (int)Math.Ceiling(_matchTimeRemaining);
            string timerText = seconds.ToString();
            Color timerColor;
            if (seconds > 10)
            {
                timerColor = Theme.UIText;
            }
            else if (seconds > 5)
            {
                timerColor = Theme.AccentP2;
            }
            else
            {
                float pulse = (MathF.Sin(_matchTimeRemaining * 6f) + 1f) / 2f;
                timerColor = Color.Lerp(Theme.AccentP2, Color.White, pulse * 0.6f);
            }

            Vector2 timerSize = _scoreFont.MeasureString(timerText);
            float timerX = (GameSettings.SCREEN_WIDTH / 2f) - (timerSize.X / 2f);
            _spriteBatch.DrawString(_scoreFont, timerText, new Vector2(timerX, 40f), timerColor);
        }

        private void DrawCountdownOverlay()
        {
            // Determine text: "3", "2", "1", or "GO!"
            string countdownText = _countdownStep > 0 ? _countdownStep.ToString() : "GO!";

            Vector2 textSize = _scoreFont!.MeasureString(countdownText);

            // Scale effect: slightly larger when stateTimer < 0.3s
            float scale = _stateTimer < 0.3f ? 1.3f - (_stateTimer / 0.3f * 0.3f) : 1.0f;
            Vector2 scaledSize = textSize * scale;

            Vector2 position = new(
                (GameSettings.SCREEN_WIDTH - scaledSize.X) / 2f,
                (GameSettings.SCREEN_HEIGHT - scaledSize.Y) / 2f
            );

            _spriteBatch!.DrawString(_scoreFont, countdownText, position, Theme.UIText, 0f,
                Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        private void DrawScoredOverlay()
        {
            string scoredText = $"PLAYER {_lastScorer} SCORES!";
            Color accentColor = _lastScorer == 1 ? Theme.AccentP1 : Theme.AccentP2;

            Vector2 textSize = _uiFont!.MeasureString(scoredText);
            Vector2 position = new(
                (GameSettings.SCREEN_WIDTH - textSize.X) / 2f,
                (GameSettings.SCREEN_HEIGHT - textSize.Y) / 2f
            );

            _spriteBatch!.DrawString(_uiFont, scoredText, position, accentColor);
        }

        private void DrawGameOverOverlay()
        {
            // Semi-transparent dark overlay
            _spriteBatch!.Draw(_pixel, new Rectangle(0, 0, GameSettings.SCREEN_WIDTH, GameSettings.SCREEN_HEIGHT),
                new Color(0, 0, 0, 180));

            // Determine winner
            int winner = 0; // 0 = draw, 1 = left, 2 = right
            if (_leftPaddle!.Score > _rightPaddle!.Score)
            {
                winner = 1;
            }
            else if (_rightPaddle.Score > _leftPaddle.Score)
            {
                winner = 2;
            }

            if (winner == 0)
            {
                // Draw "DRAW!" centered at Y=200
                const string drawText = "DRAW!";
                Vector2 drawTextSize = _displayFont!.MeasureString(drawText);
                float drawTextX = (GameSettings.SCREEN_WIDTH - drawTextSize.X) / 2f;
                _spriteBatch.DrawString(_displayFont, drawText, new Vector2(drawTextX, 200f), Theme.UIText);
            }
            else
            {
                // Draw "PLAYER X WINS!" centered at Y=200
                string winText = $"PLAYER {winner} WINS!";
                Color winnerColor = winner == 1 ? Theme.AccentP1 : Theme.AccentP2;
                Vector2 winTextSize = _displayFont!.MeasureString(winText);
                float winTextX = (GameSettings.SCREEN_WIDTH - winTextSize.X) / 2f;
                _spriteBatch.DrawString(_displayFont, winText, new Vector2(winTextX, 200f), winnerColor);
            }

            // Draw score "X  -  X" centered at Y=290
            string scoreText = $"{_leftPaddle.Score}  -  {_rightPaddle.Score}";
            Vector2 scoreTextSize = _uiFont!.MeasureString(scoreText);
            float scoreTextX = (GameSettings.SCREEN_WIDTH - scoreTextSize.X) / 2f;
            _spriteBatch.DrawString(_uiFont, scoreText, new Vector2(scoreTextX, 290f), Theme.UIText);

            // Draw buttons
            _btnPlayAgain!.Draw(_spriteBatch, _pixel!);
            _btnMenu!.Draw(_spriteBatch, _pixel!);
        }
    }
}
