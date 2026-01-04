using Microsoft.Xna.Framework;

namespace ProjectZeus.Core.Entities
{
    /// <summary>
    /// Represents a portal that transports the player to different levels
    /// </summary>
    public class Portal
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public bool IsActive { get; set; }
        public Color BaseColor { get; set; }

        public Rectangle Bounds => new Rectangle(
            (int)Position.X, 
            (int)Position.Y, 
            (int)Size.X, 
            (int)Size.Y);

        public Portal(Vector2 position, Vector2 size, Color baseColor)
        {
            Position = position;
            Size = size;
            BaseColor = baseColor;
            IsActive = true;
        }

        public bool Intersects(Rectangle playerRect)
        {
            return IsActive && Bounds.Intersects(playerRect);
        }
    }
}
