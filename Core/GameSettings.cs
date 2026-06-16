namespace PongGame.Core
{
    /// <summary>
    /// Contains global game settings and design constants for PongGame.
    /// </summary>
    public static class GameSettings
    {
        #region Screen Constants

        /// <summary>
        /// The target screen width in pixels.
        /// </summary>
        public const int SCREEN_WIDTH = 1280;

        /// <summary>
        /// The target screen height in pixels.
        /// </summary>
        public const int SCREEN_HEIGHT = 720;

        /// <summary>
        /// The target aspect ratio calculated from screen dimensions.
        /// </summary>
        public static readonly float TARGET_ASPECT_RATIO = 16f / 9f;

        #endregion

        #region Paddle Constants

        /// <summary>
        /// The width of the paddles in pixels.
        /// </summary>
        public const int PADDLE_WIDTH = 18;

        /// <summary>
        /// The height of the paddles in pixels.
        /// </summary>
        public const int PADDLE_HEIGHT = 110;

        /// <summary>
        /// The movement speed of the paddles in pixels per second.
        /// </summary>
        public static readonly float PADDLE_SPEED = 520f;

        /// <summary>
        /// The margin/distance of the paddles from the respective screen edge.
        /// </summary>
        public const int PADDLE_MARGIN = 50;

        /// <summary>
        /// The corner radius of the paddles (for visual rounding reference, not physics).
        /// </summary>
        public const int PADDLE_CORNER_RADIUS = 4;

        #endregion

        #region Ball Constants

        /// <summary>
        /// The size (width and height) of the square ball in pixels.
        /// </summary>
        public const int BALL_SIZE = 16;

        /// <summary>
        /// The initial speed of the ball in pixels per second when served.
        /// </summary>
        public static readonly float BALL_INITIAL_SPEED = 420f;

        /// <summary>
        /// The speed increment in pixels per second added to the ball on each paddle bounce.
        /// </summary>
        public static readonly float BALL_SPEED_INCREMENT = 25f;

        /// <summary>
        /// The maximum limit for the ball speed in pixels per second.
        /// </summary>
        public static readonly float MAX_BALL_SPEED = 950f;

        #endregion

        #region Scoring Constants

        /// <summary>
        /// The score required to win the game.
        /// </summary>
        public const int WINNING_SCORE = 7;

        #endregion

        #region Visual Constants

        /// <summary>
        /// The maximum number of points tracked for the advanced ball trail effect.
        /// </summary>
        public const int TRAIL_MAX_POINTS = 16;

        /// <summary>
        /// The time interval in seconds between trail position samples.
        /// </summary>
        public const float TRAIL_SAMPLE_INTERVAL = 0.02f;

        /// <summary>
        /// The lifetime of each trail point in seconds.
        /// </summary>
        public const float TRAIL_POINT_LIFETIME = 0.35f;

        /// <summary>
        /// The base opacity (alpha) of the trail when it is first created.
        /// </summary>
        public const float TRAIL_BASE_OPACITY = 0.65f;

        /// <summary>
        /// The size decay factor for the trail points.
        /// </summary>
        public const float TRAIL_SIZE_DECAY = 0.75f;

        /// <summary>
        /// The multiplier for the trail glow intensity.
        /// </summary>
        public const float TRAIL_GLOW_INTENSITY = 0.3f;

        /// <summary>
        /// The number of ghost positions tracked for the ball trail effect.
        /// </summary>
        public const int TRAIL_LENGTH = 6;

        /// <summary>
        /// The number of concentric glow layers drawn to simulate a neon glow.
        /// </summary>
        public const int GLOW_LAYERS = 3;

        /// <summary>
        /// The height of each dash in the vertical center dotted line.
        /// </summary>
        public const int CENTER_LINE_DASH_HEIGHT = 18;

        /// <summary>
        /// The gap size between dashes in the vertical center dotted line.
        /// </summary>
        public const int CENTER_LINE_GAP = 12;

        #endregion
    }
}
