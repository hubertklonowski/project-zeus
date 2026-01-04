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
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
    }

    /// <summary>
    /// Rock projectile thrown by the goat
    /// </summary>
    public class Rock
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Size { get; set; }
        public bool Active { get; set; }
    }
}
