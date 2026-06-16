using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PongGame.Core;
using PongGame.Core.Rendering;
using System;

namespace PongGame.Entities
{
    /// <summary>
    /// Manages and renders a modern, frame-rate independent neon trail effect for the ball
    /// using a highly efficient circular buffer to avoid heap or GC allocations.
    /// </summary>
    public class BallTrail
    {
        private struct TrailPoint
        {
            public Vector2 Position;
            public float Lifetime; // Remaining lifetime in seconds
            public bool IsActive;
        }

        private readonly TrailPoint[] _points;
        private int _head;
        private int _count;
        private float _sampleTimer;

        // Configurable properties initialized from global GameSettings
        public float SampleInterval { get; set; } = GameSettings.TRAIL_SAMPLE_INTERVAL;
        public float PointLifetime { get; set; } = GameSettings.TRAIL_POINT_LIFETIME;
        public float BaseOpacity { get; set; } = GameSettings.TRAIL_BASE_OPACITY;
        public float SizeDecay { get; set; } = GameSettings.TRAIL_SIZE_DECAY;
        public float GlowIntensity { get; set; } = GameSettings.TRAIL_GLOW_INTENSITY;

        public BallTrail()
        {
            _points = new TrailPoint[GameSettings.TRAIL_MAX_POINTS];
            Reset();
        }

        /// <summary>
        /// Resets the trail buffer, clearing all active points.
        /// </summary>
        public void Reset()
        {
            _head = -1;
            _count = 0;
            _sampleTimer = 0f;
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = new TrailPoint { IsActive = false };
            }
        }

        /// <summary>
        /// Updates trail point lifetimes and samples the ball's position at regular time intervals.
        /// </summary>
        public void Update(GameTime gameTime, Vector2 ballPosition, bool isBallActive)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Decay lifetimes of all active points
            for (int i = 0; i < _points.Length; i++)
            {
                if (_points[i].IsActive)
                {
                    _points[i].Lifetime -= deltaTime;
                    if (_points[i].Lifetime <= 0f)
                    {
                        _points[i].IsActive = false;
                    }
                }
            }

            if (!isBallActive)
            {
                return;
            }

            // Periodic sampling
            _sampleTimer += deltaTime;
            if (_sampleTimer >= SampleInterval)
            {
                _sampleTimer = 0f;

                // Move head to next position
                _head = (_head + 1) % _points.Length;

                _points[_head] = new TrailPoint
                {
                    Position = ballPosition,
                    Lifetime = PointLifetime,
                    IsActive = true
                };

                if (_count < _points.Length)
                {
                    _count++;
                }
            }
        }

        /// <summary>
        /// Renders the solid trail segments behind the ball.
        /// Older segments are drawn first so that newer, stronger segments render on top.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel, Color color, float baseSize)
        {
            if (_count == 0) return;

            int oldestIndex = (_head - _count + 1 + _points.Length) % _points.Length;

            for (int step = 0; step < _count; step++)
            {
                int index = (oldestIndex + step) % _points.Length;
                ref readonly var point = ref _points[index];

                if (!point.IsActive) continue;

                // Calculate fade and size decay based on normalized lifetime
                float lifeRatio = Math.Clamp(point.Lifetime / PointLifetime, 0f, 1f);
                float opacity = lifeRatio * BaseOpacity;
                float size = baseSize * MathF.Max(0.1f, MathF.Pow(lifeRatio, SizeDecay));

                Rectangle rect = new(
                    (int)MathF.Round(point.Position.X - (size / 2f)),
                    (int)MathF.Round(point.Position.Y - (size / 2f)),
                    (int)MathF.Round(size),
                    (int)MathF.Round(size)
                );

                spriteBatch.Draw(pixel, rect, color * opacity);
            }
        }

        /// <summary>
        /// Renders a subtle glow for each trail segment, integrated with the glow layer system.
        /// </summary>
        public void DrawGlow(SpriteBatch spriteBatch, Texture2D pixel, Color color, float baseSize)
        {
            if (_count == 0) return;

            int oldestIndex = (_head - _count + 1 + _points.Length) % _points.Length;

            for (int step = 0; step < _count; step++)
            {
                int index = (oldestIndex + step) % _points.Length;
                ref readonly var point = ref _points[index];

                if (!point.IsActive) continue;

                // Calculate fade and size decay based on normalized lifetime
                float lifeRatio = Math.Clamp(point.Lifetime / PointLifetime, 0f, 1f);
                float opacity = lifeRatio * BaseOpacity * GlowIntensity;
                float size = baseSize * MathF.Max(0.1f, MathF.Pow(lifeRatio, SizeDecay));

                Rectangle rect = new(
                    (int)MathF.Round(point.Position.X - (size / 2f)),
                    (int)MathF.Round(point.Position.Y - (size / 2f)),
                    (int)MathF.Round(size),
                    (int)MathF.Round(size)
                );

                // Use GlowRenderer to draw the neon glow around each trail segment
                GlowRenderer.DrawRectGlow(spriteBatch, pixel, rect, color * opacity);
            }
        }
    }
}
