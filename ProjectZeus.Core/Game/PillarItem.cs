#region File Description
//-----------------------------------------------------------------------------
// PillarItem.cs
//
// A collectible item that can be placed in a pillar
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Platformer2D
{
    /// <summary>
    /// A special item that can be collected and placed in a pillar.
    /// </summary>
    class PillarItem
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;

        public readonly Color ItemColor;

        // The item is positioned at a base location
        private Vector2 basePosition;
        private float hover;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsCollected
        {
            get { return isCollected; }
        }
        private bool isCollected;

        /// <summary>
        /// Gets the current position of this item in world space.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return basePosition + new Vector2(0.0f, hover);
            }
        }

        /// <summary>
        /// Gets a circle which bounds this item in world space.
        /// </summary>
        public Circle BoundingCircle
        {
            get
            {
                return new Circle(Position, Tile.Width / 2.5f);
            }
        }

        /// <summary>
        /// Constructs a new pillar item.
        /// </summary>
        public PillarItem(Level level, Vector2 position, Color itemColor)
        {
            this.level = level;
            this.basePosition = position;
            this.ItemColor = itemColor;
            this.isCollected = false;

            LoadContent();
        }

        /// <summary>
        /// Loads the item texture and collected sound.
        /// </summary>
        public void LoadContent()
        {
            // Create a simple placeholder texture
            texture = new Texture2D(Level.Content.ServiceProvider as IGraphicsDeviceService != null 
                ? ((IGraphicsDeviceService)Level.Content.ServiceProvider).GraphicsDevice 
                : null, 1, 1);
            
            if (texture.GraphicsDevice != null)
            {
                texture.SetData(new[] { Color.White });
            }

            origin = new Vector2(Tile.Width / 2.0f, Tile.Height / 2.0f);
            
            // Use gem collected sound as placeholder
            try
            {
                collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
            }
            catch
            {
                // If sound doesn't exist, continue without it
            }
        }

        /// <summary>
        /// Hovers up and down to attract players to collect it.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Hover control constants
            const float HoverHeight = 8.0f;
            const float HoverRate = 2.5f;

            // Hover along a sine curve over time.
            double t = gameTime.TotalGameTime.TotalSeconds * HoverRate;
            hover = (float)Math.Sin(t) * HoverHeight;
        }

        /// <summary>
        /// Called when this item has been collected by a player.
        /// </summary>
        public void OnCollected(Player collectedBy)
        {
            isCollected = true;
            if (collectedSound != null)
            {
                collectedSound.Play();
            }
        }

        /// <summary>
        /// Draws the pillar item.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!isCollected && texture != null)
            {
                // Draw as a colored square
                int size = Tile.Width / 2;
                Rectangle itemRect = new Rectangle(
                    (int)(Position.X - size / 2),
                    (int)(Position.Y - size / 2),
                    size,
                    size);
                
                spriteBatch.Draw(texture, itemRect, ItemColor);
                
                // Draw a border
                int borderThickness = 2;
                // Top border
                spriteBatch.Draw(texture, new Rectangle(itemRect.X, itemRect.Y, itemRect.Width, borderThickness), Color.White);
                // Bottom border
                spriteBatch.Draw(texture, new Rectangle(itemRect.X, itemRect.Bottom - borderThickness, itemRect.Width, borderThickness), Color.White);
                // Left border
                spriteBatch.Draw(texture, new Rectangle(itemRect.X, itemRect.Y, borderThickness, itemRect.Height), Color.White);
                // Right border
                spriteBatch.Draw(texture, new Rectangle(itemRect.Right - borderThickness, itemRect.Y, borderThickness, itemRect.Height), Color.White);
            }
        }
    }
}
