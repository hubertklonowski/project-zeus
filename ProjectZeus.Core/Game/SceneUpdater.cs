using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Levels;
using ProjectZeus.Core.Extensions;
using ProjectZeus.Core.Physics;

namespace ProjectZeus.Core.Game
{
    /// <summary>
    /// Handles scene-specific update logic
    /// </summary>
    public class SceneUpdater
    {
        private readonly SceneManager sceneManager;
        private readonly AdonisPlayer player;

        public SceneUpdater(SceneManager sceneManager, AdonisPlayer player)
        {
            this.sceneManager = sceneManager;
            this.player = player;
        }

        public void UpdatePillarRoom(GameTime gameTime, KeyboardState keyboardState, KeyboardState previousKeyboardState,
            System.Action resetPlayerToPillarRoom)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float groundTop = GameConstants.BaseScreenSize.Y - GameConstants.GroundHeight;

            float move = keyboardState.GetHorizontalInput();
            player.Velocity = new Vector2(move * GameConstants.MoveSpeed, player.Velocity.Y);

            if (player.IsOnGround && keyboardState.IsJumpPressed())
            {
                player.Velocity = new Vector2(player.Velocity.X, GameConstants.JumpVelocity);
                player.IsOnGround = false;
            }

            player.Velocity = new Vector2(player.Velocity.X, player.Velocity.Y + GameConstants.Gravity * dt);
            player.Position += player.Velocity * dt;

            player.IsOnGround = false;
            Vector2 playerSize = player.Size;
            
            if (player.Position.Y + playerSize.Y >= groundTop)
            {
                player.Position = new Vector2(player.Position.X, groundTop - playerSize.Y);
                player.Velocity = new Vector2(player.Velocity.X, 0f);
                player.IsOnGround = true;
            }

            foreach (var pillar in sceneManager.PillarRoom.Pillars)
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

            Vector2 tempPos = player.Position;
            PlatformerPhysics.ClampToScreen(ref tempPos, playerSize);
            player.Position = tempPos;
            player.Update(gameTime);

            bool hasAnyItem = sceneManager.HasCollectedMazeItem || sceneManager.HasCollectedMineItem || sceneManager.HasCollectedMountainItem;
            bool eKeyPressed = keyboardState.WasActionPressed(previousKeyboardState);

            if (eKeyPressed && hasAnyItem && sceneManager.PillarRoom.CurrentCarriedItem != PillarItemType.None)
            {
                if (sceneManager.PillarRoom.TryInsertItem(player.Position, playerSize))
                {
                    sceneManager.HasCollectedMazeItem = false;
                    sceneManager.HasCollectedMineItem = false;
                    sceneManager.HasCollectedMountainItem = false;
                }
            }

            if (eKeyPressed && sceneManager.PillarRoom.MazePortal.Intersects(player.Bounds) && !sceneManager.HasCollectedMazeItem)
            {
                sceneManager.CurrentScene = SceneManager.GameScene.MazeLevel;
                return;
            }

            if (eKeyPressed && sceneManager.PillarRoom.MinePortal.Intersects(player.Bounds) && !sceneManager.HasCollectedMineItem)
            {
                sceneManager.CurrentScene = SceneManager.GameScene.MineLevel;
                sceneManager.MineLevel.Enter();
                return;
            }

            if (eKeyPressed && sceneManager.PillarRoom.MountainPortal.Intersects(player.Bounds) && !sceneManager.HasCollectedMountainItem)
            {
                sceneManager.CurrentScene = SceneManager.GameScene.MountainLevel;
                sceneManager.MountainLevel.Reset();
                player.Position = sceneManager.MountainLevel.GetPlayerSpawnPosition(playerSize);
                player.Velocity = Vector2.Zero;
                player.IsOnGround = true;
                return;
            }

            if (sceneManager.PillarRoom.AllItemsInserted)
            {
                sceneManager.CurrentScene = SceneManager.GameScene.ZeusFight;
                float fightGroundTop = GameConstants.BaseScreenSize.Y * 0.7f;
                player.Position = new Vector2(GameConstants.BaseScreenSize.X - playerSize.X - 40f, 
                    fightGroundTop - playerSize.Y);
                player.Velocity = new Vector2(-1f, 0f);
                player.IsOnGround = true;
            }
        }

        public void UpdateMountainLevel(GameTime gameTime, KeyboardState keyboardState,
            System.Action resetPlayerToPillarRoom, System.Action respawnAfterDeath)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float move = keyboardState.GetHorizontalInput();
            player.Velocity = new Vector2(move * GameConstants.MoveSpeed, player.Velocity.Y);

