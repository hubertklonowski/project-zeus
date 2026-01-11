using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectZeus.Core.Entities;

namespace ProjectZeus.Core.Levels.Mine
{
    /// <summary>
    /// Updates mine level entities (carts, bats, guano, etc.)
    /// </summary>
    public class MineEntityUpdater
    {
        private readonly float worldWidth;
        private readonly float screenHeight;
        private readonly float groundHeight;
        private readonly Random random;

        public MineEntityUpdater(float worldWidth, float screenHeight, float groundHeight, Random random)
        {
            this.worldWidth = worldWidth;
            this.screenHeight = screenHeight;
            this.groundHeight = groundHeight;
            this.random = random;
        }

        public void UpdateCarts(List<MineCart> carts, float deltaTime)
        {
            for (int i = carts.Count - 1; i >= 0; i--)
            {
                var cart = carts[i];
                cart.Position += new Vector2(cart.Velocity.X * deltaTime, 0);
                
                if (cart.Position.X < -100)
                {
                    carts.RemoveAt(i);
                }
            }
        }

        public void UpdateBats(List<MineBat> bats, float deltaTime)
        {
            float groundTop = screenHeight - groundHeight;
            float minBatY = groundTop - 200f;
            float maxBatY = groundTop - 60f;
            
            foreach (var bat in bats)
            {
                bat.ChangeDirectionTimer -= deltaTime;
                
                if (bat.ChangeDirectionTimer <= 0)
                {
                    bat.Velocity = new Vector2(
                        (float)(random.NextDouble() * 2 - 1) * 60f, 
                        (float)(random.NextDouble() * 2 - 1) * 40f);
                    bat.ChangeDirectionTimer = 1.5f + (float)random.NextDouble();
                }
                
                bat.Position += bat.Velocity * deltaTime;
                
                if (bat.Position.Y < minBatY)
                {
                    bat.Position = new Vector2(bat.Position.X, minBatY);
                    bat.Velocity = new Vector2(bat.Velocity.X, Math.Abs(bat.Velocity.Y));
                }
                if (bat.Position.Y > maxBatY)
                {
                    bat.Position = new Vector2(bat.Position.X, maxBatY);
                    bat.Velocity = new Vector2(bat.Velocity.X, -Math.Abs(bat.Velocity.Y));
                }
            }
        }

        public void UpdateGigaBat(GigaBat gigaBat, List<Guano> guanos, float deltaTime)
        {
            if (gigaBat == null) return;
            
            gigaBat.Update(deltaTime, random, 100f, worldWidth - 100f, 100f, 250f);
            
            if (gigaBat.ShouldShoot())
            {
                guanos.Add(new Guano
                {
                    Position = gigaBat.Position,
                    Velocity = new Vector2(0, 200f)
                });
            }
        }

        public void UpdateGuano(List<Guano> guanos, float deltaTime)
        {
            for (int i = guanos.Count - 1; i >= 0; i--)
            {
                var guano = guanos[i];
                guano.Position += guano.Velocity * deltaTime;
                
                if (guano.Position.Y > screenHeight)
                {
                    guanos.RemoveAt(i);
                }
            }
        }
    }
}
