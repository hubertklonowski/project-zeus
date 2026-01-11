using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Levels;
using ProjectZeus.Core.Levels.ZeusFight;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Extensions;
using MonoGame.Aseprite;

namespace ProjectZeus.Core
{
    public class ZeusFightScene
    {
        public bool IsCompleted { get; private set; }
        public bool ShouldRestartGame { get; private set; }
        public bool PlayerTransformedToGoat => gameStateManager.PlayerTransformedToGoat;
        public bool ShouldStartMountainAsGoat { get; private set; }
        public bool ShouldShowCredits => gameStateManager.VictoryAchieved;

        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        private Texture2D solidTexture;
        private SpriteFont titleFont;
        private AsepriteSprite zeusSprite;
        private AsepriteSprite goatSprite;
        private AsepriteSprite grapesSprite;
        
        private Vector2 zeusPosition;
        private Vector2 sacrificePillarPosition;
        private Vector2 sacrificePillarSize = new Vector2(80, 100);
        private SpriteEffects goatFlip = SpriteEffects.None;
        
        private Vector2 zeusVelocity = Vector2.Zero;
        private bool zeusIsJumping = false;
        private bool zeusHasLandedOnPlayer = false;
        private Vector2 zeusJumpStartPosition;
        
        private KeyboardState previousKeyState;

        private ZeusRenderer renderer;
        private ZeusPhysicsController physicsController;
        private ZeusGameStateManager gameStateManager;
        private ZeusItemManager itemManager;

        public ZeusFightScene()
        {
            IsCompleted = false;
            ShouldRestartGame = false;
            physicsController = new ZeusPhysicsController();
            gameStateManager = new ZeusGameStateManager();
            itemManager = new ZeusItemManager();
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            solidTexture = new Texture2D(graphicsDevice, 1, 1);
            solidTexture.SetData(new[] { Color.White });
            titleFont = font;
            
            zeusSprite = AsepriteSprite.Load(graphicsDevice, "Content/Sprites/zus.aseprite");
            goatSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Goat);
            grapesSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Grapes);
            
            float groundTop = baseScreenSize.Y * 0.7f;
            Vector2 zeusSize = zeusSprite?.IsLoaded == true ? zeusSprite.Size : new Vector2(80, 120);
            zeusPosition = new Vector2(40f, groundTop - zeusSize.Y);
            sacrificePillarPosition = new Vector2(baseScreenSize.X / 2f - sacrificePillarSize.X / 2f, groundTop - sacrificePillarSize.Y);
            
