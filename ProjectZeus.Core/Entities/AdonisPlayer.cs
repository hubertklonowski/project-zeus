using System;
using System.IO;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using ProjectZeus.Core.Constants;

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
        private SpriteEffects flip = SpriteEffects.None;
        private bool isLoaded = false;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public bool IsOnGround { get; set; }
        public Vector2 Size => GameConstants.PlayerSize;

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
                }
                
                isLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load adonis.aseprite: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                adonisTexture = CreatePlaceholderTexture(graphicsDevice);
                isLoaded = true;
            }
        }

        private Texture2D CreatePlaceholderTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 32, 48);
            Color[] data = new Color[32 * 48];
            for (int i = 0; i < data.Length; i++)
                data[i] = new Color(255, 220, 180);
            texture.SetData(data);
            return texture;
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
                var origin = new Vector2(sourceRect.Width / 2f, sourceRect.Height);
                var destPos = Position + new Vector2(Size.X / 2f, Size.Y);

                flip = SpriteEffects.None;
                if (Velocity.X < 0)
                    flip = SpriteEffects.FlipHorizontally;

                spriteBatch.Draw(sprite.TextureRegion.Texture, destPos, sourceRect, Color.White, 0f, origin, 1f, flip, 0f);
            }
            else
            {
                // Fallback
                var sourceRect = new Rectangle(0, 0, 32, 48);
                var origin = new Vector2(16f, 48f);
                var destPos = Position + new Vector2(Size.X / 2f, Size.Y);

                flip = SpriteEffects.None;
                if (Velocity.X < 0)
                    flip = SpriteEffects.FlipHorizontally;

                spriteBatch.Draw(adonisTexture, destPos, sourceRect, Color.White, 0f, origin, 1f, flip, 0f);
            }
        }
    }
}
