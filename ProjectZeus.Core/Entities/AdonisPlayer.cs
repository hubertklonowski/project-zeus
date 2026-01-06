using System;
using System.IO;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ProjectZeus.Core.Entities
{
    /// <summary>
    /// Animated player character using Adonis Aseprite sprite
    /// Frame 0: Standing idle
    /// Frames 1-7: Walking animation
    /// </summary>
    public class AdonisPlayer
    {
        private Texture2D adonisTexture;
        private AsepriteFile adonisFile;
        private int frameCount = 8;
        private int spriteWidth;
        private int spriteHeight;
        private SpriteEffects flip = SpriteEffects.None;
        private bool isLoaded = false;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public bool IsOnGround { get; set; }
        
        /// <summary>
        /// Scale factor for drawing the player. Default is 1.0f.
        /// </summary>
        public float Scale { get; set; } = 1.0f;
        
        // Use actual sprite dimensions instead of hardcoded values
        public Vector2 Size => new Vector2(spriteWidth * Scale, spriteHeight * Scale);

        public Rectangle Bounds => new Rectangle(
            (int)Position.X, 
            (int)Position.Y, 
            (int)Size.X, 
            (int)Size.Y);

        public AdonisPlayer()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            IsOnGround = false;
            spriteWidth = 32;  // Default fallback
            spriteHeight = 48; // Default fallback
        }

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            try
            {
                // Load the aseprite file
                using (var stream = TitleContainer.OpenStream("Content/Sprites/adonis.aseprite"))
                {
                    adonisFile = AsepriteFileLoader.FromStream("adonis", stream);
                    var sprite = adonisFile.CreateSprite(graphicsDevice, frameIndex: 0, onlyVisibleLayers: true, includeBackgroundLayer: false, includeTilemapLayers: false);
                    adonisTexture = sprite.TextureRegion.Texture;
                    frameCount = adonisFile.Frames.Length;
                    
                    // Get actual sprite dimensions from the aseprite file
                    spriteWidth = sprite.TextureRegion.Bounds.Width;
                    spriteHeight = sprite.TextureRegion.Bounds.Height;
                }
                
                isLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load adonis.aseprite: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                isLoaded = false;
            }
        }

        public void Update(GameTime gameTime)
        {
            // Animation state is handled in Draw method
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!isLoaded || adonisTexture == null)
                return;

            // Calculate frame index based on movement
            int frameIndex = 0;
            if (Math.Abs(Velocity.X) > 10f)
            {
                const float animationSpeed = 8f;
                float totalWalkingTime = (float)gameTime.TotalGameTime.TotalSeconds * animationSpeed;
                frameIndex = 1 + (int)(totalWalkingTime % 7);
            }

            frameIndex = Math.Clamp(frameIndex, 0, frameCount - 1);

            if (adonisFile != null && frameIndex < frameCount)
            {
                // Create sprite for the current frame
                var sprite = adonisFile.CreateSprite(spriteBatch.GraphicsDevice, frameIndex, onlyVisibleLayers: true, includeBackgroundLayer: false, includeTilemapLayers: false);
                var sourceRect = sprite.TextureRegion.Bounds;
                
                // Round position to avoid sub-pixel jittering
                // Create destination rectangle with integer positions to avoid pixel artifacts
                Rectangle destRect = new Rectangle(
                    (int)Math.Floor(Position.X),
                    (int)Math.Floor(Position.Y),
                    (int)Math.Ceiling(Size.X),
                    (int)Math.Ceiling(Size.Y));

                flip = SpriteEffects.None;
                if (Velocity.X < 0)
                    flip = SpriteEffects.FlipHorizontally;

                spriteBatch.Draw(sprite.TextureRegion.Texture, destRect, sourceRect, Color.White, 0f, Vector2.Zero, flip, 0f);
            }
            else if (adonisTexture != null)
            {
                // Fallback - use same integer-based rendering
                var sourceRect = new Rectangle(0, 0, spriteWidth, spriteHeight);
                
                Rectangle destRect = new Rectangle(
                    (int)Math.Floor(Position.X),
                    (int)Math.Floor(Position.Y),
                    (int)Math.Ceiling(Size.X),
                    (int)Math.Ceiling(Size.Y));

                flip = SpriteEffects.None;
                if (Velocity.X < 0)
                    flip = SpriteEffects.FlipHorizontally;

                spriteBatch.Draw(adonisTexture, destRect, sourceRect, Color.White, 0f, Vector2.Zero, flip, 0f);
            }
        }
    }
}
