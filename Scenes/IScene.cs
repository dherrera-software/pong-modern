using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PongGame.Scenes
{
    /// <summary>
    /// Defines the contract for game scenes in the Pong game.
    /// </summary>
    public interface IScene
    {
        /// <summary>
        /// Initializes the scene, setting up default states, systems, or non-content configurations.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Loads graphical or audio assets and resources required by this scene.
        /// </summary>
        /// <param name="content">The application's content manager.</param>
        /// <param name="graphicsDevice">The active graphics device.</param>
        void LoadContent(ContentManager content, GraphicsDevice graphicsDevice);

        /// <summary>
        /// Updates the logic and state of the scene.
        /// </summary>
        /// <param name="gameTime">Provides snapshot of timing values.</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Draws the scene elements onto the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used for drawing text and 2D textures.</param>
        void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Called when this scene becomes the active scene in the game.
        /// </summary>
        void OnEnter();

        /// <summary>
        /// Called when this scene is deactivated and replaced by another scene.
        /// </summary>
        void OnExit();
    }
}
