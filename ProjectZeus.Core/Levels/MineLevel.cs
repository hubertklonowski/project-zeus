using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Physics;
using ProjectZeus.Core.Rendering;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Side-scrolling mine level where player moves right through the mine.
    /// Camera follows the player as they move. Carts drive on tracks toward the player.
    /// </summary>
    public class MineLevel
    {
        // World dimensions - level spans multiple screens horizontally
        private const float WorldWidth = 4000f; // 5 screens wide
        private const float GroundHeight = 20f;
        private const float ScreenWidth = 800f;
        private const float ScreenHeight = 480f;
        
        // Cart speed (moving toward player)
        private const float CartSpeed = 120f;
        
        // Cart spawning for return trip
        private const float CartSpawnInterval = 2.5f; // Seconds between cart spawns during return
        private float cartSpawnTimer;
        
        // Player state
        private Vector2 playerPosition;
        private Vector2 playerVelocity;
        private bool playerOnGround;
        
        // Camera
        private Vector2 cameraOffset;
        
        // Ground
        private Rectangle groundRect;
        
        // Obstacles
        private List<MineCart> carts;
        private List<Stalactite> stalactites;
        private List<MineBat> bats;
        private GigaBat gigaBat;
        private List<Guano> guanos;
        
        // Item at end of level
        private Rectangle itemRect;
        private bool itemCollected;
        
        // Exit portal at start of level
        private Portal exitPortal;
        
        // Textures and fonts
        private Texture2D solidTexture;
        private SpriteFont font;
        
        // Sprites
        private AsepriteSprite cartSprite;
        private AsepriteSprite stalactiteSprite;
        private AsepriteSprite batSprite;
        
        // Random for procedural generation
        private Random random;

        public bool IsActive { get; private set; }
        public bool HasCollectedItem { get; private set; }
        public bool PlayerDied { get; private set; }
        public Vector2 PlayerPosition => playerPosition;
        public Vector2 PlayerVelocity => playerVelocity;
        
        /// <summary>
        /// Gets the camera transformation matrix for rendering
        /// </summary>
        public Matrix GetCameraTransform()
        {
            return Matrix.CreateTranslation(-cameraOffset.X, -cameraOffset.Y, 0);
        }

        public MineLevel()
        {
            carts = new List<MineCart>();
            stalactites = new List<Stalactite>();
            bats = new List<MineBat>();
            guanos = new List<Guano>();
            random = new Random();
            IsActive = false;
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            this.font = font;
            solidTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, Color.White);
            
            // Load sprites
            cartSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Cart);
            stalactiteSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Stalactite);
            batSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Bat);
        }

        public void Enter()
        {
            IsActive = true;
            itemCollected = false;
            HasCollectedItem = false;
            PlayerDied = false;
            cartSpawnTimer = CartSpawnInterval;
            
            // Player starts on the left side of the level
            float groundTop = ScreenHeight - GroundHeight;
            playerPosition = new Vector2(100f, groundTop - GameConstants.PlayerSize.Y);
            playerVelocity = Vector2.Zero;
            playerOnGround = true;
            
            // Initialize camera at start
            cameraOffset = Vector2.Zero;
            
            // Setup ground
            groundRect = new Rectangle(0, (int)(ScreenHeight - GroundHeight), (int)WorldWidth, (int)GroundHeight);
            
            // Generate obstacles procedurally
            GenerateObstacles();
            
            // Place item at end of level
            itemRect = new Rectangle((int)(WorldWidth - 150), (int)(groundTop - 50), 30, 30);
            
            // Create exit portal at the start of the level
            exitPortal = new Portal(
                new Vector2(50f, groundTop - 80f),
                new Vector2(60f, 80f),
                new Color(100, 200, 255));
            exitPortal.IsActive = false; // Only active after collecting item
        }

        private void GenerateObstacles()
        {
            carts.Clear();
            stalactites.Clear();
            bats.Clear();
            
            float groundTop = ScreenHeight - GroundHeight;
            
            // Generate carts on the tracks - they will move toward the player
            float minCartSpacing = 300f;
            float maxCartSpacing = 500f;
            
            float nextCartX = 500f; // First cart position (ahead of player start)
            
            while (nextCartX < WorldWidth - 200)
            {
                // Carts sit on the rails and will move toward player
                carts.Add(new MineCart
                {
                    Position = new Vector2(nextCartX, groundTop - 20), // Cart on rails
                    Velocity = new Vector2(-CartSpeed, 0), // Moving left toward player
                    MinX = 0, // Can go all the way left
                    MaxX = WorldWidth, // Track bounds
                    Sprite = cartSprite
                });
                
                // Random spacing for next cart
                float spacing = minCartSpacing + (float)random.NextDouble() * (maxCartSpacing - minCartSpacing);
                nextCartX += spacing;
            }
            
            // Generate stalactites hanging from ceiling
            // Position them so player can hit their head on them when jumping
            // Player jump reaches approximately Y = groundTop - playerHeight - jumpHeight
            // With JumpVelocity of -560 and gravity 900, max jump height is about 175 pixels
            // So stalactites should extend down to around Y = 285-350 to be dangerous
            float minStalactiteSpacing = 180f;
            float maxStalactiteSpacing = 350f;
            
            float nextStalactiteX = 350f;
            
            while (nextStalactiteX < WorldWidth - 100)
            {
                // Stalactites hang from ceiling (Y=30 is below the ceiling graphic)
                // Make them long enough to reach into jump range
                int height = 80 + random.Next(100); // 80-180 pixels tall, reaching down to Y=110-210
                
                stalactites.Add(new Stalactite
                {
                    Position = new Vector2(nextStalactiteX, 30), // Start below ceiling
                    Size = new Vector2(40, height), // Increased width from 20 to 40 to block player more
                    Sprite = stalactiteSprite
                });
                
                // Random spacing for next stalactite
                float spacing = minStalactiteSpacing + (float)random.NextDouble() * (maxStalactiteSpacing - minStalactiteSpacing);
                nextStalactiteX += spacing;
            }
            
            // Generate bats flying in the play area (where player jumps)
            // Bats should be in the area where player can collide with them during jumps
            float minBatSpacing = 400f;
            float maxBatSpacing = 600f;
            
            float nextBatX = 600f; // First bat position
            
            while (nextBatX < WorldWidth - 300)
            {
                // Bats fly in the jump zone - between groundTop-200 and groundTop-80
                // This puts them at Y = 260 to 380 (reachable by jumping)
                float batY = groundTop - 200f + (float)random.NextDouble() * 120f;
                
                bats.Add(new MineBat
                {
                    Position = new Vector2(nextBatX, batY),
                    Velocity = new Vector2(
                        (float)(random.NextDouble() * 2 - 1) * 60f, 
                        (float)(random.NextDouble() * 2 - 1) * 40f),
                    ChangeDirectionTimer = (float)random.NextDouble() * 2f,
                    Sprite = batSprite
                });
                
                // Random spacing for next bat
                float spacing = minBatSpacing + (float)random.NextDouble() * (maxBatSpacing - minBatSpacing);
                nextBatX += spacing;
            }
            
            // Create one GigaBat positioned in middle of level at ceiling
            gigaBat = new GigaBat
            {
                Position = new Vector2(WorldWidth / 2, 150f),
                Velocity = new Vector2(40f, 0f),
                ChangeDirectionTimer = 3f,
                ShootTimer = 2f,
                Sprite = batSprite
            };
            
            // Clear any existing guano
            guanos.Clear();
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, KeyboardState previousKeyboardState)
        {
            if (!IsActive || PlayerDied) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Player horizontal movement (player controls their movement)
            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;
            
            playerVelocity.X = move * GameConstants.MoveSpeed;
            
            // Jump input
            if (playerOnGround && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W)))
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
            if (playerPosition.X + GameConstants.PlayerSize.X > WorldWidth)
                playerPosition.X = WorldWidth - GameConstants.PlayerSize.X;

            // Ground collision
            float groundTop = ScreenHeight - GroundHeight;
            playerOnGround = false;
            
            if (playerPosition.Y + GameConstants.PlayerSize.Y >= groundTop)
            {
                playerPosition.Y = groundTop - GameConstants.PlayerSize.Y;
                playerVelocity.Y = 0f;
                playerOnGround = true;
            }
            
            // Update camera to follow player (only moves when player moves)
            UpdateCamera();
            
            // Update carts - they move toward the player on tracks
            for (int i = carts.Count - 1; i >= 0; i--)
            {
                var cart = carts[i];
                
                // Move cart toward player (left direction)
                cart.Position += new Vector2(cart.Velocity.X * deltaTime, 0);
                
                // Remove carts that go off the left side of the world
                if (cart.Position.X < -100)
                {
                    carts.RemoveAt(i);
                }
            }
            
            // Spawn carts from the right side when player is returning (after collecting item)
            if (itemCollected)
            {
                cartSpawnTimer -= deltaTime;
                if (cartSpawnTimer <= 0)
                {
                    SpawnReturnCart();
                    cartSpawnTimer = CartSpawnInterval;
                }
            }

            // Update bats (they move around)
            foreach (var bat in bats)
            {
                bat.ChangeDirectionTimer -= deltaTime;
                
                if (bat.ChangeDirectionTimer <= 0)
                {
                    bat.Velocity = new Vector2(
                        (float)(random.NextDouble() * 2 - 1) * 60f, 
                        (float)(random.NextDouble() * 2 - 1) * 40f);
                    bat.ChangeDirectionTimer = 1.5f + (float)random.NextDouble();
                }
                
                bat.Position += bat.Velocity * deltaTime;
                
                // Keep bats in the jump zone (where player can reach them)
                float minBatY = groundTop - 200f; // Top of jump zone
                float maxBatY = groundTop - 60f;  // Just above ground
                
                if (bat.Position.Y < minBatY)
                {
                    bat.Position = new Vector2(bat.Position.X, minBatY);
                    bat.Velocity = new Vector2(bat.Velocity.X, Math.Abs(bat.Velocity.Y));
                }
                if (bat.Position.Y > maxBatY)
                {
                    bat.Position = new Vector2(bat.Position.X, maxBatY);
                    bat.Velocity = new Vector2(bat.Velocity.X, -Math.Abs(bat.Velocity.Y));
                }
            }

            // Check collision with carts
            Rectangle playerRect = new Rectangle(
                (int)playerPosition.X, 
                (int)playerPosition.Y, 
                (int)GameConstants.PlayerSize.X, 
                (int)GameConstants.PlayerSize.Y);
            
            foreach (var cart in carts)
            {
                Rectangle cartRect = cart.Bounds;
                if (playerRect.Intersects(cartRect))
                {
                    PlayerDied = true;
                    return;
                }
            }
            
            // Check collision with stalactites
            foreach (var stalactite in stalactites)
            {
                Rectangle stalactiteRect = new Rectangle(
                    (int)stalactite.Position.X,
                    (int)stalactite.Position.Y,
                    (int)stalactite.Size.X,
                    (int)stalactite.Size.Y);
                    
                if (playerRect.Intersects(stalactiteRect))
                {
                    PlayerDied = true;
                    return;
                }
            }
            
            // Check collision with bats
            foreach (var bat in bats)
            {
                if (playerRect.Intersects(bat.Bounds))
                {
                    PlayerDied = true;
                    return;
                }
            }
            
            // Update GigaBat
            if (gigaBat != null)
            {
                gigaBat.Update(deltaTime, random, 100f, WorldWidth - 100f, 100f, 250f);
                
                // Check if GigaBat should shoot
                if (gigaBat.ShouldShoot())
                {
                    // Shoot guano downward
                    guanos.Add(new Guano
                    {
                        Position = gigaBat.Position,
                        Velocity = new Vector2(0, 200f) // Falls downward
                    });
                }
                
                // Check collision with GigaBat
                if (playerRect.Intersects(gigaBat.Bounds))
                {
                    PlayerDied = true;
                    return;
                }
            }
            
            // Update guano projectiles
            for (int i = guanos.Count - 1; i >= 0; i--)
            {
                var guano = guanos[i];
                guano.Position += guano.Velocity * deltaTime;
                
                // Remove guano that goes off screen
                if (guano.Position.Y > ScreenHeight)
                {
                    guanos.RemoveAt(i);
                    continue;
                }
                
                // Check collision with player
                if (playerRect.Intersects(guano.Bounds))
                {
                    PlayerDied = true;
                    return;
                }
            }

            // Check if player reached the item
            if (!itemCollected && keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E))
            {
                Rectangle expandedItemRect = itemRect;
                expandedItemRect.Inflate(20, 20);
                if (playerRect.Intersects(expandedItemRect))
                {
                    itemCollected = true;
                    HasCollectedItem = true;
                    // Activate exit portal once item is collected
                    exitPortal.IsActive = true;
                }
            }
            
            // Exit through portal at start after collecting item
            if (itemCollected && exitPortal.Intersects(playerRect))
            {
                IsActive = false;
            }
        }
        
        private void UpdateCamera()
        {
            // Camera follows player, keeping them roughly centered horizontally
            // but only moves when player moves past certain thresholds
            float targetCameraX = playerPosition.X - ScreenWidth * 0.3f;
            
            // Clamp camera to world bounds
            targetCameraX = MathHelper.Clamp(targetCameraX, 0, WorldWidth - ScreenWidth);
            
            // Smooth camera follow
            cameraOffset.X = MathHelper.Lerp(cameraOffset.X, targetCameraX, 0.1f);
            cameraOffset.Y = 0; // No vertical scrolling
        }
        
        private void SpawnReturnCart()
        {
            float groundTop = ScreenHeight - GroundHeight;
            
            // Spawn cart just off the right side of the visible screen
            float spawnX = cameraOffset.X + ScreenWidth + 50f;
            
            // Only spawn if not too far into the level (player is returning)
            if (spawnX < WorldWidth)
            {
                carts.Add(new MineCart
                {
                    Position = new Vector2(spawnX, groundTop - 20),
                    Velocity = new Vector2(-CartSpeed, 0), // Moving left toward player
                    MinX = 0,
                    MaxX = WorldWidth,
                    Sprite = cartSprite
                });
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime, 
            AdonisPlayer player, Texture2D portalTexture)
        {
            if (!IsActive) return;

            // Clear to dark mine color
            graphicsDevice.Clear(new Color(20, 15, 30));
            
            // Draw background
            DrawBackground(spriteBatch);
            
            // Draw ceiling (top of screen)
            Rectangle ceilingRect = new Rectangle((int)cameraOffset.X, 0, (int)ScreenWidth + 100, 30);
            spriteBatch.Draw(solidTexture, ceilingRect, new Color(60, 50, 40));
            
            // Draw stalactites
            foreach (var stalactite in stalactites)
            {
                if (stalactite.Position.X >= cameraOffset.X - 50 && 
                    stalactite.Position.X <= cameraOffset.X + ScreenWidth + 50)
                {
                    stalactite.Draw(spriteBatch, solidTexture, gameTime);
                }
            }
            
            // Draw ground
            spriteBatch.Draw(solidTexture, groundRect, new Color(80, 70, 60));
            
            // Draw rails on ground (where carts drive)
            DrawRails(spriteBatch);
            
            // Draw carts (on the rails)
            foreach (var cart in carts)
            {
                if (cart.Position.X >= cameraOffset.X - 100 && 
                    cart.Position.X <= cameraOffset.X + ScreenWidth + 100)
                {
                    cart.Draw(spriteBatch, solidTexture, gameTime);
                }
            }
            
            // Draw bats
            foreach (var bat in bats)
            {
                if (bat.Position.X >= cameraOffset.X - 50 && 
                    bat.Position.X <= cameraOffset.X + ScreenWidth + 50)
                {
                    bat.Draw(spriteBatch, solidTexture, gameTime);
                }
            }
            
            // Draw GigaBat
            if (gigaBat != null && gigaBat.Position.X >= cameraOffset.X - 100 && 
                gigaBat.Position.X <= cameraOffset.X + ScreenWidth + 100)
            {
                gigaBat.Draw(spriteBatch, solidTexture, gameTime);
            }
            
            // Draw guano projectiles
            foreach (var guano in guanos)
            {
                if (guano.Position.X >= cameraOffset.X - 50 && 
                    guano.Position.X <= cameraOffset.X + ScreenWidth + 50)
                {
                    guano.Draw(spriteBatch, solidTexture);
                }
            }
            
            // Draw item if not collected
            if (!itemCollected && itemRect.X >= cameraOffset.X - 50 && 
                itemRect.X <= cameraOffset.X + ScreenWidth + 50)
            {
                spriteBatch.Draw(solidTexture, itemRect, Color.Gold);
                Rectangle glowRect = itemRect;
                glowRect.Inflate(5, 5);
                spriteBatch.Draw(solidTexture, glowRect, new Color(255, 215, 0, 100));
            }
            
            // Draw exit portal if active (after collecting item)
            if (exitPortal != null && exitPortal.IsActive && 
                exitPortal.Position.X >= cameraOffset.X - 100 && 
                exitPortal.Position.X <= cameraOffset.X + ScreenWidth + 100)
            {
                DrawingHelpers.DrawPortal(spriteBatch, portalTexture, exitPortal.Bounds, gameTime, exitPortal.BaseColor);
            }
            
            // Draw player
            player.Draw(gameTime, spriteBatch);
            
            // Draw torches at intervals for atmosphere (decoration only)
            DrawTorches(spriteBatch, gameTime);
        }
        
        private void DrawBackground(SpriteBatch spriteBatch)
        {
            // Draw dark background
            Rectangle bgRect = new Rectangle((int)cameraOffset.X, 0, (int)ScreenWidth + 100, (int)ScreenHeight);
            spriteBatch.Draw(solidTexture, bgRect, new Color(20, 15, 30));
            
            // Draw some background rock texture variation
            for (int x = (int)(cameraOffset.X / 200) * 200; x < cameraOffset.X + ScreenWidth + 200; x += 200)
            {
                // Darker patches for depth
                int patchY = 100 + (x / 200 % 3) * 80;
                Rectangle patch = new Rectangle(x, patchY, 150, 100);
                spriteBatch.Draw(solidTexture, patch, new Color(15, 10, 25));
            }
        }
        
        private void DrawRails(SpriteBatch spriteBatch)
        {
            float groundTop = ScreenHeight - GroundHeight;
            
            // Draw two parallel rails
            int railY1 = (int)(groundTop - 8);
            int railY2 = (int)(groundTop - 3);
            
            // Left rail
            Rectangle leftRail = new Rectangle(0, railY1, (int)WorldWidth, 3);
            spriteBatch.Draw(solidTexture, leftRail, new Color(100, 100, 110));
            
            // Right rail  
            Rectangle rightRail = new Rectangle(0, railY2, (int)WorldWidth, 3);
            spriteBatch.Draw(solidTexture, rightRail, new Color(100, 100, 110));
            
            // Draw rail ties (wooden planks across the rails)
            for (int x = 0; x < WorldWidth; x += 25)
            {
                if (x >= cameraOffset.X - 30 && x <= cameraOffset.X + ScreenWidth + 30)
                {
                    Rectangle tie = new Rectangle(x, railY1 - 2, 15, 12);
                    spriteBatch.Draw(solidTexture, tie, new Color(101, 67, 33));
                }
            }
        }
        
        private void DrawTorches(SpriteBatch spriteBatch, GameTime gameTime)
        {
            float flickerTime = (float)gameTime.TotalGameTime.TotalSeconds;
            float groundTop = ScreenHeight - GroundHeight;
            
            // Draw torches every 300 pixels (decoration only, mounted on ground level)
            for (int x = 100; x < WorldWidth; x += 300)
            {
                if (x >= cameraOffset.X - 50 && x <= cameraOffset.X + ScreenWidth + 50)
                {
                    // Torch post standing on ground
                    Rectangle torchPost = new Rectangle(x, (int)(groundTop - 45), 8, 45);
                    spriteBatch.Draw(solidTexture, torchPost, new Color(101, 67, 33));
                    
                    // Torch holder/bracket at top
                    Rectangle bracket = new Rectangle(x - 3, (int)(groundTop - 50), 14, 8);
                    spriteBatch.Draw(solidTexture, bracket, new Color(80, 50, 25));
                    
                    // Flame with flicker
                    float flicker = (float)Math.Sin(flickerTime * 8f + x * 0.1f) * 0.2f + 0.8f;
                    Rectangle flame = new Rectangle(x - 4, (int)(groundTop - 65), 16, 15);
                    spriteBatch.Draw(solidTexture, flame, new Color((byte)(255 * flicker), (byte)(200 * flicker), 50));
                    
                    // Inner flame (brighter)
                    Rectangle innerFlame = new Rectangle(x - 1, (int)(groundTop - 62), 10, 10);
                    spriteBatch.Draw(solidTexture, innerFlame, new Color((byte)(255 * flicker), (byte)(255 * flicker), 100));
                }
            }
        }
        
        /// <summary>
        /// Draws UI elements that should not be affected by camera
        /// </summary>
        public void DrawUI(SpriteBatch spriteBatch)
        {
            if (!IsActive) return;

            if (itemCollected)
            {
                string hasItem = "Item collected! Return to the start and exit through the portal!";
                Vector2 hasItemSize = font.MeasureString(hasItem);
                Vector2 hasItemPos = new Vector2((ScreenWidth - hasItemSize.X) / 2f, 10f);
                spriteBatch.DrawString(font, hasItem, hasItemPos, Color.LightGreen);
            }
        }
        
        /// <summary>
        /// Resets the level state
        /// </summary>
        public void Reset()
        {
            IsActive = false;
            PlayerDied = false;
            itemCollected = false;
            HasCollectedItem = false;
            carts.Clear();
            stalactites.Clear();
            bats.Clear();
            guanos.Clear();
            gigaBat = null;
            exitPortal = null;
            cameraOffset = Vector2.Zero;
        }
    }
}
