using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PongGame.Core
{
    /// <summary>
    /// Centralized subsystem for all audio playback: background music and sound effects.
    /// Handles loading, caching, playing, stopping, looping, volume control,
    /// and fade-in / fade-out transitions between music tracks.
    /// </summary>
    /// <remarks>
    /// <b>Architecture:</b> All audio playback must go through this manager.
    /// Scenes, UI components, and entities should never call <c>SoundEffect.Play()</c>
    /// or <c>MediaPlayer.Play()</c> directly.
    ///
    /// <b>Volume model:</b>
    /// <list type="bullet">
    ///   <item>Effective music volume = <see cref="MasterVolume"/> × <see cref="MusicVolume"/></item>
    ///   <item>Effective SFX volume   = <see cref="MasterVolume"/> × <see cref="SfxVolume"/></item>
    /// </list>
    ///
    /// <b>Usage pattern:</b>
    /// <list type="number">
    ///   <item>Call <see cref="Initialize"/> once in Game.LoadContent, passing the ContentManager.</item>
    ///   <item>Preload tracks with <see cref="LoadTrack"/> and effects with <see cref="LoadSfx"/>.</item>
    ///   <item>Call <see cref="PlayTrack"/> from a scene's OnEnter to start music.</item>
    ///   <item>Call <see cref="PlaySfx"/> from game logic to fire sound effects.</item>
    ///   <item>Call <see cref="Update"/> every frame so fade transitions are processed.</item>
    /// </list>
    ///
    /// <b>Adding new sound effects:</b>
    /// <list type="number">
    ///   <item>Place the <c>.wav</c> file in <c>Content/audio/sfx/</c>.</item>
    ///   <item>Register it in <c>Content.mgcb</c> with <c>WavImporter</c> + <c>SoundEffectProcessor</c>.</item>
    ///   <item>Call <c>AudioManager.LoadSfx("key", "audio/sfx/filename")</c> in <c>Game1.LoadContent</c>.</item>
    ///   <item>Trigger it anywhere with <c>AudioManager.PlaySfx("key")</c>.</item>
    /// </list>
    /// </remarks>
    public static class AudioManager
    {
        // ── Content ──────────────────────────────────────────────────────────
        private static ContentManager? _content;
        private static readonly Dictionary<string, Song> _tracks = [];
        private static readonly Dictionary<string, SoundEffect> _sfxCache = [];

        // ── Playback state ────────────────────────────────────────────────────
        private static string? _currentTrackKey;
        private static string? _pendingTrackKey;   // track to play after fade-out completes

        // ── Volume ────────────────────────────────────────────────────────────
        private static float _masterVolume = 0.50f;
        private static float _musicVolume  = 0.40f;
        private static float _sfxVolume    = 0.60f;
        private static float _currentVolume = 0.0f; // actual MediaPlayer volume (animated)

        /// <summary>
        /// Computes the target music volume: <c>MasterVolume × MusicVolume</c>.
        /// Used by fade transitions and immediate volume sync.
        /// </summary>
        private static float EffectiveMusicVolume => _masterVolume * _musicVolume;

        /// <summary>
        /// Computes the target SFX volume: <c>MasterVolume × SfxVolume</c>.
        /// Used by <see cref="PlaySfx"/>.
        /// </summary>
        private static float EffectiveSfxVolume => _masterVolume * _sfxVolume;

        // ── Fade ──────────────────────────────────────────────────────────────
        private enum FadeState { Idle, FadingOut, FadingIn }
        private static FadeState _fadeState = FadeState.Idle;

        /// <summary>Duration of each fade (in seconds). Default 0.4 s.</summary>
        public const float FADE_DURATION = 0.4f;

        private static float _fadeTimer = 0f;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the master volume applied to all audio (0.0–1.0).
        /// Changes take effect immediately for SFX.
        /// For music, changes take effect immediately unless a fade is in progress.
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
                    float effective = EffectiveMusicVolume;
                    MediaPlayer.Volume = effective;
                    _currentVolume     = effective;
                }
            }
        }

        /// <summary>
        /// Gets or sets the music volume channel (0.0–1.0). Default 0.70.
        /// The actual playback volume is <c>MasterVolume × MusicVolume</c>.
        /// </summary>
        public static float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = MathHelper.Clamp(value, 0f, 1f);
                if (_fadeState == FadeState.Idle)
                {
                    float effective = EffectiveMusicVolume;
                    MediaPlayer.Volume = effective;
                    _currentVolume     = effective;
                }
            }
        }

        /// <summary>
        /// Gets or sets the SFX volume channel (0.0–1.0). Default 1.0.
        /// The actual playback volume is <c>MasterVolume × SfxVolume</c>.
        /// </summary>
        public static float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = MathHelper.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// Initializes the AudioManager with the application's ContentManager.
        /// Must be called before <see cref="LoadTrack"/>, <see cref="LoadSfx"/>,
        /// or any playback methods.
        /// </summary>
        /// <param name="content">The game's ContentManager instance.</param>
        public static void Initialize(ContentManager content)
        {
            _content = content;
            MediaPlayer.IsRepeating  = true;
            MediaPlayer.Volume       = 0f;
            _currentVolume           = 0f;
        }

        // ── Music ─────────────────────────────────────────────────────────────

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

        // ── SFX ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Preloads a sound effect from the Content pipeline and caches it under a key.
        /// Assets are loaded once and reused for all subsequent <see cref="PlaySfx"/> calls.
        /// </summary>
        /// <param name="key">Unique identifier used to reference this effect (e.g., "paddle_hit").</param>
        /// <param name="assetPath">Content-relative path, e.g., <c>audio/sfx/paddle_hit</c>.</param>
        public static void LoadSfx(string key, string assetPath)
        {
            if (_content == null)
            {
                return;
            }

            if (!_sfxCache.ContainsKey(key))
            {
                _sfxCache[key] = _content.Load<SoundEffect>(assetPath);
            }
        }

        /// <summary>
        /// Plays the sound effect registered under <paramref name="key"/>.
        /// Uses fire-and-forget playback, supporting multiple simultaneous instances
        /// with no allocations during playback.
        /// </summary>
        /// <param name="key">The key of the sound effect to play.</param>
        public static void PlaySfx(string key)
        {
            // Guard: effective volume is zero — skip playback entirely.
            float volume = EffectiveSfxVolume;
            if (volume <= 0f)
            {
                return;
            }

            // Guard: key not found — fail silently (defensive).
            if (!_sfxCache.TryGetValue(key, out SoundEffect? sfx))
            {
                return;
            }

            sfx.Play(volume, 0f, 0f);
        }

        // ── Update ────────────────────────────────────────────────────────────

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

            float targetVolume = EffectiveMusicVolume;

            switch (_fadeState)
            {
                case FadeState.FadingOut:
                    _currentVolume     = MathHelper.Lerp(targetVolume, 0f, t);
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
                    _currentVolume     = MathHelper.Lerp(0f, targetVolume, t);
                    MediaPlayer.Volume = _currentVolume;

                    if (t >= 1f)
                    {
                        MediaPlayer.Volume = targetVolume;
                        _currentVolume     = targetVolume;
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
