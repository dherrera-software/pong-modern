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
        private SpriteFont? _subtitleFont;
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
        private const float FOOTER_MIN  = 0.30f;
        private const float FOOTER_MAX  = 0.55f;

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
            _displayFont  = content.Load<SpriteFont>("Fonts/DisplayFont");
            _uiFont       = content.Load<SpriteFont>("Fonts/UIFont");
            _subtitleFont = content.Load<SpriteFont>("Fonts/SubtitleFont");
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _renderer    = new RenderLayerManager(_spriteBatch);
            _background  = new AnimatedBackground(75);

            // Create 1x1 white pixel texture
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData([Color.White]);

            const int btnWidth = 320;
            const int btnHeight = 64;
            const int btnX  = (1280 - btnWidth) / 2;
            const int btnY1 = ((720 - btnHeight) / 2) - 80;
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

            if (TransitionManager.IsTransitioning)
            {
                return;
            }

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
                TransitionManager.StartTransition("game");
            }
        }

        /// <summary>
        /// Draws the main menu layout including animated background, title glow, and UI elements.
        /// </summary>
        /// <param name="spriteBatch">The default sprite batch (not used directly, we use our local _spriteBatch).</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_spriteBatch == null || _pixel == null || _displayFont == null
                || _uiFont == null || _subtitleFont == null || _renderer == null || _background == null)
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
            Vector2 subtitleSize = _subtitleFont!.MeasureString(subtitleText);
            float subtitleX = (GameSettings.SCREEN_WIDTH - subtitleSize.X) / 2f;
            const float subtitleY = 185f;
            _spriteBatch!.DrawString(_subtitleFont, subtitleText,
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

            // Helper to draw a bordered rectangle
            void DrawBorderedRect(Rectangle rect, Color borderColor, int borderThickness)
            {
                // Top
                _spriteBatch!.Draw(_pixel!, new Rectangle(rect.X, rect.Y, rect.Width, borderThickness), borderColor);
                // Bottom
                _spriteBatch!.Draw(_pixel!, new Rectangle(rect.X, rect.Y + rect.Height - borderThickness, rect.Width, borderThickness), borderColor);
                // Left
                _spriteBatch!.Draw(_pixel!, new Rectangle(rect.X, rect.Y + borderThickness, borderThickness, rect.Height - (2 * borderThickness)), borderColor);
                // Right
                _spriteBatch!.Draw(_pixel!, new Rectangle((rect.X + rect.Width) - borderThickness, rect.Y + borderThickness, borderThickness, rect.Height - (2 * borderThickness)), borderColor);
            }

            // ── Draw disabled VS AI button (no animation) ────────────────────
            if (_btnVsAI != null)
            {
                Rectangle b = _btnVsAI.Bounds;
                Rectangle displaced = new(b.X, (int)(b.Y + floatOff), b.Width, b.Height);

                // No background fill, only 1px border with Theme.DimText * 0.3f
                DrawBorderedRect(displaced, Theme.DimText * 0.3f, 1);

                // Label principal: solo "VS IA", centrado, color Theme.UIText * 0.35f
                const string mainLabel = "VS IA";
                Vector2 textSize = _uiFont!.MeasureString(mainLabel);
                Vector2 textPos  = new(
                    displaced.X + ((displaced.Width  - textSize.X) / 2f),
                    displaced.Y + ((displaced.Height - textSize.Y) / 2f));
                _spriteBatch!.DrawString(_uiFont, mainLabel, textPos, Theme.UIText * 0.35f);

                // Badge "PROXIMAMENTE" — dentro del botón, borde derecho
                const string badgeText = "PROXIMAMENTE";
                const float badgeScale = 0.55f;
                Vector2 originalBadgeSize = _uiFont!.MeasureString(badgeText);
                Vector2 badgeSize = originalBadgeSize * badgeScale;

                float badgePadX = 5f;
                float badgePadY = 2f;
                float badgeW = badgeSize.X + badgePadX * 2f;
                float badgeH = badgeSize.Y + badgePadY * 2f;

                // Alineado a la derecha con margen interior de 8px, centrado verticalmente
                float badgeX = displaced.X + displaced.Width - badgeW - 8f;
                float badgeY = displaced.Y + (displaced.Height - badgeH) / 2f;
                Rectangle badgeRect = new Rectangle((int)badgeX, (int)badgeY, (int)badgeW, (int)badgeH);

                // Fondo del badge
                _spriteBatch!.Draw(_pixel!, badgeRect, Theme.DimText * 0.12f);

                // Borde del badge
                DrawBorderedRect(badgeRect, Theme.DimText * 0.30f, 1);

                // Texto del badge centrado
                Vector2 badgeTextPos = new Vector2(
                    badgeRect.X + (badgeRect.Width  - badgeSize.X) / 2f,
                    badgeRect.Y + (badgeRect.Height - badgeSize.Y) / 2f
                );
                _spriteBatch!.DrawString(_uiFont, badgeText, badgeTextPos,
                    Theme.DimText * 0.70f, 0f, Vector2.Zero, badgeScale, SpriteEffects.None, 0f);
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

                float hoverT    = (scale - BTN_SCALE_NORMAL) / (BTN_SCALE_HOVER - BTN_SCALE_NORMAL);

                // Draw background only on hover with Theme.AccentP1 * 0.12f * hoverT
                if (hoverT > 0f)
                {
                    _spriteBatch!.Draw(_pixel!, dest, Theme.AccentP1 * (0.12f * hoverT));
                }

                // Draw border: 1px en reposo, 2px en hover activo
                int borderThickness = hoverT > 0.5f ? 2 : 1;
                DrawBorderedRect(dest, Theme.AccentP1, borderThickness);

                // Draw label: color interpolado entre UIText (reposo) y AccentP1 (hover)
                Color labelColor = Color.Lerp(Theme.UIText, Theme.AccentP1, hoverT);
                const string label = "1 VS 1";
                Vector2 textSize   = _uiFont!.MeasureString(label);
                Vector2 textPos    = new(
                    dest.X + ((dest.Width  - textSize.X) / 2f),
                    dest.Y + ((dest.Height - textSize.Y) / 2f));
                _spriteBatch!.DrawString(_uiFont, label, textPos, labelColor);

                // Agregar un triángulo/flecha "▶" a la izquierda del label, visible solo en hover
                if (_btnScale[0] > BTN_SCALE_NORMAL + 0.001f)
                {
                    // Draw custom triangle to the left of the label
                    const float arrowW = 5f;
                    const float arrowH = 8f;
                    float arrowX = textPos.X - arrowW - 8f;
                    float arrowY = dest.Y + ((dest.Height - arrowH) / 2f);

                    for (int col = 0; col < (int)arrowW; col++)
                    {
                        int sliceH = (int)arrowH - (col * 2);
                        if (sliceH <= 0) break;
                        int sliceY = (int)arrowY + col;
                        _spriteBatch!.Draw(_pixel!, new Rectangle((int)arrowX + col, sliceY, 1, sliceH), Theme.AccentP1);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the footer instruction text with a slow breathing opacity.
        /// </summary>
        private void DrawFooter()
        {
            float alpha = FooterAlpha;

            // Línea separadora sutil encima del footer
            const float separatorY = GameSettings.SCREEN_HEIGHT - 55f;
            const int separatorW = 320;
            int separatorX = (GameSettings.SCREEN_WIDTH - separatorW) / 2;
            _spriteBatch!.Draw(_pixel!,
                new Rectangle(separatorX, (int)separatorY, separatorW, 1),
                Theme.DimText * 0.15f * alpha);
            Color chipBorderColor = Theme.DimText * 0.4f * alpha;
            Color textColor = Theme.DimText * alpha;
            Color separatorColor = Theme.DimText * 0.2f * alpha;

            const float paddingX = 6f;
            const float paddingY = 3f;
            const float keySpacing = 4f;
            const float labelSpacing = 6f;
            const float groupSpacing = 16f;
            const float dotSize = 3f;

            const string labelMover = "mover";
            const string labelPausa = "pausa";

            float fontHeight = _uiFont!.MeasureString("W").Y;
            float chipHeight = fontHeight + (paddingY * 2f);
            const float footerY = GameSettings.SCREEN_HEIGHT - 40f;
            float centerY = footerY + (fontHeight / 2f);

            // Measure widths
            float wW = _uiFont.MeasureString("W").X + (paddingX * 2f);
            float wS = _uiFont.MeasureString("S").X + (paddingX * 2f);
            float wMover1 = _uiFont.MeasureString(labelMover).X;

            float wUp = _uiFont.MeasureString("^").X + (paddingX * 2f);
            float wDown = _uiFont.MeasureString("v").X + (paddingX * 2f);
            float wMover2 = _uiFont.MeasureString(labelMover).X;

            float wEsc = _uiFont.MeasureString("ESC").X + (paddingX * 2f);
            float wPausa = _uiFont.MeasureString(labelPausa).X;

            float totalWidth =
                wW + keySpacing + wS + labelSpacing + dotSize + labelSpacing + wMover1 +
                groupSpacing + dotSize + groupSpacing +
                wUp + keySpacing + wDown + labelSpacing + dotSize + labelSpacing + wMover2 +
                groupSpacing + dotSize + groupSpacing +
                wEsc + labelSpacing + dotSize + labelSpacing + wPausa;

            float cx = (GameSettings.SCREEN_WIDTH - totalWidth) / 2f;

            void DrawBorderedRect(Rectangle rect, Color borderColor, int borderThickness)
            {
                // Top
                _spriteBatch!.Draw(_pixel!, new Rectangle(rect.X, rect.Y, rect.Width, borderThickness), borderColor);
                // Bottom
                _spriteBatch!.Draw(_pixel!, new Rectangle(rect.X, rect.Y + rect.Height - borderThickness, rect.Width, borderThickness), borderColor);
                // Left
                _spriteBatch!.Draw(_pixel!, new Rectangle(rect.X, rect.Y + borderThickness, borderThickness, rect.Height - (2 * borderThickness)), borderColor);
                // Right
                _spriteBatch!.Draw(_pixel!, new Rectangle((rect.X + rect.Width) - borderThickness, rect.Y + borderThickness, borderThickness, rect.Height - (2 * borderThickness)), borderColor);
            }

            void DrawChip(string key)
            {
                Vector2 sz = _uiFont.MeasureString(key);
                float cw = sz.X + (paddingX * 2f);
                float cy = centerY - (chipHeight / 2f);

                Rectangle rect = new((int)cx, (int)cy, (int)cw, (int)chipHeight);
                DrawBorderedRect(rect, chipBorderColor, 1);

                Vector2 textPos = new(
                    rect.X + ((rect.Width - sz.X) / 2f),
                    rect.Y + ((rect.Height - sz.Y) / 2f)
                );
                _spriteBatch!.DrawString(_uiFont, key, textPos, textColor);

                cx += cw;
            }

            void DrawText(string text, Color color)
            {
                Vector2 sz = _uiFont.MeasureString(text);
                float ty = centerY - (sz.Y / 2f);
                _spriteBatch!.DrawString(_uiFont, text, new Vector2(cx, ty), color);
                cx += sz.X;
            }

            void DrawDot(Color color)
            {
                float dy = centerY - (dotSize / 2f);
                _spriteBatch!.Draw(_pixel!, new Rectangle((int)cx, (int)dy, (int)dotSize, (int)dotSize), color);
                cx += dotSize;
            }

            // Group 1
            DrawChip("W");
            cx += keySpacing;
            DrawChip("S");
            cx += labelSpacing;
            DrawDot(textColor);
            cx += labelSpacing;
            DrawText(labelMover, textColor);

            // Separator 1
            cx += groupSpacing;
            DrawDot(separatorColor);
            cx += groupSpacing;

            // Group 2
            DrawChip("^");
            cx += keySpacing;
            DrawChip("v");
            cx += labelSpacing;
            DrawDot(textColor);
            cx += labelSpacing;
            DrawText(labelMover, textColor);

            // Separator 2
            cx += groupSpacing;
            DrawDot(separatorColor);
            cx += groupSpacing;

            // Group 3
            DrawChip("ESC");
            cx += labelSpacing;
            DrawDot(textColor);
            cx += labelSpacing;
            DrawText(labelPausa, textColor);
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
