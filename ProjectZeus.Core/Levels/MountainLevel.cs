using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Levels;
using ProjectZeus.Core.Levels.Mountain;
using ProjectZeus.Core.Utilities;
using ProjectZeus.Core.Extensions;

namespace ProjectZeus.Core
{
    public class MountainLevel
    {
        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        private float worldHeight;
        
        private CameraController camera;
        private Texture2D solidTexture;
        private AsepriteSprite goatSprite;
        private AsepriteSprite rockSprite;
        private SpriteFont font;
        
        private static readonly Random random = new Random();
        private const float ThrowAngleVariation = 40f;
        private const float ThrowAngleOffset = 20f;
        private const int CollisionTopOffset = 2;
        private const int CollisionHeight = 6;
        private const int CollisionVerticalThreshold = 20;
        
        private List<Platform> platforms;
        private List<MovingPlatform> movingPlatforms;
        
        private Vector2 goatPosition;
        private readonly Vector2 goatSize = new Vector2(40, 40);
        private float goatThrowTimer;
        private const float GoatThrowInterval = 1f;
        private Vector2 goatVelocity;
        private const float GoatMoveSpeed = 70f;
        private Rectangle topPlatformBounds;
        
        private List<Rock> rocks;
        private float playerRockThrowCooldown = 0f;
        private const float PlayerRockThrowDelay = 0.5f;
        
        private float goatTimer = 0f;
        private const float GoatCreditsTime = 60f;
        
        private Vector2 itemPosition;
        private readonly Vector2 itemSize = new Vector2(30, 30);
        private bool itemCollected;

        private MountainRenderer renderer;
        private MountainEntityUpdater entityUpdater;

        public bool PlayerIsGoat { get; set; }
        public bool ShouldShowCredits { get; private set; }
        public bool PlayerDied { get; private set; }
        public bool ItemWasCollected { get; private set; }
        public float WorldHeight => worldHeight;
        public Vector2 CameraOffset => camera.CameraOffset;
        
        public MountainLevel()
        {
            platforms = new List<Platform>();
            movingPlatforms = new List<MovingPlatform>();
            rocks = new List<Rock>();
            itemCollected = false;
            PlayerDied = false;
            ItemWasCollected = false;
            ShouldShowCredits = false;
            entityUpdater = new MountainEntityUpdater(random);
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            solidTexture = new Texture2D(graphicsDevice, 1, 1);
            solidTexture.SetData(new[] { Color.White });
            
            this.font = font;
            goatSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Goat);
            rockSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Rock);
            
            SetupMountain();
            
            camera = new CameraController(baseScreenSize.X, baseScreenSize.Y, baseScreenSize.X, worldHeight);
            camera.SetToBottom();
            
