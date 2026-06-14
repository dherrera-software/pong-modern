using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using PongGame.Core;
using PongGame.Core.Rendering;

namespace PongGame.Entities
{
    public class Paddle
    {
        public Vector2 Position { get; private set; }
        public Rectangle Bounds { get; private set; }
        public int Score { get; set; }
        public Color AccentColor { get; }

        public Paddle(float startX, int playerIndex)
        {
            AccentColor = playerIndex == 1 ? Theme.AccentP1 : Theme.AccentP2;

            const float startY = (GameSettings.SCREEN_HEIGHT - GameSettings.PADDLE_HEIGHT) / 2f;
            Position = new Vector2(startX - (GameSettings.PADDLE_WIDTH / 2f), startY);

            UpdateBounds();
            Score = 0;
        }

        private void UpdateBounds()
        {
            Bounds = new Rectangle(
                (int)MathF.Round(Position.X),
                (int)MathF.Round(Position.Y),
                GameSettings.PADDLE_WIDTH,
                GameSettings.PADDLE_HEIGHT
            );
        }

        public void Update(GameTime gameTime, bool moveUp, bool moveDown)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float moveAmount = GameSettings.PADDLE_SPEED * deltaTime;

            float newY = Position.Y;
            if (moveUp)
            {
                newY -= moveAmount;
            }
            if (moveDown)
            {
                newY += moveAmount;
            }

            newY = MathHelper.Clamp(newY, 0f, GameSettings.SCREEN_HEIGHT - GameSettings.PADDLE_HEIGHT);
            Position = new Vector2(Position.X, newY);
            UpdateBounds();
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Draw GLOW_LAYERS concentric rectangles around the paddle:
            // Each layer expands outward by (layer+1)*2 pixels on all sides
            // Alpha decreases per layer: 80, 45, 20 (out of 255)
            int[] alphas = [80, 45, 20];

            for (int layer = 0; layer < GameSettings.GLOW_LAYERS; layer++)
            {
                int alpha = layer < alphas.Length ? alphas[layer] : 20;
                int expansion = (layer + 1) * 2;

                Rectangle glowRect = new(
                    Bounds.X - expansion,
                    Bounds.Y - expansion,
                    Bounds.Width + (expansion * 2),
                    Bounds.Height + (expansion * 2)
                );

                spriteBatch.Draw(pixel, glowRect, AccentColor * (alpha / 255f));
            }

            // Draw the solid paddle rectangle on top in AccentColor
            spriteBatch.Draw(pixel, Bounds, AccentColor);
        }

        /// <summary>
        /// Draws the neon glow around the paddle using GlowRenderer.
        /// </summary>
        /// <param name="spriteBatch">The active sprite batch.</param>
        /// <param name="pixel">A 1x1 white texture.</param>
        public void DrawGlow(SpriteBatch spriteBatch, Texture2D pixel)
        {
            GlowRenderer.DrawRectGlow(spriteBatch, pixel, Bounds, AccentColor);
        }

        public void ResetPosition()
        {
            const float startY = (GameSettings.SCREEN_HEIGHT - GameSettings.PADDLE_HEIGHT) / 2f;
            Position = new Vector2(Position.X, startY);
            UpdateBounds();
        }
    }
}
