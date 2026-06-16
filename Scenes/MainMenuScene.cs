using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PongGame.Core;
using PongGame.Core.Rendering;
using PongGame.UI;
using System;

namespace PongGame.Scenes
{
    /// <summary>
    /// Represents the main menu scene of the game.
    /// </summary>
    public class MainMenuScene : IScene
    {
        private SpriteFont? _displayFont;
        private SpriteFont? _uiFont;
        private Texture2D? _pixel;
        private Button? _btn1v1;
        private Button? _btnVsAI;
        private SpriteBatch? _spriteBatch;
        private RenderLayerManager? _renderer;
        private AnimatedBackground? _background;

        // ── Animation timers ─────────────────────────────────────────────────
        private float _titleTime    = 0f;
        private float _subtitleTime = 0f;
        private float _lineTime     = 0f;
        private float _footerTime   = 0f;
        private float _buttonTime   = 0f;

        // ── Title float ───────────────────────────────────────────────────────
        private const float TITLE_BASE_Y    = 100f;
        private const float TITLE_AMPLITUDE = 7f;
        private const float TITLE_FREQUENCY = 1.4f;

        /// <summary>Gets the current animated Y position of the title text.</summary>
        private float CurrentTitleY =>
            TITLE_BASE_Y + (MathF.Sin(_titleTime * TITLE_FREQUENCY) * TITLE_AMPLITUDE);

        // ── Subtitle breathing ────────────────────────────────────────────────
        // Period ~3 s → frequency = 2π/3
        private const float SUBTITLE_FREQ    = 2f * MathF.PI / 3f;
        private const float SUBTITLE_MIN     = 0.70f;
        private const float SUBTITLE_MAX     = 1.00f;

        private float SubtitleAlpha =>
            SUBTITLE_MIN + ((SUBTITLE_MAX - SUBTITLE_MIN) *
            ((MathF.Sin(_subtitleTime * SUBTITLE_FREQ) + 1f) * 0.5f));

        // ── Center-line pulse ─────────────────────────────────────────────────
        // Slightly different phase/period so it doesn't sync with subtitle
        private const float LINE_FREQ = 2f * MathF.PI / 4f;
        private const float LINE_MIN  = 0.30f;
        private const float LINE_MAX  = 0.60f;

        private float LineAlpha =>
            LINE_MIN + ((LINE_MAX - LINE_MIN) *
            ((MathF.Sin(_lineTime * LINE_FREQ) + 1f) * 0.5f));

        // ── Footer breathing ──────────────────────────────────────────────────
        // Slower than subtitle (~5 s period)
        private const float FOOTER_FREQ = 2f * MathF.PI / 5f;
        private const float FOOTER_MIN  = 0.50f;
        private const float FOOTER_MAX  = 0.80f;

        private float FooterAlpha =>
            FOOTER_MIN + ((FOOTER_MAX - FOOTER_MIN) *
            ((MathF.Sin(_footerTime * FOOTER_FREQ) + 1f) * 0.5f));

        // ── Button float ──────────────────────────────────────────────────────
        // 3-px amplitude, ~4 s period
        private const float BTN_FLOAT_FREQ      = 2f * MathF.PI / 4f;
        private const float BTN_FLOAT_AMPLITUDE = 3f;

        private float ButtonGroupFloatOffset =>
            MathF.Sin(_buttonTime * BTN_FLOAT_FREQ) * BTN_FLOAT_AMPLITUDE;

        // ── Button hover scale interpolation ─────────────────────────────────
        // Each button independently tracks its animated scale (0=1v1, 1=VsAI).
        private readonly float[] _btnScale = [1.0f, 1.0f];
        private const float BTN_HOVER_LERP_SPEED = 6f;   // units per second
        private const float BTN_SCALE_NORMAL     = 1.00f;
        private const float BTN_SCALE_HOVER      = 1.05f;

        // Which button index is currently "selected" (hovered by mouse).
        // -1 = none.
        private int _hoveredIndex = -1;

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes the scene. No specific state to reset for the main menu.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Loads fonts and creates UI buttons and pixel texture.
        /// </summary>
        /// <param name="content">The application's content manager.</param>
        /// <param name="graphicsDevice">The active graphics device.</param>
        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _displayFont = content.Load<SpriteFont>("Fonts/DisplayFont");
            _uiFont      = content.Load<SpriteFont>("Fonts/UIFont");
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _renderer    = new RenderLayerManager(_spriteBatch);
            _background  = new AnimatedBackground(75);

            // Create 1x1 white pixel texture
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData([Color.White]);

            const int btnWidth = 320;
            const int btnHeight = 64;
            const int btnX  = (1280 - btnWidth) / 2;
            const int btnY1 = ((720 - btnHeight) / 2) - 40;
            const int btnY2 = btnY1 + 90;

            _btn1v1 = new Button(
                new Rectangle(btnX, btnY1, btnWidth, btnHeight),
                "1 VS 1",
                _uiFont,
                Theme.AccentP1,
                Color.White
            );

