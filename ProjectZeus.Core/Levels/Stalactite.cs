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
        
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (Sprite != null && Sprite.IsLoaded)
            {
                Sprite.Draw(spriteBatch, Position, isMoving: false, gameTime, Color.White);
            }
        }
    }
}
