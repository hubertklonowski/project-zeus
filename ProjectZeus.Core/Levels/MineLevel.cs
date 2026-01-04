using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Physics;
using ProjectZeus.Core.Rendering;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Mine level with platforms, carts, bats, and collectible item
    /// </summary>
    public class MineLevel
    {
        private Vector2 playerPosition;
        private Vector2 playerVelocity;
        private bool playerOnGround;
        
        private List<Rectangle> platforms;
        private List<Rectangle> rails;
        private List<MineCart> carts;
        private List<MineBat> bats;
        private List<Rectangle> stalactites;
        private List<Rectangle> torches;
        
        private Rectangle exitRect;
        private Rectangle itemRect;
        private bool itemCollected;
        
        private Texture2D platformTexture;
        private Texture2D backgroundTexture;
        private SpriteFont font;
        
        private AsepriteSprite batSprite;
        private AsepriteSprite cartSprite;
        
        private Random random;

        public bool IsActive { get; private set; }
        public bool HasCollectedItem { get; private set; }
        public Vector2 PlayerPosition => playerPosition;
        public Vector2 PlayerVelocity => playerVelocity;

        public MineLevel()
        {
            platforms = new List<Rectangle>();
            rails = new List<Rectangle>();
            carts = new List<MineCart>();
            bats = new List<MineBat>();
            stalactites = new List<Rectangle>();
            torches = new List<Rectangle>();
            random = new Random();
            IsActive = false;
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            this.font = font;
            platformTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, new Color(100, 100, 100));
            backgroundTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, new Color(20, 15, 30));
            
            // Load sprites for bat and cart
            var batSprite = AsepriteSprite.Load(graphicsDevice, "Content/Sprites/bat.aseprite");
            var cartSprite = AsepriteSprite.Load(graphicsDevice, "Content/Sprites/cart.aseprite");
            
            // Store sprites for later assignment to entities
            this.batSprite = batSprite;
            this.cartSprite = cartSprite;
        }

        public void Enter()
        {
            IsActive = true;
            itemCollected = false;
            HasCollectedItem = false;
            
            playerPosition = new Vector2(50f, GameConstants.BaseScreenSize.Y - GameConstants.PlayerSize.Y - 30f);
            playerVelocity = Vector2.Zero;
            playerOnGround = false;

            SetupMineStructure();
        }

        private void SetupMineStructure()
        {
            platforms.Clear();
            rails.Clear();
            carts.Clear();
            bats.Clear();
            stalactites.Clear();
            torches.Clear();

            platforms.Add(new Rectangle(0, (int)(GameConstants.BaseScreenSize.Y - GameConstants.GroundHeight), 
                (int)GameConstants.BaseScreenSize.X, (int)GameConstants.GroundHeight));
            platforms.Add(new Rectangle(50, (int)(GameConstants.BaseScreenSize.Y - 120), 180, 15));
            platforms.Add(new Rectangle(250, (int)(GameConstants.BaseScreenSize.Y - 180), 120, 15));
            platforms.Add(new Rectangle(400, (int)(GameConstants.BaseScreenSize.Y - 240), 150, 15));
            platforms.Add(new Rectangle(180, (int)(GameConstants.BaseScreenSize.Y - 280), 100, 15));
            platforms.Add(new Rectangle(320, (int)(GameConstants.BaseScreenSize.Y - 340), 160, 15));
            platforms.Add(new Rectangle(550, (int)(GameConstants.BaseScreenSize.Y - 200), 180, 15));
            platforms.Add(new Rectangle(600, (int)(GameConstants.BaseScreenSize.Y - 100), 150, 15));

            rails.Add(new Rectangle(50, (int)(GameConstants.BaseScreenSize.Y - 125), 180, 5));
            rails.Add(new Rectangle(550, (int)(GameConstants.BaseScreenSize.Y - 205), 180, 5));

            carts.Add(new MineCart
            {
                Position = new Vector2(100, GameConstants.BaseScreenSize.Y - 140),
                Velocity = new Vector2(40, 0),
                MinX = 60,
                MaxX = 220,
                Sprite = cartSprite
            });

            carts.Add(new MineCart
            {
                Position = new Vector2(600, GameConstants.BaseScreenSize.Y - 220),
                Velocity = new Vector2(-50, 0),
                MinX = 560,
                MaxX = 720,
                Sprite = cartSprite
            });

            for (int i = 0; i < 4; i++)
            {
                bats.Add(new MineBat
                {
                    Position = new Vector2(200 + i * 150, 150 + i * 40),
                    Velocity = new Vector2((float)(random.NextDouble() * 2 - 1) * 60f, (float)(random.NextDouble() * 2 - 1) * 60f),
                    ChangeDirectionTimer = (float)random.NextDouble() * 2f,
                    Sprite = batSprite
                });
            }

            for (int x = 100; x < 700; x += 80)
            {
                int height = 30 + random.Next(30);
                stalactites.Add(new Rectangle(x, 0, 20, height));
            }

            torches.Add(new Rectangle(30, (int)(GameConstants.BaseScreenSize.Y - 50), 15, 25));
            torches.Add(new Rectangle(230, (int)(GameConstants.BaseScreenSize.Y - 210), 15, 25));
            torches.Add(new Rectangle(580, (int)(GameConstants.BaseScreenSize.Y - 230), 15, 25));
            torches.Add(new Rectangle(730, (int)(GameConstants.BaseScreenSize.Y - 130), 15, 25));

            itemRect = new Rectangle(350, (int)(GameConstants.BaseScreenSize.Y - 280), 30, 30);
            exitRect = new Rectangle(670, (int)(GameConstants.BaseScreenSize.Y - 150), 50, 50);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, KeyboardState previousKeyboardState)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;

            playerVelocity.X = move * GameConstants.MoveSpeed;

            if (playerOnGround && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up)))
            {
                playerVelocity.Y = GameConstants.JumpVelocity;
                playerOnGround = false;
            }

            playerVelocity.Y += GameConstants.Gravity * deltaTime;
            playerPosition += playerVelocity * deltaTime;

            playerOnGround = false;
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 
                (int)GameConstants.PlayerSize.X, (int)GameConstants.PlayerSize.Y);

            foreach (Rectangle platform in platforms)
            {
                Vector2 correctedPos;
                if (PlatformerPhysics.CheckPlatformCollision(playerRect, platform, playerVelocity, out correctedPos))
                {
                    playerPosition.Y = correctedPos.Y;
                    playerVelocity.Y = 0f;
                    playerOnGround = true;
                    playerRect.Y = (int)playerPosition.Y;
                }
            }

            PlatformerPhysics.ClampToScreen(ref playerPosition, GameConstants.PlayerSize);

            foreach (var cart in carts)
            {
                cart.Update(deltaTime);
            }

            foreach (var bat in bats)
            {
                bat.Update(deltaTime, random);
            }

            if (!itemCollected && keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E))
            {
                playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 
                    (int)GameConstants.PlayerSize.X, (int)GameConstants.PlayerSize.Y);
                if (playerRect.Intersects(itemRect))
                {
                    itemCollected = true;
                    HasCollectedItem = true;
                }
            }

            playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 
                (int)GameConstants.PlayerSize.X, (int)GameConstants.PlayerSize.Y);
            if (playerRect.Intersects(exitRect))
            {
                IsActive = false;
            }

            if (playerPosition.Y > GameConstants.BaseScreenSize.Y)
            {
                playerPosition = new Vector2(50f, GameConstants.BaseScreenSize.Y - GameConstants.PlayerSize.Y - 30f);
                playerVelocity = Vector2.Zero;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime, 
            AdonisPlayer player, Texture2D portalTexture)
        {
            if (!IsActive) return;

            graphicsDevice.Clear(new Color(20, 15, 30));

            Rectangle bgRect = new Rectangle(0, 0, (int)GameConstants.BaseScreenSize.X, (int)GameConstants.BaseScreenSize.Y);
            spriteBatch.Draw(backgroundTexture, bgRect, Color.White);

            foreach (Rectangle stalactite in stalactites)
            {
                spriteBatch.Draw(platformTexture, stalactite, new Color(120, 120, 120));
            }

            foreach (Rectangle platform in platforms)
            {
                spriteBatch.Draw(platformTexture, platform, new Color(80, 70, 60));
            }

            foreach (Rectangle rail in rails)
            {
                spriteBatch.Draw(platformTexture, rail, new Color(140, 140, 140));
                for (int x = rail.Left; x < rail.Right; x += 15)
                {
                    Rectangle tie = new Rectangle(x, rail.Top - 3, 3, 8);
                    spriteBatch.Draw(platformTexture, tie, new Color(100, 70, 50));
                }
            }

            foreach (var cart in carts)
            {
                cart.Draw(spriteBatch, platformTexture, gameTime);
            }

            float flickerTime = (float)gameTime.TotalGameTime.TotalSeconds;
            foreach (Rectangle torch in torches)
            {
                spriteBatch.Draw(platformTexture, torch, new Color(101, 67, 33));
                float flicker = (float)Math.Sin(flickerTime * 8f) * 0.2f + 0.8f;
                Rectangle flame = new Rectangle(torch.X - 5, torch.Y - 15, torch.Width + 10, 15);
                spriteBatch.Draw(platformTexture, flame, new Color((byte)(255 * flicker), (byte)(200 * flicker), 50));
            }

            if (!itemCollected)
            {
                spriteBatch.Draw(platformTexture, itemRect, Color.Gold);
                Rectangle glowRect = itemRect;
                glowRect.Inflate(5, 5);
                spriteBatch.Draw(platformTexture, glowRect, new Color((byte)255, (byte)215, (byte)0, (byte)100));
            }

            foreach (var bat in bats)
            {
                bat.Draw(spriteBatch, platformTexture, gameTime);
            }

            DrawingHelpers.DrawPortal(spriteBatch, portalTexture, exitRect, gameTime, new Color(100, 50, 200));

            player.Draw(gameTime, spriteBatch);

            if (itemCollected)
            {
                string hasItem = "Item collected! Go to exit portal!";
                Vector2 hasItemSize = font.MeasureString(hasItem);
                Vector2 hasItemPos = new Vector2((GameConstants.BaseScreenSize.X - hasItemSize.X) / 2f, 40f);
                spriteBatch.DrawString(font, hasItem, hasItemPos, Color.LightGreen);
            }
        }
    }
}
