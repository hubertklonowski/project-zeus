#region File Description
//-----------------------------------------------------------------------------
// Bat.cs
//
// Flying bat enemy in the mine level
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D
{
    /// <summary>
    /// A bat that flies around the cave.
    /// </summary>
    class Bat
    {
        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Position in world space of the center of this bat.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this bat in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - localBounds.Width / 2) + localBounds.X;
                int top = (int)Math.Round(Position.Y - localBounds.Height / 2) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Bat movement
        private Vector2 velocity;
        private Vector2 startPosition;
        private const float MoveSpeed = 48.0f;
        private float changeDirectionTime;
        private const float MaxChangeDirectionTime = 2.0f;
        private Random random;

        // Placeholder graphics
        private Texture2D texture;
        private Color color = new Color(80, 60, 90); // Dark purple color for bat
        private float wingFlap;
        private const float WingFlapSpeed = 8.0f;

        /// <summary>
        /// Constructs a new Bat.
        /// </summary>
        public Bat(Level level, Vector2 position)
        {
            this.level = level;
            this.position = position;
            this.startPosition = position;
            this.random = new Random((int)position.X + (int)position.Y);

            LoadContent();
        }

        /// <summary>
        /// Loads placeholder graphics for the bat.
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

            // Set bounds for a small bat
            int width = (int)(Tile.Width * 0.5f);
            int height = (int)(Tile.Height * 0.4f);
            localBounds = new Rectangle(-width / 2, -height / 2, width, height);

            // Initialize random movement direction
            velocity = new Vector2(
                (float)(random.NextDouble() * 2 - 1) * MoveSpeed,
                (float)(random.NextDouble() * 2 - 1) * MoveSpeed);
            changeDirectionTime = (float)random.NextDouble() * MaxChangeDirectionTime;
        }

        /// <summary>
        /// Updates the bat, making it fly around randomly.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update wing flap animation
            wingFlap += elapsed * WingFlapSpeed;

            // Change direction occasionally
            changeDirectionTime -= elapsed;
            if (changeDirectionTime <= 0)
            {
                // Pick a new random direction
                velocity = new Vector2(
                    (float)(random.NextDouble() * 2 - 1) * MoveSpeed,
                    (float)(random.NextDouble() * 2 - 1) * MoveSpeed);
                changeDirectionTime = (float)random.NextDouble() * MaxChangeDirectionTime + 0.5f;
            }

            // Move the bat
            Vector2 newPosition = position + velocity * elapsed;

            // Keep bat within level bounds and away from ground
            int minY = Tile.Height * 2; // Stay at least 2 tiles from top
            int maxY = (level.Height - 3) * Tile.Height; // Stay at least 3 tiles from bottom
            int minX = Tile.Width;
            int maxX = (level.Width - 1) * Tile.Width;

            // Bounce off boundaries
            if (newPosition.X < minX || newPosition.X > maxX)
            {
                velocity.X = -velocity.X;
                newPosition.X = Math.Max(minX, Math.Min(maxX, newPosition.X));
            }
            if (newPosition.Y < minY || newPosition.Y > maxY)
            {
                velocity.Y = -velocity.Y;
                newPosition.Y = Math.Max(minY, Math.Min(maxY, newPosition.Y));
            }

            position = newPosition;
        }

        /// <summary>
        /// Draws the bat.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (texture != null)
            {
                // Draw body
                spriteBatch.Draw(texture, BoundingRectangle, color);
                
                // Draw simple wings that flap
                float wingOffset = (float)Math.Sin(wingFlap) * 4;
                int wingWidth = 6;
                int wingHeight = 4;
                
                Rectangle leftWing = new Rectangle(
                    BoundingRectangle.Left - wingWidth + (int)wingOffset,
                    BoundingRectangle.Top + BoundingRectangle.Height / 4,
                    wingWidth,
                    wingHeight);
                Rectangle rightWing = new Rectangle(
                    BoundingRectangle.Right - (int)wingOffset,
                    BoundingRectangle.Top + BoundingRectangle.Height / 4,
                    wingWidth,
                    wingHeight);
                
                spriteBatch.Draw(texture, leftWing, new Color(60, 50, 70));
                spriteBatch.Draw(texture, rightWing, new Color(60, 50, 70));
            }
        }
    }
}
