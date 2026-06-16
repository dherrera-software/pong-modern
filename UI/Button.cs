using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using PongGame.Core;

namespace PongGame.UI
{
    /// <summary>
    /// Represents an interactive screen button supporting mouse and touch inputs.
    /// Plays <c>button_hover</c> SFX on hover-enter and <c>button_click</c> SFX on activation.
    /// </summary>
    public class Button(Rectangle bounds, string label, SpriteFont font, Color normalColor, Color hoverColor)
    {
        private readonly string _label = label;
        private readonly SpriteFont _font = font;
        private readonly Color _normalColor = normalColor;
        private readonly Color _hoverColor = hoverColor;
        private bool _isHovered;
        private bool _wasHovered;
        private MouseState _previousMouseState;
        private MouseState _currentMouseState;

        /// <summary>
        /// Gets the bounding rectangle of the button.
        /// </summary>
        public Rectangle Bounds { get; } = bounds;

        /// <summary>
        /// Gets a value indicating whether the button was clicked in the current frame.
        /// </summary>
        public bool WasClicked { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the button is disabled.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Updates the button's input state, tracking mouse and touch gestures.
        /// Plays hover and click sound effects through AudioManager.
        /// </summary>
        /// <param name="gameTime">Provides snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            _ = gameTime;
            WasClicked = false;

            if (IsDisabled)
            {
                return;
            }

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            _wasHovered = _isHovered;
            _isHovered = false;

            // PC check
            Point mousePosition = _currentMouseState.Position;
            if (Bounds.Contains(mousePosition))
            {
                _isHovered = true;
            }

            if (_currentMouseState.LeftButton == ButtonState.Released &&
                _previousMouseState.LeftButton == ButtonState.Pressed &&
                _isHovered)
            {
                WasClicked = true;
            }

            // Touch check
            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation touch in touchCollection)
            {
                if (Bounds.Contains(touch.Position))
                {
                    if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                    {
                        _isHovered = true;
                    }
                    else if (touch.State == TouchLocationState.Released)
                    {
                        WasClicked = true;
                    }
                }
            }

            // ── Sound effects ─────────────────────────────────────────────────
            // Hover: play only on the frame the cursor enters the button area.
            if (_isHovered && !_wasHovered)
            {
                AudioManager.PlaySfx("button_hover");
            }

            // Click: play once per activation.
            if (WasClicked)
            {
                AudioManager.PlaySfx("button_click");
            }
        }

        /// <summary>
        /// Draws the button background and centered text label.
        /// </summary>
        /// <param name="spriteBatch">The active sprite batch for rendering 2D elements.</param>
        /// <param name="pixel">A 1x1 white texture used for colored rectangle rendering.</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (IsDisabled)
            {
                // Draw with DimText color, skip hover effect
                spriteBatch.Draw(pixel, Bounds, Theme.DimText * 0.3f);

                // Draw label centered inside bounds using the font
                Vector2 textSize = _font.MeasureString(_label);
                Vector2 textPosition = new(
                    Bounds.X + ((Bounds.Width - textSize.X) / 2f),
                    Bounds.Y + ((Bounds.Height - textSize.Y) / 2f)
                );
                spriteBatch.DrawString(_font, _label, textPosition, Theme.UIText * 0.5f);
            }
            else
            {
                // Draw filled rectangle in normalColor (or hoverColor if hovered)
                Color rectColor = _isHovered ? _hoverColor : _normalColor;
                spriteBatch.Draw(pixel, Bounds, rectColor);

                // Draw label centered inside bounds using the font
                Vector2 textSize = _font.MeasureString(_label);
                Vector2 textPosition = new(
                    Bounds.X + ((Bounds.Width - textSize.X) / 2f),
                    Bounds.Y + ((Bounds.Height - textSize.Y) / 2f)
                );
                spriteBatch.DrawString(_font, _label, textPosition, Theme.Background);
            }
        }
    }
}
