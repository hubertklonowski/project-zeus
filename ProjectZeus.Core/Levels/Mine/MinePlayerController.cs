using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Extensions;

namespace ProjectZeus.Core.Levels.Mine
{
    /// <summary>
    /// Handles player physics in the mine level
    /// </summary>
    public class MinePlayerController
    {
        private const float ScreenHeight = 480f;
        private readonly float worldWidth;
        private float groundTop;

        public MinePlayerController(float worldWidth)
        {
            this.worldWidth = worldWidth;
            this.groundTop = ScreenHeight - 20f; // default fallback
        }

        public void SetGroundTop(float groundTop)
        {
            this.groundTop = groundTop;
        }

        public void UpdatePlayer(KeyboardState keyboardState, ref Vector2 playerPosition, ref Vector2 playerVelocity, 
            ref bool playerOnGround, float deltaTime)
        {
            // Player horizontal movement using extension method
            float move = keyboardState.GetHorizontalInput();
            playerVelocity.X = move * GameConstants.MoveSpeed;
            
            // Jump input using extension method
            if (playerOnGround && keyboardState.IsJumpPressed())
            {
                playerVelocity.Y = GameConstants.JumpVelocity;
                playerOnGround = false;
            }

            // Apply gravity
            playerVelocity.Y += GameConstants.Gravity * deltaTime;
            
            // Update player position
            playerPosition += playerVelocity * deltaTime;
            
            // Clamp player to world bounds
            if (playerPosition.X < 0)
                playerPosition.X = 0;
            if (playerPosition.X + GameConstants.PlayerSize.X > worldWidth)
                playerPosition.X = worldWidth - GameConstants.PlayerSize.X;

            // Ground collision using actual ground top
            playerOnGround = false;
            
            if (playerPosition.Y + GameConstants.PlayerSize.Y >= groundTop)
            {
                playerPosition.Y = groundTop - GameConstants.PlayerSize.Y;
                playerVelocity.Y = 0f;
                playerOnGround = true;
            }
        }
    }
}
