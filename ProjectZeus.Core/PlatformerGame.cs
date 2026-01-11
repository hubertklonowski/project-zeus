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
        private SceneUpdater sceneUpdater;

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);

#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif
            graphics.IsFullScreen = false;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            IsMouseVisible = true;
            
            // Set the window title
            Window.Title = "Project Zeus";
        }

        protected override void LoadContent()
        {
            ScalePresentationArea();
            
            var contentLoader = new GameContentLoader();
            contentLoader.LoadGameContent(
                GraphicsDevice,
                Content,
                out spriteBatch,
                out hudFont,
                out playerTexture,
                out virtualGamePad,
                out player,
                out var pillarRoom,
                out var mineLevel,
                out var mazeLevel,
                out var mountainLevel,
                out var zeusFightScene,
                out var creditsScene,
                globalTransformation);

            sceneManager = new SceneManager(player, pillarRoom, mineLevel, mazeLevel, mountainLevel, zeusFightScene, creditsScene);
            sceneUpdater = new SceneUpdater(sceneManager, player);

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
                    sceneUpdater.UpdateZeusFight(gameTime, keyboardState, RespawnAfterDeath, pos => player.Position = pos);
                    break;
                case SceneManager.GameScene.MazeLevel:
                    sceneManager.MazeLevel.Update(gameTime, keyboardState);
                    if (sceneManager.MazeLevel.PlayerCaughtByMinotaur)
                        RespawnAfterDeath();
                    else
                        sceneManager.HandleMazeLevelCompletion(GraphicsDevice, hudFont, ResetPlayerToPillarRoom);
                    break;
                case SceneManager.GameScene.MineLevel:
                    sceneManager.MineLevel.Update(gameTime, keyboardState, previousKeyboardState);
                    if (sceneManager.MineLevel.PlayerDied)
                        RespawnAfterDeath();
                    else if (!sceneManager.MineLevel.IsActive)
                        sceneManager.HandleMineLevelCompletion(ResetPlayerToPillarRoom);
                    else
                    {
                        player.Position = sceneManager.MineLevel.PlayerPosition;
                        player.Velocity = sceneManager.MineLevel.PlayerVelocity;
                    }
                    break;
                case SceneManager.GameScene.MountainLevel:
                    sceneUpdater.UpdateMountainLevel(gameTime, keyboardState, ResetPlayerToPillarRoom, RespawnAfterDeath);
                    break;
                case SceneManager.GameScene.PillarRoom:
                    sceneUpdater.UpdatePillarRoom(gameTime, keyboardState, previousKeyboardState, ResetPlayerToPillarRoom);
                    break;
                case SceneManager.GameScene.Credits:
                    sceneManager.CreditsScene.Update(gameTime, keyboardState);
                    if (sceneManager.CreditsScene.IsComplete)
                        RespawnAfterDeath();
                    break;
            }

            previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        private void RespawnAfterDeath()
        {
            sceneManager.CurrentScene = SceneManager.GameScene.PillarRoom;
            sceneManager.MountainLevel.Reset();
            sceneManager.MineLevel.Reset();
            
            var newMazeLevel = new MazeLevel();
            newMazeLevel.LoadContent(GraphicsDevice, hudFont);
            sceneManager.ReplaceMazeLevel(newMazeLevel);
            
            var newZeusFightScene = new ZeusFightScene();
            newZeusFightScene.LoadContent(GraphicsDevice, hudFont);
            sceneManager.ReplaceZeusFightScene(newZeusFightScene);
            
            sceneManager.HasCollectedMountainItem = false;
            sceneManager.HasCollectedMazeItem = false;
            sceneManager.HasCollectedMineItem = false;
            
            sceneManager.PillarRoom.ResetItems();
            sceneManager.PillarRoom.MazePortal.IsActive = true;
            sceneManager.PillarRoom.MountainPortal.IsActive = true;
            sceneManager.PillarRoom.MinePortal.IsActive = true;
            
            ResetPlayerToPillarRoom();
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (sceneManager.CurrentScene)
            {
                case SceneManager.GameScene.ZeusFight:
                    sceneManager.ZeusFightScene.Draw(spriteBatch, GraphicsDevice, player, gameTime, sceneManager.ZeusFightScene.PlayerTransformedToGoat);
                    break;

                case SceneManager.GameScene.MazeLevel:
                    sceneManager.MazeLevel.Draw(spriteBatch, GraphicsDevice, player, gameTime);
                    break;

                case SceneManager.GameScene.MountainLevel:
                    sceneManager.MountainLevel.Draw(spriteBatch, GraphicsDevice, gameTime);
                    // Draw player with camera transform applied
                    spriteBatch.Begin(transformMatrix: sceneManager.MountainLevel.GetCameraTransform());
                    
                    // Draw player as goat if they are controlling the goat on top
                    if (sceneManager.IsPlayerGoatOnMountain)
                    {
                        var goatSprite = sceneManager.MountainLevel.GetGoatSprite();
                        if (goatSprite != null && goatSprite.IsLoaded)
                        {
                            // Determine facing direction based on velocity
                            SpriteEffects flip = player.Velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                            bool isMoving = Math.Abs(player.Velocity.X) > 0;
                            goatSprite.Draw(spriteBatch, player.Position, isMoving, gameTime, Color.White, 10f, flip);
                        }
                        else
                        {
                            // Fallback to normal player drawing
                            player.Draw(gameTime, spriteBatch);
                        }
                    }
                    else
                    {
                        player.Draw(gameTime, spriteBatch);
                    }
                    
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
                
                case SceneManager.GameScene.Credits:
                    sceneManager.CreditsScene.Draw(spriteBatch, GraphicsDevice);
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
