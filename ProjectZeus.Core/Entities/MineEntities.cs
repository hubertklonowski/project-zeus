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
                // Reduce hitbox size by 40% for better gameplay
                if (Sprite != null && Sprite.IsLoaded)
                {
                    float reducedWidth = Sprite.Size.X * 0.6f;
                    float reducedHeight = Sprite.Size.Y * 0.6f;
                    return new Rectangle((int)(Position.X - reducedWidth / 2), (int)(Position.Y - reducedHeight / 2), (int)reducedWidth, (int)reducedHeight);
                }
                return new Rectangle((int)Position.X - 6, (int)Position.Y - 6, 12, 12);
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

    /// <summary>
    /// Guano projectile shot by GigaBat
    /// </summary>
    public class Guano
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        public Rectangle Bounds => new Rectangle((int)Position.X - 4, (int)Position.Y - 4, 8, 8);

        public void Draw(SpriteBatch spriteBatch, Texture2D fallbackTexture)
        {
            spriteBatch.Draw(fallbackTexture, Bounds, new Color(139, 119, 101));
        }
    }

    /// <summary>
    /// Large bat boss that shoots guano projectiles downward
    /// </summary>
    public class GigaBat
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float ChangeDirectionTimer { get; set; }
        public float ShootTimer { get; set; }
        public AsepriteSprite Sprite { get; set; }

        public Rectangle Bounds
        {
            get
            {
                // Larger size than regular bats
                if (Sprite != null && Sprite.IsLoaded)
                {
                    float scale = 2.0f; // Twice as large
                    float width = Sprite.Size.X * scale;
                    float height = Sprite.Size.Y * scale;
                    return new Rectangle((int)(Position.X - width / 2), (int)(Position.Y - height / 2), (int)width, (int)height);
                }
                return new Rectangle((int)Position.X - 30, (int)Position.Y - 30, 60, 60);
            }
        }

        private const float DirectionChangeInterval = 3f;
        private const float BatSpeed = 40f;
        private const float ShootInterval = 2f;
        
        public void Update(float deltaTime, Random random, float minX, float maxX, float minY, float maxY)
        {
            ChangeDirectionTimer -= deltaTime;
            ShootTimer -= deltaTime;
            
            if (ChangeDirectionTimer <= 0)
            {
                Velocity = new Vector2(
                    (float)(random.NextDouble() * 2 - 1) * BatSpeed, 
                    (float)(random.NextDouble() * 2 - 1) * BatSpeed * 0.5f); // Less vertical movement
                ChangeDirectionTimer = DirectionChangeInterval;
            }
            
            Position += Velocity * deltaTime;

            // Keep within bounds
            if (Position.X < minX) 
            { 
                Position = new Vector2(minX, Position.Y);
                Velocity = new Vector2(Math.Abs(Velocity.X), Velocity.Y);
            }
            if (Position.X > maxX) 
            { 
                Position = new Vector2(maxX, Position.Y);
                Velocity = new Vector2(-Math.Abs(Velocity.X), Velocity.Y);
            }
            if (Position.Y < minY) 
            { 
                Position = new Vector2(Position.X, minY);
                Velocity = new Vector2(Velocity.X, Math.Abs(Velocity.Y));
            }
            if (Position.Y > maxY) 
            { 
                Position = new Vector2(Position.X, maxY);
                Velocity = new Vector2(Velocity.X, -Math.Abs(Velocity.Y));
            }
        }

        public bool ShouldShoot()
        {
            if (ShootTimer <= 0)
            {
                ShootTimer = ShootInterval;
                return true;
            }
            return false;
        }
        
        public void Draw(SpriteBatch spriteBatch, Texture2D fallbackTexture, GameTime gameTime)
        {
            if (Sprite != null && Sprite.IsLoaded)
            {
                bool isMoving = Velocity.LengthSquared() > 0;
                Vector2 drawPos = new Vector2(Position.X - Sprite.Size.X, Position.Y - Sprite.Size.Y);
                SpriteEffects flip = Velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                // Draw at 2x scale
                var texture = Sprite.GetFrameTexture(isMoving, gameTime, 10f);
                if (texture != null)
                {
                    Rectangle destRect = new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)(Sprite.Size.X * 2), (int)(Sprite.Size.Y * 2));
                    spriteBatch.Draw(texture, destRect, null, Color.White, 0f, Vector2.Zero, flip, 0f);
                }
            }
            else
            {
                // Fallback rendering - larger bat
                spriteBatch.Draw(fallbackTexture, Bounds, new Color(60, 40, 70));
                float wingFlap = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 8f) * 10f;
                Rectangle leftWing = new Rectangle(Bounds.Left - 15 + (int)wingFlap, Bounds.Top + 10, 15, 10);
                Rectangle rightWing = new Rectangle(Bounds.Right - (int)wingFlap, Bounds.Top + 10, 15, 10);
                spriteBatch.Draw(fallbackTexture, leftWing, new Color(40, 20, 50));
                spriteBatch.Draw(fallbackTexture, rightWing, new Color(40, 20, 50));
            }
        }
    }
}
