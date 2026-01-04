using System;
using System.IO;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ProjectZeus.Core.Rendering
{
    /// <summary>
    /// Helper class for loading and rendering Aseprite sprites with animation support.
    /// Frame 0 is always the stationary/idle position.
    /// Frames 1-N are used for movement animation (if they exist).
    /// </summary>
    public class AsepriteSprite
    {
        private AsepriteFile asepriteFile;
        private GraphicsDevice graphicsDevice;
        private Texture2D[] frameTextures;
        
        public int FrameCount { get; private set; }
        public Vector2 Size { get; private set; }
        public bool IsLoaded { get; private set; }
        
        /// <summary>
        /// Load an Aseprite file from the content directory
        /// </summary>
        public static AsepriteSprite Load(GraphicsDevice graphicsDevice, string filePath)
        {
            var sprite = new AsepriteSprite();
            sprite.graphicsDevice = graphicsDevice;
            
            try
            {
                using (var stream = TitleContainer.OpenStream(filePath))
                {
                    string name = Path.GetFileNameWithoutExtension(filePath);
                    sprite.asepriteFile = AsepriteFileLoader.FromStream(name, stream);
                    sprite.FrameCount = sprite.asepriteFile.Frames.Length;
                    
                    // Pre-load all frame textures
                    sprite.frameTextures = new Texture2D[sprite.FrameCount];
                    for (int i = 0; i < sprite.FrameCount; i++)
                    {
                        var frameSprite = sprite.asepriteFile.CreateSprite(graphicsDevice, i, 
                            onlyVisibleLayers: true, includeBackgroundLayer: false, includeTilemapLayers: false);
                        sprite.frameTextures[i] = frameSprite.TextureRegion.Texture;
                        
                        // Get size from first frame
                        if (i == 0)
                        {
                            sprite.Size = new Vector2(
                                frameSprite.TextureRegion.Bounds.Width, 
                                frameSprite.TextureRegion.Bounds.Height);
                        }
                    }
                    
                    sprite.IsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load {filePath}: {ex.Message}");
                sprite.IsLoaded = false;
            }
            
            return sprite;
        }
        
        /// <summary>
        /// Create a simple fallback texture when Aseprite loading fails
        /// </summary>
        public static Texture2D CreateFallbackTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            var texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++)
                data[i] = color;
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Get the appropriate frame texture based on whether the object is moving.
        /// Frame 0: Stationary/idle position
        /// Frames 1-N: Movement animation
        /// </summary>
        public Texture2D GetFrameTexture(bool isMoving, GameTime gameTime, float animationSpeed = 10f)
        {
            if (!IsLoaded || frameTextures == null || frameTextures.Length == 0)
                return null;
            
            // If not moving or only one frame exists, return frame 0 (idle)
            if (!isMoving || FrameCount == 1)
                return frameTextures[0];
            
            // Animate through frames 1 to N when moving
            int movementFrameCount = FrameCount - 1;
            int animationFrame = 1 + (int)(gameTime.TotalGameTime.TotalSeconds * animationSpeed) % movementFrameCount;
            
            return frameTextures[animationFrame];
        }
        
        /// <summary>
        /// Get a specific frame texture by index
        /// </summary>
        public Texture2D GetFrameTexture(int frameIndex)
        {
            if (!IsLoaded || frameTextures == null || frameIndex < 0 || frameIndex >= frameTextures.Length)
                return frameTextures?[0];
            
            return frameTextures[frameIndex];
        }
        
        /// <summary>
        /// Draw the sprite at the specified position with automatic frame selection based on movement
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, bool isMoving, GameTime gameTime, 
            Color? tint = null, float animationSpeed = 10f, SpriteEffects flip = SpriteEffects.None)
        {
            if (!IsLoaded)
                return;
            
            var texture = GetFrameTexture(isMoving, gameTime, animationSpeed);
            if (texture == null)
                return;
            
            Rectangle destRect = new Rectangle((int)position.X, (int)position.Y, (int)Size.X, (int)Size.Y);
            spriteBatch.Draw(texture, destRect, null, tint ?? Color.White, 0f, Vector2.Zero, flip, 0f);
        }
        
        /// <summary>
        /// Draw the sprite at the specified rectangle
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Rectangle destRect, bool isMoving, GameTime gameTime,
            Color? tint = null, float animationSpeed = 10f, SpriteEffects flip = SpriteEffects.None)
        {
            if (!IsLoaded)
                return;
            
            var texture = GetFrameTexture(isMoving, gameTime, animationSpeed);
            if (texture == null)
                return;
            
            spriteBatch.Draw(texture, destRect, null, tint ?? Color.White, 0f, Vector2.Zero, flip, 0f);
        }
    }
}