            renderer = new ZeusRenderer(solidTexture, titleFont, zeusSprite, goatSprite, grapesSprite, baseScreenSize);
        }

        public (Vector2 velocity, bool isOnGround, bool transformedToGoat) Update(GameTime gameTime, KeyboardState keyboardState, Vector2 playerPosition, Vector2 playerSize, Vector2 playerVelocity, bool playerIsOnGround)
        {
            if (gameStateManager.VictoryAchieved)
            {
                IsCompleted = true;
                return (playerVelocity, playerIsOnGround, gameStateManager.PlayerTransformedToGoat);
            }
  
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float groundTop = baseScreenSize.Y * 0.7f;
              
            if (gameStateManager.ZeusStomp)
            {
                gameStateManager.UpdateStompAnimation(dt);

                if (!zeusIsJumping && !zeusHasLandedOnPlayer)
                {
                    physicsController.InitiateZeusJump(ref zeusPosition, ref zeusVelocity, ref zeusIsJumping,
                        ref zeusJumpStartPosition, playerPosition);
                }

                if (zeusIsJumping)
                {
                    physicsController.UpdateZeusJump(ref zeusPosition, ref zeusVelocity, ref zeusIsJumping,
                        ref zeusHasLandedOnPlayer, zeusJumpStartPosition, playerPosition, playerSize, groundTop, dt);
                    
                    if (zeusHasLandedOnPlayer)
                    {
                        IsCompleted = true;
                        ShouldStartMountainAsGoat = true;
                    }
                }

                Vector2 zeusSize = zeusSprite?.IsLoaded == true ? zeusSprite.Size : new Vector2(80, 120);
                zeusPosition.X = MathHelper.Clamp(zeusPosition.X, 0f, baseScreenSize.X - zeusSize.X);
            }
            
            gameStateManager.UpdateTimer(dt);
            
            float move = keyboardState.GetHorizontalInput();
            playerVelocity = new Vector2(move * 180f, playerVelocity.Y);
            
            if (playerVelocity.X < 0)
                goatFlip = SpriteEffects.FlipHorizontally;
            else if (playerVelocity.X > 0)
                goatFlip = SpriteEffects.None;

            if (playerIsOnGround && keyboardState.IsJumpPressed())
            {
                playerVelocity = new Vector2(playerVelocity.X, -560f);
                playerIsOnGround = false;
            }

            playerVelocity = new Vector2(playerVelocity.X, playerVelocity.Y + 900f * dt);
            
            if (!gameStateManager.PlayerTransformedToGoat)
            {
                bool qKeyPressed = keyboardState.IsKeyDown(Keys.Q) && !previousKeyState.IsKeyDown(Keys.Q);
                bool eKeyPressed = keyboardState.IsKeyDown(Keys.E) && !previousKeyState.IsKeyDown(Keys.E);
                
                if (qKeyPressed) itemManager.CyclePreviousItem();
                if (eKeyPressed) itemManager.CycleNextItem();
                
                bool confirmPressed = keyboardState.IsKeyDown(Keys.Enter) && !previousKeyState.IsKeyDown(Keys.Enter);
                if (confirmPressed && itemManager.CurrentPlacedItem != PillarItemType.None)
                {
                    gameStateManager.ValidateSacrifice(itemManager.CurrentPlacedItem, itemManager);
                }
            }
            
            previousKeyState = keyboardState;
            
            return (playerVelocity, playerIsOnGround, gameStateManager.PlayerTransformedToGoat);
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, AdonisPlayer player, GameTime gameTime, bool playerIsGoat)
        {
            graphicsDevice.Clear(new Color(20, 30, 80));
            if (solidTexture == null) return;

            spriteBatch.Begin();

            Rectangle skyRect = new Rectangle(0, 0, (int)baseScreenSize.X, (int)(baseScreenSize.Y * 0.7f));
            spriteBatch.Draw(solidTexture, skyRect, new Color(40, 70, 140));
            Rectangle groundRect = new Rectangle(0, (int)(baseScreenSize.Y * 0.7f), (int)baseScreenSize.X, (int)(baseScreenSize.Y * 0.3f));
            spriteBatch.Draw(solidTexture, groundRect, new Color(60, 50, 40));
            
            renderer.DrawSacrificePillar(spriteBatch, gameTime, sacrificePillarPosition, sacrificePillarSize, itemManager.CurrentPlacedItem);

            if (zeusSprite != null && zeusSprite.IsLoaded)
            {
                bool isMoving = zeusIsJumping || Math.Abs(zeusVelocity.X) > 0;
                zeusSprite.Draw(spriteBatch, zeusPosition, isMoving, gameTime, Color.White, 10f, SpriteEffects.None);
            }
            
            if (!gameStateManager.VictoryAchieved && !gameStateManager.ZeusStomp && !gameStateManager.PlayerTransformedToGoat && gameStateManager.CurrentDialogueIndex < gameStateManager.DialogueMappings.Length)
            {
                renderer.DrawDialogueBubble(spriteBatch, gameStateManager.DialogueMappings[gameStateManager.CurrentDialogueIndex].Dialogue, zeusPosition);
            }

            if (playerIsGoat && goatSprite != null && goatSprite.IsLoaded)
            {
                goatSprite.Draw(spriteBatch, player.Position, false, gameTime, Color.White, 10f, goatFlip);
            }
            else
            {
                player.Draw(gameTime, spriteBatch);
            }
            
            renderer.DrawUI(spriteBatch, gameStateManager.VictoryAchieved, gameStateManager.PlayerTransformedToGoat, 
                gameStateManager.ZeusStomp, itemManager.CurrentPlacedItem, gameStateManager.TimerActive, gameStateManager.RemainingTime);

            spriteBatch.End();
        }
    }
}
