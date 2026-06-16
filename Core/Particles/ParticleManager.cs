using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PongGame.Core.Particles
{
    /// <summary>
    /// Manages an allocated pool of particles, handling updates, draws, and emission.
    /// Provides zero GC allocation at runtime.
    /// </summary>
    public static class ParticleManager
    {
        private const int MAX_PARTICLES = 2048;
        private static readonly Particle[] Pool = new Particle[MAX_PARTICLES];
        private static int _activeCount = 0;
        private static Texture2D _pixel = null!;
        private static readonly Random Rng = new();

        /// <summary>
        /// Initializes the particle manager with the textures needed for rendering.
        /// </summary>
        /// <param name="pixel">A 1x1 pixel white texture used for drawing solid squares.</param>
        public static void Initialize(Texture2D pixel)
        {
            _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
            _activeCount = 0;
        }

        /// <summary>
        /// Emits a particle burst using the specified emitter settings.
        /// </summary>
        /// <param name="emitter">The emitter settings to use.</param>
        /// <param name="origin">The source position of the burst.</param>
        /// <param name="baseDirectionAngle">The baseline direction angle in radians.</param>
        public static void Emit(ParticleEmitter emitter, Vector2 origin, float baseDirectionAngle)
        {
            if (emitter == null) return;
            emitter.Emit(origin, baseDirectionAngle, Pool, ref _activeCount, MAX_PARTICLES, Rng);
        }

        /// <summary>
        /// Emits a particle burst with direction 0 (ideal for radial or multi-directional emitters).
        /// </summary>
        /// <param name="emitter">The emitter settings to use.</param>
        /// <param name="origin">The source position of the burst.</param>
        public static void Emit(ParticleEmitter emitter, Vector2 origin)
        {
            Emit(emitter, origin, 0f);
        }

        /// <summary>
        /// Emits a particle burst with dynamic color overriding the emitter's base color.
        /// Useful when paddle hits need to match player colors.
        /// </summary>
        /// <param name="emitter">The emitter configuration.</param>
        /// <param name="origin">The source position of the burst.</param>
        /// <param name="baseDirectionAngle">The baseline direction angle in radians.</param>
        /// <param name="overrideColor">The color to apply to this specific burst.</param>
        public static void Emit(ParticleEmitter emitter, Vector2 origin, float baseDirectionAngle, Color overrideColor)
        {
            if (emitter == null) return;
            
            // Temporary override
            Color originalColor = emitter.Color;
            emitter.Color = overrideColor;
            
            emitter.Emit(origin, baseDirectionAngle, Pool, ref _activeCount, MAX_PARTICLES, Rng);
            
            // Restore color
            emitter.Color = originalColor;
        }

        /// <summary>
        /// Updates all active particles: handles movements, lifetime decay, dragging, and sizing.
        /// Swaps dead particles to the end of the active list for O(1) removals.
        /// </summary>
        /// <param name="gameTime">The current game time snapshot.</param>
        public static void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = _activeCount - 1; i >= 0; i--)
            {
                // Decay lifetime
                Pool[i].Lifetime -= deltaTime;

                if (Pool[i].Lifetime <= 0f)
                {
                    // Dead! Swap with the last active particle, decrement active count.
                    _activeCount--;
                    Pool[i] = Pool[_activeCount];
                    continue;
                }

                // Apply drag (velocity decay)
                // Using an exponential decay based on a generic drag coefficient
                Pool[i].Velocity -= Pool[i].Velocity * 2.0f * deltaTime; 

                // Move particle
                Pool[i].Position += Pool[i].Velocity * deltaTime;

                // Rotate particle
                Pool[i].Rotation += Pool[i].RotationSpeed * deltaTime;

                // Shrink particle size
                Pool[i].Size = MathF.Max(0.1f, Pool[i].Size - 2f * deltaTime); // Shrink factor
            }
        }

        /// <summary>
        /// Renders particles on the screen. Designed to be called inside gameplay/alphablend pass.
        /// </summary>
        /// <param name="spriteBatch">The active sprite batch.</param>
        public static void Draw(SpriteBatch spriteBatch)
        {
            Vector2 origin = new(0.5f, 0.5f); // Draw from the center of the 1x1 pixel texture

            for (int i = 0; i < _activeCount; i++)
            {
                float alpha = Pool[i].Lifetime / Pool[i].MaxLifetime;
                Color drawColor = Pool[i].Color * alpha;

                // Solid square centered at the position
                spriteBatch.Draw(
                    _pixel,
                    Pool[i].Position,
                    null,
                    drawColor,
                    Pool[i].Rotation,
                    origin,
                    Pool[i].Size,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        /// <summary>
        /// Renders neon glow layers for particles. Designed to be called inside additive glow pass.
        /// </summary>
        /// <param name="spriteBatch">The active sprite batch.</param>
        public static void DrawGlow(SpriteBatch spriteBatch)
        {
            Vector2 origin = new(0.5f, 0.5f);

            for (int i = 0; i < _activeCount; i++)
            {
                float alpha = (Pool[i].Lifetime / Pool[i].MaxLifetime) * 0.35f; // Softer glow
                Color glowColor = Pool[i].Color * alpha;

                // Draw an outer glowing layer (larger size)
                spriteBatch.Draw(
                    _pixel,
                    Pool[i].Position,
                    null,
                    glowColor,
                    Pool[i].Rotation,
                    origin,
                    Pool[i].Size * 3.5f, // Expanded bounds for glow
                    SpriteEffects.None,
                    0f
                );
            }
        }

        /// <summary>
        /// Clears all active particles.
        /// </summary>
        public static void Clear()
        {
            _activeCount = 0;
        }
    }
}
