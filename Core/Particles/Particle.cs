using Microsoft.Xna.Framework;

namespace PongGame.Core.Particles
{
    /// <summary>
    /// Represents a single active particle in the game.
    /// Using a struct avoids GC allocation overhead.
    /// </summary>
    public struct Particle
    {
        /// <summary>
        /// Current position in screen space.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Velocity vector (pixels per second).
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// Current color (and base transparency/tint).
        /// </summary>
        public Color Color;

        /// <summary>
        /// Time remaining before the particle dies (seconds).
        /// </summary>
        public float Lifetime;

        /// <summary>
        /// Total lifetime initialized with (seconds).
        /// </summary>
        public float MaxLifetime;

        /// <summary>
        /// Size (width and height) of the particle.
        /// </summary>
        public float Size;

        /// <summary>
        /// Rotation in radians.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// Speed of rotation (radians per second).
        /// </summary>
        public float RotationSpeed;

        /// <summary>
        /// Whether the particle is currently active.
        /// </summary>
        public bool IsAlive;
    }
}
