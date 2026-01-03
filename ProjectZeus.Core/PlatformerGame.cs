#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
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

        // Scene management.
        private bool inZeusFightScene;
        private ZeusFightScene zeusFightScene;
        
        // Maze level management.
        private bool inMazeLevel;
        private MazeLevel mazeLevel;
        private bool hasCollectedItem;
        private KeyboardState previousKeyboardState;
        
        // Portal for maze entrance
        private Vector2 portalPosition;
        private Vector2 portalSize = new Vector2(60, 80);
        private Texture2D portalTexture;
        private readonly Color portalBaseColor = new Color(100, 50, 200);
        private const float portalMarginFromEdge = 100f;
        private const float portalPulseFrequency = 3f;
        private const float portalPulseAmplitude = 0.3f;
        private const float portalPulseOffset = 0.7f;

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
            portalTexture = CreateSolidTexture(GraphicsDevice, 1, 1, portalBaseColor);

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

            // Place portal on the right side of the screen
            float portalX = (pillars[0].Position.X + pillars[1].Position.X) / 2f - portalSize.X / 2f;
            portalPosition = new Vector2(portalX, groundTop - portalSize.Y);

            inZeusFightScene = false;
            zeusFightScene = new ZeusFightScene();
            zeusFightScene.LoadContent(GraphicsDevice, hudFont);
            
            inMazeLevel = false;
            mazeLevel = new MazeLevel();
            mazeLevel.LoadContent(GraphicsDevice, hudFont);
            hasCollectedItem = false;
            previousKeyboardState = Keyboard.GetState();
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

            // Interaction: press E near a pillar to insert an item
            bool eKeyPressed = keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E);
            
            if (eKeyPressed && hasCollectedItem)
            {
                TryInsertItemAtPlayer();
            }
            
            // Check portal collision to enter maze (only when not carrying item)
            if (!hasCollectedItem)
            {
                Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 
                                                     (int)playerSize.X, (int)playerSize.Y);
                Rectangle portalRect = new Rectangle((int)portalPosition.X, (int)portalPosition.Y,
                                                     (int)portalSize.X, (int)portalSize.Y);
                
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
            base.Update(gameTime);
        }

        private void TryInsertItemAtPlayer()
        {
            if (pillars == null || pillarHasItem == null)
                return;
            
            if (!hasCollectedItem)
                return;

            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)playerSize.X, (int)playerSize.Y);

            for (int i = 0; i < pillars.Length; i++)
            {
                Rectangle slotRect = pillars[i].GetSlotRectangle();

                // Define a small interaction zone around the slot.
                Rectangle interactionRect = slotRect;
                interactionRect.Inflate(20, 20);

                if (playerRect.Intersects(interactionRect))
                {
                    // Only allow placing item on empty pillars
                    if (!pillarHasItem[i])
                    {
                        pillarHasItem[i] = true;
                        hasCollectedItem = false; // Item has been placed
                    }
                    break;
                }
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

            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, globalTransformation);

            DrawMainScene(gameTime);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
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

            // Draw portal (if player doesn't have an item)
            if (!hasCollectedItem)
            {
                Rectangle portalRect = new Rectangle((int)portalPosition.X, (int)portalPosition.Y,
                                                     (int)portalSize.X, (int)portalSize.Y);
                
                // Animated portal effect using time
                float portalTime = (float)gameTime.TotalGameTime.TotalSeconds;
                float pulse = (float)(Math.Sin(portalTime * portalPulseFrequency) * portalPulseAmplitude + portalPulseOffset);
                
                // Draw portal base
                spriteBatch.Draw(portalTexture, portalRect, portalBaseColor * pulse);
                
                // Draw portal inner glow
                Rectangle innerRect = portalRect;
                innerRect.Inflate(-8, -8);
                spriteBatch.Draw(portalTexture, innerRect, new Color(150, 100, 255) * pulse);
                
                // Draw portal core
                Rectangle coreRect = portalRect;
                coreRect.Inflate(-16, -16);
                spriteBatch.Draw(portalTexture, coreRect, Color.White * pulse);
                
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

            string title = "Insert the three items of Zeus";
            Vector2 titleSize = hudFont.MeasureString(title);
            Vector2 titlePos = new Vector2((baseScreenSize.X - titleSize.X) / 2f, 40f);
            spriteBatch.DrawString(hudFont, title, titlePos, Color.Yellow);
            
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
    }
}
