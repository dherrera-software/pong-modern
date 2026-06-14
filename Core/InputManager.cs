using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace PongGame.Core
{
    /// <summary>
    /// Represents the type of input device currently being used.
    /// </summary>
    public enum InputDevice
    {
        /// <summary>
        /// Keyboard input device.
        /// </summary>
        Keyboard,

        /// <summary>
        /// Touch screen input device.
        /// </summary>
        Touch
    }

    /// <summary>
    /// Abstracts keyboard and touch input for both players in the Pong game.
    /// </summary>
    public static class InputManager
    {
        private static KeyboardState _previousKeyboardState;
        private static KeyboardState _currentKeyboardState;
        private static TouchCollection _previousTouchCollection;
        private static TouchCollection _currentTouchCollection;

        /// <summary>
        /// Gets the current active input device.
        /// </summary>
        public static InputDevice CurrentDevice { get; private set; } = InputDevice.Keyboard;

        /// <summary>
        /// Captures the current keyboard and touch states, stores the previous states,
        /// and automatically detects the active input device.
        /// </summary>
        public static void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            _previousTouchCollection = _currentTouchCollection;
            _currentTouchCollection = TouchPanel.GetState();

            // Auto-detect input device
            if (_currentKeyboardState.GetPressedKeyCount() > 0)
            {
                CurrentDevice = InputDevice.Keyboard;
            }

            if (_currentTouchCollection.Count > 0)
            {
                CurrentDevice = InputDevice.Touch;
            }
        }

        /// <summary>
        /// Checks if the move up input is active for the specified player.
        /// </summary>
        /// <param name="playerIndex">The player index (1 for left paddle, 2 for right paddle).</param>
        /// <returns>True if the move up command is active; otherwise, false.</returns>
        public static bool IsMoveUp(int playerIndex)
        {
            // Keyboard check
            if (playerIndex == 1 && _currentKeyboardState.IsKeyDown(Keys.W))
            {
                return true;
            }
            if (playerIndex == 2 && _currentKeyboardState.IsKeyDown(Keys.Up))
            {
                return true;
            }

            // Touch check
            const float halfWidth = GameSettings.SCREEN_WIDTH / 2f;
            const float halfHeight = GameSettings.SCREEN_HEIGHT / 2f;

            foreach (var touch in _currentTouchCollection)
            {
                if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                {
                    float x = touch.Position.X;
                    float y = touch.Position.Y;

                    if (playerIndex == 1)
                    {
                        // Player 1: Left half, upper zone
                        if (x >= 0 && x < halfWidth && y >= 0 && y < halfHeight)
                        {
                            return true;
                        }
                    }
                    else if (playerIndex == 2)
                    {
                        // Player 2: Right half, upper zone
                        if (x >= halfWidth && x <= GameSettings.SCREEN_WIDTH && y >= 0 && y < halfHeight)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the move down input is active for the specified player.
        /// </summary>
        /// <param name="playerIndex">The player index (1 for left paddle, 2 for right paddle).</param>
        /// <returns>True if the move down command is active; otherwise, false.</returns>
        public static bool IsMoveDown(int playerIndex)
        {
            // Keyboard check
            if (playerIndex == 1 && _currentKeyboardState.IsKeyDown(Keys.S))
            {
                return true;
            }
            if (playerIndex == 2 && _currentKeyboardState.IsKeyDown(Keys.Down))
            {
                return true;
            }

            // Touch check
            const float halfWidth = GameSettings.SCREEN_WIDTH / 2f;
            const float halfHeight = GameSettings.SCREEN_HEIGHT / 2f;

            foreach (var touch in _currentTouchCollection)
            {
                if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                {
                    float x = touch.Position.X;
                    float y = touch.Position.Y;

                    if (playerIndex == 1)
                    {
                        // Player 1: Left half, lower zone
                        if (x >= 0 && x < halfWidth && y >= halfHeight && y <= GameSettings.SCREEN_HEIGHT)
                        {
                            return true;
                        }
                    }
                    else if (playerIndex == 2)
                    {
                        // Player 2: Right half, lower zone
                        if (x >= halfWidth && x <= GameSettings.SCREEN_WIDTH && y >= halfHeight && y <= GameSettings.SCREEN_HEIGHT)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the pause input is triggered (rising edge for Escape key).
        /// </summary>
        /// <returns>True if pause is pressed on this frame; otherwise, false.</returns>
        public static bool IsPausePressed()
        {
            // Keyboard: Escape key (rising edge — only true on the frame it's pressed)
            if (_currentKeyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                return true;
            }

            // Touch: not implemented yet
            return false;
        }
    }
}
