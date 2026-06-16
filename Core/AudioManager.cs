using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PongGame.Core
{
    /// <summary>
    /// Centralized subsystem for background music playback.
    /// Handles loading, playing, stopping, looping, volume control,
    /// and fade-in / fade-out transitions between tracks.
    /// </summary>
    /// <remarks>
    /// Usage pattern:
    /// <list type="number">
    ///   <item>Call <see cref="Initialize"/> once in Game.LoadContent, passing the ContentManager.</item>
    ///   <item>Preload tracks with <see cref="LoadTrack"/>.</item>
    ///   <item>Call <see cref="PlayTrack"/> from a scene's OnEnter to start music.</item>
    ///   <item>Call <see cref="Update"/> every frame so fade transitions are processed.</item>
    /// </list>
    /// </remarks>
    public static class AudioManager
    {
        // ── Content ──────────────────────────────────────────────────────────
        private static ContentManager? _content;
        private static readonly Dictionary<string, Song> _tracks = [];

        // ── Playback state ────────────────────────────────────────────────────
        private static string? _currentTrackKey;
        private static string? _pendingTrackKey;   // track to play after fade-out completes

        // ── Volume / fade ─────────────────────────────────────────────────────
        private static float _masterVolume  = 1.0f;
        private static float _currentVolume = 0.0f; // actual MediaPlayer volume (animated)

        private enum FadeState { Idle, FadingOut, FadingIn }
        private static FadeState _fadeState = FadeState.Idle;

        /// <summary>Duration of each fade (in seconds). Default 0.4 s.</summary>
        public const float FADE_DURATION = 0.4f;

        private static float _fadeTimer = 0f;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the master volume applied to all music (0.0–1.0).
        /// Changes take effect immediately unless a fade is in progress.
        /// </summary>
        public static float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = MathHelper.Clamp(value, 0f, 1f);
                // If idle (not fading) keep MediaPlayer in sync right away.
                if (_fadeState == FadeState.Idle)
                {
                    MediaPlayer.Volume = _masterVolume;
                    _currentVolume     = _masterVolume;
                }
            }
        }

        /// <summary>
        /// Initializes the AudioManager with the application's ContentManager.
        /// Must be called before <see cref="LoadTrack"/> or <see cref="PlayTrack"/>.
        /// </summary>
        /// <param name="content">The game's ContentManager instance.</param>
        public static void Initialize(ContentManager content)
        {
            _content = content;
            MediaPlayer.IsRepeating  = true;
            MediaPlayer.Volume       = 0f;
            _currentVolume           = 0f;
        }

        /// <summary>
        /// Preloads a music track from the Content pipeline and registers it under a key.
        /// </summary>
        /// <param name="key">Unique identifier used to reference this track (e.g., "menu", "gameplay").</param>
        /// <param name="assetPath">Content-relative path, e.g., <c>audio/music/menu_theme</c>.</param>
        public static void LoadTrack(string key, string assetPath)
        {
            if (_content == null)
            {
                return;
            }

            if (!_tracks.ContainsKey(key))
            {
                _tracks[key] = _content.Load<Song>(assetPath);
            }
        }

        /// <summary>
        /// Plays the track registered under <paramref name="key"/>.
        /// If a different track is playing, a fade-out → fade-in transition is performed.
        /// If the same track is already playing, the call is a no-op (no restart, no duplicate).
        /// </summary>
        /// <param name="key">The key of the track to play.</param>
        public static void PlayTrack(string key)
        {
            // No-op: requested track is already playing and we are not transitioning away.
            if (key == _currentTrackKey && _fadeState == FadeState.Idle
                && MediaPlayer.State == MediaState.Playing)
            {
                return;
            }

            // If nothing is playing yet, skip fade-out and go straight to fade-in.
            if (MediaPlayer.State == MediaState.Stopped || _currentTrackKey == null)
            {
                StartPlayback(key);
                return;
            }

            // Begin fade-out; remember the track to start next.
            _pendingTrackKey = key;
            _fadeState       = FadeState.FadingOut;
            _fadeTimer       = 0f;
        }

        /// <summary>
        /// Stops the currently playing music with a fade-out.
        /// </summary>
        public static void Stop()
        {
            if (MediaPlayer.State == MediaState.Stopped)
            {
                return;
            }

            _pendingTrackKey = null;
            _fadeState       = FadeState.FadingOut;
            _fadeTimer       = 0f;
        }

        /// <summary>
        /// Must be called every frame from Game.Update (or SceneManager.Update) so that
        /// fade transitions are processed correctly.
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values for the current frame.</param>
        public static void Update(GameTime gameTime)
        {
            if (_fadeState == FadeState.Idle)
            {
                return;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _fadeTimer += dt;
            float t    = MathHelper.Clamp(_fadeTimer / FADE_DURATION, 0f, 1f);

            switch (_fadeState)
            {
                case FadeState.FadingOut:
                    _currentVolume     = MathHelper.Lerp(_masterVolume, 0f, t);
                    MediaPlayer.Volume = _currentVolume;

                    if (t >= 1f)
                    {
                        MediaPlayer.Stop();
                        _currentTrackKey = null;

                        if (_pendingTrackKey != null)
                        {
                            // Begin playing the next track and fade it in.
                            StartPlayback(_pendingTrackKey);
                        }
                        else
                        {
                            // Just a stop — no follow-up track.
                            _fadeState = FadeState.Idle;
                        }
                    }
                    break;

                case FadeState.FadingIn:
                    _currentVolume     = MathHelper.Lerp(0f, _masterVolume, t);
                    MediaPlayer.Volume = _currentVolume;

                    if (t >= 1f)
                    {
                        MediaPlayer.Volume = _masterVolume;
                        _currentVolume     = _masterVolume;
                        _fadeState         = FadeState.Idle;
                        _fadeTimer         = 0f;
                    }
                    break;
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Immediately starts playback of the specified track and begins the fade-in.
        /// </summary>
        private static void StartPlayback(string key)
        {
            if (!_tracks.TryGetValue(key, out Song? song))
            {
                return;
            }

            MediaPlayer.Volume      = 0f;
            _currentVolume          = 0f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);

            _currentTrackKey = key;
            _pendingTrackKey = null;
            _fadeState       = FadeState.FadingIn;
            _fadeTimer       = 0f;
        }
    }
}
