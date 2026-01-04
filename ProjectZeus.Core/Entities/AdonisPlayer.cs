using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Constants;

namespace ProjectZeus.Core.Entities
{
    /// <summary>
    /// Animated player character using Adonis Aseprite sprite
    /// Frame 0: Standing idle
    /// Frames 1-7: Walking animation
    /// 
    /// NOTE: This class requires 'Content/Sprites/adonis.aseprite' file to be present.
    /// The file should contain 8 frames (0=idle, 1-7=walking animation).
    /// </summary>
    public class AdonisPlayer
    {
        private Texture2D adonisTexture;
        private int frameCount = 8;
        private int frameWidth;
        private int frameHeight;
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
                // Try to load from Aseprite file when MonoGame.Aseprite API is available
                // For now, using placeholder logic
                // TODO: Implement proper Aseprite loading when file is available
                //using (var stream = TitleContainer.OpenStream("Content/Sprites/adonis.aseprite"))
                //{
                //    adonisFile = AsepriteFileLoader.FromStream("adonis", stream, preMultiplyAlpha: true);
                //    adonisTexture = adonisFile.CreateTexture(graphicsDevice);
                //}
                
                // Create a colored rectangle as placeholder
                adonisTexture = CreatePlaceholderTexture(graphicsDevice);
                frameWidth = 32;
                frameHeight = 48;
                isLoaded = true;
            }
            catch (Exception ex)
            {
                // If file doesn't exist, create a simple placeholder
                Console.WriteLine($"Could not load adonis.aseprite: {ex.Message}");
                adonisTexture = CreatePlaceholderTexture(graphicsDevice);
                frameWidth = 32;
                frameHeight = 48;
                isLoaded = true;
            }
        }

        private Texture2D CreatePlaceholderTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 32, 48);
            Color[] data = new Color[32 * 48];
            for (int i = 0; i < data.Length; i++)
                data[i] = new Color(255, 220, 180); // Skin tone
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

            // For placeholder, just draw a simple rectangle
            // When proper Aseprite file is loaded, this will show animated frames
            int frameIndex = 0;

            // Animate when moving
            if (Math.Abs(Velocity.X) > 10f)
            {
                const float animationSpeed = 8f;
                float totalWalkingTime = (float)gameTime.TotalGameTime.TotalSeconds * animationSpeed;
                frameIndex = 1 + (int)(totalWalkingTime % 7);
            }

            frameIndex = Math.Clamp(frameIndex, 0, frameCount - 1);

            // Placeholder: just draw the texture
            // When Aseprite is loaded, this will select the correct frame
            var sourceRect = new Rectangle(0, 0, frameWidth, frameHeight);
            var origin = new Vector2(frameWidth / 2f, frameHeight);
            var destPos = Position + new Vector2(Size.X / 2f, Size.Y);

            // Flip sprite based on movement direction
            flip = SpriteEffects.None;
            if (Velocity.X < 0)
                flip = SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(adonisTexture, destPos, sourceRect, Color.White, 0f, origin, 1f, flip, 0f);
        }
    }
}
