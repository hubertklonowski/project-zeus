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

            // Create portal to mine level (on the right side of the screen) - make it BIG and VISIBLE
            portalTexture = CreateSolidTexture(GraphicsDevice, 1, 1, Color.White);
            float portalWidth = 100f;  // Much larger
            float portalHeight = 140f;  // Much taller
            float portalX = baseScreenSize.X - portalWidth - 50f;  // Closer to edge
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

            inZeusFightScene = false;
            zeusFightScene = new ZeusFightScene();
            zeusFightScene.LoadContent(GraphicsDevice, hudFont);

            inMineLevel = false;
            hasCollectedMineItem = false;
            // Mine level will use simple shapes, no text file needed
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
            // Simple mine level - just reset player position and flags
            inMineLevel = true;
            mineItemCollected = false;
            minePlayerPosition = new Vector2(50f, baseScreenSize.Y - playerSize.Y - 30f);
            minePlayerVelocity = Vector2.Zero;
            minePlayerOnGround = false;
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
            {
                TryInsertItemAtPlayer();
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

            base.Update(gameTime);
        }

        private void TryInsertItemAtPlayer()
        {
            // Caller ensures collectedItem is not null
            if (pillars == null || pillarHasItem == null)
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

                if (playerRect.Intersects(interactionRect))
                {
                    pillarHasItem[i] = true; // Insert the item
                    hasCollectedMineItem = false; // Item has been placed
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

            if (inMineLevel)
            {
                // Draw the simple mine level
                graphics.GraphicsDevice.Clear(new Color(20, 15, 30)); // Dark cave background

                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, globalTransformation);

                // Draw background
                Rectangle bgRect = new Rectangle(0, 0, (int)baseScreenSize.X, (int)baseScreenSize.Y);
                spriteBatch.Draw(mineBackgroundTexture, bgRect, Color.White);

                // Draw platforms
                foreach (Rectangle platform in minePlatforms)
                {
                    spriteBatch.Draw(minePlatformTexture, platform, new Color(80, 70, 60));
                }

                // Draw item if not collected
                if (!mineItemCollected)
                {
                    spriteBatch.Draw(playerTexture, mineItemRect, Color.Gold);
                    // Draw item glow
                    Rectangle glowRect = mineItemRect;
                    glowRect.Inflate(5, 5);
                    spriteBatch.Draw(playerTexture, glowRect, new Color(255, 215, 0, 100));
                }

                // Draw exit portal
                float exitPulse = (float)Math.Sin(portalAnimationTime * 4.0f) * 0.3f + 0.7f;
                spriteBatch.Draw(playerTexture, mineExitRect, new Color((byte)(100 * exitPulse), (byte)(255 * exitPulse), (byte)(100 * exitPulse)));
                
                // Draw player in mine
                Rectangle minePlayerRect = new Rectangle((int)minePlayerPosition.X, (int)minePlayerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
                spriteBatch.Draw(playerTexture, minePlayerRect, Color.White);

                // Draw UI
                string instructions = "Arrow Keys/WASD: Move | Space: Jump | E: Collect Item";
                Vector2 instructionsSize = hudFont.MeasureString(instructions);
                Vector2 instructionsPos = new Vector2((baseScreenSize.X - instructionsSize.X) / 2f, 10f);
                spriteBatch.DrawString(hudFont, instructions, instructionsPos, Color.Yellow);

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

                // Draw outer glow for visibility
                Rectangle glowRect = portalRect;
                glowRect.Inflate(15, 15);
                spriteBatch.Draw(portalTexture, glowRect, new Color((byte)(255 * fastPulse), (byte)(100 * fastPulse), (byte)(255 * fastPulse), (byte)100));

                // Draw portal frame - BRIGHT
                spriteBatch.Draw(portalTexture, portalRect, portalColor1);
                
                // Draw inner portal with different color
                Rectangle innerPortal = portalRect;
                innerPortal.Inflate(-8, -8);
                spriteBatch.Draw(portalTexture, innerPortal, portalColor2);

                // Draw portal outline - THICK and BRIGHT
                DrawRectangleOutline(portalRect, new Color(255, 0, 255)); // Bright magenta
                Rectangle thickOutline = portalRect;
                thickOutline.Inflate(3, 3);
                DrawRectangleOutline(thickOutline, new Color(200, 0, 200));

                // Draw "MINE LEVEL" text above portal - LARGE and BRIGHT
                string portalLabel = ">>> MINE <<<";
                Vector2 portalLabelSize = hudFont.MeasureString(portalLabel);
                Vector2 portalLabelPos = new Vector2(portalRect.X + (portalRect.Width - portalLabelSize.X) / 2f, portalRect.Y - portalLabelSize.Y - 10f);
                // Draw text shadow for visibility
                spriteBatch.DrawString(hudFont, portalLabel, portalLabelPos + new Vector2(2, 2), Color.Black);
                spriteBatch.DrawString(hudFont, portalLabel, portalLabelPos, new Color(255, 255, 0)); // Bright yellow
                
                // Draw arrows pointing to portal
                string arrow = ">>>>";
                Vector2 arrowSize = hudFont.MeasureString(arrow);
                Vector2 leftArrowPos = new Vector2(portalRect.Left - arrowSize.X - 10, portalRect.Center.Y - arrowSize.Y / 2);
                spriteBatch.DrawString(hudFont, arrow, leftArrowPos, new Color(255, 255, 0));
            }

            // Draw the player.
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
            spriteBatch.Draw(playerTexture, playerRect, Color.White);

            string title = "Insert the three items of Zeus";
            Vector2 titleSize = hudFont.MeasureString(title);
            Vector2 titlePos = new Vector2((baseScreenSize.X - titleSize.X) / 2f, 40f);
            spriteBatch.DrawString(hudFont, title, titlePos, Color.Yellow);

            string instructions = "Walk into the BRIGHT PURPLE PORTAL on the right!";
            Vector2 instrSize = hudFont.MeasureString(instructions);
            Vector2 instrPos = new Vector2((baseScreenSize.X - instrSize.X) / 2f, 70f);
            spriteBatch.DrawString(hudFont, instructions, instrPos, new Color(255, 100, 255));

            if (hasCollectedMineItem)
            {
                string hasItem = "Press E near a pillar to place the item";
                Vector2 hasItemSize = hudFont.MeasureString(hasItem);
                Vector2 hasItemPos = new Vector2((baseScreenSize.X - hasItemSize.X) / 2f, 100f);
                spriteBatch.DrawString(hudFont, hasItem, hasItemPos, Color.LightGreen);
            }
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
    }
}
