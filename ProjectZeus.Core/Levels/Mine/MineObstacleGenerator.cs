using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;

namespace ProjectZeus.Core.Levels.Mine
{
    /// <summary>
    /// Generates obstacles for the mine level
    /// </summary>
    public class MineObstacleGenerator
    {
        private readonly Random random;
        private readonly float worldWidth;
        private readonly float screenHeight;
        private readonly float groundHeight;
        private readonly float cartSpeed;

        public MineObstacleGenerator(float worldWidth, float screenHeight, float groundHeight, float cartSpeed, Random random)
        {
            this.worldWidth = worldWidth;
            this.screenHeight = screenHeight;
            this.groundHeight = groundHeight;
            this.cartSpeed = cartSpeed;
            this.random = random;
        }

        public void GenerateObstacles(
            List<MineCart> carts,
            List<Stalactite> stalactites,
            List<MineBat> bats,
            out GigaBat gigaBat,
            AsepriteSprite cartSprite,
            AsepriteSprite stalactiteSprite,
            AsepriteSprite batSprite)
        {
            carts.Clear();
            stalactites.Clear();
            bats.Clear();
            
            float groundTop = screenHeight - groundHeight;
            
            GenerateCarts(carts, groundTop, cartSprite);
            GenerateStalactites(stalactites, stalactiteSprite);
            GenerateBats(bats, groundTop, batSprite);
            
            // Create one GigaBat positioned in middle of level at ceiling
            gigaBat = new GigaBat
            {
                Position = new Vector2(worldWidth / 2, 150f),
                Velocity = new Vector2(40f, 0f),
                ChangeDirectionTimer = 3f,
                ShootTimer = 2f,
                Sprite = batSprite
            };
        }

        private void GenerateCarts(List<MineCart> carts, float groundTop, AsepriteSprite cartSprite)
        {
            const float minCartSpacing = 300f;
            const float maxCartSpacing = 500f;
            float nextCartX = 500f;
            
            while (nextCartX < worldWidth - 200)
            {
                carts.Add(new MineCart
                {
                    Position = new Vector2(nextCartX, groundTop - 20),
                    Velocity = new Vector2(-cartSpeed, 0),
                    MinX = 0,
                    MaxX = worldWidth,
                    Sprite = cartSprite
                });
                
                float spacing = minCartSpacing + (float)random.NextDouble() * (maxCartSpacing - minCartSpacing);
                nextCartX += spacing;
            }
        }

        private void GenerateStalactites(List<Stalactite> stalactites, AsepriteSprite stalactiteSprite)
        {
            const float minStalactiteSpacing = 180f;
            const float maxStalactiteSpacing = 350f;
            float nextStalactiteX = 350f;
            
            while (nextStalactiteX < worldWidth - 100)
            {
                int height = 220 + random.Next(130);
                
                stalactites.Add(new Stalactite
                {
                    Position = new Vector2(nextStalactiteX, 30),
                    Size = new Vector2(40, height),
                    Sprite = stalactiteSprite
                });
                
                float spacing = minStalactiteSpacing + (float)random.NextDouble() * (maxStalactiteSpacing - minStalactiteSpacing);
                nextStalactiteX += spacing;
            }
        }

        private void GenerateBats(List<MineBat> bats, float groundTop, AsepriteSprite batSprite)
        {
            const float minBatSpacing = 400f;
            const float maxBatSpacing = 600f;
            float nextBatX = 600f;
            
            while (nextBatX < worldWidth - 300)
            {
                float batY = groundTop - 200f + (float)random.NextDouble() * 120f;
                
                bats.Add(new MineBat
                {
                    Position = new Vector2(nextBatX, batY),
                    Velocity = new Vector2(
                        (float)(random.NextDouble() * 2 - 1) * 60f, 
                        (float)(random.NextDouble() * 2 - 1) * 40f),
                    ChangeDirectionTimer = (float)random.NextDouble() * 2f,
                    Sprite = batSprite
                });
                
                float spacing = minBatSpacing + (float)random.NextDouble() * (maxBatSpacing - minBatSpacing);
                nextBatX += spacing;
            }
        }
    }
}