            bool isPlayerGoatOnTop = sceneManager.IsPlayerGoatOnMountain;
            
            if (player.IsOnGround && keyboardState.IsJumpPressed())
            {
                if (!isPlayerGoatOnTop)
                {
                    float mountainJumpVelocity = GameConstants.JumpVelocity * GameConstants.MountainJumpReduction;
                    player.Velocity = new Vector2(player.Velocity.X, mountainJumpVelocity);
                    player.IsOnGround = false;
                }
            }

            player.Velocity = new Vector2(player.Velocity.X, player.Velocity.Y + GameConstants.Gravity * dt);
            player.Position += player.Velocity * dt;

            Vector2 playerSize = player.Size;
            Vector2 correctedPosition;
            player.IsOnGround = sceneManager.MountainLevel.CheckPlatformCollision(player.Bounds, player.Velocity, out correctedPosition);

            if (player.IsOnGround)
            {
                player.Position = correctedPosition;
                player.Velocity = new Vector2(player.Velocity.X, 0f);
            }

            Vector2 tempPos = player.Position;
            
            if (isPlayerGoatOnTop)
            {
                float topPlatformLeft = 200f;
                float topPlatformRight = 600f;
                
                if (tempPos.X < topPlatformLeft)
                    tempPos.X = topPlatformLeft;
                if (tempPos.X + playerSize.X > topPlatformRight)
                    tempPos.X = topPlatformRight - playerSize.X;
            }
            else
            {
                if (tempPos.X < 0)
                    tempPos.X = 0;
                if (tempPos.X + playerSize.X > GameConstants.BaseScreenSize.X)
                    tempPos.X = GameConstants.BaseScreenSize.X - playerSize.X;
            }
            
            float worldHeight = sceneManager.MountainLevel.WorldHeight;
            if (tempPos.Y < 0)
                tempPos.Y = 0;
            if (tempPos.Y + playerSize.Y > worldHeight - GameConstants.GroundHeight)
            {
                tempPos.Y = worldHeight - GameConstants.GroundHeight - playerSize.Y;
                player.Velocity = new Vector2(player.Velocity.X, 0f);
                player.IsOnGround = true;
            }
            
            player.Position = tempPos;

            bool tryPickupItem = keyboardState.IsKeyDown(Keys.E);
            
            if (isPlayerGoatOnTop)
            {
                sceneManager.MountainLevel.Update(gameTime, player.Position, playerSize, tryPickupItem, keyboardState);
            }
            else
            {
                sceneManager.MountainLevel.Update(gameTime, player.Position, playerSize, tryPickupItem);
            }
            
            player.Update(gameTime);

            sceneManager.HandleMountainLevelCompletion(resetPlayerToPillarRoom, respawnAfterDeath);

            const float leftEdgeThreshold = 10f;
            if (player.Position.X <= leftEdgeThreshold && sceneManager.HasCollectedMountainItem)
            {
                sceneManager.CurrentScene = SceneManager.GameScene.PillarRoom;
                resetPlayerToPillarRoom();
            }
        }

        public void UpdateZeusFight(GameTime gameTime, KeyboardState keyboardState,
            System.Action respawnAfterDeath, System.Action<Vector2> setPlayerPosition)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float groundTop = GameConstants.BaseScreenSize.Y * 0.7f;
            Vector2 playerSize = player.Size;

            var (newVelocity, newIsOnGround, transformedToGoat) = sceneManager.ZeusFightScene.Update(
                gameTime, keyboardState, player.Position, playerSize, player.Velocity, player.IsOnGround);
            
            player.Velocity = newVelocity;
            player.IsOnGround = newIsOnGround;

            player.Position += player.Velocity * dt;

            player.IsOnGround = false;
            if (player.Position.Y + playerSize.Y >= groundTop)
            {
                player.Position = new Vector2(player.Position.X, groundTop - playerSize.Y);
                player.Velocity = new Vector2(player.Velocity.X, 0f);
                player.IsOnGround = true;
            }

            Vector2 tempPos = player.Position;
            PlatformerPhysics.ClampToScreen(ref tempPos, playerSize);
            player.Position = tempPos;
            
            player.Update(gameTime);

            if (sceneManager.ZeusFightScene.ShouldRestartGame)
            {
                respawnAfterDeath();
            }
            
            sceneManager.HandleZeusFightCompletion(setPlayerPosition);
        }
    }
}
