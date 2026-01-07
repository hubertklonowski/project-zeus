using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.IO;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Levels;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using MonoGame.Aseprite;

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
        private readonly float playerScale = 0.75f; // Scale player to fit maze cells
        private readonly Vector2 playerCollisionSize = new Vector2(24, 28); // Collision box for scaled player
        private Vector2 itemPosition;
        private bool itemCollected;
        
        // Minotaur controller
        private MinotaurController minotaurController;
        
        // Entrance/exit position
        private Vector2 entrancePosition;
        
        // Graphics
        private Texture2D solidTexture;
        private Texture2D hedgeTexture;
        private Texture2D sandTileTexture;
        private AsepriteSprite minotaurSprite;
        private AsepriteSprite grapesSprite;
        private SpriteFont font;
        
        // Visibility (slightly smaller radius to force more exploration)
        private const int visibilityRadius = 3; // cells visible around player
        
        // Configuration constants
        private const float minItemDistanceFromPlayer = 500f; // Item is well hidden but still reachable
        
        public bool IsCompleted { get; private set; }
        public bool HasItem { get; private set; }
        public bool PlayerCaughtByMinotaur { get; private set; }

        private Random random;
        
        public MazeLevel()
        {
            random = new Random();
            itemCollected = false;
            IsCompleted = false;
            HasItem = false;
            
            // Initialize minotaur controller
            Vector2 minotaurCollisionSize = new Vector2(24, 28);
            float minotaurScale = 0.75f;
            minotaurController = new MinotaurController(minotaurCollisionSize, minotaurScale, random);
        }
        
        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont spriteFont)
        {
            // Create a 1x1 solid texture for drawing rectangles
            solidTexture = new Texture2D(graphicsDevice, 1, 1);
            solidTexture.SetData(new[] { Color.White });
            
            // Load the aseprite textures
            hedgeTexture = LoadAsepriteTexture(graphicsDevice, AssetPaths.Hedge);
            sandTileTexture = LoadAsepriteTexture(graphicsDevice, AssetPaths.SandTile);
            
            // Load sprites with animation support
            minotaurSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Minotaur);
            grapesSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Grapes);
            
            font = spriteFont;
            
            GenerateMaze();
            PlacePlayerAndItem();
        }
        
        private Texture2D LoadAsepriteTexture(GraphicsDevice graphicsDevice, string filePath)
        {
            try
            {
                using (var stream = TitleContainer.OpenStream(filePath))
                {
                    var asepriteFile = AsepriteFileLoader.FromStream(Path.GetFileNameWithoutExtension(filePath), stream);
                    var sprite = asepriteFile.CreateSprite(graphicsDevice, frameIndex: 0, onlyVisibleLayers: true, includeBackgroundLayer: false, includeTilemapLayers: false);
                    return sprite.TextureRegion.Texture;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load {filePath}: {ex.Message}");
                return null;
            }
        }
        
        private void GenerateMaze()
        {
            var generator = new MazeGenerator(mazeWidth, mazeHeight, random);
            walls = generator.Generate();
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
                        // Center the collision box within the cell
                        playerPosition = new Vector2(
                            x * cellSize + (cellSize - playerCollisionSize.X) / 2,
                            y * cellSize + (cellSize - playerCollisionSize.Y) / 2);
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
                        MazeGenerator.IsReachable(playerCell, new Point(itemX, itemY), walls, mazeWidth, mazeHeight))
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
                        if (MazeGenerator.IsReachable(playerCell, target, walls, mazeWidth, mazeHeight))
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
            if (!CheckWallCollision(newPosition, playerCollisionSize))
            {
                playerPosition = newPosition;
            }
            else
            {
                // Try moving only on X axis
                Vector2 xOnly = new Vector2(newPosition.X, playerPosition.Y);
                if (!CheckWallCollision(xOnly, playerCollisionSize))
                {
                    playerPosition = xOnly;
                }
                else
                {
                    // Try moving only on Y axis
                    Vector2 yOnly = new Vector2(playerPosition.X, newPosition.Y);
                    if (!CheckWallCollision(yOnly, playerCollisionSize))
                    {
                        playerPosition = yOnly;
                    }
                }
            }
            
            // Update minotaur
            minotaurController.Update(dt, playerPosition, walls, cellSize, mazeWidth, mazeHeight);
            
            // Check if player is caught by minotaur
            if (minotaurController.IsActive && CheckMinotaurCollision())
            {
                PlayerCaughtByMinotaur = true;
                return;
            }
            
            // Check item pickup with E key
            if (!itemCollected && keyboardState.IsKeyDown(Keys.E))
            {
                Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                                     (int)playerCollisionSize.X, (int)playerCollisionSize.Y);
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
                float distance = Vector2.Distance(playerPosition + playerCollisionSize / 2, entrancePosition);
                
                if (distance < 30)
                {
                    IsCompleted = true;
                    return;
                }
            }
        }
        
        private bool CheckWallCollision(Vector2 position, Vector2 size)
        {
            // Add a small inset to prevent visual clipping at cell boundaries
            const float inset = 2f;
            float adjustedX = position.X + inset;
            float adjustedY = position.Y + inset;
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
        
        private bool CheckMinotaurCollision()
        {
            // Check collision with minotaur
            Vector2 minotaurPosition = minotaurController.Position;
            Vector2 minotaurCollisionSize = minotaurController.CollisionSize;
            
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                                 (int)playerCollisionSize.X, (int)playerCollisionSize.Y);
            Rectangle minotaurRect = new Rectangle((int)minotaurPosition.X, (int)minotaurPosition.Y,
                                                   (int)minotaurCollisionSize.X, (int)minotaurCollisionSize.Y);
            
            return playerRect.Intersects(minotaurRect);
        }
        
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, AdonisPlayer player, GameTime gameTime)
        {
            graphicsDevice.Clear(new Color(20, 20, 20));
            
            // Set player scale to fit maze cells
            player.Scale = playerScale;
            
            // Calculate visual offset - the sprite is drawn with origin at bottom-center
            // so we need to offset the position to align the visual with the collision box
            Vector2 visualOffset = new Vector2(
                (player.Size.X - playerCollisionSize.X) / 2,
                player.Size.Y - playerCollisionSize.Y
            );
            
            // Sync player position and velocity to AdonisPlayer for rendering
            // Offset so the visual sprite aligns with the collision box
            player.Position = playerPosition - visualOffset;
            player.Velocity = playerVelocity;
            
            // Use collision size for gameplay calculations
            Vector2 actualPlayerSize = playerCollisionSize;
            
            spriteBatch.Begin();
            
            int playerCellX = (int)((playerPosition.X + actualPlayerSize.X / 2) / cellSize);
            int playerCellY = (int)((playerPosition.Y + actualPlayerSize.Y / 2) / cellSize);
            
            // Draw maze cells within visibility radius
            for (int x = 0; x < mazeWidth; x++)
            {
                for (int y = 0; y < mazeHeight; y++)
                {
                    int dx = Math.Abs(x - playerCellX);
                    int dy = Math.Abs(y - playerCellY);
                    
                    if (dx <= visibilityRadius && dy <= visibilityRadius)
                    {
                        Rectangle cellRect = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                        
                        if (walls[x, y])
                        {
                            // Draw hedge texture for walls
                            spriteBatch.Draw(hedgeTexture, cellRect, Color.White);
                        }
                        else
                        {
                            // Draw sand tile texture for floors
                            spriteBatch.Draw(sandTileTexture, cellRect, Color.White);
                        }
                    }
                }
            }
            
            // Draw item (grapes) if not collected and visible
            if (!itemCollected)
            {
                int itemCellX = (int)(itemPosition.X / cellSize);
                int itemCellY = (int)(itemPosition.Y / cellSize);
                int itemDx = Math.Abs(itemCellX - playerCellX);
                int itemDy = Math.Abs(itemCellY - playerCellY);
                
                if (itemDx <= visibilityRadius && itemDy <= visibilityRadius)
                {
                    if (grapesSprite != null && grapesSprite.IsLoaded)
                    {
                        // Draw grapes sprite centered at itemPosition (grapes don't move, so always use idle frame)
                        Vector2 drawPos = new Vector2(
                            itemPosition.X - grapesSprite.Size.X / 2, 
                            itemPosition.Y - grapesSprite.Size.Y / 2);
                        grapesSprite.Draw(spriteBatch, drawPos, isMoving: false, gameTime, Color.White);
                    }
                }
            }
            
            // Draw minotaur if active and visible
            if (minotaurController.IsActive)
            {
                Vector2 minotaurPosition = minotaurController.Position;
                Vector2 minotaurVelocity = minotaurController.Velocity;
                Vector2 minotaurCollisionSize = minotaurController.CollisionSize;
                
                int minotaurCellX = (int)((minotaurPosition.X + minotaurCollisionSize.X / 2) / cellSize);
                int minotaurCellY = (int)((minotaurPosition.Y + minotaurCollisionSize.Y / 2) / cellSize);
                int minotaurDx = Math.Abs(minotaurCellX - playerCellX);
                int minotaurDy = Math.Abs(minotaurCellY - playerCellY);
                
                if (minotaurDx <= visibilityRadius && minotaurDy <= visibilityRadius)
                {
                    if (minotaurSprite != null && minotaurSprite.IsLoaded)
                    {
                        // Check if minotaur is moving
                        bool isMoving = minotaurVelocity.LengthSquared() > 0;
                        
                        // Calculate scaled sprite size
                        Vector2 scaledSpriteSize = minotaurSprite.Size * minotaurController.Scale;
                        
                        // Draw minotaur sprite scaled and centered on collision box
                        Rectangle destRect = new Rectangle(
                            (int)(minotaurPosition.X + minotaurCollisionSize.X / 2 - scaledSpriteSize.X / 2),
                            (int)(minotaurPosition.Y + minotaurCollisionSize.Y - scaledSpriteSize.Y),
                            (int)scaledSpriteSize.X,
                            (int)scaledSpriteSize.Y);
                        
                        // Flip sprite based on movement direction
                        SpriteEffects flip = minotaurVelocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        
                        var texture = minotaurSprite.GetFrameTexture(isMoving, gameTime, 10f);
                        if (texture != null)
                        {
                            spriteBatch.Draw(texture, destRect, null, Color.White, 0f, Vector2.Zero, flip, 0f);
                        }
                    }
                }
            }
            
            // Draw entrance/exit if item is collected and visible
            if (itemCollected)
            {
                int entranceCellX = (int)(entrancePosition.X / cellSize);
                int entranceCellY = (int)(entrancePosition.Y / cellSize);
                int entranceDx = Math.Abs(entranceCellX - playerCellX);
                int entranceDy = Math.Abs(entranceCellY - playerCellY);
                
                if (entranceDx <= visibilityRadius && entranceDy <= visibilityRadius)
                {
                    Rectangle entranceRect = new Rectangle((int)(entrancePosition.X - 14), (int)(entrancePosition.Y - 14), 28, 28);
                    spriteBatch.Draw(solidTexture, entranceRect, new Color(0, 255, 0, 128));
                }
            }
            
            // Draw player
            player.Draw(gameTime, spriteBatch);
            
            // Draw carried item indicator if item is collected
            if (itemCollected)
            {
                if (grapesSprite != null && grapesSprite.IsLoaded)
                {
                    // Draw same grapes sprite above the player's head
                    Vector2 carriedCenter = new Vector2(
                        playerPosition.X + playerCollisionSize.X / 2f,
                        playerPosition.Y - 20f);

                    Vector2 drawPos = new Vector2(
                        carriedCenter.X - grapesSprite.Size.X / 2f,
                        carriedCenter.Y - grapesSprite.Size.Y / 2f);

                    grapesSprite.Draw(spriteBatch, drawPos, isMoving: false, gameTime, Color.White);
                }
            }
            
            // Draw UI
            string instructions = "Navigate the maze! Press E near the golden item to collect it.";
            if (itemCollected)
                instructions = "Item collected! Return to the entrance (top-left) to exit.";
            
            spriteBatch.DrawString(font, instructions, new Vector2(10, 10), Color.Yellow);
            
            if (minotaurController.IsActive)
            {
                string warning = "MINOTAUR NEARBY!";
                Vector2 warningSize = font.MeasureString(warning);
                spriteBatch.DrawString(font, warning, new Vector2(baseScreenSize.X - warningSize.X - 10, 10), Color.Red);
            }
            
            spriteBatch.End();
        }
    }
}
