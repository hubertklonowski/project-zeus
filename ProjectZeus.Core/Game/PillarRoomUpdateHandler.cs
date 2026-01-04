using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Levels;
using ProjectZeus.Core.Physics;

namespace ProjectZeus.Core.Game
{
    /// <summary>
    /// Handles player update logic for the Pillar Room scene
    /// </summary>
    public class PillarRoomUpdateHandler
    {
        public void Update(GameTime gameTime, AdonisPlayer player, PillarRoom pillarRoom, 
            InputManager input, SceneManager sceneManager, MineLevel mineLevel, MountainLevel mountainLevel)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float groundTop = GameConstants.BaseScreenSize.Y - GameConstants.GroundHeight;

            // Player movement
            float move = input.GetHorizontalMovement();
            player.Velocity = new Vector2(move * GameConstants.MoveSpeed, player.Velocity.Y);

            // Jumping
            if (player.IsOnGround && input.IsJumpPressed())
            {
                player.Velocity = new Vector2(player.Velocity.X, GameConstants.JumpVelocity);
                player.IsOnGround = false;
            }

            // Apply gravity
            player.Velocity = new Vector2(player.Velocity.X, player.Velocity.Y + GameConstants.Gravity * dt);
            player.Position += player.Velocity * dt;

            player.IsOnGround = false;

            // Ground collision
            if (player.Position.Y + player.Size.Y >= groundTop)
            {
                player.Position = new Vector2(player.Position.X, groundTop - player.Size.Y);
                player.Velocity = new Vector2(player.Velocity.X, 0f);
                player.IsOnGround = true;
            }

            // Pillar collisions
            foreach (Pillar pillar in pillarRoom.Pillars)
            {
                Rectangle pillarRect = pillar.GetPillarRectangle();
                Vector2 correctedPos;
                if (PlatformerPhysics.CheckPlatformCollision(player.Bounds, pillarRect, player.Velocity, out correctedPos))
                {
                    player.Position = correctedPos;
                    player.Velocity = new Vector2(player.Velocity.X, 0f);
                    player.IsOnGround = true;
                }
            }

            // Clamp to screen
            Vector2 tempPos = player.Position;
            PlatformerPhysics.ClampToScreen(ref tempPos, player.Size);
            player.Position = tempPos;
            player.Update(gameTime);

            // Handle item insertion
            if (input.IsKeyJustPressed(Keys.E) && sceneManager.HasAnyItem)
            {
                if (pillarRoom.TryInsertItem(player.Position, player.Size))
                {
                    sceneManager.ResetItems();
                }
            }

            // Handle scene transitions
            sceneManager.HandlePillarRoomTransitions(player, pillarRoom, mineLevel, mountainLevel, input);
        }
    }
}
