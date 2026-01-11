using Microsoft.Xna.Framework;

namespace ProjectZeus.Core.Extensions
{
    /// <summary>
    /// Extension methods for collision detection
    /// </summary>
    public static class CollisionExtensions
    {
        /// <summary>
        /// Checks if a rectangle intersects with another rectangle
        /// </summary>
        public static bool IntersectsWith(this Rectangle rect, Rectangle other)
        {
            return rect.Intersects(other);
        }

        /// <summary>
        /// Checks if a rectangle intersects with a position and size
        /// </summary>
        public static bool IntersectsWith(this Rectangle rect, Vector2 position, Vector2 size)
        {
            Rectangle other = new Rectangle(
                (int)position.X,
                (int)position.Y,
                (int)size.X,
                (int)size.Y);
            return rect.Intersects(other);
        }

        /// <summary>
        /// Creates a rectangle from position and size vectors
        /// </summary>
        public static Rectangle ToRectangle(this Vector2 position, Vector2 size)
        {
            return new Rectangle(
                (int)position.X,
                (int)position.Y,
                (int)size.X,
                (int)size.Y);
        }

        /// <summary>
        /// Checks collision between player and entity rectangles
        /// </summary>
        public static bool CheckEntityCollision(Vector2 playerPosition, Vector2 playerSize, Vector2 entityPosition, Vector2 entitySize)
        {
            Rectangle playerRect = playerPosition.ToRectangle(playerSize);
            Rectangle entityRect = entityPosition.ToRectangle(entitySize);
            return playerRect.Intersects(entityRect);
        }

        /// <summary>
        /// Checks if player rectangle intersects an expanded pickup area
        /// </summary>
        public static bool CheckPickupCollision(Rectangle playerRect, Rectangle itemRect, int expandBy = 20)
        {
            Rectangle expandedRect = itemRect;
            expandedRect.Inflate(expandBy, expandBy);
            return playerRect.Intersects(expandedRect);
        }
    }
}
