using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PongGame.Core.Rendering
{
    /// <summary>
    /// Provides helper methods to render neon glow effects for rectangles and text.
    /// </summary>
    public static class GlowRenderer
    {
        private static readonly int[] GlowSizes = { 4, 10, 20, 36, 58, 86 };
        private static readonly float[] GlowAlphas = { 0.70f, 0.35f, 0.18f, 0.09f, 0.04f, 0.02f };

        /// <summary>
        /// Draws an outer neon glow around a rectangle.
        /// Precondition: The <paramref name="spriteBatch"/> must be in <see cref="BlendState.Additive"/> blend mode.
        /// </summary>
        /// <param name="spriteBatch">The active sprite batch to draw onto.</param>
        /// <param name="pixel">A 1x1 white texture used for drawing rectangles.</param>
        /// <param name="bounds">The core bounding rectangle of the entity.</param>
        /// <param name="color">The color of the glow effect.</param>
        public static void DrawRectGlow(SpriteBatch spriteBatch, Texture2D pixel, Rectangle bounds, Color color)
        {
            for (int i = GlowSizes.Length - 1; i >= 0; i--)
            {
                int size = GlowSizes[i];
                Rectangle glowRect = new Rectangle(
                    bounds.X - size,
                    bounds.Y - size,
                    bounds.Width + (size * 2),
                    bounds.Height + (size * 2)
                );
                spriteBatch.Draw(pixel, glowRect, color * GlowAlphas[i]);
            }
        }

        /// <summary>
        /// Draws an outer text glow using offset layers.
        /// Precondition: The <paramref name="spriteBatch"/> must be in <see cref="BlendState.Additive"/> blend mode.
        /// </summary>
        /// <param name="spriteBatch">The active sprite batch to draw onto.</param>
        /// <param name="font">The font to use for the text.</param>
        /// <param name="text">The text string to render.</param>
        /// <param name="position">The baseline position of the text.</param>
        /// <param name="color">The base color of the glow effect.</param>
        /// <param name="layers">The number of offset layers to render.</param>
        public static void DrawTextGlow(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int layers = 4)
        {
            for (int layer = layers; layer >= 1; layer--)
            {
                float alpha = 0.5f / (layer * 1.6f);
                Color layerColor = color * alpha;
                float l = layer;

                spriteBatch.DrawString(font, text, position + new Vector2(-l, 0), layerColor);
                spriteBatch.DrawString(font, text, position + new Vector2(l, 0), layerColor);
                spriteBatch.DrawString(font, text, position + new Vector2(0, -l), layerColor);
                spriteBatch.DrawString(font, text, position + new Vector2(0, l), layerColor);
                spriteBatch.DrawString(font, text, position + new Vector2(-l, -l), layerColor);
                spriteBatch.DrawString(font, text, position + new Vector2(l, -l), layerColor);
                spriteBatch.DrawString(font, text, position + new Vector2(-l, l), layerColor);
                spriteBatch.DrawString(font, text, position + new Vector2(l, l), layerColor);
            }
        }
    }
}
