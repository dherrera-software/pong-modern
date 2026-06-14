using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using PongGame.Core;

namespace PongGame.Entities
{
    public class Ball
    {
        private readonly Random _random = new();

        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public Rectangle Bounds { get; private set; }
        public bool IsActive { get; private set; }
        public Vector2[] Trail { get; }

        public Ball(Vector2 center)
        {
            Position = center;
            Trail = new Vector2[GameSettings.TRAIL_LENGTH];
            for (int i = 0; i < Trail.Length; i++)
            {
                Trail[i] = center;
            }
            IsActive = false;
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            Bounds = new Rectangle(
                (int)MathF.Round(Position.X - (GameSettings.BALL_SIZE / 2f)),
                (int)MathF.Round(Position.Y - (GameSettings.BALL_SIZE / 2f)),
                GameSettings.BALL_SIZE,
                GameSettings.BALL_SIZE
            );
        }

        public void Reset(Vector2 center)
        {
            Position = center;
            UpdateBounds();

            // Random initial angle between -35 and 35 degrees
            float angleDegrees = (float)((_random.NextDouble() * 70.0) - 35.0);
            float angleRadians = angleDegrees * (MathF.PI / 180f);

            // Randomly point left or right
            float directionX = _random.Next(2) == 0 ? 1f : -1f;
            Vector2 direction = new(directionX * MathF.Cos(angleRadians), MathF.Sin(angleRadians));

            Velocity = direction * GameSettings.BALL_INITIAL_SPEED;

            // Clear trail to center
            for (int i = 0; i < Trail.Length; i++)
            {
                Trail[i] = center;
            }

            IsActive = true;
        }

        public void Update(GameTime gameTime, Paddle leftPaddle, Paddle rightPaddle, out int scorer)
        {
            scorer = 0;
            if (!IsActive)
            {
                return;
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Shift trail
            for (int i = Trail.Length - 1; i > 0; i--)
            {
                Trail[i] = Trail[i - 1];
            }
            Trail[0] = Position;

            // Move
            Position += Velocity * deltaTime;
            UpdateBounds();

            // Bounce on top/bottom
            const float halfSize = GameSettings.BALL_SIZE / 2f;
            if (Position.Y - halfSize is <= 0f)
            {
                Velocity = new Vector2(Velocity.X, MathF.Abs(Velocity.Y));
                Position = new Vector2(Position.X, halfSize);
                UpdateBounds();
            }
            else if (Position.Y + halfSize >= GameSettings.SCREEN_HEIGHT)
            {
                Velocity = new Vector2(Velocity.X, -MathF.Abs(Velocity.Y));
                Position = new Vector2(Position.X, GameSettings.SCREEN_HEIGHT - halfSize);
                UpdateBounds();
            }

            // Paddle collision
            if (Bounds.Intersects(leftPaddle.Bounds))
            {
                HandlePaddleCollision(leftPaddle);
            }
            else if (Bounds.Intersects(rightPaddle.Bounds))
            {
                HandlePaddleCollision(rightPaddle);
            }

            // Scoring checking
            if (Position.X < 0)
            {
                scorer = 2;
                IsActive = false;
            }
            else if (Position.X > GameSettings.SCREEN_WIDTH)
            {
                scorer = 1;
                IsActive = false;
            }
        }

        private void HandlePaddleCollision(Paddle paddle)
        {
            // Flip Velocity.X depending on the paddle side
            bool isLeftPaddle = paddle.Bounds.X < GameSettings.SCREEN_WIDTH / 2f;
            if (isLeftPaddle)
            {
                Velocity = new Vector2(MathF.Abs(Velocity.X), Velocity.Y);
                // Adjust position to avoid clipping
                Position = new Vector2(paddle.Bounds.Right + (GameSettings.BALL_SIZE / 2f), Position.Y);
            }
            else
            {
                Velocity = new Vector2(-MathF.Abs(Velocity.X), Velocity.Y);
                // Adjust position to avoid clipping
                Position = new Vector2(paddle.Bounds.Left - (GameSettings.BALL_SIZE / 2f), Position.Y);
            }

            // Angle variation
            float paddleCenterY = paddle.Bounds.Y + (paddle.Bounds.Height / 2f);
            float hitOffset = (Position.Y - paddleCenterY) / (GameSettings.PADDLE_HEIGHT / 2f);
            Velocity = new Vector2(Velocity.X, Velocity.Y + (hitOffset * 180f));

            // Increment speed and clamp to max
            float currentSpeed = Velocity.Length();
            float newSpeed = MathF.Min(currentSpeed + GameSettings.BALL_SPEED_INCREMENT, GameSettings.MAX_BALL_SPEED);

            if (Velocity != Vector2.Zero)
            {
                Velocity = Vector2.Normalize(Velocity) * newSpeed;
            }

            UpdateBounds();
        }

        /// <summary>
        /// Draws the ball with trail effect, neon glow layers, and solid center.
        /// </summary>
        /// <param name="spriteBatch">The active sprite batch for rendering.</param>
        /// <param name="pixel">A 1x1 white texture used for rectangle rendering.</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (!IsActive)
            {
                return;
            }

            // Trail: draw each Trail[i] as a shrinking, fading rectangle
            for (int i = Trail.Length - 1; i >= 0; i--)
            {
                int size = Math.Max(2, GameSettings.BALL_SIZE - (i * 2));
                int alpha = Math.Max(0, 180 - (i * 28));
                Rectangle trailRect = new(
                    (int)MathF.Round(Trail[i].X - (size / 2f)),
                    (int)MathF.Round(Trail[i].Y - (size / 2f)),
                    size,
                    size
                );
                spriteBatch.Draw(pixel, trailRect, Theme.BallColor * (alpha / 255f));
            }

            // Glow: GLOW_LAYERS concentric rectangles expanding around Position
            int[] glowAlphas = [90, 50, 20];
            for (int layer = 0; layer < GameSettings.GLOW_LAYERS; layer++)
            {
                int expansion = (layer + 1) * 3;
                int glowSize = GameSettings.BALL_SIZE + (expansion * 2);
                int alpha = layer < glowAlphas.Length ? glowAlphas[layer] : 20;
                Rectangle glowRect = new(
                    (int)MathF.Round(Position.X - (glowSize / 2f)),
                    (int)MathF.Round(Position.Y - (glowSize / 2f)),
                    glowSize,
                    glowSize
                );
                spriteBatch.Draw(pixel, glowRect, Theme.BallColor * (alpha / 255f));
            }

            // Solid ball: BALL_SIZE square centered at Position
            Rectangle ballRect = new(
                (int)MathF.Round(Position.X - (GameSettings.BALL_SIZE / 2f)),
                (int)MathF.Round(Position.Y - (GameSettings.BALL_SIZE / 2f)),
                GameSettings.BALL_SIZE,
                GameSettings.BALL_SIZE
            );
            spriteBatch.Draw(pixel, ballRect, Theme.BallColor);
        }
    }
}
