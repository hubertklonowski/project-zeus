using Microsoft.Xna.Framework;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Platform entity for mountain level
    /// </summary>
    public class Platform
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Color Color { get; set; }
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
    }

    /// <summary>
    /// Moving platform that oscillates between two points
    /// </summary>
    public class MovingPlatform : Platform
    {
        public Vector2 StartPosition { get; set; }
        public Vector2 EndPosition { get; set; }
        public float Speed { get; set; }
        public float Progress { get; set; } // 0 to 1, position between start and end
        public bool MovingToEnd { get; set; } = true;
        
        /// <summary>
        /// Updates the platform position based on elapsed time
        /// </summary>
        public void Update(float deltaTime)
        {
            float distance = Vector2.Distance(StartPosition, EndPosition);
            float progressDelta = (Speed * deltaTime) / distance;
            
            if (MovingToEnd)
            {
                Progress += progressDelta;
                if (Progress >= 1f)
                {
                    Progress = 1f;
                    MovingToEnd = false;
                }
            }
            else
            {
                Progress -= progressDelta;
                if (Progress <= 0f)
                {
                    Progress = 0f;
                    MovingToEnd = true;
                }
            }
            
            Position = Vector2.Lerp(StartPosition, EndPosition, Progress);
        }
    }

    /// <summary>
    /// Rock projectile thrown by the goat
    /// </summary>
    public class Rock
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public float RotationSpeed { get; set; }
        public bool Active { get; set; }
    }
}
