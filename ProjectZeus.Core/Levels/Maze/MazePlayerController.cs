using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Extensions;

namespace ProjectZeus.Core.Levels.Maze
{
    /// <summary>
    /// Handles player movement in the maze
    /// </summary>
    public class MazePlayerController
    {
        private readonly int mazeWidth;
        private readonly int mazeHeight;
        private readonly int cellSize;

        public MazePlayerController(int mazeWidth, int mazeHeight, int cellSize)
        {
            this.mazeWidth = mazeWidth;
            this.mazeHeight = mazeHeight;
            this.cellSize = cellSize;
        }

        public void UpdatePlayer(KeyboardState keyboardState, ref Vector2 playerPosition, ref Vector2 playerVelocity,
            Vector2 playerCollisionSize, float deltaTime, bool[,] walls)
        {
            const float moveSpeed = 120f;
            
            // Player movement using extensions
            Vector2 movement = new Vector2(
                keyboardState.GetHorizontalInput(),
                keyboardState.GetVerticalInput());
            
            if (movement.LengthSquared() > 0)
                movement.Normalize();
            
            playerVelocity = movement * moveSpeed;
            Vector2 newPosition = playerPosition + playerVelocity * deltaTime;
            
            // Collision detection with walls
            if (!CheckWallCollision(newPosition, playerCollisionSize, walls))
            {
                playerPosition = newPosition;
            }
            else
            {
                // Try moving only on X axis
                Vector2 xOnly = new Vector2(newPosition.X, playerPosition.Y);
                if (!CheckWallCollision(xOnly, playerCollisionSize, walls))
                {
                    playerPosition = xOnly;
                }
                else
                {
                    // Try moving only on Y axis
                    Vector2 yOnly = new Vector2(playerPosition.X, newPosition.Y);
                    if (!CheckWallCollision(yOnly, playerCollisionSize, walls))
                    {
                        playerPosition = yOnly;
                    }
                }
            }
        }

        private bool CheckWallCollision(Vector2 position, Vector2 size, bool[,] walls)
        {
            const float inset = 2f;
            float adjustedX = position.X + inset;
            float adjustedY = position.Y + inset;
            float adjustedWidth = size.X - inset * 2;
            float adjustedHeight = size.Y - inset * 2;
            
            int left = (int)(adjustedX / cellSize);
            int right = (int)((adjustedX + adjustedWidth) / cellSize);
            int top = (int)(adjustedY / cellSize);
            int bottom = (int)((adjustedY + adjustedHeight) / cellSize);
            
            left = System.Math.Max(0, System.Math.Min(mazeWidth - 1, left));
            right = System.Math.Max(0, System.Math.Min(mazeWidth - 1, right));
            top = System.Math.Max(0, System.Math.Min(mazeHeight - 1, top));
            bottom = System.Math.Max(0, System.Math.Min(mazeHeight - 1, bottom));
            
            if (walls[left, top] || walls[right, top] || 
                walls[left, bottom] || walls[right, bottom])
            {
                return true;
            }
            
            return false;
        }
    }
}