            _btnVsAI = new Button(
                new Rectangle(btnX, btnY2, btnWidth, btnHeight),
                "VS IA  [PROXIMAMENTE]",
                _uiFont,
                Theme.DimText,
                Theme.DimText
            )
            {
                IsDisabled = true
            };
        }

        /// <summary>
        /// Updates the main menu input controls and button states.
        /// </summary>
        /// <param name="gameTime">Provides snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Advance background and all independent timers
            _background?.Update(gameTime);
            _titleTime    += dt;
            _subtitleTime += dt;
            _lineTime     += dt;
            _footerTime   += dt;
            _buttonTime   += dt;

            // Update buttons (handles click detection)
            _btn1v1?.Update(gameTime);
            _btnVsAI?.Update(gameTime);

            // Determine which button is hovered via their WasClicked/IsHovered
            // We probe mouse state through the bounding rectangles directly.
            // Button exposes Bounds, so we reconstruct hovered state here.
            _hoveredIndex = -1;
            Microsoft.Xna.Framework.Input.MouseState ms =
                Microsoft.Xna.Framework.Input.Mouse.GetState();
            Point mp = ms.Position;

            if (_btn1v1?.IsDisabled is false && _btn1v1.Bounds.Contains(mp))
            {
                _hoveredIndex = 0;
            }
            else if (_btnVsAI?.IsDisabled is false && _btnVsAI.Bounds.Contains(mp))
            {
                _hoveredIndex = 1;
            }

            // Smoothly interpolate each button's scale toward its target
            float lerpAmount = MathHelper.Clamp(BTN_HOVER_LERP_SPEED * dt, 0f, 1f);
            for (int i = 0; i < _btnScale.Length; i++)
            {
                float target = (i == _hoveredIndex) ? BTN_SCALE_HOVER : BTN_SCALE_NORMAL;
                _btnScale[i] += (target - _btnScale[i]) * lerpAmount;
            }

