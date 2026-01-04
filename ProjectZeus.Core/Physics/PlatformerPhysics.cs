using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Constants;

namespace ProjectZeus.Core.Physics
{
    /// <summary>
    /// Helper methods for platformer physics and movement
    /// </summary>
    public static class PlatformerPhysics
    {
        /// <summary>
        /// Applies standard platformer movement to a position and velocity
        /// </summary>
        public static void ApplyPlatformerMovement(
            KeyboardState keyboardState,
            ref Vector2 position,
            ref Vector2 velocity,
            ref bool isOnGround,
            float deltaTime,
            Vector2 playerSize,
            float groundTop)
        {
            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;

            velocity.X = move * GameConstants.MoveSpeed;

            if (isOnGround && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up)))
            {
                velocity.Y = GameConstants.JumpVelocity;
                isOnGround = false;
            }

            velocity.Y += GameConstants.Gravity * deltaTime;
            position += velocity * deltaTime;

            if (position.Y + playerSize.Y >= groundTop)
            {
                position.Y = groundTop - playerSize.Y;
                velocity.Y = 0f;
                isOnGround = true;
            }
            else
            {
                isOnGround = false;
            }
        }

        /// <summary>
        /// Clamps a position to screen bounds
        /// </summary>
        public static void ClampToScreen(ref Vector2 position, Vector2 playerSize)
        {
            if (position.X < 0)
                position.X = 0;
            if (position.X + playerSize.X > GameConstants.BaseScreenSize.X)
                position.X = GameConstants.BaseScreenSize.X - playerSize.X;
        }

        /// <summary>
        /// Checks if a player rectangle intersects a platform top
        /// </summary>
        public static bool CheckPlatformCollision(
            Rectangle playerRect,
            Rectangle platformRect,
            Vector2 velocity,
            out Vector2 correctedPosition)
        {
            correctedPosition = new Vector2(playerRect.X, playerRect.Y);

            const int collisionOffset = 2;
            const int collisionHeight = 6;
            const int verticalThreshold = 20;

            Rectangle topRect = new Rectangle(
                platformRect.X, 
                platformRect.Y - collisionOffset, 
                platformRect.Width, 
                collisionHeight);

            if (playerRect.Bottom > topRect.Top &&
                playerRect.Bottom <= topRect.Top + verticalThreshold &&
                playerRect.Right > topRect.Left &&
                playerRect.Left < topRect.Right &&
                velocity.Y >= 0)
            {
                correctedPosition.Y = topRect.Top - playerRect.Height;
                return true;
            }

            return false;
        }
    }
}
