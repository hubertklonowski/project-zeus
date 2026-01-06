using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Rendering;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Represents a stalactite hazard in the mine level
    /// </summary>
    public class Stalactite
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public AsepriteSprite Sprite { get; set; }
        
        public void Draw(SpriteBatch spriteBatch, Texture2D fallbackTexture, GameTime gameTime)
        {
            if (Sprite != null && Sprite.IsLoaded)
            {
                // Draw using the aseprite sprite
                Sprite.Draw(spriteBatch, Position, isMoving: false, gameTime, Color.White);
            }
            else
            {
                // Fallback to simple rectangle
                Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
                spriteBatch.Draw(fallbackTexture, rect, new Color(120, 120, 120));
            }
        }
    }
}