            if (_btn1v1?.WasClicked is true)
            {
                SceneManager.ChangeScene("game");
            }
        }

        /// <summary>
        /// Draws the main menu layout including animated background, title glow, and UI elements.
        /// </summary>
        /// <param name="spriteBatch">The default sprite batch (not used directly, we use our local _spriteBatch).</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_spriteBatch == null || _pixel == null || _displayFont == null
                || _uiFont == null || _renderer == null || _background == null)
            {
                return;
            }

            // 1. Clear background
            _spriteBatch.GraphicsDevice.Clear(Theme.Background);

            // Pass 1: Background (additive particles)
            _renderer.ExecuteBackgroundPass(sb => _background.Draw(sb, _pixel));

            // Pass 2: Glow (title halo + button hover glow)
            _renderer.ExecuteGlowPass(sb =>
            {
                // Title glow
                float titleY = CurrentTitleY;
                Vector2 titleSize = _displayFont.MeasureString("PONG");
                float titleX = (GameSettings.SCREEN_WIDTH - titleSize.X) / 2f;
                GlowRenderer.DrawTextGlow(sb, _displayFont, "PONG",
                    new Vector2(titleX, titleY), Theme.AccentP1, layers: 3);

                // Hover glow on the active (1v1) button
                if (_btn1v1 != null && _btnScale[0] > BTN_SCALE_NORMAL + 0.001f)
                {
                    float glowStrength = (_btnScale[0] - BTN_SCALE_NORMAL) /
                                        (BTN_SCALE_HOVER  - BTN_SCALE_NORMAL);

                    // Compute the scaled & floated bounds for the glow
                    Rectangle b    = _btn1v1.Bounds;
                    float floatOff = ButtonGroupFloatOffset;
                    Vector2 center = new(b.X + (b.Width / 2f), b.Y + (b.Height / 2f) + floatOff);
                    float scale    = _btnScale[0];
                    Rectangle glowBounds = new(
                        (int)(center.X - (b.Width  * scale * 0.5f)),
                        (int)(center.Y - (b.Height * scale * 0.5f)),
                        (int)(b.Width  * scale),
                        (int)(b.Height * scale)
                    );
                    GlowRenderer.DrawRectGlow(sb, _pixel, glowBounds,
                        Theme.AccentP1 * glowStrength * 0.6f);
                }
            });

            // Pass 3: UI (all interface elements)
            _renderer.ExecuteUIPass(_ =>
            {
                DrawCenterLine();
                DrawTitle();
                DrawSubtitle();
                DrawButtons();
                DrawFooter();
            });
        }

        /// <summary>
        /// Draws the vertical dashed center line with an animated alpha pulse.
        /// </summary>
        private void DrawCenterLine()
        {
            const int dashHeight = GameSettings.CENTER_LINE_DASH_HEIGHT;
            const int gap        = GameSettings.CENTER_LINE_GAP;
            const int lineX      = (GameSettings.SCREEN_WIDTH - 3) / 2;
            float lineAlpha      = LineAlpha;

            for (int y = 0; y < GameSettings.SCREEN_HEIGHT; y += dashHeight + gap)
            {
                int currentDashHeight = Math.Min(dashHeight, GameSettings.SCREEN_HEIGHT - y);
                if (currentDashHeight > 0)
                {
                    _spriteBatch!.Draw(_pixel,
                        new Rectangle(lineX, y, 3, currentDashHeight),
                        Theme.DimText * lineAlpha);
                }
            }
        }

        /// <summary>
        /// Draws the solid "PONG" title text at the animated Y position.
        /// </summary>
        private void DrawTitle()
        {
            const string titleText = "PONG";
            Vector2 titleSize = _displayFont!.MeasureString(titleText);
            float titleX = (GameSettings.SCREEN_WIDTH - titleSize.X) / 2f;
            float titleY = CurrentTitleY;
            _spriteBatch!.DrawString(_displayFont, titleText, new Vector2(titleX, titleY), Theme.AccentP1);
        }

        /// <summary>
        /// Draws the "MODERN EDITION" subtitle with a slow breathing opacity.
        /// </summary>
        private void DrawSubtitle()
        {
            const string subtitleText = "MODERN  EDITION";
            Vector2 subtitleSize = _uiFont!.MeasureString(subtitleText);
            float subtitleX = (GameSettings.SCREEN_WIDTH - subtitleSize.X) / 2f;
            const float subtitleY = 185f;
            _spriteBatch!.DrawString(_uiFont, subtitleText,
                new Vector2(subtitleX, subtitleY),
                Theme.UIText * SubtitleAlpha);
        }

        /// <summary>
        /// Draws both buttons with float offset and smooth scale hover animation.
        /// The disabled button is drawn normally; the active button is drawn scaled.
        /// </summary>
        private void DrawButtons()
        {
            float floatOff = ButtonGroupFloatOffset;

            // ── Draw disabled VS AI button (no animation) ────────────────────
            if (_btnVsAI != null)
            {
                Rectangle b = _btnVsAI.Bounds;
                Rectangle displaced = new(b.X, (int)(b.Y + floatOff), b.Width, b.Height);
                _spriteBatch!.Draw(_pixel, displaced, Theme.DimText * 0.3f);
                Vector2 textSize = _uiFont!.MeasureString("VS IA  [PROXIMAMENTE]");
                Vector2 textPos  = new(
                    displaced.X + ((displaced.Width  - textSize.X) / 2f),
                    displaced.Y + ((displaced.Height - textSize.Y) / 2f));
                _spriteBatch.DrawString(_uiFont, "VS IA  [PROXIMAMENTE]", textPos, Theme.UIText * 0.5f);
            }

            // ── Draw 1v1 button with scale + float ───────────────────────────
            if (_btn1v1 != null)
            {
                Rectangle b     = _btn1v1.Bounds;
                float scale     = _btnScale[0];
                Vector2 center  = new(b.X + (b.Width / 2f), b.Y + (b.Height / 2f) + floatOff);
                int scaledW     = (int)(b.Width  * scale);
                int scaledH     = (int)(b.Height * scale);
                Rectangle dest  = new(
                    (int)(center.X - (scaledW * 0.5f)),
                    (int)(center.Y - (scaledH * 0.5f)),
                    scaledW, scaledH);

                // Interpolate background color toward brighter when hovered
                float hoverT    = (scale - BTN_SCALE_NORMAL) / (BTN_SCALE_HOVER - BTN_SCALE_NORMAL);
                Color bgColor   = Color.Lerp(Theme.AccentP1, Color.White, hoverT * 0.25f);
                _spriteBatch!.Draw(_pixel, dest, bgColor);

                // Draw label centered in the scaled rect
                const string label = "1 VS 1";
                Vector2 textSize   = _uiFont!.MeasureString(label);
                Vector2 textPos    = new(
                    dest.X + ((dest.Width  - textSize.X) / 2f),
                    dest.Y + ((dest.Height - textSize.Y) / 2f));
                _spriteBatch.DrawString(_uiFont, label, textPos, Theme.Background);
            }
        }

        /// <summary>
        /// Draws the footer instruction text with a slow breathing opacity.
        /// </summary>
        private void DrawFooter()
        {
            const string footerText = "W / S     Up / Down   to move      ESC to pause";
            Vector2 footerSize = _uiFont!.MeasureString(footerText);
            float footerX = (GameSettings.SCREEN_WIDTH - footerSize.X) / 2f;
            const float footerY = GameSettings.SCREEN_HEIGHT - 40f;
            _spriteBatch!.DrawString(_uiFont, footerText,
                new Vector2(footerX, footerY),
                Theme.DimText * FooterAlpha);
        }

        /// <summary>
        /// Called when the scene is activated. Starts the menu background music with a fade-in.
        /// </summary>
        public void OnEnter()
        {
            AudioManager.PlayTrack("menu");
        }

        /// <summary>
        /// Called when the scene is deactivated. The AudioManager crossfade is driven
        /// by the next scene's <see cref="AudioManager.PlayTrack"/> call, so no explicit
        /// stop is required here. However, Stop is safe to call as a safety guard.
        /// </summary>
        public void OnExit()
        {
        }
    }
}
