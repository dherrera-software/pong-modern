using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PongGame.Core.Rendering
{
    /// <summary>
    /// Renders a field of drifting ambient particles as a background layer.
    /// </summary>
    public sealed class AnimatedBackground
    {
        private struct Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public float Alpha;
            public Color Color;
        }

        private readonly Particle[] _particles;
        private readonly Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedBackground"/> class.
        /// </summary>
        /// <param name="particleCount">The number of ambient particles to simulate.</param>
        public AnimatedBackground(int particleCount = 150)
        {
            _random = new Random();
            _particles = new Particle[particleCount];

            for (int i = 0; i < _particles.Length; i++)
            {
                ResetParticle(i, randomizeY: true);
            }
        }

        /// <summary>
        /// Resets a particle to a new random state.
        /// </summary>
        /// <param name="index">The index of the particle to reset.</param>
        /// <param name="randomizeY">
        /// If true, places the particle at a random vertical position (initial scatter).
        /// If false, spawns the particle just above the screen (respawn after exit).
        /// </param>
        private void ResetParticle(int index, bool randomizeY = false)
        {
            ref Particle p = ref _particles[index];

            p.Position.X = (float)(_random.NextDouble() * GameSettings.SCREEN_WIDTH);
            p.Position.Y = randomizeY
                ? (float)(_random.NextDouble() * GameSettings.SCREEN_HEIGHT)
                : -4f;

            p.Velocity.X = (float)((_random.NextDouble() * 30f) - 15f);
            p.Velocity.Y = (float)((_random.NextDouble() * 40f) + 20f);

            p.Size = (float)((_random.NextDouble() * 3f) + 3f);
            p.Alpha = (float)((_random.NextDouble() * 0.35f) + 0.25f);

            int colorChoice = _random.Next(3);
            p.Color = colorChoice switch
            {
                0 => Theme.AccentP1 * p.Alpha,
                1 => Theme.AccentP2 * p.Alpha,
                _ => Theme.UIText * p.Alpha
            };
        }

        /// <summary>
        /// Updates all particle positions and respawns any that drift off-screen.
        /// </summary>
        /// <param name="gameTime">Provides snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < _particles.Length; i++)
            {
                _particles[i].Position += _particles[i].Velocity * deltaTime;

                if (_particles[i].Position.Y > GameSettings.SCREEN_HEIGHT + 4f)
                {
                    ResetParticle(i, randomizeY: false);
                }
            }
        }

        /// <summary>
        /// Draws all particles to the provided sprite batch.
        /// Precondition: The <paramref name="spriteBatch"/> must already have <c>Begin</c> called.
        /// </summary>
        /// <param name="spriteBatch">The active sprite batch to draw onto.</param>
        /// <param name="pixel">A 1x1 white texture used for drawing particle rectangles.</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                ref readonly Particle p = ref _particles[i];
                int size = Math.Max(1, (int)p.Size);
                Rectangle rect = new(
                    (int)(p.Position.X - (size / 2f)),
                    (int)(p.Position.Y - (size / 2f)),
                    size,
                    size
                );
                spriteBatch.Draw(pixel, rect, p.Color);
            }
        }
    }
}
