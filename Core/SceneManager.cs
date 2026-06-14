using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PongGame.Scenes;
using System;
using System.Collections.Generic;

namespace PongGame.Core
{
    /// <summary>
    /// Manages game scene registration, initialization, lifecycle, logic updates, rendering, and navigation.
    /// </summary>
    public static class SceneManager
    {
        private static readonly Dictionary<string, IScene> _scenes = [];
        private static IScene? _currentScene;
        private static ContentManager? _content;
        private static GraphicsDevice? _graphicsDevice;

        /// <summary>
        /// Gets the unique string key identifying the currently active scene, or null if no scene is active.
        /// </summary>
        public static string? CurrentSceneKey { get; private set; }

        /// <summary>
        /// Initializes the SceneManager with the application's ContentManager and GraphicsDevice context.
        /// </summary>
        /// <param name="content">The application's content manager.</param>
        /// <param name="graphicsDevice">The active graphics device.</param>
        public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _content = content;
            _graphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Registers a scene under a unique key, immediately initializing it and loading its assets.
        /// </summary>
        /// <param name="key">The unique key to identify the scene.</param>
        /// <param name="scene">The scene instance to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if the registered scene or key is null.</exception>
        public static void RegisterScene(string key, IScene scene)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(scene);

            if (_content == null || _graphicsDevice == null)
            {
                throw new InvalidOperationException("SceneManager must be initialized before registering scenes.");
            }

            scene.Initialize();
            scene.LoadContent(_content, _graphicsDevice);
            _scenes[key] = scene;
        }

        /// <summary>
        /// Changes the active scene to the scene registered under the specified key, triggering transition lifecycles.
        /// </summary>
        /// <param name="key">The key of the scene to switch to.</param>
        /// <exception cref="KeyNotFoundException">Thrown if no scene has been registered with the specified key.</exception>
        public static void ChangeScene(string key)
        {
            if (key == null || !_scenes.TryGetValue(key, out var nextScene))
            {
                throw new KeyNotFoundException($"Scene '{key}' not found. Register it before calling ChangeScene.");
            }

            _currentScene?.OnExit();
            _currentScene = nextScene;
            CurrentSceneKey = key;
            _currentScene.OnEnter();
        }

        /// <summary>
        /// Updates the currently active scene.
        /// </summary>
        /// <param name="gameTime">Provides snapshot of timing values.</param>
        public static void Update(GameTime gameTime)
        {
            _currentScene?.Update(gameTime);
        }

        /// <summary>
        /// Draws the currently active scene.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used to render 2D graphics.</param>
        public static void Draw(SpriteBatch spriteBatch)
        {
            _currentScene?.Draw(spriteBatch);
        }
    }
}
