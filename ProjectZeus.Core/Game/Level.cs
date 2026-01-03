#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace Platformer2D
{
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private Texture2D[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Gem> gems = new List<Gem>();
        private List<Enemy> enemies = new List<Enemy>();
        private List<Cart> carts = new List<Cart>();
        private List<Bat> bats = new List<Bat>();
        private List<PillarItem> pillarItems = new List<PillarItem>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private SoundEffect exitReachedSound;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            timeRemaining = TimeSpan.FromMinutes(2.0);

            LoadTiles(fileStream);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Texture2D[3];
            for (int i = 0; i < layers.Length; ++i)
            {
                // Choose a random segment if each background layer for level variety.
                int segmentIndex = levelIndex;
                layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + segmentIndex);
            }

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }

        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, exit, etc.
        /// </summary>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Gem
                case 'G':
                    return LoadGemTile(x, y);

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Various enemies
                case 'A':
                    return LoadEnemyTile(x, y, "MonsterA");
                case 'B':
                    return LoadEnemyTile(x, y, "MonsterB");
                case 'C':
                    return LoadEnemyTile(x, y, "MonsterC");
                case 'D':
                    return LoadEnemyTile(x, y, "MonsterD");

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                // Cart (moves on rails)
                case 'R':
                    return LoadCartTile(x, y);

                // Bat (flying enemy)
                case 'F':
                    return LoadBatTile(x, y);

                // Pillar Item (collectible for pillar)
                case 'I':
                    return LoadPillarItemTile(x, y);

                // Stalactite (decorative, hanging from ceiling)
                case 'V':
                    return LoadStalactiteTile();

                // Torch (decorative)
                case 'T':
                    return LoadTorchTile();

                // Rail (passable, decorative track)
                case '=':
                    return LoadRailTile();

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }


        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }


        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates a gem and puts it in the level.
        /// </summary>
        private Tile LoadGemTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            gems.Add(new Gem(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates a cart and puts it in the level.
        /// </summary>
        private Tile LoadCartTile(int x, int y)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            carts.Add(new Cart(this, position));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates a bat and puts it in the level.
        /// </summary>
        private Tile LoadBatTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            bats.Add(new Bat(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates a pillar item and puts it in the level.
        /// </summary>
        private Tile LoadPillarItemTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            // Create item with a distinct color (gold for first item)
            Color itemColor = Color.Gold;
            pillarItems.Add(new PillarItem(this, new Vector2(position.X, position.Y), itemColor));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Loads a stalactite tile (decorative hanging spike).
        /// </summary>
        private Tile LoadStalactiteTile()
        {
            // Create a simple placeholder texture for stalactite
            Texture2D stalactiteTexture = CreateStalactiteTexture();
            return new Tile(stalactiteTexture, TileCollision.Passable);
        }

        /// <summary>
        /// Loads a torch tile (decorative light source).
        /// </summary>
        private Tile LoadTorchTile()
        {
            // Create a simple placeholder texture for torch
            Texture2D torchTexture = CreateTorchTexture();
            return new Tile(torchTexture, TileCollision.Passable);
        }

        /// <summary>
        /// Loads a rail tile (decorative track for carts).
        /// </summary>
        private Tile LoadRailTile()
        {
            // Create a simple placeholder texture for rails
            Texture2D railTexture = CreateRailTexture();
            return new Tile(railTexture, TileCollision.Passable);
        }

        /// <summary>
        /// Creates a placeholder texture for a stalactite.
        /// </summary>
        private Texture2D CreateStalactiteTexture()
        {
            var graphicsDevice = (Content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService)?.GraphicsDevice;
            if (graphicsDevice == null) return null;

            Texture2D texture = new Texture2D(graphicsDevice, Tile.Width, Tile.Height);
            Color[] data = new Color[Tile.Width * Tile.Height];
            
            // Fill with transparent background
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;

            // Draw a simple stalactite shape (triangle pointing down)
            int centerX = Tile.Width / 2;
            for (int y = 0; y < Tile.Height / 2; y++)
            {
                int width = (Tile.Width / 4) - (y * Tile.Width / (Tile.Height * 2));
                for (int x = centerX - width; x <= centerX + width; x++)
                {
                    if (x >= 0 && x < Tile.Width)
                    {
                        int index = y * Tile.Width + x;
                        data[index] = new Color(150, 150, 150); // Gray stone
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Creates a placeholder texture for a torch.
        /// </summary>
        private Texture2D CreateTorchTexture()
        {
            var graphicsDevice = (Content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService)?.GraphicsDevice;
            if (graphicsDevice == null) return null;

            Texture2D texture = new Texture2D(graphicsDevice, Tile.Width, Tile.Height);
            Color[] data = new Color[Tile.Width * Tile.Height];
            
            // Fill with transparent background
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;

            // Draw a simple torch (stick with flame on top)
            int centerX = Tile.Width / 2;
            
            // Stick (brown)
            for (int y = Tile.Height / 3; y < Tile.Height; y++)
            {
                for (int x = centerX - 2; x <= centerX + 2; x++)
                {
                    if (x >= 0 && x < Tile.Width)
                    {
                        int index = y * Tile.Width + x;
                        data[index] = new Color(101, 67, 33);
                    }
                }
            }

            // Flame (yellow/orange)
            for (int y = Tile.Height / 6; y < Tile.Height / 3; y++)
            {
                int flameWidth = 4 - (y * 2 / Tile.Height);
                for (int x = centerX - flameWidth; x <= centerX + flameWidth; x++)
                {
                    if (x >= 0 && x < Tile.Width)
                    {
                        int index = y * Tile.Width + x;
                        data[index] = new Color(255, 200, 50);
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Creates a placeholder texture for rails.
        /// </summary>
        private Texture2D CreateRailTexture()
        {
            var graphicsDevice = (Content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService)?.GraphicsDevice;
            if (graphicsDevice == null) return null;

            Texture2D texture = new Texture2D(graphicsDevice, Tile.Width, Tile.Height);
            Color[] data = new Color[Tile.Width * Tile.Height];
            
            // Fill with transparent background
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;

            // Draw horizontal rails
            int rail1Y = Tile.Height / 2 - 3;
            int rail2Y = Tile.Height / 2 + 3;
            
            for (int x = 0; x < Tile.Width; x++)
            {
                // Upper rail
                for (int y = rail1Y; y < rail1Y + 2; y++)
                {
                    int index = y * Tile.Width + x;
                    data[index] = new Color(120, 120, 120);
                }
                // Lower rail
                for (int y = rail2Y; y < rail2Y + 2; y++)
                {
                    int index = y * Tile.Width + x;
                    data[index] = new Color(120, 120, 120);
                }
            }

            // Draw wooden ties
            for (int tieX = 0; tieX < Tile.Width; tieX += Tile.Width / 3)
            {
                for (int x = tieX; x < tieX + 4 && x < Tile.Width; x++)
                {
                    for (int y = rail1Y - 2; y < rail2Y + 4; y++)
                    {
                        if (y >= 0 && y < Tile.Height)
                        {
                            int index = y * Tile.Width + x;
                            data[index] = new Color(101, 67, 33);
                        }
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * PointsPerSecond;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, keyboardState, gamePadState, accelState, orientation);
                UpdateGems(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);

                // Update carts, bats, and pillar items
                UpdateCarts(gameTime);
                UpdateBats(gameTime);
                UpdatePillarItems(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }

        /// <summary>
        /// Animates each gem and checks to allows the player to collect them.
        /// </summary>
        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < gems.Count; ++i)
            {
                Gem gem = gems[i];

                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    gems.RemoveAt(i--);
                    OnGemCollected(gem, Player);
                }
            }
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    OnPlayerKilled(enemy);
                }
            }
        }

        /// <summary>
        /// Updates each cart.
        /// </summary>
        private void UpdateCarts(GameTime gameTime)
        {
            foreach (Cart cart in carts)
            {
                cart.Update(gameTime);
            }
        }

        /// <summary>
        /// Updates each bat.
        /// </summary>
        private void UpdateBats(GameTime gameTime)
        {
            foreach (Bat bat in bats)
            {
                bat.Update(gameTime);
            }
        }

        /// <summary>
        /// Animates each pillar item and checks if the player can collect them.
        /// </summary>
        private void UpdatePillarItems(GameTime gameTime)
        {
            for (int i = 0; i < pillarItems.Count; ++i)
            {
                PillarItem item = pillarItems[i];

                item.Update(gameTime);

                // Check if player is near and not already collected
                if (!item.IsCollected && item.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    // Note: We don't automatically collect the item - player needs to press E
                    // This will be handled in Player.cs or a separate interaction system
                }
            }
        }

        /// <summary>
        /// Called when a gem is collected.
        /// </summary>
        /// <param name="gem">The gem that was collected.</param>
        /// <param name="collectedBy">The player who collected this gem.</param>
        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            score += gem.PointValue;

            gem.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        /// <summary>
        /// Tries to collect a pillar item near the player if E is pressed.
        /// </summary>
        public PillarItem TryCollectPillarItem(Rectangle playerBounds)
        {
            for (int i = 0; i < pillarItems.Count; ++i)
            {
                PillarItem item = pillarItems[i];
                if (!item.IsCollected && item.BoundingCircle.Intersects(playerBounds))
                {
                    item.OnCollected(Player);
                    return item;
                }
            }
            return null;
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i <= EntityLayer; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);

            DrawTiles(spriteBatch);

            foreach (Gem gem in gems)
                gem.Draw(gameTime, spriteBatch);

            foreach (PillarItem item in pillarItems)
                item.Draw(gameTime, spriteBatch);

            foreach (Cart cart in carts)
                cart.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            foreach (Bat bat in bats)
                bat.Draw(gameTime, spriteBatch);

            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        #endregion
    }
}