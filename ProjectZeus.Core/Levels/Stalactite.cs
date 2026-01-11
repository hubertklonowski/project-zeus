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
        
        public Rectangle Bounds
        {
            get
            {
                if (Sprite != null && Sprite.IsLoaded)
                    return new Rectangle((int)Position.X, (int)Position.Y, (int)Sprite.Size.X, (int)Sprite.Size.Y);
                return new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            }
        }
        
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (Sprite != null && Sprite.IsLoaded)
            {
                Sprite.Draw(spriteBatch, Position, isMoving: false, gameTime, Color.White);
            }
        }
    }
}
