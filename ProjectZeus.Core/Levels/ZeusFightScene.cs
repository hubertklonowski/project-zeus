using System;
using System.Collections.Generic;
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
        public bool PlayerTransformedToGoat { get; private set; }
        public bool ShouldStartMountainAsGoat { get; private set; }
        public bool ShouldShowCredits { get; private set; }

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
        private HashSet<PillarItemType> usedItems = new HashSet<PillarItemType>();
        
        private const float timeLimit = 10f;
        private float remainingTime = timeLimit;
        private bool timerActive = false;
        
        private bool zeusStomp = false;
        private float stompAnimationTime = 0f;
        private const float stompDuration = 2f;
        
        private Vector2 zeusVelocity = Vector2.Zero;
        private bool zeusIsJumping = false;
        private bool zeusHasLandedOnPlayer = false;
        private Vector2 zeusJumpStartPosition;
        
        private bool playerTransformedToGoat = false;
        private bool victoryAchieved = false;
        private KeyboardState previousKeyState;

        private ZeusRenderer renderer;
        private ZeusPhysicsController physicsController;

        public ZeusFightScene()
        {
            IsCompleted = false;
            ShouldRestartGame = false;
            ShouldShowCredits = false;
            physicsController = new ZeusPhysicsController();
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
            
            ShuffleDialogues();
            timerActive = true;
            remainingTime = timeLimit;
            
            renderer = new ZeusRenderer(solidTexture, titleFont, zeusSprite, goatSprite, grapesSprite, baseScreenSize);
        }
        
        private void ShuffleDialogues()
        {
            Random rng = new Random();
            int n = dialogueMappings.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (dialogueMappings[k], dialogueMappings[n]) = (dialogueMappings[n], dialogueMappings[k]);
            }
        }

        public (Vector2 velocity, bool isOnGround, bool transformedToGoat) Update(GameTime gameTime, KeyboardState keyboardState, Vector2 playerPosition, Vector2 playerSize, Vector2 playerVelocity, bool playerIsOnGround)
        {
            if (victoryAchieved)
            {
                IsCompleted = true;
                return (playerVelocity, playerIsOnGround, playerTransformedToGoat);
            }
 
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float groundTop = baseScreenSize.Y * 0.7f;
             
            // Handle Zeus stomping after goat transformation
            if (zeusStomp)
            {
                stompAnimationTime += dt;

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
                    else if (!zeusIsJumping)
                    {
                        stompAnimationTime = 0f;
                    }
                }

                Vector2 zeusSize = zeusSprite?.IsLoaded == true ? zeusSprite.Size : new Vector2(80, 120);
                zeusPosition.X = MathHelper.Clamp(zeusPosition.X, 0f, baseScreenSize.X - zeusSize.X);
            }
            
            // Update timer only if not transformed to goat
            if (timerActive && !playerTransformedToGoat)
            {
                remainingTime -= dt;
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
            
            if (!playerTransformedToGoat)
            {
                bool qKeyPressed = keyboardState.IsKeyDown(Keys.Q) && !previousKeyState.IsKeyDown(Keys.Q);
                bool eKeyPressed = keyboardState.IsKeyDown(Keys.E) && !previousKeyState.IsKeyDown(Keys.E);
                
                if (qKeyPressed)
                {
                    // Cycle backwards through available (unused) items only
                    CyclePreviousItem();
                }
                
                if (eKeyPressed)
                {
                    // Cycle forwards through available (unused) items only
                    CycleNextItem();
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
                usedItems.Add(currentPlacedItem); // Mark item as used
                currentPlacedItem = PillarItemType.None;
                
                if (correctAnswers >= 3)
                {
                    // Victory!
                    victoryAchieved = true;
                    timerActive = false;
                    ShouldShowCredits = true;
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

        private void CycleNextItem()
        {
            // Start with first item if currently None
            if (currentPlacedItem == PillarItemType.None)
            {
                // Find first unused item in order: Mountain -> Mine -> Maze
                if (!usedItems.Contains(PillarItemType.Mountain))
                    currentPlacedItem = PillarItemType.Mountain;
                else if (!usedItems.Contains(PillarItemType.Mine))
                    currentPlacedItem = PillarItemType.Mine;
                else if (!usedItems.Contains(PillarItemType.Maze))
                    currentPlacedItem = PillarItemType.Maze;
                // If all used, stay at None
                return;
            }
            
            // Cycle forward through unused items only, skipping None
            PillarItemType nextItem = currentPlacedItem;
            int attempts = 0;
            
            do
            {
                if (nextItem == PillarItemType.Mountain)
                    nextItem = PillarItemType.Mine;
                else if (nextItem == PillarItemType.Mine)
                    nextItem = PillarItemType.Maze;
                else if (nextItem == PillarItemType.Maze)
                    nextItem = PillarItemType.Mountain;
                    
                attempts++;
                
                // Prevent infinite loop - if we've checked all items, keep current
                if (attempts > 3)
                    return;
            }
            while (usedItems.Contains(nextItem));
            
            currentPlacedItem = nextItem;
        }

        private void CyclePreviousItem()
        {
            // Start with last item if currently None
            if (currentPlacedItem == PillarItemType.None)
            {
                // Find last unused item in reverse order: Maze -> Mine -> Mountain
                if (!usedItems.Contains(PillarItemType.Maze))
                    currentPlacedItem = PillarItemType.Maze;
                else if (!usedItems.Contains(PillarItemType.Mine))
                    currentPlacedItem = PillarItemType.Mine;
                else if (!usedItems.Contains(PillarItemType.Mountain))
                    currentPlacedItem = PillarItemType.Mountain;
                // If all used, stay at None
                return;
            }
            
            // Cycle backward through unused items only, skipping None
            PillarItemType previousItem = currentPlacedItem;
            int attempts = 0;
            
            do
            {
                if (previousItem == PillarItemType.Mountain)
                    previousItem = PillarItemType.Maze;
                else if (previousItem == PillarItemType.Maze)
                    previousItem = PillarItemType.Mine;
                else if (previousItem == PillarItemType.Mine)
                    previousItem = PillarItemType.Mountain;
                    
                attempts++;
                
                // Prevent infinite loop - if we've checked all items, keep current
                if (attempts > 3)
                    return;
            }
            while (usedItems.Contains(previousItem));
            
            currentPlacedItem = previousItem;
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
            
            DrawSacrificePillar(spriteBatch, gameTime);

            if (zeusSprite != null && zeusSprite.IsLoaded)
            {
                bool isMoving = zeusIsJumping || Math.Abs(zeusVelocity.X) > 0;
                zeusSprite.Draw(spriteBatch, zeusPosition, isMoving, gameTime, Color.White, 10f, SpriteEffects.None);
            }
            
            if (!victoryAchieved && !zeusStomp && !playerTransformedToGoat && currentDialogueIndex < dialogueMappings.Length)
            {
                DrawDialogueBubble(spriteBatch, dialogueMappings[currentDialogueIndex].Dialogue);
            }

            if (playerIsGoat && goatSprite != null && goatSprite.IsLoaded)
            {
                goatSprite.Draw(spriteBatch, player.Position, false, gameTime, Color.White, 10f, goatFlip);
            }
            else
            {
                player.Draw(gameTime, spriteBatch);
            }
            
            DrawUI(spriteBatch);

            spriteBatch.End();
        }
        
        private void DrawSacrificePillar(SpriteBatch spriteBatch, GameTime gameTime)
        {
            float pulseAmount = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2f) * 0.3f + 0.7f;
            Color lightColor = new Color(255, 255, 200, (int)(150 * pulseAmount));
            
            Rectangle lightBeam = new Rectangle((int)(sacrificePillarPosition.X - 20), 0, (int)(sacrificePillarSize.X + 40), (int)sacrificePillarPosition.Y);
            spriteBatch.Draw(solidTexture, lightBeam, lightColor);
            
            Rectangle pillarRect = new Rectangle((int)sacrificePillarPosition.X, (int)sacrificePillarPosition.Y, (int)sacrificePillarSize.X, (int)sacrificePillarSize.Y);
            spriteBatch.Draw(solidTexture, pillarRect, new Color(180, 180, 180));
            
            Rectangle slotRect = new Rectangle((int)(sacrificePillarPosition.X + 10), (int)(sacrificePillarPosition.Y - 30), (int)(sacrificePillarSize.X - 20), 30);
            spriteBatch.Draw(solidTexture, slotRect, new Color(200, 200, 220));
            
            if (currentPlacedItem != PillarItemType.None)
            {
                Vector2 slotCenter = new Vector2(slotRect.Center.X, slotRect.Center.Y);
                
                if (currentPlacedItem == PillarItemType.Maze && grapesSprite != null && grapesSprite.IsLoaded)
                {
                    Vector2 drawPos = new Vector2(slotCenter.X - grapesSprite.Size.X / 2f, slotCenter.Y - grapesSprite.Size.Y / 2f);
                    grapesSprite.Draw(spriteBatch, drawPos, isMoving: false, gameTime, Color.White);
                }
                else
                {
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
            
            Rectangle bubbleRect = new Rectangle((int)bubblePosition.X, (int)bubblePosition.Y, (int)bubbleSize.X, (int)bubbleSize.Y);
            spriteBatch.Draw(solidTexture, bubbleRect, new Color(255, 255, 255, 230));
            DrawingHelpers.DrawRectangleOutline(spriteBatch, solidTexture, bubbleRect, Color.Black);
            
            spriteBatch.DrawString(titleFont, dialogue, bubblePosition + new Vector2(10, 10), Color.Black);
        }
        
        private void DrawUI(SpriteBatch spriteBatch)
        {
            if (titleFont == null) return;
            
            if (victoryAchieved)
            {
                string victoryText = "GODS ARE SATISFIED!";
                float scale = 2.0f;
                Vector2 textSize = titleFont.MeasureString(victoryText) * scale;
                Vector2 textPos = new Vector2((baseScreenSize.X - textSize.X) / 2f, 40);
                
                spriteBatch.DrawString(titleFont, victoryText, textPos + new Vector2(3, 3), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(titleFont, victoryText, textPos, Color.Gold, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(titleFont, victoryText, textPos + new Vector2(1, 0), Color.Gold, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(titleFont, victoryText, textPos + new Vector2(-1, 0), Color.Gold, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(titleFont, victoryText, textPos + new Vector2(0, 1), Color.Gold, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(titleFont, victoryText, textPos + new Vector2(0, -1), Color.Gold, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            else if (playerTransformedToGoat)
            {
                string goatText = "You are now a goat! Move around freely.";
                Vector2 textPos = new Vector2((baseScreenSize.X - titleFont.MeasureString(goatText).X) / 2f, 20);
                spriteBatch.DrawString(titleFont, goatText, textPos, Color.LightGreen);
            }
            else if (!zeusStomp)
            {
                string instruction = "Use Q/E keys to select item, ENTER to confirm";
                Vector2 textPos = new Vector2((baseScreenSize.X - titleFont.MeasureString(instruction).X) / 2f, 20);
                spriteBatch.DrawString(titleFont, instruction, textPos, Color.Yellow);
                
                if (currentPlacedItem != PillarItemType.None)
                {
                    string itemName = currentPlacedItem == PillarItemType.Mountain ? "Mountain Item (Gold)" :
                                      currentPlacedItem == PillarItemType.Mine ? "Mine Item (Blue)" : "Maze Item (Purple)";
                    Vector2 itemTextPos = new Vector2((baseScreenSize.X - titleFont.MeasureString(itemName).X) / 2f, 45);
                    spriteBatch.DrawString(titleFont, itemName, itemTextPos, Color.White);
                }
                
                if (timerActive)
                {
                    string greekTime = ConvertToGreekNumber((int)Math.Ceiling(remainingTime));
                    Vector2 timerTextPos = new Vector2((baseScreenSize.X - titleFont.MeasureString(greekTime).X) / 2f, baseScreenSize.Y - 50);
                    spriteBatch.DrawString(titleFont, greekTime, timerTextPos, remainingTime <= 3f ? Color.Red : Color.White);
                }
            }
        }
        
        private string ConvertToGreekNumber(int number) => number switch
        {
            10 => "δέκα", 9 => "εννέα", 8 => "οκτώ", 7 => "επτά", 6 => "έξι",
            5 => "πέντε", 4 => "τέσσερα", 3 => "τρία", 2 => "δύο", 1 => "ένα", 0 => "μηδέν",
            _ => number.ToString()
        };
    }
}
