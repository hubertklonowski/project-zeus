using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Manages the minotaur enemy behavior in the maze.
    /// </summary>
    public class MinotaurController
    {
        private Vector2 position;
        private Vector2 velocity;
        private readonly Vector2 collisionSize;
        private readonly float scale;
        private bool isActive;
        private float timer;
        private readonly Random random;

        private const float Lifetime = 15f; // Minotaur disappears after 15 seconds
        private const float SpawnDelay = 5f; // Respawn after 5 seconds
        private const float MoveSpeed = 80f;
        private const double DirectionChangeChance = 0.02; // 2% chance per frame
        private const float MinSpawnDistance = 150f;

        public bool IsActive => isActive;
        public Vector2 Position => position;
        public Vector2 Velocity => velocity;
        public Vector2 CollisionSize => collisionSize;
        public float Scale => scale;

        public MinotaurController(Vector2 collisionSize, float scale, Random random = null)
        {
            this.collisionSize = collisionSize;
            this.scale = scale;
            this.random = random ?? new Random();
            this.isActive = false;
            this.timer = SpawnDelay;
        }

        public void Update(float dt, Vector2 playerPosition, bool[,] walls, int cellSize, int mazeWidth, int mazeHeight)
        {
            timer -= dt;

            if (!isActive)
            {
                // Check if it's time to spawn minotaur
                if (timer <= 0)
                {
                    Spawn(playerPosition, walls, cellSize, mazeWidth, mazeHeight);
                    timer = Lifetime;
                    isActive = true;
                }
            }
            else
            {
                // Minotaur is active, move it
                Move(dt, playerPosition, walls, cellSize, mazeWidth, mazeHeight);

                // Check if it's time for minotaur to disappear
                if (timer <= 0)
                {
                    isActive = false;
                    timer = SpawnDelay;
                }
            }
        }

        private void Spawn(Vector2 playerPosition, bool[,] walls, int cellSize, int mazeWidth, int mazeHeight)
        {
            // Spawn minotaur in a random passage away from player
            int attempts = 0;
            while (attempts < 50)
            {
                int x = random.Next(1, mazeWidth - 1);
                int y = random.Next(1, mazeHeight - 1);

                if (!walls[x, y])
                {
                    Vector2 spawnPos = new Vector2(
                        x * cellSize + cellSize / 2 - collisionSize.X / 2,
                        y * cellSize + cellSize / 2 - collisionSize.Y / 2);
                    float distance = Vector2.Distance(playerPosition, spawnPos);

                    if (distance > MinSpawnDistance)
                    {
                        position = spawnPos;
                        // Random initial direction
                        float angle = (float)(random.NextDouble() * Math.PI * 2);
                        velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MoveSpeed;
                        break;
                    }
                }
                attempts++;
            }
        }

        private void Move(float dt, Vector2 playerPosition, bool[,] walls, int cellSize, int mazeWidth, int mazeHeight)
        {
            Vector2 newPos = position + velocity * dt;

            // Check collision with walls
            if (CheckWallCollision(newPos, walls, cellSize, mazeWidth, mazeHeight))
            {
                // Bounce in a random direction
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MoveSpeed;
            }
            else
            {
                position = newPos;

                // Occasionally change direction randomly
                if (random.NextDouble() < DirectionChangeChance)
                {
                    float angle = (float)(random.NextDouble() * Math.PI * 2);
                    velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MoveSpeed;
                }
            }
        }

        public Vector2 HandlePlayerCollision(Vector2 playerPosition, Vector2 playerCollisionSize, float dt, bool[,] walls, int cellSize, int mazeWidth, int mazeHeight)
        {
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                                 (int)playerCollisionSize.X, (int)playerCollisionSize.Y);
            Rectangle minotaurRect = new Rectangle((int)position.X, (int)position.Y,
                                                   (int)collisionSize.X, (int)collisionSize.Y);

            if (playerRect.Intersects(minotaurRect))
            {
                // Push player away from minotaur
                Vector2 pushDir = playerPosition - position;
                if (pushDir.LengthSquared() > 0)
                    pushDir.Normalize();

                Vector2 pushedPosition = playerPosition + pushDir * 50f * dt;

                // Only apply push if it doesn't push player into a wall
                if (!CheckWallCollision(pushedPosition, playerCollisionSize, walls, cellSize, mazeWidth, mazeHeight))
                {
                    return pushedPosition;
                }
            }

            return playerPosition;
        }

        private bool CheckWallCollision(Vector2 pos, bool[,] walls, int cellSize, int mazeWidth, int mazeHeight)
        {
            return CheckWallCollision(pos, collisionSize, walls, cellSize, mazeWidth, mazeHeight);
        }

        private static bool CheckWallCollision(Vector2 pos, Vector2 size, bool[,] walls, int cellSize, int mazeWidth, int mazeHeight)
        {
            // Add a small inset to prevent visual clipping at cell boundaries
            const float inset = 2f;
            float adjustedX = pos.X + inset;
            float adjustedY = pos.Y + inset;
            float adjustedWidth = size.X - inset * 2;
            float adjustedHeight = size.Y - inset * 2;

            // Check four corners of the entity
            int left = (int)(adjustedX / cellSize);
            int right = (int)((adjustedX + adjustedWidth) / cellSize);
            int top = (int)(adjustedY / cellSize);
            int bottom = (int)((adjustedY + adjustedHeight) / cellSize);

            // Clamp to maze bounds
            left = Math.Max(0, Math.Min(mazeWidth - 1, left));
            right = Math.Max(0, Math.Min(mazeWidth - 1, right));
            top = Math.Max(0, Math.Min(mazeHeight - 1, top));
            bottom = Math.Max(0, Math.Min(mazeHeight - 1, bottom));

            // Check if any corner is in a wall
            if (walls[left, top] || walls[right, top] ||
                walls[left, bottom] || walls[right, bottom])
            {
                return true;
            }

            return false;
        }
    }
}
