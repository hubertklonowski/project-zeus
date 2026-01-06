using System;
using System.Collections.Generic;
using System.IO;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using ProjectZeus.Core.Rendering;

namespace ProjectZeus.Core
{
    /// <summary>
    /// Mountain climbing level where player must reach the top to collect an item.
    /// A goat at the top throws rocks that can kill the player.
    /// </summary>
    public class MountainLevel
    {
        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        
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
        
        // Goat enemy
        private Vector2 goatPosition;
        private readonly Vector2 goatSize = new Vector2(40, 40);
        private float goatThrowTimer;
        private const float GoatThrowInterval = 2.5f;
        private Vector2 goatVelocity;
        private const float GoatMoveSpeed = 60f;
        private Rectangle topPlatformBounds;
        
        // Rocks (projectiles)
        private List<Rock> rocks;
        
        // Collectible item at mountain top
        private Vector2 itemPosition;
        private readonly Vector2 itemSize = new Vector2(30, 30);
        private bool itemCollected;
        
        public bool PlayerDied { get; private set; }
        public bool ItemWasCollected { get; private set; }
        
        public MountainLevel()
        {
            platforms = new List<Platform>();
            rocks = new List<Rock>();
            itemCollected = false;
            PlayerDied = false;
            ItemWasCollected = false;
        }
        
        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            // Create a 1x1 solid texture for simple rectangles
            solidTexture = new Texture2D(graphicsDevice, 1, 1);
            solidTexture.SetData(new[] { Color.White });
            
            this.font = font;
            
            // Load goat sprite
            goatSprite = AsepriteSprite.Load(graphicsDevice, "Content/Sprites/goat.aseprite");
            
            // Load rock sprite for projectiles
            rockSprite = AsepriteSprite.Load(graphicsDevice, "Content/Sprites/rock.aseprite");
            
            // Build the mountain structure with platforms
            SetupMountain();
            
            // The top platform is now much higher and the maze is more complex
            float topPlatformY = 30f; // Much higher up for more challenging level
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
        }
        
