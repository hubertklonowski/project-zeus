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
using ProjectZeus.Core.Levels.Maze;
using ProjectZeus.Core.Extensions;
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
        private readonly int mazeWidth = 25;
        private readonly int mazeHeight = 15;
        private readonly int cellSize = 32;
        
        // Maze data
        private bool[,] walls;
        private Vector2 playerPosition;
        private Vector2 playerVelocity;
        private readonly float playerScale = 0.75f;
        private readonly Vector2 playerCollisionSize = new Vector2(24, 28);
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
        
        // Helpers
        private MazePlayerController playerController;
        private MazeRenderer renderer;
        
        // Visibility
        private const int visibilityRadius = 3;
        
        // Configuration
        private const float minItemDistanceFromPlayer = 500f;
        
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
            
            Vector2 minotaurCollisionSize = new Vector2(24, 28);
            float minotaurScale = 0.75f;
            minotaurController = new MinotaurController(minotaurCollisionSize, minotaurScale, random);
            
            playerController = new MazePlayerController(mazeWidth, mazeHeight, cellSize);
        }
        
        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont spriteFont)
        {
            solidTexture = new Texture2D(graphicsDevice, 1, 1);
            solidTexture.SetData(new[] { Color.White });
            
            hedgeTexture = LoadAsepriteTexture(graphicsDevice, AssetPaths.Hedge);
            sandTileTexture = LoadAsepriteTexture(graphicsDevice, AssetPaths.SandTile);
            
            minotaurSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Minotaur);
            grapesSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Grapes);
            
            font = spriteFont;
            
            GenerateMaze();
            PlacePlayerAndItem();
            
            renderer = new MazeRenderer(solidTexture, hedgeTexture, sandTileTexture, minotaurSprite, grapesSprite, font, cellSize);
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
            bool playerPlaced = false;
            for (int y = 1; y < mazeHeight - 1 && !playerPlaced; y++)
            {
                for (int x = 1; x < mazeWidth - 1 && !playerPlaced; x++)
                {
                    if (!walls[x, y])
                    {
                        playerPosition = new Vector2(
                            x * cellSize + (cellSize - playerCollisionSize.X) / 2,
                            y * cellSize + (cellSize - playerCollisionSize.Y) / 2);
                        playerVelocity = Vector2.Zero;
                        entrancePosition = new Vector2(x * cellSize + cellSize / 2, y * cellSize + cellSize / 2);
                        playerPlaced = true;
                    }
                }
            }
            
            Point playerCell = new Point((int)(playerPosition.X / cellSize), (int)(playerPosition.Y / cellSize));
            itemPosition = FindItemPosition(playerCell);
        }

        private Vector2 FindItemPosition(Point playerCell)
        {
            bool preferRightSide = playerCell.X < mazeWidth / 2;
            bool preferBottomSide = playerCell.Y < mazeHeight / 2;
            
            for (int attempts = 0; attempts < 150; attempts++)
            {
                int itemX = random.NextDouble() < 0.7
                    ? (preferRightSide ? random.Next(mazeWidth / 2, mazeWidth - 1) : random.Next(1, mazeWidth / 2))
                    : random.Next(1, mazeWidth - 1);
                int itemY = random.NextDouble() < 0.7
                    ? (preferBottomSide ? random.Next(mazeHeight / 2, mazeHeight - 1) : random.Next(1, mazeHeight / 2))
                    : random.Next(1, mazeHeight - 1);
                
                if (!walls[itemX, itemY])
                {
                    Vector2 itemPos = new Vector2(itemX * cellSize + cellSize / 2, itemY * cellSize + cellSize / 2);
                    if (Vector2.Distance(playerPosition, itemPos) > minItemDistanceFromPlayer &&
                        MazeGenerator.IsReachable(playerCell, new Point(itemX, itemY), walls, mazeWidth, mazeHeight))
                    {
                        return itemPos;
                    }
                }
            }
            
            // Fallback: choose furthest reachable cell
            Point bestCell = playerCell;
            float bestDistance = 0f;
            
            for (int x = 1; x < mazeWidth - 1; x++)
            {
                for (int y = 1; y < mazeHeight - 1; y++)
                {
                    if (!walls[x, y] && MazeGenerator.IsReachable(playerCell, new Point(x, y), walls, mazeWidth, mazeHeight))
                    {
                        float dist = Vector2.Distance(playerPosition, new Vector2(x * cellSize + cellSize / 2, y * cellSize + cellSize / 2));
                        if (dist > bestDistance)
                        {
                            bestDistance = dist;
                            bestCell = new Point(x, y);
                        }
                    }
                }
            }
            
            return new Vector2(bestCell.X * cellSize + cellSize / 2, bestCell.Y * cellSize + cellSize / 2);
        }
        
        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            if (IsCompleted)
                return;
            
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update player using controller
            playerController.UpdatePlayer(keyboardState, ref playerPosition, ref playerVelocity, 
                playerCollisionSize, dt, walls);
            
            // Update minotaur
            minotaurController.Update(dt, playerPosition, walls, cellSize, mazeWidth, mazeHeight);
            
            // Check if player is caught by minotaur
            if (minotaurController.IsActive && CheckMinotaurCollision())
            {
                PlayerCaughtByMinotaur = true;
                return;
            }
            
            // Check item pickup with E key using extension
            if (!itemCollected && keyboardState.IsKeyDown(Keys.E))
            {
                Rectangle playerRect = playerPosition.ToRectangle(playerCollisionSize);
                Rectangle itemRect = new Rectangle((int)(itemPosition.X - 12), (int)(itemPosition.Y - 12), 24, 24);
                
                if (playerRect.Intersects(itemRect))
                {
                    itemCollected = true;
                    HasItem = true;
                }
            }
            
            // Check if player wants to exit
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
        
        private bool CheckMinotaurCollision()
        {
            Vector2 minotaurPosition = minotaurController.Position;
            Vector2 minotaurCollisionSize = minotaurController.CollisionSize;
            
            Rectangle playerRect = playerPosition.ToRectangle(playerCollisionSize);
            Rectangle minotaurRect = minotaurPosition.ToRectangle(minotaurCollisionSize);
            
            return playerRect.Intersects(minotaurRect);
        }
        
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, AdonisPlayer player, GameTime gameTime)
        {
            graphicsDevice.Clear(new Color(20, 20, 20));
            
            player.Scale = playerScale;
            
            Vector2 visualOffset = new Vector2(
                (player.Size.X - playerCollisionSize.X) / 2,
                player.Size.Y - playerCollisionSize.Y
            );
            
            player.Position = playerPosition - visualOffset;
            player.Velocity = playerVelocity;
            
            spriteBatch.Begin();
            
            int playerCellX = (int)((playerPosition.X + playerCollisionSize.X / 2) / cellSize);
            int playerCellY = (int)((playerPosition.Y + playerCollisionSize.Y / 2) / cellSize);
            
            renderer.DrawMaze(spriteBatch, walls, mazeWidth, mazeHeight, playerCellX, playerCellY, visibilityRadius);
            renderer.DrawItem(spriteBatch, gameTime, itemPosition, itemCollected, playerCellX, playerCellY, visibilityRadius);
            renderer.DrawMinotaur(spriteBatch, gameTime, minotaurController, playerCellX, playerCellY, visibilityRadius);
            renderer.DrawEntrance(spriteBatch, entrancePosition, itemCollected, playerCellX, playerCellY, visibilityRadius);
            
            player.Draw(gameTime, spriteBatch);
            
            renderer.DrawCarriedItem(spriteBatch, gameTime, playerPosition, playerCollisionSize, itemCollected);
            renderer.DrawUI(spriteBatch, baseScreenSize, itemCollected, minotaurController.IsActive);
            
            spriteBatch.End();
        }
    }
}
