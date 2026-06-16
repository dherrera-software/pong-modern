using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using PongGame.Core;
using PongGame.Core.Rendering;
using System;

namespace PongGame.UI
{
    /// <summary>
    /// An interactive slider UI component for adjusting float values (e.g., volumes) between 0.0 and 1.0.
    /// Supports mouse dragging and touch inputs, with customizable labels and neon styling.
    /// </summary>
    public class Slider
    {
        private readonly Rectangle _bounds; // bounds of the track itself
        private readonly string _label;
        private readonly SpriteFont _font;
        private readonly Color _themeColor;
        private float _value;
        private bool _isDragging;
        private bool _isHovered;
        private bool _wasHovered;
        private MouseState _previousMouseState;
        private MouseState _currentMouseState;

        // Visual constants
        private const int HANDLE_WIDTH = 14;
        private const int HANDLE_HEIGHT = 26;
        private const int TRACK_HEIGHT = 4;

        /// <summary>
        /// Gets or sets the slider value (clamped between 0.0 and 1.0).
        /// </summary>
        public float Value
        {
            get => _value;
            set => _value = MathHelper.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// Event fired when the value changes.
        /// </summary>
        public event Action<float>? OnValueChanged;

        public Slider(Rectangle bounds, string label, SpriteFont font, Color themeColor, float initialValue)
        {
            _bounds = bounds;
            _label = label;
            _font = font;
            _themeColor = themeColor;
            _value = MathHelper.Clamp(initialValue, 0f, 1f);
        }

        /// <summary>
        /// Updates the slider interaction state.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            _currentMouseState = Mouse.GetState();

            _wasHovered = _isHovered;
            _isHovered = false;

            // Define the interaction area (track area + padding for ease of clicking/dragging)
            Rectangle interactionBounds = new(
                _bounds.X - 10,
                _bounds.Y - 15,
                _bounds.Width + 20,
                _bounds.Height + 30
            );

            Point mousePos = _currentMouseState.Position;
            if (interactionBounds.Contains(mousePos))
            {
                _isHovered = true;
            }

            // Mouse input
            if (_isHovered && _currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                _isDragging = true;
                AudioManager.PlaySfx("button_click");
            }

            if (_currentMouseState.LeftButton == ButtonState.Released)
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                UpdateValueFromX(mousePos.X);
            }

            // Touch input
            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation touch in touchCollection)
            {
                if (interactionBounds.Contains(touch.Position))
                {
                    _isHovered = true;

                    if (touch.State == TouchLocationState.Pressed)
                    {
                        _isDragging = true;
                        AudioManager.PlaySfx("button_click");
                    }

                    if (_isDragging && (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved))
                    {
                        UpdateValueFromX((int)touch.Position.X);
                    }
                }

                if (touch.State == TouchLocationState.Released)
                {
                    _isDragging = false;
                }
            }

            // Audio feedback for hover transitions
            if (_isHovered && !_wasHovered)
            {
                AudioManager.PlaySfx("button_hover");
            }

            _previousMouseState = _currentMouseState;
        }

        private void UpdateValueFromX(int mouseX)
        {
            float relativeX = mouseX - _bounds.X;
            float newValue = relativeX / _bounds.Width;
            float clamped = MathHelper.Clamp(newValue, 0f, 1f);

            if (MathF.Abs(_value - clamped) > 0.001f)
            {
                _value = clamped;
                OnValueChanged?.Invoke(_value);
            }
        }

        private Rectangle GetHandleRect()
        {
            int handleX = _bounds.X + (int)(_value * _bounds.Width);
            int handleY = _bounds.Y + (_bounds.Height / 2);
            return new Rectangle(handleX - (HANDLE_WIDTH / 2), handleY - (HANDLE_HEIGHT / 2), HANDLE_WIDTH, HANDLE_HEIGHT);
        }

        /// <summary>
        /// Draws the glow layer for the slider track fill and handle.
        /// </summary>
        public void DrawGlow(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Highlight fill track glow
            int fillWidth = (int)(_value * _bounds.Width);
            if (fillWidth > 0)
            {
                Rectangle fillRect = new(_bounds.X, _bounds.Y + (_bounds.Height / 2) - (TRACK_HEIGHT / 2), fillWidth, TRACK_HEIGHT);
                GlowRenderer.DrawRectGlow(spriteBatch, pixel, fillRect, _themeColor * 0.4f);
            }

            // Handle glow
            Rectangle handleRect = GetHandleRect();
            GlowRenderer.DrawRectGlow(spriteBatch, pixel, handleRect, _themeColor * 0.6f);
        }

        /// <summary>
        /// Draws the solid components of the slider (track, fill, handle, and labels).
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Draw background track (dim color)
            Rectangle trackRect = new(_bounds.X, _bounds.Y + (_bounds.Height / 2) - (TRACK_HEIGHT / 2), _bounds.Width, TRACK_HEIGHT);
            spriteBatch.Draw(pixel, trackRect, Theme.DimText * 0.4f);

            // Draw filled portion of the track
            int fillWidth = (int)(_value * _bounds.Width);
            if (fillWidth > 0)
            {
                Rectangle fillRect = new(_bounds.X, _bounds.Y + (_bounds.Height / 2) - (TRACK_HEIGHT / 2), fillWidth, TRACK_HEIGHT);
                spriteBatch.Draw(pixel, fillRect, _themeColor);
            }

            // Draw handle
            Rectangle handleRect = GetHandleRect();
            Color handleColor = _isHovered || _isDragging ? Color.White : _themeColor;
            spriteBatch.Draw(pixel, handleRect, handleColor);

            // Draw Slider Labels
            // Draw text label above track
            Vector2 labelPos = new(_bounds.X, _bounds.Y - 26);
            spriteBatch.DrawString(_font, _label, labelPos, Theme.UIText);

            // Draw percentage value next to the label on the right
            string percentText = $"{(int)Math.Round(_value * 100)}%";
            Vector2 percentSize = _font.MeasureString(percentText);
            Vector2 percentPos = new(_bounds.Right - percentSize.X, _bounds.Y - 26);
            spriteBatch.DrawString(_font, percentText, percentPos, _themeColor);
        }
    }
}
