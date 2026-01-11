using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Levels;
using MonoGame.Aseprite;

namespace ProjectZeus.Core
{
    /// <summary>
    /// Zeus fight scene with the boss character
    /// </summary>
    public class ZeusFightScene
    {
        public bool IsCompleted { get; private set; }
        public bool ShouldRestartGame { get; private set; }

        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        private Texture2D solidTexture;
        private SpriteFont titleFont;
        private AsepriteSprite zeusSprite;
        
        private Vector2 zeusPosition;
        private Vector2 sacrificePillarPosition;
        private Vector2 sacrificePillarSize = new Vector2(80, 100);
        
        // Zeus dialogue state
        private string[] zeusDialogueOptions = { "κρι-κρι", "χρυσός του Μίδα", "κρασί του Διονύσου" };
        private PillarItemType[] expectedItems = { PillarItemType.Mountain, PillarItemType.Mine, PillarItemType.Maze };
        private int currentDialogueIndex = 0;
        private PillarItemType currentPlacedItem = PillarItemType.None;
        private int correctAnswers = 0;
        private bool waitingForItem = true;
        
        // Zeus animation state
        private bool zeusAngry = false;
        private float angryAnimationTime = 0f;
        private const float stompDuration = 2f;
        
        // Victory state
        private bool victoryAchieved = false;
        
        private KeyboardState previousKeyState;

        public ZeusFightScene()
        {
            IsCompleted = false;
            ShouldRestartGame = false;
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            // Create a 1x1 solid texture for simple rectangles.
            solidTexture = new Texture2D(graphicsDevice, 1, 1);
            solidTexture.SetData(new[] { Color.White });

            titleFont = font;
            
            // Load Zeus sprite
            zeusSprite = AsepriteSprite.Load(graphicsDevice, "Content/Sprites/zus.aseprite");
            
            // Position Zeus on the left side of the screen, standing on ground
            float groundTop = baseScreenSize.Y * 0.7f; // This is where ground starts (y = 336)
            float zeusMarginFromLeft = 40f;
            
            // Use actual sprite size if loaded, otherwise use fallback dimensions
            Vector2 zeusSize = zeusSprite?.IsLoaded == true ? zeusSprite.Size : new Vector2(80, 120);
            
            // Zeus should be positioned so his bottom is at groundTop
            zeusPosition = new Vector2(zeusMarginFromLeft, groundTop - zeusSize.Y);
            
            // Position sacrifice pillar in the center
            float centerX = baseScreenSize.X / 2f;
            sacrificePillarPosition = new Vector2(centerX - sacrificePillarSize.X / 2f, groundTop - sacrificePillarSize.Y);
            
            // Randomize the order of dialogues
            ShuffleDialogues();
        }
        
        private void ShuffleDialogues()
        {
            Random rng = new Random();
            int n = zeusDialogueOptions.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                // Swap dialogues
                string tempDialogue = zeusDialogueOptions[k];
                zeusDialogueOptions[k] = zeusDialogueOptions[n];
                zeusDialogueOptions[n] = tempDialogue;
                // Swap expected items
                PillarItemType tempItem = expectedItems[k];
                expectedItems[k] = expectedItems[n];
                expectedItems[n] = tempItem;
            }
        }

