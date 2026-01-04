using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Rendering;

namespace ProjectZeus.Core.Entities
{
    /// <summary>
    /// Moving mine cart obstacle
    /// </summary>
    public class MineCart
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public AsepriteSprite Sprite { get; set; }

        public Rectangle Bounds
        {
            get
            {
                if (Sprite != null && Sprite.IsLoaded)
                    return new Rectangle((int)(Position.X - Sprite.Size.X / 2), (int)(Position.Y - Sprite.Size.Y / 2), (int)Sprite.Size.X, (int)Sprite.Size.Y);
                return new Rectangle((int)Position.X - 20, (int)Position.Y - 15, 40, 30);
            }
        }

        public void Update(float deltaTime)
        {
            Position += new Vector2(Velocity.X * deltaTime, 0);
            
            if (Position.X < MinX || Position.X > MaxX)
            {
                Velocity = new Vector2(-Velocity.X, Velocity.Y);
                Position = new Vector2(Math.Max(MinX, Math.Min(MaxX, Position.X)), Position.Y);
            }
        }
        
        public void Draw(SpriteBatch spriteBatch, Texture2D fallbackTexture, GameTime gameTime)
        {
            if (Sprite != null && Sprite.IsLoaded)
            {
                bool isMoving = Velocity.LengthSquared() > 0;
                Vector2 drawPos = new Vector2(Position.X - Sprite.Size.X / 2, Position.Y - Sprite.Size.Y / 2);
                SpriteEffects flip = Velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Sprite.Draw(spriteBatch, drawPos, isMoving, gameTime, Color.White, 10f, flip);
            }
            else
            {
                // Fallback rendering
                spriteBatch.Draw(fallbackTexture, Bounds, new Color(139, 69, 19));
                Rectangle wheel1 = new Rectangle(Bounds.Left + 5, Bounds.Bottom - 5, 8, 8);
                Rectangle wheel2 = new Rectangle(Bounds.Right - 13, Bounds.Bottom - 5, 8, 8);
                spriteBatch.Draw(fallbackTexture, wheel1, Color.Black);
                spriteBatch.Draw(fallbackTexture, wheel2, Color.Black);
            }
        }
    }

    /// <summary>
    /// Flying bat enemy in the mine
    /// </summary>
    public class MineBat
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float ChangeDirectionTimer { get; set; }
        public AsepriteSprite Sprite { get; set; }

        public Rectangle Bounds
        {
            get
            {
                if (Sprite != null && Sprite.IsLoaded)
                    return new Rectangle((int)(Position.X - Sprite.Size.X / 2), (int)(Position.Y - Sprite.Size.Y / 2), (int)Sprite.Size.X, (int)Sprite.Size.Y);
                return new Rectangle((int)Position.X - 10, (int)Position.Y - 10, 20, 20);
            }
        }

        private const float DirectionChangeInterval = 2f;
        private const float BatSpeed = 60f;
        private const float MinX = 50f;
        private const float MaxX = 750f;
        private const float MinY = 100f;
        private const float MaxY = 380f;

        public void Update(float deltaTime, Random random)
        {
            ChangeDirectionTimer -= deltaTime;
            
            if (ChangeDirectionTimer <= 0)
            {
                Velocity = new Vector2(
                    (float)(random.NextDouble() * 2 - 1) * BatSpeed, 
                    (float)(random.NextDouble() * 2 - 1) * BatSpeed);
                ChangeDirectionTimer = DirectionChangeInterval;
            }
            
            Position += Velocity * deltaTime;

            if (Position.X < MinX) 
            { 
                Position = new Vector2(MinX, Position.Y);
                Velocity = new Vector2(Math.Abs(Velocity.X), Velocity.Y);
            }
            if (Position.X > MaxX) 
            { 
                Position = new Vector2(MaxX, Position.Y);
                Velocity = new Vector2(-Math.Abs(Velocity.X), Velocity.Y);
            }
            if (Position.Y < MinY) 
            { 
                Position = new Vector2(Position.X, MinY);
                Velocity = new Vector2(Velocity.X, Math.Abs(Velocity.Y));
            }
            if (Position.Y > MaxY) 
            { 
                Position = new Vector2(Position.X, MaxY);
                Velocity = new Vector2(Velocity.X, -Math.Abs(Velocity.Y));
            }
        }
        
        public void Draw(SpriteBatch spriteBatch, Texture2D fallbackTexture, GameTime gameTime)
        {
            if (Sprite != null && Sprite.IsLoaded)
            {
                bool isMoving = Velocity.LengthSquared() > 0;
                Vector2 drawPos = new Vector2(Position.X - Sprite.Size.X / 2, Position.Y - Sprite.Size.Y / 2);
                SpriteEffects flip = Velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Sprite.Draw(spriteBatch, drawPos, isMoving, gameTime, Color.White, 10f, flip);
            }
            else
            {
                // Fallback rendering with wing animation
                spriteBatch.Draw(fallbackTexture, Bounds, new Color(80, 60, 90));
                float wingFlap = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10f) * 5f;
                Rectangle leftWing = new Rectangle(Bounds.Left - 8 + (int)wingFlap, Bounds.Top + 5, 8, 5);
                Rectangle rightWing = new Rectangle(Bounds.Right - (int)wingFlap, Bounds.Top + 5, 8, 5);
                spriteBatch.Draw(fallbackTexture, leftWing, new Color(60, 50, 70));
                spriteBatch.Draw(fallbackTexture, rightWing, new Color(60, 50, 70));
            }
        }
    }
}
