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
using Platformer2D;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Levels;
using ProjectZeus.Core.Rendering;

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

        private AdonisPlayer player;
        private PillarRoom pillarRoom;
        private MineLevel mineLevel;
        private MazeLevel mazeLevel;
        private MountainLevel mountainLevel;
        private ZeusFightScene zeusFightScene;

        private bool hasCollectedMazeItem;
        private bool hasCollectedMineItem;
        private bool hasCollectedMountainItem;

        private enum GameScene
        {
            PillarRoom,
            MineLevel,
            MazeLevel,
            MountainLevel,
            ZeusFight
        }

        private GameScene currentScene = GameScene.PillarRoom;

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
            player.LoadContent(Content);

            pillarRoom = new PillarRoom();
            pillarRoom.LoadContent(GraphicsDevice, hudFont);

            mineLevel = new MineLevel();
            mineLevel.LoadContent(GraphicsDevice, hudFont);

            mazeLevel = new MazeLevel();
            mazeLevel.LoadContent(GraphicsDevice, hudFont);

            mountainLevel = new MountainLevel();
            mountainLevel.LoadContent(GraphicsDevice, hudFont);

            zeusFightScene = new ZeusFightScene();
            zeusFightScene.LoadContent(GraphicsDevice, hudFont);

            ResetPlayerToPillarRoom();
            
            hasCollectedMazeItem = false;
            hasCollectedMineItem = false;
            hasCollectedMountainItem = false;
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
            
            player.Position = new Vector2(centerX - spacing - 80f, groundTop - GameConstants.PlayerSize.Y);
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

            switch (currentScene)
            {
                case GameScene.ZeusFight:
                    zeusFightScene.Update(gameTime);
                    break;

                case GameScene.MazeLevel:
                    mazeLevel.Update(gameTime, keyboardState);
                    if (mazeLevel.IsCompleted)
                    {
                        currentScene = GameScene.PillarRoom;
                        hasCollectedMazeItem = mazeLevel.HasItem;
                        if (hasCollectedMazeItem)
                        {
                            pillarRoom.MazePortal.IsActive = false;
                        }
                        mazeLevel = new MazeLevel();
                        mazeLevel.LoadContent(GraphicsDevice, hudFont);
                        ResetPlayerToPillarRoom();
                    }
                    break;

                case GameScene.MountainLevel:
                    UpdateMountainLevel(gameTime);
                    break;

                case GameScene.MineLevel:
                    mineLevel.Update(gameTime, keyboardState, previousKeyboardState);
                    if (!mineLevel.IsActive)
                    {
                        currentScene = GameScene.PillarRoom;
                        if (mineLevel.HasCollectedItem)
                        {
                            hasCollectedMineItem = true;
                            pillarRoom.MinePortal.IsActive = false;
                        }
                        ResetPlayerToPillarRoom();
                    }
                    else
                    {
                        player.Position = mineLevel.PlayerPosition;
                        player.Velocity = mineLevel.PlayerVelocity;
                    }
                    break;

                case GameScene.PillarRoom:
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

            if (player.Position.Y + GameConstants.PlayerSize.Y >= groundTop)
            {
                player.Position = new Vector2(player.Position.X, groundTop - GameConstants.PlayerSize.Y);
                player.Velocity = new Vector2(player.Velocity.X, 0f);
                player.IsOnGround = true;
            }

            foreach (Pillar pillar in pillarRoom.Pillars)
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
            Physics.PlatformerPhysics.ClampToScreen(ref tempPos, GameConstants.PlayerSize);
            player.Position = tempPos;
            player.Update(gameTime);

            bool hasAnyItem = hasCollectedMazeItem || hasCollectedMineItem || hasCollectedMountainItem;
            bool eKeyPressed = keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E);

            if (eKeyPressed && hasAnyItem)
            {
                if (pillarRoom.TryInsertItem(player.Position, GameConstants.PlayerSize))
                {
                    hasCollectedMazeItem = false;
                    hasCollectedMineItem = false;
                    hasCollectedMountainItem = false;
                }
            }

            if (pillarRoom.MazePortal.Intersects(player.Bounds) && !hasCollectedMazeItem)
            {
                currentScene = GameScene.MazeLevel;
                return;
            }

            if (pillarRoom.MinePortal.Intersects(player.Bounds) && !hasCollectedMineItem)
            {
                currentScene = GameScene.MineLevel;
                mineLevel.Enter();
                return;
            }

            if (eKeyPressed && pillarRoom.MountainPortal.Intersects(player.Bounds) && !hasCollectedMountainItem)
            {
                currentScene = GameScene.MountainLevel;
                mountainLevel.Reset();
                player.Position = new Vector2(60f, groundTop - GameConstants.PlayerSize.Y);
                player.Velocity = Vector2.Zero;
                player.IsOnGround = true;
                return;
            }

            if (pillarRoom.AllItemsInserted)
            {
                currentScene = GameScene.ZeusFight;
                float fightGroundTop = GameConstants.BaseScreenSize.Y * 0.7f;
                player.Position = new Vector2(GameConstants.BaseScreenSize.X - GameConstants.PlayerSize.X - 40f, 
                    fightGroundTop - GameConstants.PlayerSize.Y);
                player.Velocity = Vector2.Zero;
                player.IsOnGround = true;
            }
        }

        private void UpdateMountainLevel(GameTime gameTime)
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

            Vector2 correctedPosition;
            player.IsOnGround = mountainLevel.CheckPlatformCollision(player.Bounds, player.Velocity, out correctedPosition);

            if (player.IsOnGround)
            {
                player.Position = correctedPosition;
                player.Velocity = new Vector2(player.Velocity.X, 0f);
            }

            Vector2 tempPos2 = player.Position;
            Physics.PlatformerPhysics.ClampToScreen(ref tempPos2, GameConstants.PlayerSize);
            player.Position = tempPos2;

            bool tryPickupItem = keyboardState.IsKeyDown(Keys.E);
            mountainLevel.Update(gameTime, player.Position, GameConstants.PlayerSize, tryPickupItem);
            player.Update(gameTime);

            if (mountainLevel.PlayerDied)
            {
                RespawnAfterDeath();
            }

            if (mountainLevel.ItemWasCollected && !hasCollectedMountainItem)
            {
                hasCollectedMountainItem = true;
                pillarRoom.MountainPortal.IsActive = false;
                currentScene = GameScene.PillarRoom;
                ResetPlayerToPillarRoom();
            }

            const float leftEdgeThreshold = 10f;
            if (player.Position.X <= leftEdgeThreshold && hasCollectedMountainItem)
            {
                currentScene = GameScene.PillarRoom;
                ResetPlayerToPillarRoom();
            }
        }

        private void RespawnAfterDeath()
        {
            currentScene = GameScene.PillarRoom;
            mountainLevel.Reset();
            hasCollectedMountainItem = false;
            hasCollectedMazeItem = false;
            hasCollectedMineItem = false;
            pillarRoom.ResetItems();
            pillarRoom.MazePortal.IsActive = true;
            pillarRoom.MountainPortal.IsActive = true;
            pillarRoom.MinePortal.IsActive = true;
            ResetPlayerToPillarRoom();
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (currentScene)
            {
                case GameScene.ZeusFight:
                    zeusFightScene.Draw(spriteBatch, GraphicsDevice, playerTexture, player.Position, GameConstants.PlayerSize);
                    break;

                case GameScene.MazeLevel:
                    mazeLevel.Draw(spriteBatch, GraphicsDevice);
                    break;

                case GameScene.MountainLevel:
                    mountainLevel.Draw(spriteBatch, GraphicsDevice);
                    spriteBatch.Begin();
                    player.Draw(gameTime, spriteBatch);
                    spriteBatch.End();
                    break;

                case GameScene.MineLevel:
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, globalTransformation);
                    mineLevel.Draw(spriteBatch, GraphicsDevice, gameTime, playerTexture, 
                        DrawingHelpers.CreateSolidTexture(GraphicsDevice, 1, 1, Color.White));
                    spriteBatch.End();
                    break;

                case GameScene.PillarRoom:
                    graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, globalTransformation);
                    
                    bool hasAnyItem = hasCollectedMazeItem || hasCollectedMineItem || hasCollectedMountainItem;
                    pillarRoom.Draw(spriteBatch, gameTime, hasAnyItem);
                    player.Draw(gameTime, spriteBatch);
                    pillarRoom.DrawUI(spriteBatch, playerTexture, hasAnyItem);
                    
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

            virtualGamePad.Update(gameTime);
        }
    }
}
