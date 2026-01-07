using System;
using System.Collections.Generic;
using System.IO;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Levels;

namespace ProjectZeus.Core
{
    /// <summary>
    /// Mountain climbing level where player must reach the top to collect an item.
    /// A goat at the top throws rocks that can kill the player.
    /// </summary>
    public class MountainLevel
    {
        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        
        // World dimensions - level spans multiple screens vertically
        private float worldHeight;
        
        // Camera offset for scrolling
        private Vector2 cameraOffset;
        
        // Textures
        private Texture2D solidTexture;
        private AsepriteSprite goatSprite;
        private AsepriteSprite rockSprite;
        private SpriteFont font;
        
        // Random number generator for rock throwing
        private static readonly System.Random random = new System.Random();
        
        // Constants for rock throwing
        private const float ThrowAngleVariation = 40f; // Degrees of variation from straight down
        private const float ThrowAngleOffset = 20f; // Offset to center the variation
        
        // Constants for platform collision detection
        private const int CollisionTopOffset = 2;
        private const int CollisionHeight = 6;
        private const int CollisionVerticalThreshold = 20;
        
        // Mountain platforms
        private List<Platform> platforms;
        private List<MovingPlatform> movingPlatforms;
        
        // Goat enemy
        private Vector2 goatPosition;
        private readonly Vector2 goatSize = new Vector2(40, 40);
        private float goatThrowTimer;
        private const float GoatThrowInterval = 1f; // Increased frequency - was 2.5f
        private Vector2 goatVelocity;
        private const float GoatMoveSpeed = 70f;
        private Rectangle topPlatformBounds;
        
        // Rocks (projectiles)
        private List<Rock> rocks;
        
        // Collectible item at mountain top
        private Vector2 itemPosition;
        private readonly Vector2 itemSize = new Vector2(30, 30);
        private bool itemCollected;
        
        public bool PlayerDied { get; private set; }
        public bool ItemWasCollected { get; private set; }
        
        /// <summary>
        /// Gets the total world height for the level
        /// </summary>
        public float WorldHeight => worldHeight;
        
        /// <summary>
        /// Gets the current camera offset for rendering
        /// </summary>
        public Vector2 CameraOffset => cameraOffset;
        
        public MountainLevel()
        {
            platforms = new List<Platform>();
            movingPlatforms = new List<MovingPlatform>();
            rocks = new List<Rock>();
            itemCollected = false;
            PlayerDied = false;
            ItemWasCollected = false;
            cameraOffset = Vector2.Zero;
        }
        
        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            // Create a 1x1 solid texture for simple rectangles
            solidTexture = new Texture2D(graphicsDevice, 1, 1);
            solidTexture.SetData(new[] { Color.White });
            
            this.font = font;
            
            // Load goat sprite
            goatSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Goat);
            
