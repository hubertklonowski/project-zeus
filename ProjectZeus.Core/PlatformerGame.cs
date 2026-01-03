#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Platformer2D;

namespace ProjectZeus.Core
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        Vector2 baseScreenSize = new Vector2(800, 480);
        private Matrix globalTransformation;
        int backbufferWidth, backbufferHeight;

        // Global content.
        private SpriteFont hudFont;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        // Meta-level game state (level removed for now, single-scene only).
        private bool wasContinuePressed;

        // We store our input states so that we only poll once per frame.
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private KeyboardState previousKeyboardState;
        private TouchCollection touchState;

        private VirtualGamePad virtualGamePad;

        // Main scene content (three pillars with empty item slots).
        private Texture2D pillarTexture;
        private Texture2D slotTexture;
        private Texture2D skyTexture;
        private Pillar[] pillars;

        // Simple player.
        private Texture2D playerTexture;
        private Vector2 playerPosition;
        private Vector2 playerVelocity;
        private readonly Vector2 playerSize = new Vector2(32, 48);
        private bool isOnGround;

        // Simple pillar item state.
        private bool[] pillarHasItem;
        private Color[] pillarItemColors;

        // Portal to mine level
        private Texture2D portalTexture;
        private Rectangle portalRect;
        private float portalAnimationTime;

        // Scene management.
        private bool inZeusFightScene;
        private ZeusFightScene zeusFightScene;
        private bool inMineLevel;
        private bool hasCollectedMineItem;  // Simple flag instead of PillarItem
        
        // Mine level simple scene (no Level.cs, using simple shapes)
        private Vector2 minePlayerPosition;
        private Vector2 minePlayerVelocity;
        private bool minePlayerOnGround;
        private List<Rectangle> minePlatforms;
        private List<Rectangle> mineRails;  // Rails for visual decoration
        private List<MineCart> mineCarts;   // Moving carts
        private List<MineBat> mineBats;     // Flying bats
        private List<Rectangle> mineStalactites;  // Hanging stalactites
        private List<Rectangle> mineTorches;      // Light sources
        private Rectangle mineExitRect;
        private Rectangle mineItemRect;
        private bool mineItemCollected;
        private Texture2D minePlatformTexture;
        private Texture2D mineBackgroundTexture;

        // Portal animation constants
        private const int PORTAL_OUTER_RED = 255;
        private const int PORTAL_OUTER_GREEN = 100;
        private const int PORTAL_OUTER_BLUE = 255;
        private const int PORTAL_INNER_RED = 200;
        private const int PORTAL_INNER_GREEN = 50;
        private const int PORTAL_INNER_BLUE = 200;

        // Random instance for mine level
        private Random mineRandom = new Random();
        
        // Maze level management.
        private bool inMazeLevel;
        private MazeLevel mazeLevel;
        private bool hasCollectedItem;
        private KeyboardState previousKeyboardState;
        
        // Portal for maze entrance
        private Vector2 mazePortalPosition;
        private Vector2 mazePortalSize = new Vector2(60, 80);
        private Texture2D portalTexture;
        private readonly Color mazePortalBaseColor = new Color(100, 50, 200);
        private const float portalMarginFromEdge = 100f;
        private const float portalPulseFrequency = 3f;
        private const float portalPulseAmplitude = 0.3f;
        private const float portalPulseOffset = 0.7f;
        
        // Mountain level management.
        private bool inMountainLevel;
        private MountainLevel mountainLevel;

        // Player inventory for mountain item
        private bool hasItemFromMountain;
        
        // Portal to mountain level
        private Vector2 mountainPortalPosition;
        private Vector2 mountainPortalSize = new Vector2(60, 80);

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

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            this.Content.RootDirectory = "Content";

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");

            ScalePresentationArea();

            virtualGamePad = new VirtualGamePad(baseScreenSize, globalTransformation, Content.Load<Texture2D>("Sprites/VirtualControlArrow"));

            if (!OperatingSystem.IsIOS())
            {
                //Known issue that you get exceptions if you use Media Player while connected to your PC
                //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
                //Which means its impossible to test this from VS.
                //So we have to catch the exception and throw it away
                try
                {
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
                }
                catch { }
            }

            // Create simple textures for pillars, sky and empty item slots.
            pillarTexture = CreateSolidTexture(GraphicsDevice, 1, 1, new Color(230, 230, 230));
            slotTexture = CreateSolidTexture(GraphicsDevice, 1, 1, new Color(200, 200, 255));
            skyTexture = CreateSolidTexture(GraphicsDevice, 1, 1, new Color(135, 206, 235));
            playerTexture = CreateSolidTexture(GraphicsDevice, 1, 1, new Color(255, 220, 180));
            portalTexture = CreateSolidTexture(GraphicsDevice, 1, 1, mazePortalBaseColor);

            // Set up three pillars across the base screen, starting from the bottom.
            float centerX = baseScreenSize.X / 2f;
            float groundY = baseScreenSize.Y; // pillars go from bottom up
            float spacing = 200f;

            // Make pillars shorter so the player can reach the top.
            Vector2 pillarSize = new Vector2(60f, 140f);
            Vector2 slotSize = new Vector2(80f, 60f);
            float slotOffsetY = 10f;

            pillars = new[]
            {
                new Pillar { Position = new Vector2(centerX - spacing, groundY), Size = pillarSize, SlotSize = slotSize, SlotOffsetY = slotOffsetY },
                new Pillar { Position = new Vector2(centerX,          groundY), Size = pillarSize, SlotSize = slotSize, SlotOffsetY = slotOffsetY },
                new Pillar { Position = new Vector2(centerX + spacing, groundY), Size = pillarSize, SlotSize = slotSize, SlotOffsetY = slotOffsetY }
            };

            pillarHasItem = new bool[pillars.Length];
            pillarItemColors = new[] { Color.Gold, Color.DeepSkyBlue, Color.MediumVioletRed };

            // Place player on the ground near the left pillar.
            float groundTop = baseScreenSize.Y - 20f;
            playerPosition = new Vector2(centerX - spacing - 80f, groundTop - playerSize.Y);
            playerVelocity = Vector2.Zero;
            isOnGround = true;

            // Create portal to mine level between pillars 2 and 3
            portalTexture = CreateSolidTexture(GraphicsDevice, 1, 1, Color.White);
            float portalWidth = 80f;
            float portalHeight = 130f;

            // Position horizontally midway between middle and right pillars
            float xBetween = (pillars[1].Position.X + pillars[2].Position.X) / 2f;
            float portalX = xBetween - portalWidth / 2f;
            float portalY = groundTop - portalHeight;

            portalRect = new Rectangle((int)portalX, (int)portalY, (int)portalWidth, (int)portalHeight);
            portalAnimationTime = 0f;

            // Initialize mine level simple scene
            minePlatforms = new List<Rectangle>();
            mineItemCollected = false;
            minePlatformTexture = CreateSolidTexture(GraphicsDevice, 1, 1, new Color(100, 100, 100));
            mineBackgroundTexture = CreateSolidTexture(GraphicsDevice, 1, 1, new Color(20, 15, 30));
            
            // Create simple mine platforms
            minePlatforms.Add(new Rectangle(0, (int)(baseScreenSize.Y - 20), (int)baseScreenSize.X, 20)); // Ground
            minePlatforms.Add(new Rectangle(50, (int)(baseScreenSize.Y - 120), 150, 20)); // Platform 1
            minePlatforms.Add(new Rectangle(300, (int)(baseScreenSize.Y - 200), 150, 20)); // Platform 2
            minePlatforms.Add(new Rectangle(550, (int)(baseScreenSize.Y - 120), 200, 20)); // Platform 3 (exit platform)
            
            // Mine exit (portal back)
            mineExitRect = new Rectangle(650, (int)(baseScreenSize.Y - 180), 60, 60);
            
            // Mine item to collect
            mineItemRect = new Rectangle(130, (int)(baseScreenSize.Y - 160), 30, 30);
            // Place maze portal between first and second pillar
            float mazePortalX = (pillars[0].Position.X + pillars[1].Position.X) / 2f - mazePortalSize.X / 2f;
            mazePortalPosition = new Vector2(mazePortalX, groundTop - mazePortalSize.Y);

            inZeusFightScene = false;
            zeusFightScene = new ZeusFightScene();
            zeusFightScene.LoadContent(GraphicsDevice, hudFont);

            inMineLevel = false;
            hasCollectedMineItem = false;
            // Mine level will use simple shapes, no text file needed
            
            inMazeLevel = false;
            mazeLevel = new MazeLevel();
            mazeLevel.LoadContent(GraphicsDevice, hudFont);
            hasCollectedItem = false;
            previousKeyboardState = Keyboard.GetState();

            inMountainLevel = false;
            mountainLevel = new MountainLevel();
            mountainLevel.LoadContent(GraphicsDevice, hudFont);
            hasItemFromMountain = false;
            
            // Position portal to mountain level on the right side of the screen
            float mountainPortalX = baseScreenSize.X - 100f;
            mountainPortalPosition = new Vector2(mountainPortalX, groundTop - mountainPortalSize.Y);
        }

        public void ScalePresentationArea()
        {
            //Work out how much we need to scale our graphics to fill the screen
            backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            float horScaling = backbufferWidth / baseScreenSize.X;
            float verScaling = backbufferHeight / baseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            globalTransformation = Matrix.CreateScale(screenScalingFactor);
            System.Diagnostics.Debug.WriteLine("Screen Size - Width[" + GraphicsDevice.PresentationParameters.BackBufferWidth + "] Height [" + GraphicsDevice.PresentationParameters.BackBufferHeight + "]");

        }

        private void LoadMineLevel()
        {
            // Simple mine level - reset player and create a MUCH BIGGER, more interesting cave
            inMineLevel = true;
            mineItemCollected = false;
            minePlayerPosition = new Vector2(50f, baseScreenSize.Y - playerSize.Y - 30f);
            minePlayerVelocity = Vector2.Zero;
            minePlayerOnGround = false;

            // Clear and rebuild platforms for a BIGGER cave experience
            minePlatforms.Clear();
            
            // Ground
            minePlatforms.Add(new Rectangle(0, (int)(baseScreenSize.Y - 20), (int)baseScreenSize.X, 20));
            
            // Many more platforms at various heights for exploration
            minePlatforms.Add(new Rectangle(50, (int)(baseScreenSize.Y - 120), 180, 15));
            minePlatforms.Add(new Rectangle(250, (int)(baseScreenSize.Y - 180), 120, 15));
            minePlatforms.Add(new Rectangle(400, (int)(baseScreenSize.Y - 240), 150, 15));
            minePlatforms.Add(new Rectangle(180, (int)(baseScreenSize.Y - 280), 100, 15));
            minePlatforms.Add(new Rectangle(320, (int)(baseScreenSize.Y - 340), 160, 15));
            minePlatforms.Add(new Rectangle(550, (int)(baseScreenSize.Y - 200), 180, 15));
            minePlatforms.Add(new Rectangle(600, (int)(baseScreenSize.Y - 100), 150, 15)); // Exit platform
            
            // Create rails (visual decoration on some platforms)
            mineRails = new List<Rectangle>();
            mineRails.Add(new Rectangle(50, (int)(baseScreenSize.Y - 125), 180, 5));  // Rail on platform 1
            mineRails.Add(new Rectangle(550, (int)(baseScreenSize.Y - 205), 180, 5)); // Rail on platform 6
            
            // Create carts moving on rails
            mineCarts = new List<MineCart>();
            
            mineCarts.Add(new MineCart 
            { 
                Position = new Vector2(100, baseScreenSize.Y - 140), 
                Velocity = new Vector2(40, 0),
                MinX = 60,
                MaxX = 220
            });
            
            mineCarts.Add(new MineCart 
            { 
                Position = new Vector2(600, baseScreenSize.Y - 220), 
                Velocity = new Vector2(-50, 0),
                MinX = 560,
                MaxX = 720
            });
            
            // Create bats flying around
            mineBats = new List<MineBat>();
            for (int i = 0; i < 4; i++)
            {
                mineBats.Add(new MineBat
                {
                    Position = new Vector2(200 + i * 150, 150 + i * 40),
                    Velocity = new Vector2((float)(mineRandom.NextDouble() * 2 - 1) * 60f, (float)(mineRandom.NextDouble() * 2 - 1) * 60f),
                    ChangeDirectionTimer = (float)mineRandom.NextDouble() * 2f
                });
            }
            
            // Create stalactites hanging from ceiling
            mineStalactites = new List<Rectangle>();
            for (int x = 100; x < 700; x += 80)
            {
                int height = 30 + mineRandom.Next(30);
                mineStalactites.Add(new Rectangle(x, 0, 20, height));
            }
            
            // Create torches for atmosphere
            mineTorches = new List<Rectangle>();
            mineTorches.Add(new Rectangle(30, (int)(baseScreenSize.Y - 50), 15, 25));
            mineTorches.Add(new Rectangle(230, (int)(baseScreenSize.Y - 210), 15, 25));
            mineTorches.Add(new Rectangle(580, (int)(baseScreenSize.Y - 230), 15, 25));
            mineTorches.Add(new Rectangle(730, (int)(baseScreenSize.Y - 130), 15, 25));
            
            // Item to collect (on middle-upper platform)
            mineItemRect = new Rectangle(350, (int)(baseScreenSize.Y - 280), 30, 30);
            
            // Exit portal (on far right upper platform)
            mineExitRect = new Rectangle(670, (int)(baseScreenSize.Y - 150), 50, 50);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (inZeusFightScene)
            {
                zeusFightScene.Update(gameTime);
                base.Update(gameTime);
                return;
            }
            
            if (inMazeLevel)
            {
                // Handle polling for maze input separately
                keyboardState = Keyboard.GetState();
                mazeLevel.Update(gameTime, keyboardState);
                
                // Check if player completed the maze
                if (mazeLevel.IsCompleted)
                {
                    inMazeLevel = false;
                    hasCollectedItem = mazeLevel.HasItem;
                    
                    // Create a new maze for next time
                    mazeLevel = new MazeLevel();
                    mazeLevel.LoadContent(GraphicsDevice, hudFont);
                }
                
                previousKeyboardState = keyboardState;
                base.Update(gameTime);
                return;
            }

            if (inMountainLevel)
            {
                UpdateMountainLevel(gameTime);
                previousKeyboardState = keyboardState;
                return;
            }

            if (inMineLevel)
            {
                UpdateMineLevel(gameTime);
                base.Update(gameTime);
                return;
            }

            //Confirm the screen has not been resized by the user
            if (backbufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
                backbufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
            {
                ScalePresentationArea();
            }

            // Handle polling for our input and handling high-level input
            HandleInput(gameTime);

            UpdatePillarRoom(gameTime);

            base.Update(gameTime);
        }

        private void UpdatePillarRoom(GameTime gameTime)
        {
            // Simple platformer-style movement.
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float moveSpeed = 180f;
            float jumpVelocity = -560f;
            float gravity = 900f;

            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;

            playerVelocity.X = move * moveSpeed;

            if (isOnGround && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up)))
            {
                playerVelocity.Y = jumpVelocity;
                isOnGround = false;
            }

            playerVelocity.Y += gravity * dt;
            playerPosition += playerVelocity * dt;

            float groundTop = baseScreenSize.Y - 20f;
            isOnGround = false;

            // Ground collision
            if (playerPosition.Y + playerSize.Y >= groundTop)
            {
                playerPosition.Y = groundTop - playerSize.Y;
                playerVelocity.Y = 0f;
                isOnGround = true;
            }

            // Pillar top collision (simple AABB against top surfaces).
            if (pillars != null)
            {
                Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
                foreach (Pillar pillar in pillars)
                {
                    Rectangle pillarRect = pillar.GetPillarRectangle();

                    Rectangle topRect = new Rectangle(pillarRect.X, pillarRect.Y - 2, pillarRect.Width, 6);

                    if (playerRect.Bottom > topRect.Top &&
                        playerRect.Bottom <= topRect.Top + 20 &&
                        playerRect.Right > topRect.Left &&
                        playerRect.Left < topRect.Right &&
                        playerVelocity.Y >= 0)
                    {
                        playerPosition.Y = topRect.Top - playerSize.Y;
                        playerVelocity.Y = 0f;
                        isOnGround = true;
                        playerRect.Y = (int)playerPosition.Y;
                    }
                }
            }

            // Prevent leaving the screen horizontally.
            if (playerPosition.X < 0)
                playerPosition.X = 0;
            if (playerPosition.X + playerSize.X > baseScreenSize.X)
                playerPosition.X = baseScreenSize.X - playerSize.X;

            // Update portal animation
            portalAnimationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Check if player enters the portal to the mine level (ONLY WAY TO ENTER)
            Rectangle playerRect2 = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
            if (playerRect2.Intersects(portalRect) && !inMineLevel)
            {
                LoadMineLevel();
                return; // Exit this update, next frame will use mine level update
            }

            // Interaction: press E near a pillar to insert an item (if we have one).
            if (keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E) && hasCollectedMineItem)
            // Interaction: press E near a pillar to insert an item or enter portals
            bool eKeyPressed = keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E);
            
            if (eKeyPressed)
            {
                // Try placing maze or mountain item in a pillar
                if (hasCollectedItem || hasItemFromMountain)
                {
                    TryInsertItemAtPlayer();
                }

                // Try entering mountain portal on E press
                TryEnterMountainPortal();
            }

            // Check portal collision to enter maze (only when not carrying item)
            if (!hasCollectedItem)
            {
                Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 
                                                     (int)playerSize.X, (int)playerSize.Y);
                Rectangle portalRect = new Rectangle((int)mazePortalPosition.X, (int)mazePortalPosition.Y,
                                                     (int)mazePortalSize.X, (int)mazePortalSize.Y);
                
                if (playerRect.Intersects(portalRect))
                {
                    // Enter maze level to collect an item
                    inMazeLevel = true;
                }
            }

            // Check if all items have been inserted; if so, switch to ZeusFightScene.
            if (pillarHasItem != null)
            {
                bool allInserted = true;
                for (int i = 0; i < pillarHasItem.Length; i++)
                {
                    if (!pillarHasItem[i])
                    {
                        allInserted = false;
                        break;
                    }
                }

                if (allInserted)
                {
                    inZeusFightScene = true;

                    // When we enter the fight scene, move the player to the right side
                    // of the screen so they start opposite Zeus.
                    float fightGroundTop = baseScreenSize.Y * 0.7f; // match ZeusFightScene ground band
                    playerPosition = new Vector2(baseScreenSize.X - playerSize.X - 40f, fightGroundTop - playerSize.Y);
                    playerVelocity = Vector2.Zero;
                    isOnGround = true;
                }
            }

            previousKeyboardState = keyboardState;
        }

        private void UpdateMountainLevel(GameTime gameTime)
        {
            // Handle polling for our input
            HandleInput(gameTime);

            // Simple platformer-style movement
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float moveSpeed = 180f;
            float jumpVelocity = -560f;
            float gravity = 900f;

            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;

            playerVelocity.X = move * moveSpeed;

            if (isOnGround && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up)))
            {
                playerVelocity.Y = jumpVelocity;
                isOnGround = false;
            }

            playerVelocity.Y += gravity * dt;
            playerPosition += playerVelocity * dt;

            // Check platform collision in mountain level
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
            Vector2 correctedPosition;
            isOnGround = mountainLevel.CheckPlatformCollision(playerRect, playerVelocity, out correctedPosition);

            if (isOnGround)
            {
                playerPosition = correctedPosition;
                playerVelocity.Y = 0f;
            }

            // Prevent leaving the screen horizontally
            if (playerPosition.X < 0)
                playerPosition.X = 0;
            if (playerPosition.X + playerSize.X > baseScreenSize.X)
                playerPosition.X = baseScreenSize.X - playerSize.X;

            // Check if trying to pick up item
            bool tryPickupItem = keyboardState.IsKeyDown(Keys.E);

            // Update mountain level (goat, rocks, item collection)
            mountainLevel.Update(gameTime, playerPosition, playerSize, tryPickupItem);

            // Check if player died
            if (mountainLevel.PlayerDied)
            {
                RespawnInPillarRoom();
            }

            // Check if item was collected
            if (mountainLevel.ItemWasCollected && !hasItemFromMountain)
            {
                hasItemFromMountain = true;
                // Return to pillar room automatically
                ReturnToPillarRoom();
            }

            // Allow player to return to pillar room by going to left edge
            if (playerPosition.X <= 10f && hasItemFromMountain)
            {
                ReturnToPillarRoom();
            }

            previousKeyboardState = keyboardState;
        }

        private void TryEnterMountainPortal()
        {
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
            Rectangle portalRect = new Rectangle((int)mountainPortalPosition.X, (int)mountainPortalPosition.Y, (int)mountainPortalSize.X, (int)mountainPortalSize.Y);

            if (playerRect.Intersects(portalRect))
            {
                EnterMountainLevel();
            }
        }

        private void EnterMountainLevel()
        {
            inMountainLevel = true;
            mountainLevel.Reset();
            
            // Place player at bottom left of mountain
            float groundTop = baseScreenSize.Y - 20f;
            playerPosition = new Vector2(60f, groundTop - playerSize.Y);
            playerVelocity = Vector2.Zero;
            isOnGround = true;
        }

        private void ReturnToPillarRoom()
        {
            inMountainLevel = false;
            
            // Place player near portal
            float groundTop = baseScreenSize.Y - 20f;
            playerPosition = new Vector2(mountainPortalPosition.X - 60f, groundTop - playerSize.Y);
            playerVelocity = Vector2.Zero;
            isOnGround = true;
        }

        private void RespawnInPillarRoom()
        {
            inMountainLevel = false;
            
            // Reset mountain level
            mountainLevel.Reset();
            
            // Clear all inventory items
            hasItemFromMountain = false;
            
            // Reset pillar items
            if (pillarHasItem != null)
            {
                for (int i = 0; i < pillarHasItem.Length; i++)
                {
                    pillarHasItem[i] = false;
                }
            }
            
            // Respawn player at starting position
            float centerX = baseScreenSize.X / 2f;
            float spacing = 200f;
            float groundTop = baseScreenSize.Y - 20f;
            playerPosition = new Vector2(centerX - spacing - 80f, groundTop - playerSize.Y);
            playerVelocity = Vector2.Zero;
            isOnGround = true;
        }

        private void TryInsertItemAtPlayer()
        {
            // Caller ensures collectedItem is not null
            if (pillars == null || pillarHasItem == null)
                return;

            // Nothing to place
            if (!hasCollectedItem && !hasItemFromMountain)
                return;

            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)playerSize.X, (int)playerSize.Y);

            for (int i = 0; i < pillars.Length; i++)
            {
                // Skip if this pillar already has an item
                if (pillarHasItem[i])
                    continue;

                Rectangle slotRect = pillars[i].GetSlotRectangle();

                // Define a small interaction zone around the slot.
                Rectangle interactionRect = slotRect;
                interactionRect.Inflate(20, 20);

                if (playerRect.Intersects(interactionRect) && !pillarHasItem[i])
                {
                    pillarHasItem[i] = true; // Insert the item
                    hasCollectedMineItem = false; // Item has been placed
                    // Place maze or mountain item into the pillar slot
                    pillarHasItem[i] = true;

                    if (hasCollectedItem)
                    {
                        hasCollectedItem = false;
                    }
                    else if (hasItemFromMountain)
                    {
                        hasItemFromMountain = false;
                    }

                    break;
                }
            }
        }

        private void UpdateMineLevel(GameTime gameTime)
        {
            // Handle polling for our input
            HandleInput(gameTime);

            // Simple platformer physics for mine level
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float moveSpeed = 180f;
            float jumpVelocity = -560f;
            float gravity = 900f;

            // Movement
            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;

            minePlayerVelocity.X = move * moveSpeed;

            // Jumping
            if (minePlayerOnGround && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up)))
            {
                minePlayerVelocity.Y = jumpVelocity;
                minePlayerOnGround = false;
            }

            // Gravity
            minePlayerVelocity.Y += gravity * dt;
            minePlayerPosition += minePlayerVelocity * dt;

            // Collision with platforms
            minePlayerOnGround = false;
            Rectangle playerRect = new Rectangle((int)minePlayerPosition.X, (int)minePlayerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
            
            foreach (Rectangle platform in minePlatforms)
            {
                if (playerRect.Bottom > platform.Top &&
                    playerRect.Bottom <= platform.Top + 20 &&
                    playerRect.Right > platform.Left &&
                    playerRect.Left < platform.Right &&
                    minePlayerVelocity.Y >= 0)
                {
                    minePlayerPosition.Y = platform.Top - playerSize.Y;
                    minePlayerVelocity.Y = 0f;
                    minePlayerOnGround = true;
                    playerRect.Y = (int)minePlayerPosition.Y;
                }
            }

            // Prevent leaving screen
            if (minePlayerPosition.X < 0)
                minePlayerPosition.X = 0;
            if (minePlayerPosition.X + playerSize.X > baseScreenSize.X)
                minePlayerPosition.X = baseScreenSize.X - playerSize.X;

            // Update carts
            if (mineCarts != null)
            {
                foreach (var cart in mineCarts)
                {
                    cart.Update(dt);
                }
            }

            // Update bats
            if (mineBats != null)
            {
                foreach (var bat in mineBats)
                {
                    bat.Update(dt, mineRandom);
                }
            }

            // Check if player collects the item
            if (!mineItemCollected && keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E))
            {
                playerRect = new Rectangle((int)minePlayerPosition.X, (int)minePlayerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
                if (playerRect.Intersects(mineItemRect))
                {
                    mineItemCollected = true;
                    hasCollectedMineItem = true; // Mark that we have the item for pillar placement
                }
            }

            // Check if player reaches exit portal
            playerRect = new Rectangle((int)minePlayerPosition.X, (int)minePlayerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
            if (playerRect.Intersects(mineExitRect))
            {
                // Return to pillar room
                inMineLevel = false;
                float groundTop = baseScreenSize.Y - 20f;
                float centerX = baseScreenSize.X / 2f;
                float spacing = 200f;
                playerPosition = new Vector2(centerX - spacing - 80f, groundTop - playerSize.Y);
                playerVelocity = Vector2.Zero;
                isOnGround = true;
                // Keep the collected item if we got it
            }

            // Reset if player falls off
            if (minePlayerPosition.Y > baseScreenSize.Y)
            {
                minePlayerPosition = new Vector2(50f, baseScreenSize.Y - playerSize.Y - 30f);
                minePlayerVelocity = Vector2.Zero;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (inZeusFightScene)
            {
                // Let the Zeus fight scene draw its background and the player.
                zeusFightScene.Draw(spriteBatch, GraphicsDevice, playerTexture, playerPosition, playerSize);
                base.Draw(gameTime);
                return;
            }
            
            if (inMazeLevel)
            {
                // Draw the maze level
                mazeLevel.Draw(spriteBatch, GraphicsDevice);
                base.Draw(gameTime);
                return;
            }

            if (inMountainLevel)
            {
                // Let the mountain level draw everything
                mountainLevel.Draw(spriteBatch, GraphicsDevice);
                
                // Draw the player on top
                spriteBatch.Begin();
                Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
                spriteBatch.Draw(playerTexture, playerRect, Color.White);
                spriteBatch.End();
                
                base.Draw(gameTime);
                return;
            }

            if (inMineLevel)
            {
                // Draw the simple mine level
                graphics.GraphicsDevice.Clear(new Color(20, 15, 30)); // Dark cave background

                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, globalTransformation);

                // Draw background
                Rectangle bgRect = new Rectangle(0, 0, (int)baseScreenSize.X, (int)baseScreenSize.Y);
                spriteBatch.Draw(mineBackgroundTexture, bgRect, Color.White);

                // Draw stalactites
                if (mineStalactites != null)
                {
                    foreach (Rectangle stalactite in mineStalactites)
                    {
                        spriteBatch.Draw(minePlatformTexture, stalactite, new Color(120, 120, 120));
                    }
                }

                // Draw platforms
                foreach (Rectangle platform in minePlatforms)
                {
                    spriteBatch.Draw(minePlatformTexture, platform, new Color(80, 70, 60));
                }

                // Draw rails
                if (mineRails != null)
                {
                    foreach (Rectangle rail in mineRails)
                    {
                        spriteBatch.Draw(minePlatformTexture, rail, new Color(140, 140, 140));
                        // Draw rail ties
                        for (int x = rail.Left; x < rail.Right; x += 15)
                        {
                            Rectangle tie = new Rectangle(x, rail.Top - 3, 3, 8);
                            spriteBatch.Draw(minePlatformTexture, tie, new Color(100, 70, 50));
                        }
                    }
                }

                // Draw carts
                if (mineCarts != null)
                {
                    foreach (var cart in mineCarts)
                    {
                        spriteBatch.Draw(minePlatformTexture, cart.Bounds, new Color(139, 69, 19)); // Brown
                        // Draw wheels
                        Rectangle wheel1 = new Rectangle(cart.Bounds.Left + 5, cart.Bounds.Bottom - 5, 8, 8);
                        Rectangle wheel2 = new Rectangle(cart.Bounds.Right - 13, cart.Bounds.Bottom - 5, 8, 8);
                        spriteBatch.Draw(minePlatformTexture, wheel1, Color.Black);
                        spriteBatch.Draw(minePlatformTexture, wheel2, Color.Black);
                    }
                }

                // Draw torches
                if (mineTorches != null)
                {
                    float flickerTime = (float)gameTime.TotalGameTime.TotalSeconds;
                    foreach (Rectangle torch in mineTorches)
                    {
                        // Torch stick
                        spriteBatch.Draw(minePlatformTexture, torch, new Color(101, 67, 33));
                        // Flame
                        float flicker = (float)Math.Sin(flickerTime * 8f) * 0.2f + 0.8f;
                        Rectangle flame = new Rectangle(torch.X - 5, torch.Y - 15, torch.Width + 10, 15);
                        spriteBatch.Draw(minePlatformTexture, flame, new Color((byte)(255 * flicker), (byte)(200 * flicker), 50));
                    }
                }

                // Draw item if not collected
                if (!mineItemCollected)
                {
                    spriteBatch.Draw(playerTexture, mineItemRect, Color.Gold);
                    // Draw item glow
                    Rectangle glowRect = mineItemRect;
                    glowRect.Inflate(5, 5);
                    spriteBatch.Draw(playerTexture, glowRect, new Color((byte)255, (byte)215, (byte)0, (byte)100));
                }

                // Draw bats
                if (mineBats != null)
                {
                    float wingTime = (float)gameTime.TotalGameTime.TotalSeconds;
                    foreach (var bat in mineBats)
                    {
                        // Bat body
                        spriteBatch.Draw(minePlatformTexture, bat.Bounds, new Color(80, 60, 90));
                        // Wings
                        float wingFlap = (float)Math.Sin(wingTime * 10f) * 5f;
                        Rectangle leftWing = new Rectangle(bat.Bounds.Left - 8 + (int)wingFlap, bat.Bounds.Top + 5, 8, 5);
                        Rectangle rightWing = new Rectangle(bat.Bounds.Right - (int)wingFlap, bat.Bounds.Top + 5, 8, 5);
                        spriteBatch.Draw(minePlatformTexture, leftWing, new Color(60, 50, 70));
                        spriteBatch.Draw(minePlatformTexture, rightWing, new Color(60, 50, 70));
                    }
                }

                // Draw exit portal
                float exitPulse = (float)Math.Sin(portalAnimationTime * 4.0f) * 0.3f + 0.7f;
                spriteBatch.Draw(playerTexture, mineExitRect, new Color((byte)(100 * exitPulse), (byte)(255 * exitPulse), (byte)(100 * exitPulse)));
                
                // Draw player in mine
                Rectangle minePlayerRect = new Rectangle((int)minePlayerPosition.X, (int)minePlayerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
                spriteBatch.Draw(playerTexture, minePlayerRect, Color.White);

                // Draw UI
                if (mineItemCollected)
                {
                    string hasItem = "Item collected! Go to green exit portal!";
                    Vector2 hasItemSize = hudFont.MeasureString(hasItem);
                    Vector2 hasItemPos = new Vector2((baseScreenSize.X - hasItemSize.X) / 2f, 40f);
                    spriteBatch.DrawString(hudFont, hasItem, hasItemPos, Color.LightGreen);
                }

                spriteBatch.End();

                base.Draw(gameTime);
                return;
            }

            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, globalTransformation);

            DrawMainScene(gameTime);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            // Save previous state before getting new state
            previousKeyboardState = keyboardState;
            
            // get all of our input states
            keyboardState = Keyboard.GetState();
            touchState = TouchPanel.GetState();
            gamePadState = virtualGamePad.GetState(touchState, GamePad.GetState(PlayerIndex.One));

            if (!OperatingSystem.IsIOS())
            {
                // Exit the game when back is pressed.
                if (gamePadState.Buttons.Back == ButtonState.Pressed)
                    Exit();
            }

            wasContinuePressed = false;

            virtualGamePad.Update(gameTime);
        }

        private static Texture2D CreateSolidTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++)
                data[i] = color;
            texture.SetData(data);
            return texture;
        }

        private sealed class Pillar
        {
            public Vector2 Position;
            public Vector2 Size;
            public Vector2 SlotSize;
            public float SlotOffsetY;

            public Rectangle GetPillarRectangle()
            {
                return new Rectangle(
                    (int)(Position.X - Size.X / 2f),
                    (int)(Position.Y - Size.Y),
                    (int)Size.X,
                    (int)Size.Y);
            }

            public Rectangle GetSlotRectangle()
            {
                Rectangle pillarRect = GetPillarRectangle();
                return new Rectangle(
                    pillarRect.X + (pillarRect.Width - (int)SlotSize.X) / 2,
                    pillarRect.Y - (int)SlotOffsetY - (int)SlotSize.Y,
                    (int)SlotSize.X,
                    (int)SlotSize.Y);
            }
        }

        private void DrawMainScene(GameTime gameTime)
        {
            // Sky background.
            Rectangle skyRect = new Rectangle(0, 0, (int)baseScreenSize.X, (int)baseScreenSize.Y);
            spriteBatch.Draw(skyTexture, skyRect, Color.White);

            // Simple moving clouds.
            float t = (float)gameTime.TotalGameTime.TotalSeconds;
            float cloudSpeed = 20f;
            int cloudWidth = 160;
            int cloudHeight = 60;

            for (int i = 0; i < 3; i++)
            {
                float x = ((t * cloudSpeed) + i * 200f) % (baseScreenSize.X + cloudWidth) - cloudWidth;
                float y = 60f + i * 40f;
                Rectangle cloudRect = new Rectangle((int)x, (int)y, cloudWidth, cloudHeight);
                spriteBatch.Draw(skyTexture, cloudRect, new Color(250, 250, 250));
            }

            // Ground strip.
            Rectangle groundRect = new Rectangle(0, (int)(baseScreenSize.Y - 20), (int)baseScreenSize.X, 20);
            spriteBatch.Draw(pillarTexture, groundRect, new Color(180, 180, 180));

            if (pillars != null)
            {
                for (int i = 0; i < pillars.Length; i++)
                {
                    Pillar pillar = pillars[i];
                    Rectangle pillarRect = pillar.GetPillarRectangle();
                    spriteBatch.Draw(pillarTexture, pillarRect, Color.White);

                    int stripeCount = 4;
                    int stripeWidth = pillarRect.Width / (stripeCount * 2);
                    for (int s = 0; s < stripeCount; s++)
                    {
                        int x = pillarRect.X + stripeWidth + s * stripeWidth * 2;
                        Rectangle stripe = new Rectangle(x, pillarRect.Y, stripeWidth, pillarRect.Height);
                        spriteBatch.Draw(pillarTexture, stripe, new Color(210, 210, 210));
                    }

                    Rectangle capitalRect = new Rectangle(pillarRect.X - 5, pillarRect.Y - 10, pillarRect.Width + 10, 10);
                    spriteBatch.Draw(pillarTexture, capitalRect, new Color(240, 240, 240));

                    Rectangle slotRect = pillar.GetSlotRectangle();
                    spriteBatch.Draw(slotTexture, slotRect, Color.White);

                    if (pillarHasItem != null && pillarHasItem[i])
                    {
                        // Draw a simple item as a filled rectangle inside the slot, with a per-pillar color.
                        Rectangle itemRect = slotRect;
                        itemRect.Inflate(-10, -10);
                        Color itemColor = (pillarItemColors != null && i < pillarItemColors.Length)
                            ? pillarItemColors[i]
                            : Color.Gold;
                        spriteBatch.Draw(playerTexture, itemRect, itemColor);
                    }

                    DrawRectangleOutline(slotRect, Color.DarkBlue);
                }
            }

            // Draw the portal with BRIGHT, VERY VISIBLE animation
            if (portalTexture != null)
            {
                // Animated portal effect - BRIGHT pulsing colors
                float pulse = (float)Math.Sin(portalAnimationTime * 3.0f) * 0.3f + 0.7f;
                float fastPulse = (float)Math.Sin(portalAnimationTime * 6.0f) * 0.5f + 0.5f;
                
                // Very bright magenta/purple portal
                Color portalColor1 = new Color((byte)(255 * pulse), (byte)(100 * pulse), (byte)(255 * pulse));
                Color portalColor2 = new Color((byte)(200 * fastPulse), (byte)(50 * fastPulse), (byte)(255 * fastPulse));

                // Draw portal frame - BRIGHT
                spriteBatch.Draw(portalTexture, portalRect, portalColor1);
                
                // Draw iner portal with different color
                Rectangle innerPortal = portalRect;
                innerPortal.Inflate(-8, -8);
                spriteBatch.Draw(portalTexture, innerPortal, portalColor2);

            // Draw portal (if player doesn't have an item)
            if (!hasCollectedItem)
            {
                Rectangle portalRect = new Rectangle((int)mazePortalPosition.X, (int)mazePortalPosition.Y,
                                                     (int)mazePortalSize.X, (int)mazePortalSize.Y);
                
                // Animated portal effect using time
                var portalTimeNew = (float)gameTime.TotalGameTime.TotalSeconds;
                float pulseNew = (float)(Math.Sin(portalTimeNew * portalPulseFrequency) * portalPulseAmplitude + portalPulseOffset);
                
                // Draw portal base
                spriteBatch.Draw(portalTexture, portalRect, mazePortalBaseColor * pulseNew);
                
                // Draw portal inner glow
                Rectangle innerRect = portalRect;
                innerRect.Inflate(-8, -8);
                spriteBatch.Draw(portalTexture, innerRect, new Color(150, 100, 255) * pulseNew);
                
                // Draw portal core
                Rectangle coreRect = portalRect;
                coreRect.Inflate(-16, -16);
                spriteBatch.Draw(portalTexture, coreRect, Color.White * pulseNew);
                
                DrawRectangleOutline(portalRect, new Color(150, 100, 255));
            }

            // Draw the player.
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
            spriteBatch.Draw(playerTexture, playerRect, Color.White);
            
            // If player has an item, draw it above their head
            if (hasCollectedItem)
            {
                Rectangle itemRect = new Rectangle((int)playerPosition.X + (int)playerSize.X / 2 - 12, 
                                                   (int)playerPosition.Y - 25, 24, 24);
                spriteBatch.Draw(playerTexture, itemRect, Color.Gold);
                DrawRectangleOutline(itemRect, Color.Orange);
            }

            // Draw portal to mountain level (same graphic as maze, different position + label)
            Rectangle mountainPortalRect = new Rectangle(
                (int)mountainPortalPosition.X,
                (int)mountainPortalPosition.Y,
                (int)mountainPortalSize.X,
                (int)mountainPortalSize.Y);

            // Reuse same pulse animation as maze portal
            float portalTime = (float)gameTime.TotalGameTime.TotalSeconds;
            float pulse = (float)(Math.Sin(portalTime * portalPulseFrequency) * portalPulseAmplitude + portalPulseOffset);

            // Base
            spriteBatch.Draw(portalTexture, mountainPortalRect, mazePortalBaseColor * pulse);

            // Inner glow
            Rectangle mountainInnerRect = mountainPortalRect;
            mountainInnerRect.Inflate(-8, -8);
            spriteBatch.Draw(portalTexture, mountainInnerRect, new Color(150, 100, 255) * pulse);

            // Core
            Rectangle mountainCoreRect = mountainPortalRect;
            mountainCoreRect.Inflate(-16, -16);
            spriteBatch.Draw(portalTexture, mountainCoreRect, Color.White * pulse);

            // Outline
            DrawRectangleOutline(mountainPortalRect, new Color(150, 100, 255));

            // Draw inventory indicator if player has mountain item
            if (hasItemFromMountain)
            {
                Rectangle inventoryRect = new Rectangle(10, 10, 40, 40);
                spriteBatch.Draw(playerTexture, inventoryRect, Color.Gold);
                DrawRectangleOutline(inventoryRect, Color.Yellow);
                
                if (hudFont != null)
                {
                    string inventoryText = "Item collected! Place it in a pillar.";
                    Vector2 inventoryTextSize = hudFont.MeasureString(inventoryText);
                    Vector2 inventoryTextPos = new Vector2(60f, 20f);
                    spriteBatch.DrawString(hudFont, inventoryText, inventoryTextPos, Color.Gold);
                }
            }

            string title = hasItemFromMountain ? "Place the item in a pillar slot" : "Enter portal (E) or insert the three items of Zeus";
            Vector2 titleSize = hudFont.MeasureString(title);
            Vector2 titlePos = new Vector2((baseScreenSize.X - titleSize.X) / 2f, 40f);
            spriteBatch.DrawString(hudFont, title, titlePos, Color.Yellow);


            if (hasCollectedMineItem)
            {
                string hasItem = "Press E near a pillar to place the item";
                Vector2 hasItemSize = hudFont.MeasureString(hasItem);
                Vector2 hasItemPos = new Vector2((baseScreenSize.X - hasItemSize.X) / 2f, 100f);
                spriteBatch.DrawString(hudFont, hasItem, hasItemPos, Color.LightGreen);
            }
            
            // Instructions
            string instructions = hasCollectedItem 
                ? "Press E near an empty pillar to place the item" 
                : "Walk into the portal to enter the maze";
            Vector2 instructionsSize = hudFont.MeasureString(instructions);
            Vector2 instructionsPos = new Vector2((baseScreenSize.X - instructionsSize.X) / 2f, 70f);
            spriteBatch.DrawString(hudFont, instructions, instructionsPos, Color.White);
        }

        private void DrawRectangleOutline(Rectangle rect, Color color)
        {
            if (pillarTexture == null)
                return;

            spriteBatch.Draw(pillarTexture, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
            spriteBatch.Draw(pillarTexture, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), color);
            spriteBatch.Draw(pillarTexture, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
            spriteBatch.Draw(pillarTexture, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), color);
        }

        // Simple mine cart class for moving obstacles/decoration
        private class MineCart
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float MinX, MaxX;
            public Rectangle Bounds => new Rectangle((int)Position.X - 20, (int)Position.Y - 15, 40, 30);

            public void Update(float dt)
            {
                Position.X += Velocity.X * dt;
                if (Position.X < MinX || Position.X > MaxX)
                {
                    Velocity.X = -Velocity.X;
                    Position.X = Math.Max(MinX, Math.Min(MaxX, Position.X));
                }
            }
        }

        // Simple bat class for flying around
        private class MineBat
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float ChangeDirectionTimer;
            public Rectangle Bounds => new Rectangle((int)Position.X - 10, (int)Position.Y - 10, 20, 20);

            public void Update(float dt, Random random)
            {
                ChangeDirectionTimer -= dt;
                if (ChangeDirectionTimer <= 0)
                {
                    Velocity = new Vector2((float)(random.NextDouble() * 2 - 1) * 60f, (float)(random.NextDouble() * 2 - 1) * 60f);
                    ChangeDirectionTimer = 2f;
                }
                Position += Velocity * dt;
                
                // Keep in bounds
                if (Position.X < 50) { Position.X = 50; Velocity.X = Math.Abs(Velocity.X); }
                if (Position.X > 750) { Position.X = 750; Velocity.X = -Math.Abs(Velocity.X); }
                if (Position.Y < 100) { Position.Y = 100; Velocity.Y = Math.Abs(Velocity.Y); }
                if (Position.Y > 380) { Position.Y = 380; Velocity.Y = -Math.Abs(Velocity.Y); }
            }
        }
    }
}
