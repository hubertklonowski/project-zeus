using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Utilities;
using ProjectZeus.Core.Extensions;
using ProjectZeus.Core.Levels.Mine;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Side-scrolling mine level where player moves right through the mine.
    /// Camera follows the player as they move. Carts drive on tracks toward the player.
    /// </summary>
    public class MineLevel
    {
        // World dimensions - level spans multiple screens horizontally
        private const float WorldWidth = 4000f;
        private const float GroundHeight = 60f;
        private const float ScreenWidth = 800f;
        private const float ScreenHeight = 480f;
        
        // Cart speed (moving toward player)
        private const float CartSpeed = 120f;
        
        // Cart spawning for return trip
        private const float CartSpawnInterval = 2.5f;
        private float cartSpawnTimer;
        
        // Player state
        private Vector2 playerPosition;
        private Vector2 playerVelocity;
        private bool playerOnGround;
        
        // Camera
        private CameraController camera;
        
        // Ground
        private Rectangle groundRect;
        private List<FloorTile> floorTiles;
        
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
        private AsepriteSprite[] floorSprites;
        
        // Helpers
        private Random random;
        private MineObstacleGenerator obstacleGenerator;
        private MineEntityUpdater entityUpdater;
        private MineCollisionChecker collisionChecker;
        private MinePlayerController playerController;

        public bool IsActive { get; private set; }
        public bool HasCollectedItem { get; private set; }
        public bool PlayerDied { get; private set; }
        public Vector2 PlayerPosition => playerPosition;
        public Vector2 PlayerVelocity => playerVelocity;
        
        public Matrix GetCameraTransform()
        {
            return camera.GetTransform();
        }

        public MineLevel()
        {
            carts = new List<MineCart>();
            stalactites = new List<Stalactite>();
            bats = new List<MineBat>();
            guanos = new List<Guano>();
            floorTiles = new List<FloorTile>();
            random = new Random();
            IsActive = false;
            camera = new CameraController(ScreenWidth, ScreenHeight, WorldWidth, ScreenHeight);
            obstacleGenerator = new MineObstacleGenerator(WorldWidth, ScreenHeight, GroundHeight, CartSpeed, random);
            entityUpdater = new MineEntityUpdater(WorldWidth, ScreenHeight, GroundHeight, random);
            collisionChecker = new MineCollisionChecker();
            playerController = new MinePlayerController(WorldWidth);
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            this.font = font;
            solidTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, Color.White);
            
            // Load sprites
            cartSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Cart);
            stalactiteSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Stalactite);
            batSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Bat);
            
            // Load floor sprites
            floorSprites = new AsepriteSprite[3];
            floorSprites[0] = AsepriteSprite.Load(graphicsDevice, AssetPaths.MineFloor1);
            floorSprites[1] = AsepriteSprite.Load(graphicsDevice, AssetPaths.MineFloor2);
            floorSprites[2] = AsepriteSprite.Load(graphicsDevice, AssetPaths.MineFloor3);
        }

        public void Enter()
        {
            IsActive = true;
            itemCollected = false;
            HasCollectedItem = false;
            PlayerDied = false;
            cartSpawnTimer = CartSpawnInterval;
            
            // Determine tile height for proper ground positioning using actual loaded sprite
            float tileHeight = 32f; // default fallback
            if (floorSprites != null && floorSprites.Length > 0 && floorSprites[0] != null && floorSprites[0].IsLoaded)
            {
                tileHeight = floorSprites[0].Size.Y;
            }
            
            // Ground top is now at the top of the floor tiles
            float groundTop = ScreenHeight - tileHeight;
            
            // Set the ground top for player controller
            playerController.SetGroundTop(groundTop);
            
            // Player starts on the left side of the level, standing on the new ground level
            playerPosition = new Vector2(100f, groundTop - GameConstants.PlayerSize.Y);
            playerVelocity = Vector2.Zero;
            playerOnGround = true;
            
            // Initialize camera at start
            camera.Reset();
            
            // Setup ground rect (using actual tile height for physics)
            groundRect = new Rectangle(0, (int)groundTop, (int)WorldWidth, (int)tileHeight);
            
            // Generate random floor tiles
            GenerateFloorTiles(groundTop);
            
            // Generate obstacles using the actual ground top position
            obstacleGenerator = new MineObstacleGenerator(WorldWidth, ScreenHeight, tileHeight, CartSpeed, random);
            obstacleGenerator.GenerateObstacles(carts, stalactites, bats, out gigaBat, 
                cartSprite, stalactiteSprite, batSprite);
            
            // Clear any existing guano
            guanos.Clear();
            
            // Place item at end of level
            itemRect = new Rectangle((int)(WorldWidth - 150), (int)(groundTop - 50), 30, 30);
            
            // Create exit portal at the start of the level
            exitPortal = new Portal(
                new Vector2(50f, groundTop - 80f),
                new Vector2(60f, 80f),
                new Color(100, 200, 255));
            exitPortal.IsActive = false; // Only active after collecting item
        }
        
        private void GenerateFloorTiles(float groundTop)
        {
            floorTiles.Clear();
            
            // Determine tile size from the first loaded sprite, or use default
            float tileWidth = 32f;
            float tileHeight = 32f;
            
            if (floorSprites != null && floorSprites.Length > 0 && floorSprites[0] != null && floorSprites[0].IsLoaded)
            {
                tileWidth = floorSprites[0].Size.X;
                tileHeight = floorSprites[0].Size.Y;
            }
            
            // Position tiles at the bottom of the screen so they fill from bottom up
            // This ensures no gap between tiles and screen bottom
            float tileY = ScreenHeight - tileHeight;
            
            // Generate tiles across the entire world width
            for (float x = 0; x < WorldWidth; x += tileWidth)
            {
                int spriteIndex = random.Next(0, 3); // Randomly choose from 3 floor sprites
                floorTiles.Add(new FloorTile
                {
                    Position = new Vector2(x, tileY),
                    SpriteIndex = spriteIndex
                });
            }
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, KeyboardState previousKeyboardState)
        {
            if (!IsActive || PlayerDied) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update player physics
            playerController.UpdatePlayer(keyboardState, ref playerPosition, ref playerVelocity, ref playerOnGround, deltaTime);
            
            // Update camera to follow player
            camera.FollowPlayerHorizontal(playerPosition);
            
            // Update entities using entity updater
            entityUpdater.UpdateCarts(carts, deltaTime);
            
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

            entityUpdater.UpdateBats(bats, deltaTime);
            entityUpdater.UpdateGigaBat(gigaBat, guanos, deltaTime);
            entityUpdater.UpdateGuano(guanos, deltaTime);

            // Check all collisions
            Rectangle playerRect = playerPosition.ToRectangle(GameConstants.PlayerSize);
            
            if (collisionChecker.CheckPlayerDeath(playerRect, carts, stalactites, bats, gigaBat, guanos))
            {
                PlayerDied = true;
                return;
            }

            // Check if player reached the item using extension method
            if (!itemCollected && keyboardState.WasActionPressed(previousKeyboardState))
            {
                if (CollisionExtensions.CheckPickupCollision(playerRect, itemRect))
                {
                    itemCollected = true;
                    HasCollectedItem = true;
                    exitPortal.IsActive = true;
                }
            }
            
            // Exit through portal at start after collecting item
            if (itemCollected && exitPortal.Intersects(playerRect))
            {
                IsActive = false;
            }
        }
        
        private void SpawnReturnCart()
        {
            // Determine tile height for proper ground positioning - same as in Enter()
            float tileHeight = 32f; // default
            if (floorSprites != null && floorSprites.Length > 0 && floorSprites[0] != null && floorSprites[0].IsLoaded)
            {
                tileHeight = floorSprites[0].Size.Y;
            }
            
            // Ground top is at the top of the floor tiles
            float groundTop = ScreenHeight - tileHeight;
            
            // Spawn cart just off the right side of the visible screen
            float spawnX = camera.CameraOffset.X + ScreenWidth + 50f;
            
            // Only spawn if not too far into the level (player is returning)
            if (spawnX < WorldWidth)
            {
                // Calculate cart height to position it properly on the ground
                float cartHeight = cartSprite?.IsLoaded == true ? cartSprite.Size.Y : 30f;
                float cartY = groundTop - cartHeight / 2f; // Position center of cart above ground
                
                carts.Add(new MineCart
                {
                    Position = new Vector2(spawnX, cartY),
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

            graphicsDevice.Clear(new Color(20, 15, 30));
            
            float cameraOffsetX = camera.CameraOffset.X;
            MineRenderer renderer = new MineRenderer(solidTexture);
            
            // Draw all mine level elements
            renderer.DrawBackground(spriteBatch, cameraOffsetX, ScreenWidth, ScreenHeight);
            
            Rectangle ceilingRect = new Rectangle((int)cameraOffsetX, 0, (int)ScreenWidth + 100, 30);
            spriteBatch.Draw(solidTexture, ceilingRect, new Color(60, 50, 40));
            
            renderer.DrawVisibleEntities(spriteBatch, gameTime, cameraOffsetX, stalactites, carts, bats, gigaBat, guanos);
            
            // Draw floor with random tiles
            renderer.DrawFloor(spriteBatch, floorTiles, floorSprites, cameraOffsetX, ScreenWidth, groundRect);
            renderer.DrawCollectibles(spriteBatch, gameTime, portalTexture, cameraOffsetX, itemRect, itemCollected, exitPortal);
            
            player.Draw(gameTime, spriteBatch);
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
            floorTiles.Clear();
            gigaBat = null;
            exitPortal = null;
            camera.Reset();
        }
    }
}