        private void SetupMountain()
        {
            platforms.Clear();
            
            // Ground at bottom
            platforms.Add(new Platform
            {
                Position = new Vector2(0, baseScreenSize.Y - 20),
                Size = new Vector2(baseScreenSize.X, 20),
                Color = new Color(100, 80, 60)
            });
            
            // COMPLEX MAZE STRUCTURE - Much larger and more challenging
            // The maze forces players to navigate left and right, with dead ends and tricky jumps
            
            // ===== LEVEL 1 - Starting platforms (Y: 410-430) =====
            platforms.Add(new Platform
            {
                Position = new Vector2(50, 430),
                Size = new Vector2(80, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(200, 420),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(350, 425),
                Size = new Vector2(75, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(500, 420),
                Size = new Vector2(80, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(660, 430),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            // ===== LEVEL 2 - Lower maze (Y: 360-380) =====
            platforms.Add(new Platform
            {
                Position = new Vector2(100, 375),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            // Dead end on left
            platforms.Add(new Platform
            {
                Position = new Vector2(10, 360),
                Size = new Vector2(60, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(250, 365),
                Size = new Vector2(80, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(410, 370),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(560, 365),
                Size = new Vector2(75, 15),
                Color = new Color(120, 100, 80)
            });
            
            // Dead end on right
            platforms.Add(new Platform
            {
                Position = new Vector2(720, 370),
                Size = new Vector2(60, 15),
                Color = new Color(120, 100, 80)
            });
            
            // ===== LEVEL 3 - Mid-lower maze (Y: 305-325) =====
            platforms.Add(new Platform
            {
                Position = new Vector2(30, 315),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(170, 310),
                Size = new Vector2(80, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(320, 305),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(470, 315),
                Size = new Vector2(75, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(620, 310),
                Size = new Vector2(80, 15),
                Color = new Color(120, 100, 80)
            });
            
            // ===== LEVEL 4 - Middle maze (Y: 250-270) =====
            platforms.Add(new Platform
            {
                Position = new Vector2(80, 265),
                Size = new Vector2(75, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(230, 255),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            // Central platform
            platforms.Add(new Platform
            {
                Position = new Vector2(380, 250),
                Size = new Vector2(85, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(530, 260),
                Size = new Vector2(75, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(680, 255),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            // ===== LEVEL 5 - Mid-upper maze (Y: 195-215) =====
            platforms.Add(new Platform
            {
                Position = new Vector2(50, 210),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(190, 200),
                Size = new Vector2(80, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(340, 195),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(480, 205),
                Size = new Vector2(75, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(630, 200),
                Size = new Vector2(80, 15),
                Color = new Color(120, 100, 80)
            });
            
            // ===== LEVEL 6 - Upper maze (Y: 140-160) =====
            platforms.Add(new Platform
            {
                Position = new Vector2(100, 155),
                Size = new Vector2(75, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(250, 145),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(390, 140),
                Size = new Vector2(80, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(540, 150),
                Size = new Vector2(75, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(690, 145),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            // ===== LEVEL 7 - High platforms (Y: 85-105) =====
            platforms.Add(new Platform
            {
                Position = new Vector2(70, 100),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(210, 90),
                Size = new Vector2(75, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(700, 90),
                Size = new Vector2(80, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(560, 95),
                Size = new Vector2(75, 15),
                Color = new Color(120, 100, 80)
            });
            
            // ===== LEVEL 8 - Near top (Y: 50-65) =====
            platforms.Add(new Platform
            {
                Position = new Vector2(140, 60),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(280, 55),
                Size = new Vector2(65, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(420, 50),
                Size = new Vector2(70, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(560, 55),
                Size = new Vector2(65, 15),
                Color = new Color(120, 100, 80)
            });
            
            // ===== TOP LEVEL - Goat platform (Y: 30) =====
            // Top platform where goat patrols - center of screen
            platforms.Add(new Platform
            {
                Position = new Vector2(baseScreenSize.X / 2f - 150f, 30),
                Size = new Vector2(300, 15),
                Color = new Color(140, 120, 100)
            });
        }
        
        public void Update(GameTime gameTime, Vector2 playerPosition, Vector2 playerSize, bool tryPickupItem)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
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
                
                // Remove rocks that go off screen
                if (rocks[i].Position.Y > baseScreenSize.Y + 50)
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
            
            return onPlatform;
        }
        
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            // Clear to sky blue
            graphicsDevice.Clear(new Color(135, 206, 235));
            
            if (solidTexture == null)
                return;
            
            spriteBatch.Begin();
            
            // Draw sky background
            Rectangle skyRect = new Rectangle(0, 0, (int)baseScreenSize.X, (int)baseScreenSize.Y);
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
        
        private void DrawMountainBackground(SpriteBatch spriteBatch)
        {
            // Draw a simple triangular mountain shape in the background
            Color mountainColor = new Color(160, 140, 120);
            
            // Draw mountain as a series of horizontal strips getting narrower toward top
            int strips = 30;
            float baseWidth = baseScreenSize.X * 0.8f;
            float startX = baseScreenSize.X * 0.1f;
            float bottomY = baseScreenSize.Y - 20;
            float topY = 50;
            
            for (int i = 0; i < strips; i++)
            {
                float t = (float)i / strips;
                float y = MathHelper.Lerp(bottomY, topY, t);
                float width = baseWidth * (1 - t);
                float x = startX + (baseWidth - width) / 2f;
                
                Rectangle strip = new Rectangle((int)x, (int)y, (int)width, (int)((bottomY - topY) / strips) + 2);
                spriteBatch.Draw(solidTexture, strip, mountainColor * (0.3f + t * 0.3f));
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
        }
        
        private class Platform
        {
            public Vector2 Position { get; set; }
            public Vector2 Size { get; set; }
            public Color Color { get; set; }
        }
        
        private class Rock
        {
            public Vector2 Position { get; set; }
            public Vector2 Size { get; set; }
            public Vector2 Velocity { get; set; }
            public float Rotation { get; set; }
            public float RotationSpeed { get; set; }
        }
    }
}
