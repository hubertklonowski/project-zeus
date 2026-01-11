using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Constants;

namespace ProjectZeus.Core.Levels.ZeusFight
{
    public class ZeusPhysicsController
    {
        private const float zeusJumpSpeed = -600f;
        private const float zeusGravity = 1200f;
        private const float zeusHorizontalSpeed = 200f;

        public void UpdateZeusJump(ref Vector2 zeusPosition, ref Vector2 zeusVelocity, ref bool zeusIsJumping,
            ref bool zeusHasLandedOnPlayer, Vector2 zeusJumpStartPosition, Vector2 playerPosition, 
            Vector2 playerSize, float groundTop, float deltaTime)
        {
            if (!zeusIsJumping) return;

            zeusVelocity = new Vector2(zeusVelocity.X, zeusVelocity.Y + zeusGravity * deltaTime);
            zeusPosition += zeusVelocity * deltaTime;

            if (zeusPosition.Y >= groundTop - 120)
            {
                zeusPosition = new Vector2(zeusPosition.X, groundTop - 120);
                zeusVelocity = Vector2.Zero;
                zeusIsJumping = false;

                Rectangle zeusRect = new Rectangle((int)zeusPosition.X, (int)zeusPosition.Y, 80, 120);
                Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 
                    (int)playerSize.X, (int)playerSize.Y);

                if (zeusRect.Intersects(playerRect))
                {
                    zeusHasLandedOnPlayer = true;
                }
            }
        }

        public void InitiateZeusJump(ref Vector2 zeusPosition, ref Vector2 zeusVelocity, ref bool zeusIsJumping,
            ref Vector2 zeusJumpStartPosition, Vector2 playerPosition)
        {
            zeusJumpStartPosition = zeusPosition;
            zeusIsJumping = true;

            float horizontalDirection = playerPosition.X > zeusPosition.X ? 1f : -1f;
            zeusVelocity = new Vector2(horizontalDirection * zeusHorizontalSpeed, zeusJumpSpeed);
        }
    }
}
