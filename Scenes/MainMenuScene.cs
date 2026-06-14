using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PongGame.Core;
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
            _uiFont = content.Load<SpriteFont>("Fonts/UIFont");
            _spriteBatch = new SpriteBatch(graphicsDevice);

            // Create 1x1 white pixel texture
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData([Color.White]);

            const int btnWidth = 320;
            const int btnHeight = 64;
            const int btnX = (1280 - btnWidth) / 2;
            const int btnY1 = ((720 - btnHeight) / 2) - 40; // Centered horizontally, vertically centered slightly above middle
            const int btnY2 = btnY1 + 90;                  // 90px below _btn1v1

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
            _btn1v1?.Update(gameTime);
            _btnVsAI?.Update(gameTime);

            if (_btn1v1?.WasClicked is true)
            {
                SceneManager.ChangeScene("game");
            }
        }

        /// <summary>
        /// Draws the main menu layout including title, subtitle, dashed center line, buttons, and footer.
        /// </summary>
        /// <param name="spriteBatch">The default sprite batch (not used directly, we use our local _spriteBatch).</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_spriteBatch == null || _pixel == null || _displayFont == null || _uiFont == null)
            {
                return;
            }

            // 1. Clear background
            _spriteBatch.GraphicsDevice.Clear(Theme.Background);

            _spriteBatch.Begin();

            // 2. Center dashed line (vertical)
            const int dashHeight = GameSettings.CENTER_LINE_DASH_HEIGHT;
            const int gap = GameSettings.CENTER_LINE_GAP;
            const int lineX = (1280 - 3) / 2;
            for (int y = 0; y < 720; y += dashHeight + gap)
            {
                int currentDashHeight = Math.Min(dashHeight, 720 - y);
                if (currentDashHeight > 0)
                {
                    _spriteBatch.Draw(_pixel, new Rectangle(lineX, y, 3, currentDashHeight), Theme.DimText * 0.5f);
                }
            }

            // 3. Title "PONG" with glow layers
            const string titleText = "PONG";
            Vector2 titleSize = _displayFont.MeasureString(titleText);
            float titleX = (1280 - titleSize.X) / 2f;
            const float titleY = 100f;

            // Draw 3 glow layers behind: offset each by 0/1/2px, alpha 60/35/15
            // Offset 2px (Alpha 15%)
            DrawTitleGlow(_spriteBatch, titleText, titleX, titleY, 2, Theme.AccentP1 * 0.15f);
            // Offset 1px (Alpha 35%)
            DrawTitleGlow(_spriteBatch, titleText, titleX, titleY, 1, Theme.AccentP1 * 0.35f);
            // Offset 0px (Alpha 60%)
            _spriteBatch.DrawString(_displayFont, titleText, new Vector2(titleX, titleY), Theme.AccentP1 * 0.60f);

            // Solid Title (Alpha 100%)
            _spriteBatch.DrawString(_displayFont, titleText, new Vector2(titleX, titleY), Theme.AccentP1);

            // 4. Subtitle "MODERN EDITION"
            const string subtitleText = "MODERN  EDITION";
            Vector2 subtitleSize = _uiFont.MeasureString(subtitleText);
            float subtitleX = (1280 - subtitleSize.X) / 2f;
            const float subtitleY = 185f;
            _spriteBatch.DrawString(_uiFont, subtitleText, new Vector2(subtitleX, subtitleY), Theme.UIText * 0.7f);

            // 5. Draw _btn1v1 and _btnVsAI
            _btn1v1?.Draw(_spriteBatch, _pixel);
            _btnVsAI?.Draw(_spriteBatch, _pixel);

            // 6. Footer
            const string footerText = "W / S     Up / Down   to move      ESC to pause";
            Vector2 footerSize = _uiFont.MeasureString(footerText);
            float footerX = (1280 - footerSize.X) / 2f;
            const float footerY = 720f - 40f;
            _spriteBatch.DrawString(_uiFont, footerText, new Vector2(footerX, footerY), Theme.DimText);

            _spriteBatch.End();
        }

        /// <summary>
        /// Called when the scene is activated.
        /// </summary>
        public void OnEnter()
        {
        }

        /// <summary>
        /// Called when the scene is deactivated.
        /// </summary>
        public void OnExit()
        {
        }

        private void DrawTitleGlow(SpriteBatch spriteBatch, string text, float x, float y, float offset, Color color)
        {
            if (_displayFont == null)
            {
                return;
            }

            if (offset == 0)
            {
                spriteBatch.DrawString(_displayFont, text, new Vector2(x, y), color);
            }
            else
            {
                spriteBatch.DrawString(_displayFont, text, new Vector2(x - offset, y), color);
                spriteBatch.DrawString(_displayFont, text, new Vector2(x + offset, y), color);
                spriteBatch.DrawString(_displayFont, text, new Vector2(x, y - offset), color);
                spriteBatch.DrawString(_displayFont, text, new Vector2(x, y + offset), color);
            }
        }
    }
}
