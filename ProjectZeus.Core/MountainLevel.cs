using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ProjectZeus.Core
{
    /// <summary>
    /// Mountain climbing level where player must reach the top to collect an item.
    /// A goat at the top throws rocks that can kill the player.
    /// </summary>
    public class MountainLevel
    {
        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        
        // Textures (simple placeholder colored rectangles)
        private Texture2D solidTexture;
        private SpriteFont font;
        
        // Mountain platforms
        private List<Platform> platforms;
        
        // Goat enemy
        private Vector2 goatPosition;
        private readonly Vector2 goatSize = new Vector2(40, 40);
        private float goatThrowTimer;
        private const float GoatThrowInterval = 2.5f;
        
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
            
            // Build the mountain structure with platforms
            SetupMountain();
            
            // Position goat at top of mountain
            goatPosition = new Vector2(baseScreenSize.X / 2f - goatSize.X / 2f, 60f);
            goatThrowTimer = GoatThrowInterval;
            
            // Position item next to goat at mountain top
            itemPosition = new Vector2(goatPosition.X + 60f, goatPosition.Y + 5f);
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
            
            // Mountain platforms going up - creating a climbing path
            // Left side platforms
            platforms.Add(new Platform
            {
                Position = new Vector2(50, 390),
                Size = new Vector2(120, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(180, 330),
                Size = new Vector2(100, 15),
                Color = new Color(120, 100, 80)
            });
            
            // Right side platforms
            platforms.Add(new Platform
            {
                Position = new Vector2(500, 380),
                Size = new Vector2(130, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(540, 310),
                Size = new Vector2(110, 15),
                Color = new Color(120, 100, 80)
            });
            
            // Middle platforms
            platforms.Add(new Platform
            {
                Position = new Vector2(300, 260),
                Size = new Vector2(140, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(150, 200),
                Size = new Vector2(120, 15),
                Color = new Color(120, 100, 80)
            });
            
            platforms.Add(new Platform
            {
                Position = new Vector2(470, 190),
                Size = new Vector2(120, 15),
                Color = new Color(120, 100, 80)
            });
            
            // Near top platforms
            platforms.Add(new Platform
            {
                Position = new Vector2(320, 140),
                Size = new Vector2(100, 15),
                Color = new Color(120, 100, 80)
            });
            
            // Top platform (where goat and item are)
            platforms.Add(new Platform
            {
                Position = new Vector2(baseScreenSize.X / 2f - 80f, 100),
                Size = new Vector2(160, 15),
                Color = new Color(140, 120, 100)
            });
        }
        
        public void Update(GameTime gameTime, Vector2 playerPosition, Vector2 playerSize, bool tryPickupItem)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
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
            // Goat throws rock downward with some randomness
            float angle = MathHelper.ToRadians(90f + (float)(new System.Random().NextDouble() * 40 - 20)); // 70-110 degrees
            float speed = 200f;
            
            rocks.Add(new Rock
            {
                Position = new Vector2(goatPosition.X + goatSize.X / 2f - 10f, goatPosition.Y + goatSize.Y),
                Size = new Vector2(20, 20),
                Velocity = new Vector2((float)System.Math.Cos(angle) * speed, (float)System.Math.Sin(angle) * speed),
                Color = new Color(80, 70, 60)
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
                Rectangle topRect = new Rectangle(platformRect.X, platformRect.Y - 2, platformRect.Width, 6);
                
                if (playerRect.Bottom > topRect.Top &&
                    playerRect.Bottom <= topRect.Top + 20 &&
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
        
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
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
            
            // Draw goat (simple rectangle placeholder)
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
            
            // Draw rocks
            foreach (var rock in rocks)
            {
                Rectangle rockRect = new Rectangle(
                    (int)rock.Position.X,
                    (int)rock.Position.Y,
                    (int)rock.Size.X,
                    (int)rock.Size.Y);
                spriteBatch.Draw(solidTexture, rockRect, rock.Color);
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
            public Color Color { get; set; }
        }
    }
}