            // Load rock sprite for projectiles
            rockSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Rock);
            
            // Build the mountain structure with platforms
            SetupMountain();
        }
        
        private void SetupMountain()
        {
            // Get world height from the platform builder
            worldHeight = MountainPlatformBuilder.WorldHeight;
            
            // Build platforms (both static and moving)
            var (staticPlatforms, movingPlats) = MountainPlatformBuilder.BuildPlatforms(baseScreenSize);
            platforms = staticPlatforms;
            movingPlatforms = movingPlats;
            
            // The top platform is at the very top of the extended world
            float topPlatformY = MountainPlatformBuilder.GetTopPlatformY();
            float topPlatformWidth = 300f;
            float topPlatformX = baseScreenSize.X / 2f - topPlatformWidth / 2f;
            
            // Store top platform bounds for goat movement constraint
            topPlatformBounds = new Rectangle(
                (int)topPlatformX, 
                (int)topPlatformY, 
                (int)topPlatformWidth, 
                15);
            
            goatPosition = new Vector2(topPlatformX + 50f, topPlatformY - goatSize.Y);
            goatVelocity = new Vector2(GoatMoveSpeed, 0f); // Start moving right
            goatThrowTimer = GoatThrowInterval;
            
            // Position item on the top platform with the goat
            itemPosition = new Vector2(topPlatformX + topPlatformWidth - 60f, topPlatformY - itemSize.Y);
            
            // Initialize camera to show bottom of level (where player starts)
            cameraOffset = new Vector2(0, worldHeight - baseScreenSize.Y);
        }
        
        /// <summary>
        /// Updates the camera position to follow the player
        /// </summary>
        public void UpdateCamera(Vector2 playerPosition)
        {
            // Camera follows player vertically, keeping player in center-bottom portion of screen
            float targetCameraY = playerPosition.Y - baseScreenSize.Y * 0.6f;
            
            // Clamp camera to world bounds
            targetCameraY = MathHelper.Clamp(targetCameraY, 0, worldHeight - baseScreenSize.Y);
            
            // Smooth camera follow
            cameraOffset.Y = MathHelper.Lerp(cameraOffset.Y, targetCameraY, 0.1f);
            
            // Keep X at 0 (no horizontal scrolling)
            cameraOffset.X = 0;
        }
        
        /// <summary>
        /// Gets the player's spawn position at the bottom of the level
        /// </summary>
        public Vector2 GetPlayerSpawnPosition(Vector2 playerSize)
        {
            float groundTop = worldHeight - 20; // Ground is at bottom of world
            return new Vector2(60f, groundTop - playerSize.Y);
        }
        
        public void Update(GameTime gameTime, Vector2 playerPosition, Vector2 playerSize, bool tryPickupItem)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update camera to follow player
            UpdateCamera(playerPosition);
            
            // Update moving platforms
            foreach (var movingPlatform in movingPlatforms)
            {
                movingPlatform.Update(dt);
            }
            
            // Update goat movement (patrol back and forth on top platform)
            goatPosition += goatVelocity * dt;
            
            // Keep goat on the platform - reverse direction when reaching edges
            if (goatPosition.X <= topPlatformBounds.X)
            {
                goatPosition.X = topPlatformBounds.X;
                goatVelocity.X = GoatMoveSpeed; // Move right
            }
            else if (goatPosition.X + goatSize.X >= topPlatformBounds.X + topPlatformBounds.Width)
            {
                goatPosition.X = topPlatformBounds.X + topPlatformBounds.Width - goatSize.X;
                goatVelocity.X = -GoatMoveSpeed; // Move left
            }
            
            // Update goat throw timer
            goatThrowTimer -= dt;
            if (goatThrowTimer <= 0f)
            {
                ThrowRock();
                goatThrowTimer = GoatThrowInterval;
            }
            
            // Update rocks
            for (int i = rocks.Count - 1; i >= 0; i--)
            {
                rocks[i].Position += rocks[i].Velocity * dt;
                rocks[i].Rotation += rocks[i].RotationSpeed * dt;
                
                // Remove rocks that go off the bottom of the world
                if (rocks[i].Position.Y > worldHeight + 50)
                {
                    rocks.RemoveAt(i);
                    continue;
                }
                
                // Check collision with player
                Rectangle rockRect = new Rectangle(
                    (int)rocks[i].Position.X,
                    (int)rocks[i].Position.Y,
                    (int)rocks[i].Size.X,
                    (int)rocks[i].Size.Y);
                    
                Rectangle playerRect = new Rectangle(
                    (int)playerPosition.X,
                    (int)playerPosition.Y,
                    (int)playerSize.X,
                    (int)playerSize.Y);
                
                if (rockRect.Intersects(playerRect))
                {
                    PlayerDied = true;
                }
            }
            
            // Check if player can pick up item
            if (!itemCollected && tryPickupItem)
            {
                Rectangle itemRect = new Rectangle(
                    (int)itemPosition.X,
                    (int)itemPosition.Y,
                    (int)itemSize.X,
                    (int)itemSize.Y);
                    
                Rectangle playerRect = new Rectangle(
                    (int)playerPosition.X,
                    (int)playerPosition.Y,
                    (int)playerSize.X,
                    (int)playerSize.Y);
                
                // Check if player is near the item
                Rectangle expandedItemRect = itemRect;
                expandedItemRect.Inflate(20, 20);
                
                if (expandedItemRect.Intersects(playerRect))
                {
                    itemCollected = true;
                    ItemWasCollected = true;
                }
            }
        }
        
        private void ThrowRock()
        {
            // Goat throws rock downward with some randomness (70-110 degrees)
            float angle = MathHelper.ToRadians(90f + (float)(random.NextDouble() * ThrowAngleVariation - ThrowAngleOffset));
            float speed = 200f;
            
            // Use rock sprite size if available, otherwise fallback to 20x20
            Vector2 rockSize = (rockSprite != null && rockSprite.IsLoaded) 
                ? rockSprite.Size 
                : new Vector2(20, 20);
            
            rocks.Add(new Rock
            {
                Position = new Vector2(goatPosition.X + goatSize.X / 2f - rockSize.X / 2f, goatPosition.Y + goatSize.Y),
                Size = rockSize,
                Velocity = new Vector2((float)System.Math.Cos(angle) * speed, (float)System.Math.Sin(angle) * speed),
                Rotation = 0f,
                RotationSpeed = (float)(random.NextDouble() * 10f - 5f) // Random rotation speed
            });
        }
        
        public bool CheckPlatformCollision(Rectangle playerRect, Vector2 playerVelocity, out Vector2 correctedPosition)
        {
            correctedPosition = new Vector2(playerRect.X, playerRect.Y);
            bool onPlatform = false;
            
            // Check static platforms
            foreach (var platform in platforms)
            {
                Rectangle platformRect = new Rectangle(
                    (int)platform.Position.X,
                    (int)platform.Position.Y,
                    (int)platform.Size.X,
                    (int)platform.Size.Y);
                
                // Top collision (landing on platform)
                Rectangle topRect = new Rectangle(platformRect.X, platformRect.Y - CollisionTopOffset, platformRect.Width, CollisionHeight);
                
                if (playerRect.Bottom > topRect.Top &&
                    playerRect.Bottom <= topRect.Top + CollisionVerticalThreshold &&
                    playerRect.Right > topRect.Left &&
                    playerRect.Left < topRect.Right &&
                    playerVelocity.Y >= 0)
                {
                    correctedPosition.Y = topRect.Top - playerRect.Height;
                    onPlatform = true;
                }
            }
            
            // Check moving platforms
            foreach (var movingPlatform in movingPlatforms)
            {
                Rectangle platformRect = new Rectangle(
                    (int)movingPlatform.Position.X,
                    (int)movingPlatform.Position.Y,
                    (int)movingPlatform.Size.X,
                    (int)movingPlatform.Size.Y);
                
                // Top collision (landing on platform)
                Rectangle topRect = new Rectangle(platformRect.X, platformRect.Y - CollisionTopOffset, platformRect.Width, CollisionHeight);
                
                if (playerRect.Bottom > topRect.Top &&
                    playerRect.Bottom <= topRect.Top + CollisionVerticalThreshold &&
                    playerRect.Right > topRect.Left &&
                    playerRect.Left < topRect.Right &&
                    playerVelocity.Y >= 0)
                {
                    correctedPosition.Y = topRect.Top - playerRect.Height;
                    onPlatform = true;
                }
            }
            
            return onPlatform;
        }
        
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            // Clear to sky blue
            graphicsDevice.Clear(new Color(135, 206, 235));
            
            if (solidTexture == null)
                return;
            
            // Create camera transformation matrix
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraOffset.X, -cameraOffset.Y, 0);
            
            spriteBatch.Begin(transformMatrix: cameraTransform);
            
            // Draw sky background (stretched to cover visible area)
            Rectangle skyRect = new Rectangle(0, (int)cameraOffset.Y, (int)baseScreenSize.X, (int)baseScreenSize.Y);
            spriteBatch.Draw(solidTexture, skyRect, new Color(135, 206, 235));
            
            // Draw mountain body (triangular shape in background)
            DrawMountainBackground(spriteBatch);
            
            // Draw platforms
            foreach (var platform in platforms)
            {
                Rectangle platformRect = new Rectangle(
                    (int)platform.Position.X,
                    (int)platform.Position.Y,
                    (int)platform.Size.X,
                    (int)platform.Size.Y);
                spriteBatch.Draw(solidTexture, platformRect, platform.Color);
                
                // Draw platform outline
                DrawRectangleOutline(spriteBatch, platformRect, new Color(80, 60, 40));
            }
            
            // Draw moving platforms with distinct appearance
            foreach (var movingPlatform in movingPlatforms)
            {
                Rectangle platformRect = new Rectangle(
                    (int)movingPlatform.Position.X,
                    (int)movingPlatform.Position.Y,
                    (int)movingPlatform.Size.X,
                    (int)movingPlatform.Size.Y);
                spriteBatch.Draw(solidTexture, platformRect, movingPlatform.Color);
                
                // Draw platform outline with different color to indicate it moves
                DrawRectangleOutline(spriteBatch, platformRect, new Color(100, 80, 60));
                
                // Draw arrows to indicate movement direction
                int arrowY = platformRect.Y + platformRect.Height / 2 - 3;
                Rectangle leftArrow = new Rectangle(platformRect.X + 5, arrowY, 8, 6);
                Rectangle rightArrow = new Rectangle(platformRect.Right - 13, arrowY, 8, 6);
                spriteBatch.Draw(solidTexture, leftArrow, new Color(60, 50, 40));
                spriteBatch.Draw(solidTexture, rightArrow, new Color(60, 50, 40));
            }
            
            // Draw goat using aseprite sprite
            if (goatSprite != null && goatSprite.IsLoaded)
            {
                // Check if goat is moving
                bool isMoving = goatVelocity.LengthSquared() > 0;
                
                // Draw goat sprite centered at position
                Vector2 drawPos = new Vector2(
                    goatPosition.X + goatSize.X / 2 - goatSprite.Size.X / 2,
                    goatPosition.Y + goatSize.Y / 2 - goatSprite.Size.Y / 2);
                
                // Flip sprite based on movement direction
                SpriteEffects flip = goatVelocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                goatSprite.Draw(spriteBatch, drawPos, isMoving, gameTime, Color.White, 10f, flip);
            }
            else
            {
                // Fallback: Draw simple rectangle placeholder goat
                Rectangle goatRect = new Rectangle(
                    (int)goatPosition.X,
                    (int)goatPosition.Y,
                    (int)goatSize.X,
                    (int)goatSize.Y);
                spriteBatch.Draw(solidTexture, goatRect, Color.White);
                
                // Draw simple goat face details
                Rectangle goatEye1 = new Rectangle((int)goatPosition.X + 10, (int)goatPosition.Y + 10, 6, 6);
                Rectangle goatEye2 = new Rectangle((int)goatPosition.X + 24, (int)goatPosition.Y + 10, 6, 6);
                spriteBatch.Draw(solidTexture, goatEye1, Color.Black);
                spriteBatch.Draw(solidTexture, goatEye2, Color.Black);
                
                // Draw horns
                Rectangle horn1 = new Rectangle((int)goatPosition.X + 5, (int)goatPosition.Y - 5, 4, 8);
                Rectangle horn2 = new Rectangle((int)goatPosition.X + 31, (int)goatPosition.Y - 5, 4, 8);
                spriteBatch.Draw(solidTexture, horn1, new Color(220, 220, 200));
                spriteBatch.Draw(solidTexture, horn2, new Color(220, 220, 200));
            }
            
            // Draw rocks using rock sprite
            foreach (var rock in rocks)
            {
                if (rockSprite != null && rockSprite.IsLoaded)
                {
                    // Draw rock sprite with rotation
                    var texture = rockSprite.GetFrameTexture(0);
                    if (texture != null)
                    {
                        Vector2 origin = new Vector2(rockSprite.Size.X / 2f, rockSprite.Size.Y / 2f);
                        Vector2 drawPos = rock.Position + origin;
                        spriteBatch.Draw(texture, drawPos, null, Color.White, rock.Rotation, origin, 1f, SpriteEffects.None, 0f);
                    }
                }
                else
                {
                    // Fallback to simple rectangle
                    Rectangle rockRect = new Rectangle(
                        (int)rock.Position.X,
                        (int)rock.Position.Y,
                        (int)rock.Size.X,
                        (int)rock.Size.Y);
                    spriteBatch.Draw(solidTexture, rockRect, new Color(80, 70, 60));
                }
            }
            
            // Draw item if not collected
            if (!itemCollected)
            {
                Rectangle itemRect = new Rectangle(
                    (int)itemPosition.X,
                    (int)itemPosition.Y,
                    (int)itemSize.X,
                    (int)itemSize.Y);
                spriteBatch.Draw(solidTexture, itemRect, Color.Gold);
                
                // Draw item glow/outline
                DrawRectangleOutline(spriteBatch, itemRect, Color.Yellow);
            }
            
            spriteBatch.End();
            
            // Draw UI elements (not affected by camera)
            spriteBatch.Begin();
            
            // Draw title
            if (font != null)
            {
                string title = itemCollected ? "Item collected! Return to pillar room" : "Climb the mountain and collect the item!";
                Vector2 titleSize = font.MeasureString(title);
                Vector2 titlePos = new Vector2((baseScreenSize.X - titleSize.X) / 2f, 10f);
                spriteBatch.DrawString(font, title, titlePos, Color.White);
            }
            
            spriteBatch.End();
        }
        
        /// <summary>
        /// Draws the player with camera offset applied. Call this after Draw() with a separate SpriteBatch.
        /// </summary>
        public Matrix GetCameraTransform()
        {
            return Matrix.CreateTranslation(-cameraOffset.X, -cameraOffset.Y, 0);
        }
        
        private void DrawMountainBackground(SpriteBatch spriteBatch)
        {
            // Draw a simple triangular mountain shape in the background
            // Scaled to cover the entire world height
            Color mountainColor = new Color(160, 140, 120);
            
            // Draw mountain as a series of horizontal strips getting narrower toward top
            int strips = 100; // More strips for taller mountain
            float baseWidth = baseScreenSize.X * 0.9f;
            float startX = baseScreenSize.X * 0.05f;
            float bottomY = worldHeight - 20;
            float topY = 20;
            
            for (int i = 0; i < strips; i++)
            {
                float t = (float)i / strips;
                float y = MathHelper.Lerp(bottomY, topY, t);
                float width = baseWidth * (1 - t * 0.8f); // Narrower at top
                float x = startX + (baseWidth - width) / 2f;
                
                float stripHeight = (bottomY - topY) / strips + 2;
                Rectangle strip = new Rectangle((int)x, (int)y, (int)width, (int)stripHeight);
                spriteBatch.Draw(solidTexture, strip, mountainColor * (0.2f + t * 0.4f));
            }
        }
        
        private void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(solidTexture, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
            spriteBatch.Draw(solidTexture, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), color);
            spriteBatch.Draw(solidTexture, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
            spriteBatch.Draw(solidTexture, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), color);
        }
        
        public void Reset()
        {
            rocks.Clear();
            itemCollected = false;
            ItemWasCollected = false;
            PlayerDied = false;
            goatThrowTimer = GoatThrowInterval;
            
            // Reset goat position and velocity
            if (topPlatformBounds.Width > 0)
            {
                goatPosition = new Vector2(topPlatformBounds.X + 50f, topPlatformBounds.Y - goatSize.Y);
                goatVelocity = new Vector2(GoatMoveSpeed, 0f);
            }
            
            // Reset moving platforms to starting positions
            foreach (var movingPlatform in movingPlatforms)
            {
                movingPlatform.Position = movingPlatform.StartPosition;
                movingPlatform.Progress = 0f;
                movingPlatform.MovingToEnd = true;
            }
            
            // Reset camera to show bottom of level
            cameraOffset = new Vector2(0, worldHeight - baseScreenSize.Y);
        }
    }
}
