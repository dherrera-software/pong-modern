using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PongGame.Core
{
    /// <summary>
    /// Defines the supported transition types.
    /// </summary>
    public enum TransitionType
    {
        Fade
    }

    /// <summary>
    /// Centralized transition manager for smooth and safe full-screen transitions between scenes.
    /// </summary>
    public static class TransitionManager
    {
        private static Texture2D? _pixel;
        private static string? _targetSceneKey;
        private static float _durationMs;
        private static float _elapsedTimeMs;
        private static TransitionType _type;
        private static bool _isTransitioning;

        /// <summary>
        /// Gets whether a transition is currently in progress.
        /// </summary>
        public static bool IsTransitioning => _isTransitioning;

        /// <summary>
        /// Initializes the TransitionManager with a GraphicsDevice context.
        /// </summary>
        /// <param name="graphicsDevice">The active graphics device.</param>
        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        /// <summary>
        /// Requests a transition to a new scene. Duplicate requests are ignored while a transition is active.
        /// </summary>
        /// <param name="targetSceneKey">The key of the scene to transition to.</param>
        /// <param name="durationMs">The total duration of the transition in milliseconds.</param>
        /// <param name="type">The type of transition effect to apply.</param>
        public static void StartTransition(string targetSceneKey, float durationMs = 600f, TransitionType type = TransitionType.Fade)
        {
            if (_isTransitioning)
            {
                return;
            }

            _targetSceneKey = targetSceneKey;
            _durationMs = durationMs;
            _elapsedTimeMs = 0f;
            _type = type;
            _isTransitioning = true;
        }

        /// <summary>
        /// Updates the active transition progress and triggers scene switching at the midpoint.
        /// </summary>
        /// <param name="gameTime">Provides snapshot of timing values.</param>
        public static void Update(GameTime gameTime)
        {
            if (!_isTransitioning)
            {
                return;
            }

            float dtMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            float halfDuration = _durationMs / 2f;
            float previousElapsed = _elapsedTimeMs;
            _elapsedTimeMs += dtMs;

            // Trigger scene switch exactly at the midpoint (transitioning from Fade Out to Fade In)
            if (previousElapsed < halfDuration && _elapsedTimeMs >= halfDuration && _targetSceneKey != null)
            {
                SceneManager.ChangeScene(_targetSceneKey);
                _targetSceneKey = null; // Scene changed, clear target
            }

            // End transition
            if (_elapsedTimeMs >= _durationMs)
            {
                _isTransitioning = false;
                _targetSceneKey = null;
            }
        }

        /// <summary>
        /// Draws the transition overlay on top of the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used to render 2D graphics.</param>
        public static void Draw(SpriteBatch spriteBatch)
        {
            if (!_isTransitioning || _pixel == null)
            {
                return;
            }

            float opacity = 0f;
            float halfDuration = _durationMs / 2f;

            if (_elapsedTimeMs < halfDuration)
            {
                // Fade out (0 to 1 opacity)
                opacity = _elapsedTimeMs / halfDuration;
            }
            else
            {
                // Fade in (1 to 0 opacity)
                opacity = 1f - ((_elapsedTimeMs - halfDuration) / halfDuration);
            }

            opacity = MathHelper.Clamp(opacity, 0f, 1f);

            // Draw the effect based on transition type
            switch (_type)
            {
                case TransitionType.Fade:
                default:
                    spriteBatch.Begin();
                    spriteBatch.Draw(
                        _pixel,
                        new Rectangle(0, 0, GameSettings.SCREEN_WIDTH, GameSettings.SCREEN_HEIGHT),
                        Color.Black * opacity
                    );
                    spriteBatch.End();
                    break;
            }
        }
    }
}
