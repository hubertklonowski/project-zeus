using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Levels;
using MonoGame.Aseprite;

namespace ProjectZeus.Core.Levels.ZeusFight
{
    public class ZeusRenderer
    {
        private readonly Texture2D solidTexture;
        private readonly SpriteFont titleFont;
        private readonly AsepriteSprite zeusSprite;
        private readonly AsepriteSprite goatSprite;
        private readonly AsepriteSprite grapesSprite;
        private readonly Vector2 baseScreenSize;

        public ZeusRenderer(Texture2D solidTexture, SpriteFont titleFont, AsepriteSprite zeusSprite, 
            AsepriteSprite goatSprite, AsepriteSprite grapesSprite, Vector2 baseScreenSize)
        {
            this.solidTexture = solidTexture;
            this.titleFont = titleFont;
            this.zeusSprite = zeusSprite;
            this.goatSprite = goatSprite;
            this.grapesSprite = grapesSprite;
            this.baseScreenSize = baseScreenSize;
        }

        public void DrawSacrificePillar(SpriteBatch spriteBatch, GameTime gameTime, Vector2 pillarPosition, 
            Vector2 pillarSize, PillarItemType currentPlacedItem)
        {
            float pulseAmount = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2f) * 0.3f + 0.7f;
            Color lightColor = new Color(255, 255, 200, (int)(150 * pulseAmount));
            
            Rectangle lightBeam = new Rectangle((int)(pillarPosition.X - 20), 0, (int)(pillarSize.X + 40), (int)pillarPosition.Y);
            spriteBatch.Draw(solidTexture, lightBeam, lightColor);
            
            Rectangle pillarRect = new Rectangle((int)pillarPosition.X, (int)pillarPosition.Y, (int)pillarSize.X, (int)pillarSize.Y);
            spriteBatch.Draw(solidTexture, pillarRect, new Color(180, 180, 180));
            
            Rectangle slotRect = new Rectangle((int)(pillarPosition.X + 10), (int)(pillarPosition.Y - 30), (int)(pillarSize.X - 20), 30);
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

        public void DrawDialogueBubble(SpriteBatch spriteBatch, string dialogue, Vector2 zeusPosition)
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

        public void DrawUI(SpriteBatch spriteBatch, bool victoryAchieved, bool playerTransformedToGoat, 
            bool zeusStomp, PillarItemType currentPlacedItem, bool timerActive, float remainingTime)
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
