using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PongGame.Core;
using PongGame.Core.Particles;
using PongGame.Scenes;

namespace PongGame
{
    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch = null!;
        private readonly GraphicsDeviceManager _graphics;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = GameSettings.SCREEN_WIDTH,
                PreferredBackBufferHeight = GameSettings.SCREEN_HEIGHT
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Performs one-time initialization of the game instance.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Loads graphical resources, initializes the scene manager, and registers all game scenes.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize audio subsystem and preload all music tracks.
            AudioManager.Initialize(Content);
            AudioManager.LoadTrack("menu",     "audio/music/menu_theme");
            AudioManager.LoadTrack("gameplay", "audio/music/gameplay_theme");

            // Create a temporary pixel for ParticleManager initialization
            Texture2D particlePixel = new Texture2D(GraphicsDevice, 1, 1);
            particlePixel.SetData([Color.White]);
            ParticleManager.Initialize(particlePixel);

            SceneManager.Initialize(Content, GraphicsDevice);
            TransitionManager.Initialize(GraphicsDevice);
            SceneManager.RegisterScene("menu", new MainMenuScene());
            SceneManager.RegisterScene("game", new GameScene());
            SceneManager.ChangeScene("menu");
        }

        /// <summary>
        /// Updates input state and delegates per-frame logic to the active scene.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                // Escape / Back handling is managed inside individual scenes.
            }

            InputManager.Update();
            AudioManager.Update(gameTime);
            TransitionManager.Update(gameTime);
            SceneManager.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// Clears the screen and delegates rendering to the active scene.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Theme.Background);
            SceneManager.Draw(_spriteBatch);
            TransitionManager.Draw(_spriteBatch);
            base.Draw(gameTime);
        }
    }
}
