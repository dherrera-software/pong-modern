using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PongGame.Core.Rendering
{
    /// <summary>
    /// Manages the rendering passes and layered drawing sequences of the game.
    /// </summary>
    public sealed class RenderLayerManager
    {
        private readonly SpriteBatch _spriteBatch;

        /// <summary>
        /// Gets or sets the transformation matrix applied to world-space rendering passes.
        /// </summary>
        public Matrix? TransformMatrix { get; set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderLayerManager"/> class.
        /// </summary>
        /// <param name="spriteBatch">The active sprite batch to use for rendering passes.</param>
        /// <exception cref="ArgumentNullException">Thrown when spriteBatch is null.</exception>
        public RenderLayerManager(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        }

        /// <summary>
        /// Executes a drawing pass with additive blend state and the world-space transformation matrix.
        /// </summary>
        /// <param name="drawAction">The drawing action to execute within the pass.</param>
        public void ExecuteGlowPass(Action<SpriteBatch> drawAction)
        {
            if (drawAction == null) return;
            _spriteBatch.Begin(
                blendState: BlendState.Additive,
                transformMatrix: TransformMatrix
            );
            drawAction(_spriteBatch);
            _spriteBatch.End();
        }

        /// <summary>
        /// Executes a drawing pass with alpha blend state and the world-space transformation matrix.
        /// </summary>
        /// <param name="drawAction">The drawing action to execute within the pass.</param>
        public void ExecuteGameplayPass(Action<SpriteBatch> drawAction)
        {
            if (drawAction == null) return;
            _spriteBatch.Begin(
                blendState: BlendState.AlphaBlend,
                transformMatrix: TransformMatrix
            );
            drawAction(_spriteBatch);
            _spriteBatch.End();
        }

        /// <summary>
        /// Executes a drawing pass with alpha blend state and no transformation matrix (screen-space).
        /// </summary>
        /// <param name="drawAction">The drawing action to execute within the pass.</param>
        public void ExecuteUIPass(Action<SpriteBatch> drawAction)
        {
            if (drawAction == null) return;
            _spriteBatch.Begin(
                blendState: BlendState.AlphaBlend,
                transformMatrix: null
            );
            drawAction(_spriteBatch);
            _spriteBatch.End();
        }
    }
}
