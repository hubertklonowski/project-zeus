#region File Description
//-----------------------------------------------------------------------------
// Cart.cs
//
// Cart that moves on rails in the mine level
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D
{
    /// <summary>
    /// A cart that moves back and forth on rails.
    /// </summary>
    class Cart
    {
        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Position in world space of the bottom center of this cart.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this cart in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - localBounds.Width / 2) + localBounds.X;
                int top = (int)Math.Round(Position.Y - localBounds.Height) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Cart movement
        private Vector2 velocity;
        private const float MoveSpeed = 64.0f;
        private float waitTime;
        private const float MaxWaitTime = 0.5f;
        private FaceDirection direction = FaceDirection.Left;

        enum FaceDirection
        {
            Left = -1,
            Right = 1,
        }

        // Placeholder graphics
        private Texture2D texture;
        private Color color = new Color(139, 69, 19); // Brown color for cart

        /// <summary>
        /// Constructs a new Cart.
        /// </summary>
        public Cart(Level level, Vector2 position)
        {
            this.level = level;
            this.position = position;

            LoadContent();
        }

        /// <summary>
        /// Loads placeholder graphics for the cart.
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

            // Set bounds for a cart (slightly larger than a tile)
            int width = (int)(Tile.Width * 1.2f);
            int height = (int)(Tile.Height * 0.8f);
            localBounds = new Rectangle(-width / 2, -height, width, height);
        }

        /// <summary>
        /// Updates the cart, moving it along the rails.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position
            int posX = (int)Math.Floor(position.X / Tile.Width);
            int posY = (int)Math.Floor(position.Y / Tile.Height);

            // If cart is waiting on edge, wait a bit before turning around
            if (waitTime > 0)
            {
                waitTime = Math.Max(0.0f, waitTime - elapsed);
                if (waitTime <= 0.0f)
                {
                    // Turn around
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                // Move in current direction
                velocity.X = (int)direction * MoveSpeed;
                position.X += velocity.X * elapsed;

                // Check for platform edge or wall
                int nextPosX = (int)Math.Floor((position.X + (int)direction * Tile.Width / 2) / Tile.Width);
                int groundY = posY + 1;

                // If we're approaching an edge or wall, stop and wait
                if (nextPosX >= 0 && nextPosX < level.Width && groundY >= 0 && groundY < level.Height)
                {
                    TileCollision nextGround = level.GetCollision(nextPosX, groundY);
                    TileCollision nextAhead = level.GetCollision(nextPosX, posY);

                    // Turn around if there's no ground ahead or there's a wall
                    if (nextGround == TileCollision.Passable || nextAhead == TileCollision.Impassable)
                    {
                        waitTime = MaxWaitTime;
                    }
                }
                else
                {
                    // At level boundary, turn around
                    waitTime = MaxWaitTime;
                }
            }
        }

        /// <summary>
        /// Draws the cart.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (texture != null)
            {
                spriteBatch.Draw(texture, BoundingRectangle, color);
                
                // Draw wheels (simple circles)
                int wheelRadius = 4;
                int wheelOffset = localBounds.Width / 3;
                Rectangle leftWheel = new Rectangle(
                    BoundingRectangle.Left + wheelOffset - wheelRadius,
                    BoundingRectangle.Bottom - wheelRadius,
                    wheelRadius * 2,
                    wheelRadius * 2);
                Rectangle rightWheel = new Rectangle(
                    BoundingRectangle.Right - wheelOffset - wheelRadius,
                    BoundingRectangle.Bottom - wheelRadius,
                    wheelRadius * 2,
                    wheelRadius * 2);
                
                spriteBatch.Draw(texture, leftWheel, Color.Black);
                spriteBatch.Draw(texture, rightWheel, Color.Black);
            }
        }
    }
}