        public (Vector2 velocity, bool isOnGround) Update(GameTime gameTime, KeyboardState keyboardState, Vector2 playerPosition, Vector2 playerSize, Vector2 playerVelocity, bool playerIsOnGround)
        {
            if (victoryAchieved)
            {
                IsCompleted = true;
                return (playerVelocity, playerIsOnGround);
            }
            
            if (zeusAngry)
            {
                angryAnimationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (angryAnimationTime >= stompDuration)
                {
                    ShouldRestartGame = true;
                }
                return (playerVelocity, playerIsOnGround);
            }
            
            // Apply physics to player
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float groundTop = baseScreenSize.Y * 0.7f;
            
            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;

            playerVelocity = new Vector2(move * 180f, playerVelocity.Y);

            if (playerIsOnGround && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up)))
            {
                playerVelocity = new Vector2(playerVelocity.X, -560f);
                playerIsOnGround = false;
            }

            playerVelocity = new Vector2(playerVelocity.X, playerVelocity.Y + 900f * dt);
            
            // Check if waiting for item placement
            if (waitingForItem)
            {
                bool eKeyPressed = keyboardState.IsKeyDown(Keys.E) && !previousKeyState.IsKeyDown(Keys.E);
                
                if (eKeyPressed)
                {
                    // Check if player is near sacrifice pillar
                    Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)playerSize.X, (int)playerSize.Y);
                    Rectangle pillarRect = new Rectangle((int)sacrificePillarPosition.X, (int)sacrificePillarPosition.Y, 
                        (int)sacrificePillarSize.X, (int)sacrificePillarSize.Y);
                    pillarRect.Inflate(30, 30);
                    
                    if (playerRect.Intersects(pillarRect))
                    {
                        // Cycle through items: None -> Mountain -> Mine -> Maze -> None
                        if (currentPlacedItem == PillarItemType.None)
                            currentPlacedItem = PillarItemType.Mountain;
                        else if (currentPlacedItem == PillarItemType.Mountain)
                            currentPlacedItem = PillarItemType.Mine;
                        else if (currentPlacedItem == PillarItemType.Mine)
                            currentPlacedItem = PillarItemType.Maze;
                        else
                            currentPlacedItem = PillarItemType.None;
                    }
                }
                
                // Check if player confirms the item (press E away from pillar or second E press)
                bool confirmPressed = keyboardState.IsKeyDown(Keys.Enter) && !previousKeyState.IsKeyDown(Keys.Enter);
                
                if (confirmPressed && currentPlacedItem != PillarItemType.None)
                {
                    ValidateSacrifice();
                }
            }
            
            previousKeyState = keyboardState;
            
            return (playerVelocity, playerIsOnGround);
        }
        
        private void ValidateSacrifice()
        {
            PillarItemType expectedItem = expectedItems[currentDialogueIndex];
            
            if (currentPlacedItem == expectedItem)
            {
                // Correct item!
                correctAnswers++;
                currentPlacedItem = PillarItemType.None;
                
                if (correctAnswers >= 3)
                {
                    // Victory!
                    victoryAchieved = true;
                }
                else
                {
                    // Move to next dialogue
                    currentDialogueIndex++;
                    waitingForItem = true;
                }
            }
            else
            {
                // Wrong item - Zeus gets angry
                zeusAngry = true;
                angryAnimationTime = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, AdonisPlayer player, GameTime gameTime)
        {
            graphicsDevice.Clear(new Color(20, 30, 80));

            if (solidTexture == null)
                return;

            spriteBatch.Begin();

            Rectangle skyRect = new Rectangle(0, 0, (int)baseScreenSize.X, (int)(baseScreenSize.Y * 0.7f));
            spriteBatch.Draw(solidTexture, skyRect, new Color(40, 70, 140));

            Rectangle groundRect = new Rectangle(0, (int)(baseScreenSize.Y * 0.7f), (int)baseScreenSize.X, (int)(baseScreenSize.Y * 0.3f));
            spriteBatch.Draw(solidTexture, groundRect, new Color(60, 50, 40));
            
            // Draw sacrifice pillar with spotlight
            DrawSacrificePillar(spriteBatch, gameTime);

            // Draw Zeus using sprite or fallback
            if (zeusSprite != null && zeusSprite.IsLoaded)
            {
                // Zeus is moving if angry (stomping)
                bool isMoving = zeusAngry;
                Color zeusColor = zeusAngry ? Color.Red : Color.White;
                zeusSprite.Draw(spriteBatch, zeusPosition, isMoving, gameTime, zeusColor, 10f, SpriteEffects.None);
            }
            else
            {
                // Fallback rendering - use actual sprite size if available
                Vector2 zeusSize = zeusSprite?.IsLoaded == true ? zeusSprite.Size : new Vector2(80, 120);
                Rectangle zeusRect = new Rectangle(
                    (int)zeusPosition.X,
                    (int)zeusPosition.Y,
                    (int)zeusSize.X,
                    (int)zeusSize.Y);
                Color zeusColor = zeusAngry ? Color.Red : new Color(220, 220, 240);
                spriteBatch.Draw(solidTexture, zeusRect, zeusColor);
            }
            
            // Draw Zeus dialogue bubble
            if (!victoryAchieved && !zeusAngry && currentDialogueIndex < zeusDialogueOptions.Length)
            {
                DrawDialogueBubble(spriteBatch, zeusDialogueOptions[currentDialogueIndex]);
            }

            player.Draw(gameTime, spriteBatch);
            
            // Draw UI
            DrawUI(spriteBatch);

            spriteBatch.End();
        }
        
        private void DrawSacrificePillar(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Draw light beam from top
            float pulseAmount = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2f) * 0.3f + 0.7f;
            Color lightColor = new Color(255, 255, 200, (int)(150 * pulseAmount));
            
            Rectangle lightBeam = new Rectangle(
                (int)(sacrificePillarPosition.X - 20),
                0,
                (int)(sacrificePillarSize.X + 40),
                (int)sacrificePillarPosition.Y);
            spriteBatch.Draw(solidTexture, lightBeam, lightColor);
            
            // Draw pillar
            Rectangle pillarRect = new Rectangle(
                (int)sacrificePillarPosition.X,
                (int)sacrificePillarPosition.Y,
                (int)sacrificePillarSize.X,
                (int)sacrificePillarSize.Y);
            spriteBatch.Draw(solidTexture, pillarRect, new Color(180, 180, 180));
            
            // Draw slot on top
            Rectangle slotRect = new Rectangle(
                (int)(sacrificePillarPosition.X + 10),
                (int)(sacrificePillarPosition.Y - 30),
                (int)(sacrificePillarSize.X - 20),
                30);
            spriteBatch.Draw(solidTexture, slotRect, new Color(200, 200, 220));
            
            // Draw current item if placed
            if (currentPlacedItem != PillarItemType.None)
            {
                Color itemColor = currentPlacedItem == PillarItemType.Mountain ? Color.Gold :
                                  currentPlacedItem == PillarItemType.Mine ? Color.DeepSkyBlue :
                                  Color.MediumVioletRed;
                Rectangle itemRect = slotRect;
                itemRect.Inflate(-5, -5);
                spriteBatch.Draw(solidTexture, itemRect, itemColor);
            }
        }
        
        private void DrawDialogueBubble(SpriteBatch spriteBatch, string dialogue)
        {
            if (titleFont == null) return;
            
            Vector2 textSize = titleFont.MeasureString(dialogue);
            Vector2 bubbleSize = textSize + new Vector2(20, 20);
            
            Vector2 bubblePosition = zeusPosition + new Vector2(0, -bubbleSize.Y - 20);
            
            // Draw bubble background
            Rectangle bubbleRect = new Rectangle(
                (int)bubblePosition.X,
                (int)bubblePosition.Y,
                (int)bubbleSize.X,
                (int)bubbleSize.Y);
            spriteBatch.Draw(solidTexture, bubbleRect, new Color(255, 255, 255, 230));
            
            // Draw bubble border
            DrawingHelpers.DrawRectangleOutline(spriteBatch, solidTexture, bubbleRect, Color.Black);
            
            // Draw text
            Vector2 textPosition = bubblePosition + new Vector2(10, 10);
            spriteBatch.DrawString(titleFont, dialogue, textPosition, Color.Black);
        }
        
        private void DrawUI(SpriteBatch spriteBatch)
        {
            if (titleFont == null) return;
            
            if (victoryAchieved)
            {
                string victoryText = "GODS ARE SATISFIED!";
                Vector2 textSize = titleFont.MeasureString(victoryText);
                Vector2 textPos = new Vector2((baseScreenSize.X - textSize.X) / 2f, baseScreenSize.Y / 2f - textSize.Y / 2f);
                spriteBatch.DrawString(titleFont, victoryText, textPos, Color.Gold);
            }
            else if (!zeusAngry)
            {
                string instruction = currentPlacedItem == PillarItemType.None 
                    ? "Press E near pillar to select item" 
                    : "Press ENTER to confirm sacrifice";
                Vector2 textSize = titleFont.MeasureString(instruction);
                Vector2 textPos = new Vector2((baseScreenSize.X - textSize.X) / 2f, 20);
                spriteBatch.DrawString(titleFont, instruction, textPos, Color.Yellow);
            }
        }
    }
}