            renderer = new MountainRenderer(solidTexture, goatSprite, rockSprite, font, baseScreenSize);
        }
        
        private void SetupMountain()
        {
            worldHeight = MountainPlatformBuilder.WorldHeight;
            var (staticPlatforms, movingPlats) = MountainPlatformBuilder.BuildPlatforms(baseScreenSize);
            platforms = staticPlatforms;
            movingPlatforms = movingPlats;
            
            float topPlatformY = MountainPlatformBuilder.GetTopPlatformY();
            float topPlatformWidth = 400f;
            float topPlatformX = baseScreenSize.X / 2f - topPlatformWidth / 2f;
            
            topPlatformBounds = new Rectangle((int)topPlatformX, (int)topPlatformY, (int)topPlatformWidth, 15);
            goatPosition = new Vector2(topPlatformX + 50f, topPlatformY - goatSize.Y);
            goatVelocity = new Vector2(GoatMoveSpeed, 0f);
            goatThrowTimer = GoatThrowInterval;
            itemPosition = new Vector2(topPlatformX + topPlatformWidth - 60f, topPlatformY - itemSize.Y);
        }
        
        public void UpdateCamera(Vector2 playerPosition)
        {
            camera.FollowPlayerVertical(playerPosition);
        }
        
        public Vector2 GetPlayerSpawnPosition(Vector2 playerSize)
        {
            float groundTop = worldHeight - 20;
            return new Vector2(60f, groundTop - playerSize.Y);
        }
        
        public Vector2 GetGoatSpawnPosition(Vector2 playerSize)
        {
            float x = goatPosition.X;
            float y = goatPosition.Y + (goatSize.Y - playerSize.Y);
            return new Vector2(x, y);
        }
        
        public void Update(GameTime gameTime, Vector2 playerPosition, Vector2 playerSize, bool tryPickupItem)
        {
            Update(gameTime, playerPosition, playerSize, tryPickupItem, null);
        }
        
        public void Update(GameTime gameTime, Vector2 playerPosition, Vector2 playerSize, bool tryPickupItem, KeyboardState? keyboardState)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            UpdateCamera(playerPosition);
            
            if (PlayerIsGoat)
            {
                goatTimer += dt;
                if (goatTimer >= GoatCreditsTime)
                {
                    ShouldShowCredits = true;
                }
            }
            
            entityUpdater.UpdateMovingPlatforms(movingPlatforms, dt);
            
            if (playerRockThrowCooldown > 0)
            {
                playerRockThrowCooldown -= dt;
            }
            
            if (!PlayerIsGoat)
            {
                entityUpdater.UpdateGoat(ref goatPosition, ref goatVelocity, ref goatThrowTimer, topPlatformBounds, 
                    dt, GoatMoveSpeed, GoatThrowInterval, rocks, rockSprite, goatSize);
            }
            else if (keyboardState.HasValue)
            {
                entityUpdater.UpdatePlayerGoat(keyboardState.Value, ref playerRockThrowCooldown, dt, 
                    playerPosition, playerSize, rocks, rockSprite, PlayerRockThrowDelay);
            }
            
            entityUpdater.UpdateRocks(rocks, dt, worldHeight);
            
            if (!PlayerIsGoat)
            {
                foreach (var rock in rocks)
                {
                    if (CollisionExtensions.CheckEntityCollision(playerPosition, playerSize, rock.Position, rock.Size))
                    {
                        PlayerDied = true;
                        break;
                    }
                }
            }
            
            if (!PlayerIsGoat && !itemCollected && tryPickupItem)
            {
                Rectangle playerRect = playerPosition.ToRectangle(playerSize);
                Rectangle itemRect = itemPosition.ToRectangle(itemSize);
                
                if (CollisionExtensions.CheckPickupCollision(playerRect, itemRect))
                {
                    itemCollected = true;
                    ItemWasCollected = true;
                }
            }
        }
        
        public bool CheckPlatformCollision(Rectangle playerRect, Vector2 playerVelocity, out Vector2 correctedPosition)
        {
            correctedPosition = new Vector2(playerRect.X, playerRect.Y);
            bool onPlatform = false;
            
            var allPlatforms = platforms.Cast<object>().Concat(movingPlatforms.Cast<object>());
            foreach (var p in allPlatforms)
            {
                Vector2 pos = p is Platform plt ? plt.Position : ((MovingPlatform)p).Position;
                Vector2 size = p is Platform plat ? plat.Size : ((MovingPlatform)p).Size;
                Rectangle platformRect = pos.ToRectangle(size);
                Rectangle topRect = new Rectangle(platformRect.X, platformRect.Y - CollisionTopOffset, platformRect.Width, CollisionHeight);
                
                if (playerRect.Bottom > topRect.Top && playerRect.Bottom <= topRect.Top + CollisionVerticalThreshold &&
                    playerRect.Right > topRect.Left && playerRect.Left < topRect.Right && playerVelocity.Y >= 0)
                {
                    correctedPosition.Y = topRect.Top - playerRect.Height;
                    onPlatform = true;
                }
            }
            
            return onPlatform;
        }
        
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            graphicsDevice.Clear(new Color(135, 206, 235));
            if (solidTexture == null) return;
            
            Matrix cameraTransform = camera.GetTransform();
            spriteBatch.Begin(transformMatrix: cameraTransform);
            
            renderer.DrawBackground(spriteBatch, camera.CameraOffset, worldHeight);
            
            Color mountainColor = new Color(160, 140, 120);
            int strips = 100;
            float baseWidth = baseScreenSize.X * 0.9f;
            float startX = baseScreenSize.X * 0.05f;
            float bottomY = worldHeight - 20;
            float topY = 20;
            for (int i = 0; i < strips; i++)
            {
                float t = (float)i / strips;
                float y = MathHelper.Lerp(bottomY, topY, t);
                float width = baseWidth * (1 - t * 0.8f);
                float x = startX + (baseWidth - width) / 2f;
                float stripHeight = (bottomY - topY) / strips + 2;
                spriteBatch.Draw(solidTexture, new Rectangle((int)x, (int)y, (int)width, (int)stripHeight), mountainColor * (0.2f + t * 0.4f));
            }
            
            renderer.DrawPlatforms(spriteBatch, platforms, movingPlatforms);
            if (!PlayerIsGoat) renderer.DrawGoat(spriteBatch, gameTime, goatPosition, goatVelocity, goatSize, PlayerIsGoat);
            renderer.DrawRocks(spriteBatch, gameTime, rocks);
            renderer.DrawItem(spriteBatch, itemPosition, itemSize, itemCollected);
            
            spriteBatch.End();
            
            spriteBatch.Begin();
            renderer.DrawUI(spriteBatch, itemCollected, PlayerIsGoat);
            spriteBatch.End();
        }
        
        public Matrix GetCameraTransform() => camera.GetTransform();
        
        public void Reset()
        {
            rocks.Clear();
            itemCollected = false;
            ItemWasCollected = false;
            PlayerDied = false;
            goatThrowTimer = GoatThrowInterval;
            playerRockThrowCooldown = 0f;
            goatTimer = 0f;
            ShouldShowCredits = false;
            
            if (topPlatformBounds.Width > 0)
            {
                goatPosition = new Vector2(topPlatformBounds.X + 50f, topPlatformBounds.Y - goatSize.Y);
                goatVelocity = new Vector2(GoatMoveSpeed, 0f);
            }

            PlayerIsGoat = false;
            
            foreach (var movingPlatform in movingPlatforms)
            {
                movingPlatform.Position = movingPlatform.StartPosition;
                movingPlatform.Progress = 0f;
                movingPlatform.MovingToEnd = true;
            }
            
            camera.SetToBottom();
        }

        public AsepriteSprite GetGoatSprite()
        {
            return goatSprite;
        }
    }
}
