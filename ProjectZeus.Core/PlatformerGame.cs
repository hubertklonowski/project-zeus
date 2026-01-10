#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Levels;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Utilities;
using ProjectZeus.Core.Game;

namespace ProjectZeus.Core
{
    /// <summary>
    /// Main game orchestrator that manages scene transitions
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Matrix globalTransformation;
        private int backbufferWidth, backbufferHeight;

        private SpriteFont hudFont;
        private Texture2D playerTexture;

        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private KeyboardState previousKeyboardState;
        private TouchCollection touchState;
        private VirtualGamePad virtualGamePad;

        private SceneManager sceneManager;
        private AdonisPlayer player;

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);

#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif
            graphics.IsFullScreen = false;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            this.Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(GraphicsDevice);
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            ScalePresentationArea();

            virtualGamePad = new VirtualGamePad(GameConstants.BaseScreenSize, globalTransformation, 
                Content.Load<Texture2D>("Sprites/VirtualControlArrow"));

            if (!OperatingSystem.IsIOS())
            {
                try
                {
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
                }
                catch { }
            }

            playerTexture = DrawingHelpers.CreateSolidTexture(GraphicsDevice, 1, 1, new Color(255, 220, 180));

            player = new AdonisPlayer();
            player.LoadContent(GraphicsDevice);

            var pillarRoom = new PillarRoom();
            pillarRoom.LoadContent(GraphicsDevice, hudFont);

            var mineLevel = new MineLevel();
            mineLevel.LoadContent(GraphicsDevice, hudFont);

            var mazeLevel = new MazeLevel();
            mazeLevel.LoadContent(GraphicsDevice, hudFont);

            var mountainLevel = new MountainLevel();
            mountainLevel.LoadContent(GraphicsDevice, hudFont);

            var zeusFightScene = new ZeusFightScene();
            zeusFightScene.LoadContent(GraphicsDevice, hudFont);

            sceneManager = new SceneManager(player, pillarRoom, mineLevel, mazeLevel, mountainLevel, zeusFightScene);

            ResetPlayerToPillarRoom();
            
            previousKeyboardState = Keyboard.GetState();
        }

        public void ScalePresentationArea()
        {
            backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            float horScaling = backbufferWidth / GameConstants.BaseScreenSize.X;
            float verScaling = backbufferHeight / GameConstants.BaseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            globalTransformation = Matrix.CreateScale(screenScalingFactor);
        }

        private void ResetPlayerToPillarRoom()
        {
            float centerX = GameConstants.BaseScreenSize.X / 2f;
            float spacing = 200f;
            float groundTop = GameConstants.BaseScreenSize.Y - GameConstants.GroundHeight;
            
            // Use player.Size for consistent positioning
            player.Position = new Vector2(centerX - spacing - 80f, groundTop - player.Size.Y);
            player.Velocity = Vector2.Zero;
            player.IsOnGround = true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (backbufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
                backbufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
            {
                ScalePresentationArea();
            }

            HandleInput(gameTime);

            switch (sceneManager.CurrentScene)
            {
                case SceneManager.GameScene.ZeusFight:
                    sceneManager.ZeusFightScene.Update(gameTime);
                    break;

                case SceneManager.GameScene.MazeLevel:
                    sceneManager.MazeLevel.Update(gameTime, keyboardState);
                    
                    // Handle player caught by minotaur - use unified death handler
                    if (sceneManager.MazeLevel.PlayerCaughtByMinotaur)
                    {
                        RespawnAfterDeath();
                    }
                    else
                    {
                        sceneManager.HandleMazeLevelCompletion(GraphicsDevice, hudFont, ResetPlayerToPillarRoom);
                    }
                    break;

                case SceneManager.GameScene.MineLevel:
                    sceneManager.MineLevel.Update(gameTime, keyboardState, previousKeyboardState);
                    
                    // Handle death - use unified death handler
                    if (sceneManager.MineLevel.PlayerDied)
                    {
                        RespawnAfterDeath();
                    }
                    else if (!sceneManager.MineLevel.IsActive)
                    {
                        sceneManager.HandleMineLevelCompletion(ResetPlayerToPillarRoom);
                    }
                    else
                    {
                        player.Position = sceneManager.MineLevel.PlayerPosition;
                        player.Velocity = sceneManager.MineLevel.PlayerVelocity;
                    }
                    break;

                case SceneManager.GameScene.MountainLevel:
                    UpdateMountainLevel(gameTime);
                    break;

                case SceneManager.GameScene.PillarRoom:
                    UpdatePillarRoom(gameTime);
                    break;
            }

            previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        private void UpdatePillarRoom(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float groundTop = GameConstants.BaseScreenSize.Y - GameConstants.GroundHeight;

            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;

            player.Velocity = new Vector2(move * GameConstants.MoveSpeed, player.Velocity.Y);

            if (player.IsOnGround && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up)))
            {
                player.Velocity = new Vector2(player.Velocity.X, GameConstants.JumpVelocity);
                player.IsOnGround = false;
            }

            player.Velocity = new Vector2(player.Velocity.X, player.Velocity.Y + GameConstants.Gravity * dt);
            player.Position += player.Velocity * dt;

            player.IsOnGround = false;

            // Use player.Size for consistent collision with visual size
            Vector2 playerSize = player.Size;
            
            if (player.Position.Y + playerSize.Y >= groundTop)
            {
                player.Position = new Vector2(player.Position.X, groundTop - playerSize.Y);
                player.Velocity = new Vector2(player.Velocity.X, 0f);
                player.IsOnGround = true;
            }

            foreach (Pillar pillar in sceneManager.PillarRoom.Pillars)
            {
                Rectangle pillarRect = pillar.GetPillarRectangle();
                Vector2 correctedPos;
                if (Physics.PlatformerPhysics.CheckPlatformCollision(player.Bounds, pillarRect, player.Velocity, out correctedPos))
                {
                    player.Position = correctedPos;
                    player.Velocity = new Vector2(player.Velocity.X, 0f);
                    player.IsOnGround = true;
                }
            }

            Vector2 tempPos = player.Position;
            Physics.PlatformerPhysics.ClampToScreen(ref tempPos, playerSize);
            player.Position = tempPos;
            player.Update(gameTime);

            bool hasAnyItem = sceneManager.HasCollectedMazeItem || sceneManager.HasCollectedMineItem || sceneManager.HasCollectedMountainItem;
            bool eKeyPressed = keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E);

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
                // Spawn player at the bottom of the extended mountain level
                player.Position = sceneManager.MountainLevel.GetPlayerSpawnPosition(playerSize);
                player.Velocity = Vector2.Zero;
                player.IsOnGround = true;
                return;
            }

            if (sceneManager.PillarRoom.AllItemsInserted)
            {
                sceneManager.CurrentScene = SceneManager.GameScene.ZeusFight;
                float fightGroundTop = GameConstants.BaseScreenSize.Y * 0.7f;
                // Position player on the right side, facing left toward Zeus
                player.Position = new Vector2(GameConstants.BaseScreenSize.X - playerSize.X - 40f, 
                    fightGroundTop - playerSize.Y);
                // Set negative velocity to make player face left
                player.Velocity = new Vector2(-1f, 0f);
                player.IsOnGround = true;
            }
        }

        private void UpdateMountainLevel(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;

            player.Velocity = new Vector2(move * GameConstants.MoveSpeed, player.Velocity.Y);

            if (player.IsOnGround && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up)))
            {
                // Reduced jump height for mountain level to increase difficulty
                float mountainJumpVelocity = GameConstants.JumpVelocity * GameConstants.MountainJumpReduction;
                player.Velocity = new Vector2(player.Velocity.X, mountainJumpVelocity);
                player.IsOnGround = false;
            }

            player.Velocity = new Vector2(player.Velocity.X, player.Velocity.Y + GameConstants.Gravity * dt);
            player.Position += player.Velocity * dt;

            // Use player.Size for consistent collision
            Vector2 playerSize = player.Size;

            Vector2 correctedPosition;
            player.IsOnGround = sceneManager.MountainLevel.CheckPlatformCollision(player.Bounds, player.Velocity, out correctedPosition);

            if (player.IsOnGround)
            {
                player.Position = correctedPosition;
                player.Velocity = new Vector2(player.Velocity.X, 0f);
            }

            // Clamp player X position to screen bounds, Y position to world bounds
            Vector2 tempPos = player.Position;
            
            // Clamp X to screen
            if (tempPos.X < 0)
                tempPos.X = 0;
            if (tempPos.X + playerSize.X > GameConstants.BaseScreenSize.X)
                tempPos.X = GameConstants.BaseScreenSize.X - playerSize.X;
            
            // Clamp Y to world bounds (extended level height)
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
            sceneManager.MountainLevel.Update(gameTime, player.Position, playerSize, tryPickupItem);
            player.Update(gameTime);

            sceneManager.HandleMountainLevelCompletion(ResetPlayerToPillarRoom, RespawnAfterDeath);

            const float leftEdgeThreshold = 10f;
            if (player.Position.X <= leftEdgeThreshold && sceneManager.HasCollectedMountainItem)
            {
                sceneManager.CurrentScene = SceneManager.GameScene.PillarRoom;
                ResetPlayerToPillarRoom();
            }
        }

        private void RespawnAfterDeath()
        {
            // Unified death handler - resets all progress regardless of which level player died in
            sceneManager.CurrentScene = SceneManager.GameScene.PillarRoom;
            
            // Reset all levels
            sceneManager.MountainLevel.Reset();
            sceneManager.MineLevel.Reset();
            
            // Recreate maze level (it doesn't have a Reset method, needs fresh instance)
            var newMazeLevel = new MazeLevel();
            newMazeLevel.LoadContent(GraphicsDevice, hudFont);
            sceneManager.ReplaceMazeLevel(newMazeLevel);
            
            // Clear all collected items
            sceneManager.HasCollectedMountainItem = false;
            sceneManager.HasCollectedMazeItem = false;
            sceneManager.HasCollectedMineItem = false;
            
            // Reset pillar room state
            sceneManager.PillarRoom.ResetItems();
            sceneManager.PillarRoom.MazePortal.IsActive = true;
            sceneManager.PillarRoom.MountainPortal.IsActive = true;
            sceneManager.PillarRoom.MinePortal.IsActive = true;
            
            // Reset player position
            ResetPlayerToPillarRoom();
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (sceneManager.CurrentScene)
            {
                case SceneManager.GameScene.ZeusFight:
                    sceneManager.ZeusFightScene.Draw(spriteBatch, GraphicsDevice, player, gameTime);
                    break;

                case SceneManager.GameScene.MazeLevel:
                    sceneManager.MazeLevel.Draw(spriteBatch, GraphicsDevice, player, gameTime);
                    break;

                case SceneManager.GameScene.MountainLevel:
                    sceneManager.MountainLevel.Draw(spriteBatch, GraphicsDevice, gameTime);
                    // Draw player with camera transform applied
                    spriteBatch.Begin(transformMatrix: sceneManager.MountainLevel.GetCameraTransform());
                    player.Draw(gameTime, spriteBatch);
                    spriteBatch.End();
                    break;

                case SceneManager.GameScene.MineLevel:
                    // Draw world with camera transform
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, 
                        sceneManager.MineLevel.GetCameraTransform());
                    sceneManager.MineLevel.Draw(spriteBatch, GraphicsDevice, gameTime, player,
                        DrawingHelpers.CreateSolidTexture(GraphicsDevice, 1, 1, Color.White));
                    spriteBatch.End();
                    
                    // Draw UI without camera transform
                    spriteBatch.Begin();
                    sceneManager.MineLevel.DrawUI(spriteBatch);
                    spriteBatch.End();
                    break;

                case SceneManager.GameScene.PillarRoom:
                    graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, globalTransformation);
                    
                    bool hasAnyItem = sceneManager.PillarRoom.CurrentCarriedItem != PillarItemType.None;
                    sceneManager.PillarRoom.Draw(spriteBatch, gameTime, hasAnyItem);
                    player.Draw(gameTime, spriteBatch);
                    sceneManager.PillarRoom.DrawUI(spriteBatch, playerTexture, hasAnyItem);
                    
                    spriteBatch.End();
                    break;
            }

            base.Draw(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();
            touchState = TouchPanel.GetState();
            gamePadState = virtualGamePad.GetState(touchState, GamePad.GetState(PlayerIndex.One));

            if (!OperatingSystem.IsIOS())
            {
                if (gamePadState.Buttons.Back == ButtonState.Pressed)
                    Exit();
            }

#if DEBUG
            // F key shortcut to jump to Zeus fight scene
            if (keyboardState.IsKeyDown(Keys.F) && !previousKeyboardState.IsKeyDown(Keys.F))
            {
                sceneManager.CurrentScene = SceneManager.GameScene.ZeusFight;
                float fightGroundTop = GameConstants.BaseScreenSize.Y * 0.7f;
                // Position player on the right side, facing left toward Zeus
                player.Position = new Vector2(GameConstants.BaseScreenSize.X - player.Size.X - 40f, 
                    fightGroundTop - player.Size.Y);
                // Set negative velocity to make player face left
                player.Velocity = new Vector2(-1f, 0f);
                player.IsOnGround = true;
            }
#endif

            virtualGamePad.Update(gameTime);
        }
    }
}
