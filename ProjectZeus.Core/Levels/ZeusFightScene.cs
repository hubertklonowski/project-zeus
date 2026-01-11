using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Levels;
using ProjectZeus.Core.Constants;
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
        public bool PlayerTransformedToGoat { get; private set; }

        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        private Texture2D solidTexture;
        private SpriteFont titleFont;
        private AsepriteSprite zeusSprite;
        private AsepriteSprite goatSprite;
        private AsepriteSprite grapesSprite;
        
        private Vector2 zeusPosition;
        private Vector2 sacrificePillarPosition;
        private Vector2 sacrificePillarSize = new Vector2(80, 100);
        
        // Track goat facing direction
        private SpriteEffects goatFlip = SpriteEffects.None;
        
        // Zeus dialogue state - mapping of dialogue to expected item
        private struct DialogueMapping
        {
            public string Dialogue;
            public PillarItemType ExpectedItem;
            
            public DialogueMapping(string dialogue, PillarItemType expectedItem)
            {
                Dialogue = dialogue;
                ExpectedItem = expectedItem;
            }
        }
        
        private DialogueMapping[] dialogueMappings = new[]
        {
            new DialogueMapping("κρι-κρι", PillarItemType.Mountain),
            new DialogueMapping("χρυσός του Μίδα", PillarItemType.Mine),
            new DialogueMapping("κρασί του Διονύσου", PillarItemType.Maze)
        };
        
        private int currentDialogueIndex = 0;
        private PillarItemType currentPlacedItem = PillarItemType.None;
        private int correctAnswers = 0;
        
        // Timer state
        private const float timeLimit = 10f;
        private float remainingTime = timeLimit;
        private bool timerActive = false;
        
        // Zeus stomp animation state
        private bool zeusStomp = false;
        private float stompAnimationTime = 0f;
        private const float stompDuration = 2f;
        
        // Player transformation state
        private bool playerTransformedToGoat = false;
        
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
            
            // Load goat sprite for transformation
            goatSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Goat);
            
            // Load grapes sprite for maze item (matching PillarRoom)
            grapesSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Grapes);
            
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
            
            // Start timer when first dialogue shows
            timerActive = true;
            remainingTime = timeLimit;
        }
        
        private void ShuffleDialogues()
        {
            Random rng = new Random();
            int n = dialogueMappings.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                // Swap dialogue mappings
                DialogueMapping temp = dialogueMappings[k];
                dialogueMappings[k] = dialogueMappings[n];
                dialogueMappings[n] = temp;
            }
        }

        public (Vector2 velocity, bool isOnGround, bool transformedToGoat) Update(GameTime gameTime, KeyboardState keyboardState, Vector2 playerPosition, Vector2 playerSize, Vector2 playerVelocity, bool playerIsOnGround)
        {
            if (victoryAchieved)
            {
                IsCompleted = true;
                return (playerVelocity, playerIsOnGround, playerTransformedToGoat);
            }
            
            // Handle Zeus stomping after goat transformation
            if (zeusStomp)
            {
                stompAnimationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                // Don't restart game, just continue
                // Player can still move as goat
            }
            
            // Update timer only if not transformed to goat
            if (timerActive && !playerTransformedToGoat)
            {
                remainingTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (remainingTime <= 0)
                {
                    // Time's up! Wrong answer - transform to goat and Zeus stomps
                    zeusStomp = true;
                    playerTransformedToGoat = true;
                    PlayerTransformedToGoat = true;
                    stompAnimationTime = 0f;
                    timerActive = false;
                }
            }
            
            // Apply physics to player (even as goat)
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float groundTop = baseScreenSize.Y * 0.7f;
            
            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;

            playerVelocity = new Vector2(move * 180f, playerVelocity.Y);
            
            // Update goat facing direction based on movement
            if (playerVelocity.X < 0)
                goatFlip = SpriteEffects.FlipHorizontally;
            else if (playerVelocity.X > 0)
                goatFlip = SpriteEffects.None;

            if (playerIsOnGround && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W)))
            {
                playerVelocity = new Vector2(playerVelocity.X, -560f);
                playerIsOnGround = false;
            }

            playerVelocity = new Vector2(playerVelocity.X, playerVelocity.Y + 900f * dt);
            
            // Handle item selection with Q/E keys only if not transformed to goat
            if (!playerTransformedToGoat)
            {
                bool qKeyPressed = keyboardState.IsKeyDown(Keys.Q) && !previousKeyState.IsKeyDown(Keys.Q);
                bool eKeyPressed = keyboardState.IsKeyDown(Keys.E) && !previousKeyState.IsKeyDown(Keys.E);
                
                if (qKeyPressed)
                {
                    // Cycle backwards: Mountain <- Mine <- Maze <- None <- Mountain
                    if (currentPlacedItem == PillarItemType.None)
                        currentPlacedItem = PillarItemType.Mountain;
                    else if (currentPlacedItem == PillarItemType.Mountain)
                        currentPlacedItem = PillarItemType.Maze;
                    else if (currentPlacedItem == PillarItemType.Maze)
                        currentPlacedItem = PillarItemType.Mine;
                    else if (currentPlacedItem == PillarItemType.Mine)
                        currentPlacedItem = PillarItemType.None;
                }
                
                if (eKeyPressed)
                {
                    // Cycle forwards: None -> Mountain -> Mine -> Maze -> None
                    if (currentPlacedItem == PillarItemType.None)
                        currentPlacedItem = PillarItemType.Mountain;
                    else if (currentPlacedItem == PillarItemType.Mountain)
                        currentPlacedItem = PillarItemType.Mine;
                    else if (currentPlacedItem == PillarItemType.Mine)
                        currentPlacedItem = PillarItemType.Maze;
                    else if (currentPlacedItem == PillarItemType.Maze)
                        currentPlacedItem = PillarItemType.None;
                }
                
                // Check if player confirms the item
                bool confirmPressed = keyboardState.IsKeyDown(Keys.Enter) && !previousKeyState.IsKeyDown(Keys.Enter);
                
                if (confirmPressed && currentPlacedItem != PillarItemType.None)
                {
                    ValidateSacrifice();
                }
            }
            
            previousKeyState = keyboardState;
            
            return (playerVelocity, playerIsOnGround, playerTransformedToGoat);
        }
        
        private void ValidateSacrifice()
        {
            PillarItemType expectedItem = dialogueMappings[currentDialogueIndex].ExpectedItem;
            
            if (currentPlacedItem == expectedItem)
            {
                // Correct item!
                correctAnswers++;
                currentPlacedItem = PillarItemType.None;
                
                if (correctAnswers >= 3)
                {
                    // Victory!
                    victoryAchieved = true;
                    timerActive = false;
                }
                else
                {
                    // Move to next dialogue and reset timer
                    currentDialogueIndex++;
                    remainingTime = timeLimit;
                    timerActive = true;
                }
            }
            else
            {
                // Wrong item - transform to goat and Zeus stomps
                zeusStomp = true;
                playerTransformedToGoat = true;
                PlayerTransformedToGoat = true;
                stompAnimationTime = 0f;
                timerActive = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, AdonisPlayer player, GameTime gameTime, bool playerIsGoat)
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

            // Draw Zeus using zus.aseprite sprite
            if (zeusSprite != null && zeusSprite.IsLoaded)
            {
                // Zeus is moving/animated when stomping
                bool isMoving = zeusStomp;
                zeusSprite.Draw(spriteBatch, zeusPosition, isMoving, gameTime, Color.White, 10f, SpriteEffects.None);
            }
            
            // Draw Zeus dialogue bubble (only show if not stomping and not transformed)
            if (!victoryAchieved && !zeusStomp && !playerTransformedToGoat)
            {
                if (currentDialogueIndex < dialogueMappings.Length)
                {
                    DrawDialogueBubble(spriteBatch, dialogueMappings[currentDialogueIndex].Dialogue);
                }
            }

            // Draw player or goat
            if (playerIsGoat && goatSprite != null && goatSprite.IsLoaded)
            {
                // Draw goat with proper facing direction based on movement
                goatSprite.Draw(spriteBatch, player.Position, false, gameTime, Color.White, 10f, goatFlip);
            }
            else
            {
                player.Draw(gameTime, spriteBatch);
            }
            
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
            
            // Draw current item if selected (matching PillarRoom behavior)
            if (currentPlacedItem != PillarItemType.None)
            {
                Vector2 slotCenter = new Vector2(slotRect.Center.X, slotRect.Center.Y);
                
                if (currentPlacedItem == PillarItemType.Maze && grapesSprite != null && grapesSprite.IsLoaded)
                {
                    // Draw grapes sprite for maze item
                    Vector2 drawPos = new Vector2(
                        slotCenter.X - grapesSprite.Size.X / 2f,
                        slotCenter.Y - grapesSprite.Size.Y / 2f);
                    grapesSprite.Draw(spriteBatch, drawPos, isMoving: false, gameTime, Color.White);
                }
                else
                {
                    // Mine and Mountain use colored rectangles (matching PillarRoom)
                    // Mountain = Gold, Mine = DeepSkyBlue
                    Color itemColor = currentPlacedItem == PillarItemType.Mountain ? Color.Gold : Color.DeepSkyBlue;
                    Rectangle itemRect = slotRect;
                    itemRect.Inflate(-5, -5);
                    spriteBatch.Draw(solidTexture, itemRect, itemColor);
                }
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
            else if (playerTransformedToGoat)
            {
                // Show goat message
                string goatText = "You are now a goat! Move around freely.";
                Vector2 textSize = titleFont.MeasureString(goatText);
                Vector2 textPos = new Vector2((baseScreenSize.X - textSize.X) / 2f, 20);
                spriteBatch.DrawString(titleFont, goatText, textPos, Color.LightGreen);
            }
            else if (!zeusStomp)
            {
                string instruction = "Use Q/E keys to select item, ENTER to confirm";
                Vector2 textSize = titleFont.MeasureString(instruction);
                Vector2 textPos = new Vector2((baseScreenSize.X - textSize.X) / 2f, 20);
                spriteBatch.DrawString(titleFont, instruction, textPos, Color.Yellow);
                
                // Show current item selection
                if (currentPlacedItem != PillarItemType.None)
                {
                    string itemName = currentPlacedItem == PillarItemType.Mountain ? "Mountain Item (Gold)" :
                                      currentPlacedItem == PillarItemType.Mine ? "Mine Item (Blue)" :
                                      "Maze Item (Purple)";
                    Vector2 itemTextSize = titleFont.MeasureString(itemName);
                    Vector2 itemTextPos = new Vector2((baseScreenSize.X - itemTextSize.X) / 2f, 45);
                    spriteBatch.DrawString(titleFont, itemName, itemTextPos, Color.White);
                }
                
                // Show countdown timer in Greek
                if (timerActive)
                {
                    string greekTime = ConvertToGreekNumber((int)Math.Ceiling(remainingTime));
                    Vector2 timerTextSize = titleFont.MeasureString(greekTime);
                    Vector2 timerTextPos = new Vector2((baseScreenSize.X - timerTextSize.X) / 2f, baseScreenSize.Y - 50);
                    Color timerColor = remainingTime <= 3f ? Color.Red : Color.White;
                    spriteBatch.DrawString(titleFont, greekTime, timerTextPos, timerColor);
                }
            }
        }
        
        private string ConvertToGreekNumber(int number)
        {
            // Convert numbers to Greek words
            return number switch
            {
                10 => "δέκα",
                9 => "εννέα",
                8 => "οκτώ",
                7 => "επτά",
                6 => "έξι",
                5 => "πέντε",
                4 => "τέσσερα",
                3 => "τρία",
                2 => "δύο",
                1 => "ένα",
                0 => "μηδέν",
                _ => number.ToString()
            };
        }
    }
}
