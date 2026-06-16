using System;
using Microsoft.Xna.Framework;

namespace PongGame.Core.Particles
{
    /// <summary>
    /// Configures and controls a burst of particles emitted from a source.
    /// Emitters can be customized, and instances can be reused.
    /// </summary>
    public class ParticleEmitter
    {
        // Reusable static presets to avoid allocation overhead during gameplay events.
        public static readonly ParticleEmitter PaddleHit = CreatePaddleHitEmitter();
        public static readonly ParticleEmitter WallBounce = CreateWallBounceEmitter();
        public static readonly ParticleEmitter Score = CreateScoreEmitter();

        public int MinParticles { get; set; }
        public int MaxParticles { get; set; }
        public float SpeedMin { get; set; }
        public float SpeedMax { get; set; }
        public float LifetimeMin { get; set; }
        public float LifetimeMax { get; set; }
        public float SizeMin { get; set; }
        public float SizeMax { get; set; }
        public Color Color { get; set; }
        public float SpreadAngle { get; set; }
        public float RotationSpeedMax { get; set; }
        public float Drag { get; set; }
        public float ShrinkRate { get; set; }

        /// <summary>
        /// Emits particles into the provided particle pool.
        /// </summary>
        /// <param name="origin">The source position of the burst.</param>
        /// <param name="baseDirectionAngle">The central direction angle in radians.</param>
        /// <param name="pool">The particle pool array.</param>
        /// <param name="activeCount">Reference to the current active particle count.</param>
        /// <param name="maxCapacity">The total capacity of the particle pool.</param>
        /// <param name="rng">Random number generator.</param>
        public void Emit(Vector2 origin, float baseDirectionAngle, Particle[] pool, ref int activeCount, int maxCapacity, Random rng)
        {
            int count = rng.Next(MinParticles, MaxParticles + 1);

            for (int i = 0; i < count; i++)
            {
                if (activeCount >= maxCapacity)
                {
                    break; // Pool is full
                }

                // Calculate random angle within the spread cone
                float angle = baseDirectionAngle + (float)((rng.NextDouble() - 0.5) * SpreadAngle);
                float speed = SpeedMin + (float)(rng.NextDouble() * (SpeedMax - SpeedMin));

                Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                float lifetime = LifetimeMin + (float)(rng.NextDouble() * (LifetimeMax - LifetimeMin));
                float size = SizeMin + (float)(rng.NextDouble() * (SizeMax - SizeMin));
                float rotSpeed = (float)((rng.NextDouble() - 0.5) * 2.0 * RotationSpeedMax);

                pool[activeCount] = new Particle
                {
                    Position = origin,
                    Velocity = velocity,
                    Color = Color,
                    Lifetime = lifetime,
                    MaxLifetime = lifetime,
                    Size = size,
                    Rotation = (float)(rng.NextDouble() * MathF.PI * 2.0),
                    RotationSpeed = rotSpeed,
                    IsAlive = true
                };

                activeCount++;
            }
        }

        private static ParticleEmitter CreatePaddleHitEmitter()
        {
            return new ParticleEmitter
            {
                MinParticles = 20,
                MaxParticles = 30,
                SpeedMin = 150f,
                SpeedMax = 450f,
                LifetimeMin = 0.3f,
                LifetimeMax = 0.7f,
                SizeMin = 3f,
                SizeMax = 7f,
                Color = Theme.AccentP1, // Will be dynamically matched or set to P1 (cyan) by default
                SpreadAngle = MathHelper.ToRadians(80f),
                RotationSpeedMax = 5f,
                Drag = 1.5f,
                ShrinkRate = 4f
            };
        }

        private static ParticleEmitter CreateWallBounceEmitter()
        {
            return new ParticleEmitter
            {
                MinParticles = 8,
                MaxParticles = 14,
                SpeedMin = 80f,
                SpeedMax = 220f,
                LifetimeMin = 0.2f,
                LifetimeMax = 0.5f,
                SizeMin = 2f,
                SizeMax = 5f,
                Color = new Color(0, 180, 200, 200), // Dimmed cyan
                SpreadAngle = MathHelper.ToRadians(120f),
                RotationSpeedMax = 3f,
                Drag = 2.0f,
                ShrinkRate = 3f
            };
        }

        private static ParticleEmitter CreateScoreEmitter()
        {
            return new ParticleEmitter
            {
                MinParticles = 40,
                MaxParticles = 60,
                SpeedMin = 100f,
                SpeedMax = 500f,
                LifetimeMin = 0.4f,
                LifetimeMax = 0.9f,
                SizeMin = 3f,
                SizeMax = 8f,
                Color = Color.White,
                SpreadAngle = MathF.PI * 2f, // Full circle
                RotationSpeedMax = 8f,
                Drag = 1.0f,
                ShrinkRate = 5f
            };
        }
    }
}
