using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ProjectZeus.Core
{
    /// <summary>
    /// A maze level with random generation, limited visibility, collectible item, and roaming minotaur.
    /// </summary>
    public class MazeLevel
    {
        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        private readonly int mazeWidth = 25;  // Maze fits inside the 800px width
        private readonly int mazeHeight = 15; // Maze fits inside the 480px height
        private readonly int cellSize = 32;
        
        // Maze data
        private bool[,] walls; // true = wall, false = passage
        private Vector2 playerPosition;
        private Vector2 playerVelocity;
        private readonly Vector2 playerSize = new Vector2(24, 24);
        private Vector2 itemPosition;
        private bool itemCollected;
        
        // Minotaur
        private Vector2 minotaurPosition;
        private Vector2 minotaurVelocity;
        private readonly Vector2 minotaurSize = new Vector2(28, 28);
        private bool minotaurActive;
        private float minotaurTimer;
        private const float minotaurLifetime = 15f; // Minotaur disappears after 15 seconds
        private const float minotaurSpawnDelay = 5f; // Respawn after 5 seconds
        private Random random;
        
        // Entrance/exit position
        private Vector2 entrancePosition;
        
        // Graphics
        private Texture2D solidTexture;
        private SpriteFont font;
        
        // Visibility (slightly smaller radius to force more exploration)
        private const int visibilityRadius = 3; // cells visible around player
        
        // Configuration constants
        private const float minItemDistanceFromPlayer = 500f; // Increased so the item is well hidden
        private const float minMinotaurSpawnDistance = 150f;
        private const double minotaurDirectionChangeChance = 0.02; // 2% chance per frame
        
        public bool IsCompleted { get; private set; }
        public bool HasItem { get; private set; }
        
        public MazeLevel()
        {
            random = new Random();
            itemCollected = false;
            IsCompleted = false;
            HasItem = false;
            minotaurActive = false;
            minotaurTimer = minotaurSpawnDelay;
        }
        
        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont spriteFont)
        {
            // Create a 1x1 solid texture for drawing rectangles
            solidTexture = new Texture2D(graphicsDevice, 1, 1);
            solidTexture.SetData(new[] { Color.White });
            font = spriteFont;
            
            GenerateMaze();
            PlacePlayerAndItem();
        }
        
        private void GenerateMaze()
        {
            // Initialize all cells as walls
            walls = new bool[mazeWidth, mazeHeight];
            for (int x = 0; x < mazeWidth; x++)
            {
                for (int y = 0; y < mazeHeight; y++)
                {
                    walls[x, y] = true;
                }
            }
            
            // Use recursive backtracking to generate maze
            Stack<Point> stack = new Stack<Point>();
            Point start = new Point(1, 1);
            walls[start.X, start.Y] = false;
            stack.Push(start);
            
            while (stack.Count > 0)
            {
                Point current = stack.Peek();
                List<Point> unvisitedNeighbors = GetUnvisitedNeighbors(current);
                
                if (unvisitedNeighbors.Count > 0)
                {
                    Point next = unvisitedNeighbors[random.Next(unvisitedNeighbors.Count)];
                    
                    // Remove wall between current and next
                    int wallX = (current.X + next.X) / 2;
                    int wallY = (current.Y + next.Y) / 2;
                    walls[wallX, wallY] = false;
                    walls[next.X, next.Y] = false;
                    
                    stack.Push(next);
                }
                else
                {
                    stack.Pop();
                }
            }
            
            // Ensure outer border is always walls to prevent going off-screen
            for (int x = 0; x < mazeWidth; x++)
            {
                walls[x, 0] = true;
                walls[x, mazeHeight - 1] = true;
            }
            for (int y = 0; y < mazeHeight; y++)
            {
                walls[0, y] = true;
                walls[mazeWidth - 1, y] = true;
            }
        }
        
        private List<Point> GetUnvisitedNeighbors(Point cell)
        {
            List<Point> neighbors = new List<Point>();
            
            // Check all four directions (2 cells away)
            Point[] directions = new[]
            {
                new Point(cell.X - 2, cell.Y),
                new Point(cell.X + 2, cell.Y),
                new Point(cell.X, cell.Y - 2),
                new Point(cell.X, cell.Y + 2)
            };
            
            foreach (Point dir in directions)
            {
                if (dir.X > 0 && dir.X < mazeWidth - 1 &&
                    dir.Y > 0 && dir.Y < mazeHeight - 1 &&
                    walls[dir.X, dir.Y])
                {
                    neighbors.Add(dir);
                }
            }
            
            return neighbors;
        }
        
        private void PlacePlayerAndItem()
        {
            // Place player at top-left passage
            bool playerPlaced = false;
            for (int y = 1; y < mazeHeight - 1 && !playerPlaced; y++)
            {
                for (int x = 1; x < mazeWidth - 1 && !playerPlaced; x++)
                {
                    if (!walls[x, y])
                    {
                        playerPosition = new Vector2(x * cellSize + cellSize / 2 - playerSize.X / 2,
                                                     y * cellSize + cellSize / 2 - playerSize.Y / 2);
                        playerVelocity = Vector2.Zero;
                        entrancePosition = new Vector2(x * cellSize + cellSize / 2,
                                                      y * cellSize + cellSize / 2);
                        playerPlaced = true;
                    }
                }
            }
            
            // Prefer placing the item in the opposite half of the maze to hide it better
            Point playerCell = new Point((int)(playerPosition.X / cellSize), (int)(playerPosition.Y / cellSize));
            bool preferRightSide = playerCell.X < mazeWidth / 2;
            bool preferBottomSide = playerCell.Y < mazeHeight / 2;
            
            int attempts = 0;
            while (attempts < 150)
            {
                int itemX;
                int itemY;
                
                // Bias item towards far quadrant
                if (random.NextDouble() < 0.7)
                {
                    itemX = preferRightSide ? random.Next(mazeWidth / 2, mazeWidth - 1)
                                            : random.Next(1, mazeWidth / 2);
                    itemY = preferBottomSide ? random.Next(mazeHeight / 2, mazeHeight - 1)
                                              : random.Next(1, mazeHeight / 2);
                }
                else
                {
                    itemX = random.Next(1, mazeWidth - 1);
                    itemY = random.Next(1, mazeHeight - 1);
                }
                
                if (!walls[itemX, itemY])
                {
                    Vector2 itemPos = new Vector2(itemX * cellSize + cellSize / 2,
                                                  itemY * cellSize + cellSize / 2);
                    float distance = Vector2.Distance(playerPosition, itemPos);
                    
                    if (distance > minItemDistanceFromPlayer &&
                        IsReachableCell(playerCell, new Point(itemX, itemY)))
                    {
                        itemPosition = itemPos;
                        return;
                    }
                }
                attempts++;
            }
            
            // Fallback: choose any reachable cell furthest from player
            Point bestCell = playerCell;
            float bestDistance = 0f;
            
            for (int x = 1; x < mazeWidth - 1; x++)
            {
                for (int y = 1; y < mazeHeight - 1; y++)
                {
                    if (!walls[x, y])
                    {
                        Point target = new Point(x, y);
                        if (IsReachableCell(playerCell, target))
                        {
                            Vector2 cellCenter = new Vector2(x * cellSize + cellSize / 2,
                                                             y * cellSize + cellSize / 2);
                            float dist = Vector2.Distance(playerPosition, cellCenter);
                            if (dist > bestDistance)
                            {
                                bestDistance = dist;
                                bestCell = target;
                            }
                        }
                    }
                }
            }
            
            itemPosition = new Vector2(bestCell.X * cellSize + cellSize / 2,
                                       bestCell.Y * cellSize + cellSize / 2);
        }
        
        // Simple BFS over passage cells to ensure there is a walkable path between two cells
        private bool IsReachableCell(Point start, Point target)
        {
            if (walls[target.X, target.Y])
                return false;
            
            bool[,] visited = new bool[mazeWidth, mazeHeight];
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(start);
            visited[start.X, start.Y] = true;
            
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            
            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();
                if (current == target)
                    return true;
                
                for (int dir = 0; dir < 4; dir++)
                {
                    int nx = current.X + dx[dir];
                    int ny = current.Y + dy[dir];
                    
                    if (nx > 0 && nx < mazeWidth - 1 && ny > 0 && ny < mazeHeight - 1 &&
                        !walls[nx, ny] && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Point(nx, ny));
                    }
                }
            }
            
            return false;
        }
        
        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            if (IsCompleted)
                return;
            
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float moveSpeed = 120f;
            
            // Player movement
            Vector2 movement = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                movement.X -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                movement.X += 1f;
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
                movement.Y -= 1f;
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
                movement.Y += 1f;
            
            if (movement.LengthSquared() > 0)
                movement.Normalize();
            
            playerVelocity = movement * moveSpeed;
            Vector2 newPosition = playerPosition + playerVelocity * dt;
            
            // Collision detection with walls
            if (!CheckWallCollision(newPosition, playerSize))
            {
                playerPosition = newPosition;
            }
            else
            {
                // Try moving only on X axis
                Vector2 xOnly = new Vector2(newPosition.X, playerPosition.Y);
                if (!CheckWallCollision(xOnly, playerSize))
                {
                    playerPosition = xOnly;
                }
                else
                {
                    // Try moving only on Y axis
                    Vector2 yOnly = new Vector2(playerPosition.X, newPosition.Y);
                    if (!CheckWallCollision(yOnly, playerSize))
                    {
                        playerPosition = yOnly;
                    }
                }
            }
            
            // Maze already fits inside the window; no additional clamping needed.
            
            // Update minotaur
            UpdateMinotaur(dt);
            
            // Check item pickup with E key
            if (!itemCollected && keyboardState.IsKeyDown(Keys.E))
            {
                Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                                     (int)playerSize.X, (int)playerSize.Y);
                Rectangle itemRect = new Rectangle((int)(itemPosition.X - 12), (int)(itemPosition.Y - 12), 24, 24);
                
                if (playerRect.Intersects(itemRect))
                {
                    itemCollected = true;
                    HasItem = true;
                }
            }
            
            // Check if player wants to exit (reached starting position with item)
            if (itemCollected)
            {
                float distance = Vector2.Distance(playerPosition + playerSize / 2, entrancePosition);
                
                if (distance < 30)
                {
                    IsCompleted = true;
                    return;
                }
            }
        }
        
        private void UpdateMinotaur(float dt)
        {
            minotaurTimer -= dt;
            
            if (!minotaurActive)
            {
                // Check if it's time to spawn minotaur
                if (minotaurTimer <= 0)
                {
                    SpawnMinotaur();
                    minotaurTimer = minotaurLifetime;
                    minotaurActive = true;
                }
            }
            else
            {
                // Minotaur is active, move it
                MoveMinotaur(dt);
                
                // Check if it's time for minotaur to disappear
                if (minotaurTimer <= 0)
                {
                    minotaurActive = false;
                    minotaurTimer = minotaurSpawnDelay;
                }
            }
        }
        
        private void SpawnMinotaur()
        {
            // Spawn minotaur in a random passage away from player
            int attempts = 0;
            while (attempts < 50)
            {
                int x = random.Next(1, mazeWidth - 1);
                int y = random.Next(1, mazeHeight - 1);
                
                if (!walls[x, y])
                {
                    Vector2 spawnPos = new Vector2(x * cellSize + cellSize / 2 - minotaurSize.X / 2,
                                                   y * cellSize + cellSize / 2 - minotaurSize.Y / 2);
                    float distance = Vector2.Distance(playerPosition, spawnPos);
                    
                    if (distance > minMinotaurSpawnDistance) // Don't spawn too close to player
                    {
                        minotaurPosition = spawnPos;
                        // Random initial direction
                        float angle = (float)(random.NextDouble() * Math.PI * 2);
                        minotaurVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 80f;
                        break;
                    }
                }
                attempts++;
            }
        }
        
        private void MoveMinotaur(float dt)
        {
            Vector2 newPos = minotaurPosition + minotaurVelocity * dt;
            
            // Check collision with walls
            if (CheckWallCollision(newPos, minotaurSize))
            {
                // Bounce in a random direction
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                minotaurVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 80f;
            }
            else
            {
                minotaurPosition = newPos;
                
                // Occasionally change direction randomly
                if (random.NextDouble() < minotaurDirectionChangeChance)
                {
                    float angle = (float)(random.NextDouble() * Math.PI * 2);
                    minotaurVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 80f;
                }
            }
            
            // Check collision with player
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                                 (int)playerSize.X, (int)playerSize.Y);
            Rectangle minotaurRect = new Rectangle((int)minotaurPosition.X, (int)minotaurPosition.Y,
                                                   (int)minotaurSize.X, (int)minotaurSize.Y);
            
            if (playerRect.Intersects(minotaurRect))
            {
                // Push player away from minotaur
                Vector2 pushDir = playerPosition - minotaurPosition;
                if (pushDir.LengthSquared() > 0)
                    pushDir.Normalize();
                
                Vector2 pushedPosition = playerPosition + pushDir * 50f * dt;
                
                // Only apply push if it doesn't push player into a wall
                if (!CheckWallCollision(pushedPosition, playerSize))
                {
                    playerPosition = pushedPosition;
                }
            }
        }
        
        private bool CheckWallCollision(Vector2 position, Vector2 size)
        {
            // Check four corners of the entity
            int left = (int)(position.X / cellSize);
            int right = (int)((position.X + size.X) / cellSize);
            int top = (int)(position.Y / cellSize);
            int bottom = (int)((position.Y + size.Y) / cellSize);
            
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
        
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(new Color(20, 20, 20)); // Dark background
            
            spriteBatch.Begin();
            
            // Calculate the cell the player is currently in
            int playerCellX = (int)((playerPosition.X + playerSize.X / 2) / cellSize);
            int playerCellY = (int)((playerPosition.Y + playerSize.Y / 2) / cellSize);
            
            // Draw visible maze cells
            for (int x = 0; x < mazeWidth; x++)
            {
                for (int y = 0; y < mazeHeight; y++)
                {
                    // Calculate distance from player
                    int dx = Math.Abs(x - playerCellX);
                    int dy = Math.Abs(y - playerCellY);
                    
                    // Only draw cells within visibility radius
                    if (dx <= visibilityRadius && dy <= visibilityRadius)
                    {
                        Rectangle cellRect = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                        
                        if (walls[x, y])
                        {
                            // Draw wall
                            spriteBatch.Draw(solidTexture, cellRect, new Color(100, 100, 120));
                            // Wall border
                            DrawRectangleOutline(spriteBatch, cellRect, new Color(80, 80, 100));
                        }
                        else
                        {
                            // Draw passage
                            spriteBatch.Draw(solidTexture, cellRect, new Color(40, 40, 45));
                        }
                    }
                }
            }
            
            // Draw item if not collected and visible
            if (!itemCollected)
            {
                int itemCellX = (int)(itemPosition.X / cellSize);
                int itemCellY = (int)(itemPosition.Y / cellSize);
                int itemDx = Math.Abs(itemCellX - playerCellX);
                int itemDy = Math.Abs(itemCellY - playerCellY);
                
                if (itemDx <= visibilityRadius && itemDy <= visibilityRadius)
                {
                    Rectangle itemRect = new Rectangle((int)(itemPosition.X - 12), (int)(itemPosition.Y - 12), 24, 24);
                    spriteBatch.Draw(solidTexture, itemRect, Color.Gold);
                    DrawRectangleOutline(spriteBatch, itemRect, Color.Orange);
                }
            }
            
            // Draw minotaur if active and visible
            if (minotaurActive)
            {
                int minotaurCellX = (int)((minotaurPosition.X + minotaurSize.X / 2) / cellSize);
                int minotaurCellY = (int)((minotaurPosition.Y + minotaurSize.Y / 2) / cellSize);
                int minotaurDx = Math.Abs(minotaurCellX - playerCellX);
                int minotaurDy = Math.Abs(minotaurCellY - playerCellY);
                
                if (minotaurDx <= visibilityRadius && minotaurDy <= visibilityRadius)
                {
                    Rectangle minotaurRect = new Rectangle((int)minotaurPosition.X, (int)minotaurPosition.Y,
                                                           (int)minotaurSize.X, (int)minotaurSize.Y);
                    spriteBatch.Draw(solidTexture, minotaurRect, new Color(139, 69, 19)); // Brown
                    DrawRectangleOutline(spriteBatch, minotaurRect, new Color(101, 51, 15));
                    
                    // Draw horns
                    Rectangle horn1 = new Rectangle((int)minotaurPosition.X + 2, (int)minotaurPosition.Y, 6, 8);
                    Rectangle horn2 = new Rectangle((int)minotaurPosition.X + (int)minotaurSize.X - 8, (int)minotaurPosition.Y, 6, 8);
                    spriteBatch.Draw(solidTexture, horn1, Color.White);
                    spriteBatch.Draw(solidTexture, horn2, Color.White);
                }
            }
            
            // Draw entrance marker if visible and item is collected
            if (itemCollected)
            {
                int entranceCellX = (int)(entrancePosition.X / cellSize);
                int entranceCellY = (int)(entrancePosition.Y / cellSize);
                int entranceDx = Math.Abs(entranceCellX - playerCellX);
                int entranceDy = Math.Abs(entranceCellY - playerCellY);
                
                if (entranceDx <= visibilityRadius && entranceDy <= visibilityRadius)
                {
                    Rectangle entranceRect = new Rectangle((int)(entrancePosition.X - 14), (int)(entrancePosition.Y - 14), 28, 28);
                    spriteBatch.Draw(solidTexture, entranceRect, new Color(0, 255, 0, 128)); // Green with transparency
                    DrawRectangleOutline(spriteBatch, entranceRect, Color.LimeGreen);
                }
            }
            
            // Draw player
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                                 (int)playerSize.X, (int)playerSize.Y);
            spriteBatch.Draw(solidTexture, playerRect, new Color(255, 220, 180));
            DrawRectangleOutline(spriteBatch, playerRect, new Color(200, 160, 120));
            
            // If carrying item, draw it above player
            if (itemCollected)
            {
                Rectangle carriedItemRect = new Rectangle((int)playerPosition.X + (int)playerSize.X / 2 - 10,
                                                         (int)playerPosition.Y - 18, 20, 20);
                spriteBatch.Draw(solidTexture, carriedItemRect, Color.Gold);
                DrawRectangleOutline(spriteBatch, carriedItemRect, Color.Orange);
            }
            
            // Draw UI
            string instructions = "Navigate the maze! Press E near the golden item to collect it.";
            if (itemCollected)
                instructions = "Item collected! Return to the entrance (top-left) to exit.";
            
            spriteBatch.DrawString(font, instructions, new Vector2(10, 10), Color.Yellow);
            
            if (minotaurActive)
            {
                string warning = "MINOTAUR NEARBY!";
                Vector2 warningSize = font.MeasureString(warning);
                spriteBatch.DrawString(font, warning, new Vector2(baseScreenSize.X - warningSize.X - 10, 10), Color.Red);
            }
            
            spriteBatch.End();
        }
        
        private void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            int thickness = 2;
            spriteBatch.Draw(solidTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(solidTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(solidTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(solidTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}
