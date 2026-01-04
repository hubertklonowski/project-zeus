using System;
using Microsoft.Xna.Framework;

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

        public Rectangle Bounds => new Rectangle((int)Position.X - 20, (int)Position.Y - 15, 40, 30);

        public void Update(float deltaTime)
        {
            Position += new Vector2(Velocity.X * deltaTime, 0);
            
            if (Position.X < MinX || Position.X > MaxX)
            {
                Velocity = new Vector2(-Velocity.X, Velocity.Y);
                Position = new Vector2(Math.Max(MinX, Math.Min(MaxX, Position.X)), Position.Y);
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

        public Rectangle Bounds => new Rectangle((int)Position.X - 10, (int)Position.Y - 10, 20, 20);

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
    }
}
