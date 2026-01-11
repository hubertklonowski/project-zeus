using System;
using System.Collections.Generic;
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

        public void DrawBackground(SpriteBatch spriteBatch)
        {
            Rectangle sky = new Rectangle(0, 0, (int)baseScreenSize.X, (int)baseScreenSize.Y);
            spriteBatch.Draw(solidTexture, sky, new Color(20, 20, 40));
        }

        public void DrawZeus(SpriteBatch spriteBatch, GameTime gameTime, Vector2 zeusPosition, bool zeusStomp)
        {
            if (zeusSprite != null && zeusSprite.IsLoaded)
            {
                zeusSprite.Draw(spriteBatch, zeusPosition, zeusStomp, gameTime, Color.White, 10f, SpriteEffects.None);
            }
            else
            {
                spriteBatch.Draw(solidTexture, new Rectangle((int)zeusPosition.X, (int)zeusPosition.Y, 80, 120), Color.Blue);
            }
        }

        public void DrawPlayer(SpriteBatch spriteBatch, GameTime gameTime, Vector2 playerPosition, 
            bool transformedToGoat, SpriteEffects goatFlip)
        {
            if (transformedToGoat && goatSprite != null && goatSprite.IsLoaded)
            {
                goatSprite.Draw(spriteBatch, playerPosition, true, gameTime, Color.White, 10f, goatFlip);
            }
        }

        public void DrawSacrificePillar(SpriteBatch spriteBatch, Vector2 pillarPosition, Vector2 pillarSize, 
            PillarItemType currentPlacedItem)
        {
            spriteBatch.Draw(solidTexture, new Rectangle((int)pillarPosition.X, (int)pillarPosition.Y, 
                (int)pillarSize.X, (int)pillarSize.Y), new Color(139, 69, 19));
            
            if (currentPlacedItem != PillarItemType.None)
            {
                Vector2 itemPosition = new Vector2(pillarPosition.X + pillarSize.X / 2 - 15, pillarPosition.Y - 30);
                Color itemColor = currentPlacedItem == PillarItemType.Mine ? Color.Gold :
                                 currentPlacedItem == PillarItemType.Mountain ? Color.White : Color.Purple;
                
                if (currentPlacedItem == PillarItemType.Maze && grapesSprite != null && grapesSprite.IsLoaded)
                {
                    grapesSprite.Draw(spriteBatch, itemPosition, false, null, Color.White, 1f, SpriteEffects.None);
                }
                else
                {
                    spriteBatch.Draw(solidTexture, new Rectangle((int)itemPosition.X, (int)itemPosition.Y, 30, 30), itemColor);
                }
            }
        }

        public void DrawUI(SpriteBatch spriteBatch, string currentDialogue, float remainingTime, bool timerActive,
            bool victoryAchieved, bool playerTransformedToGoat)
        {
            if (victoryAchieved)
            {
                string victoryText = "Victory! Zeus transforms you into a goat.";
                Vector2 victorySize = titleFont.MeasureString(victoryText);
                spriteBatch.DrawString(titleFont, victoryText, 
                    new Vector2((baseScreenSize.X - victorySize.X) / 2, baseScreenSize.Y / 2), Color.Gold);
                return;
            }

            if (!string.IsNullOrEmpty(currentDialogue))
            {
                Vector2 dialogueSize = titleFont.MeasureString(currentDialogue);
                spriteBatch.DrawString(titleFont, currentDialogue, 
                    new Vector2((baseScreenSize.X - dialogueSize.X) / 2, 20), Color.Yellow);
            }

            if (timerActive)
            {
                string timerText = $"Time: {(int)Math.Ceiling(remainingTime)}s";
                spriteBatch.DrawString(titleFont, timerText, new Vector2(baseScreenSize.X - 100, 20), 
                    remainingTime < 3 ? Color.Red : Color.White);
            }

            if (playerTransformedToGoat)
            {
                string goatText = "You are now a goat! Press E to place items on the pillar.";
                Vector2 goatSize = titleFont.MeasureString(goatText);
                spriteBatch.DrawString(titleFont, goatText, 
                    new Vector2((baseScreenSize.X - goatSize.X) / 2, baseScreenSize.Y - 40), Color.Cyan);
            }
        }
    }
}
