using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Constants;
using PillarEntity = ProjectZeus.Core.Entities.Pillar;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;
using MonoGame.Aseprite;

namespace ProjectZeus.Core.Levels.Pillar
{
    /// <summary>
    /// Handles rendering for the pillar room
    /// </summary>
    public class PillarRoomRenderer
    {
        private readonly Texture2D pillarTexture;
        private readonly Texture2D slotTexture;
        private readonly Texture2D skyTexture;
        private readonly Texture2D portalTexture;
        private readonly SpriteFont font;
        private readonly AsepriteSprite mazeItemSprite;

        public PillarRoomRenderer(Texture2D pillarTexture, Texture2D slotTexture, Texture2D skyTexture, 
            Texture2D portalTexture, SpriteFont font, AsepriteSprite mazeItemSprite)
        {
            this.pillarTexture = pillarTexture;
            this.slotTexture = slotTexture;
            this.skyTexture = skyTexture;
            this.portalTexture = portalTexture;
            this.font = font;
            this.mazeItemSprite = mazeItemSprite;
        }

        public void DrawBackground(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Rectangle skyRect = new Rectangle(0, 0, (int)GameConstants.BaseScreenSize.X, (int)GameConstants.BaseScreenSize.Y);
            spriteBatch.Draw(skyTexture, skyRect, Color.White);

            float t = (float)gameTime.TotalGameTime.TotalSeconds;
            float cloudSpeed = 20f;
            int cloudWidth = 160;
            int cloudHeight = 60;

            for (int i = 0; i < 3; i++)
            {
                float x = ((t * cloudSpeed) + i * 200f) % (GameConstants.BaseScreenSize.X + cloudWidth) - cloudWidth;
                float y = 60f + i * 40f;
                Rectangle cloudRect = new Rectangle((int)x, (int)y, cloudWidth, cloudHeight);
                spriteBatch.Draw(skyTexture, cloudRect, new Color(250, 250, 250));
            }

            Rectangle groundRect = new Rectangle(0, (int)(GameConstants.BaseScreenSize.Y - GameConstants.GroundHeight), 
                (int)GameConstants.BaseScreenSize.X, (int)GameConstants.GroundHeight);
            spriteBatch.Draw(pillarTexture, groundRect, new Color(180, 180, 180));
        }

        public void DrawPillars(SpriteBatch spriteBatch, PillarEntity[] pillars, PillarItemType[] pillarItems, GameTime gameTime)
        {
            for (int i = 0; i < pillars.Length; i++)
            {
                PillarEntity pillar = pillars[i];
                Rectangle pillarRect = pillar.GetPillarRectangle();
                spriteBatch.Draw(pillarTexture, pillarRect, Color.White);

                int stripeCount = 4;
                int stripeWidth = pillarRect.Width / (stripeCount * 2);
                for (int s = 0; s < stripeCount; s++)
                {
                    int x = pillarRect.X + stripeWidth + s * stripeWidth * 2;
                    Rectangle stripe = new Rectangle(x, pillarRect.Y, stripeWidth, pillarRect.Height);
                    spriteBatch.Draw(pillarTexture, stripe, new Color(210, 210, 210));
                }

                Rectangle capitalRect = new Rectangle(pillarRect.X - 5, pillarRect.Y - 10, pillarRect.Width + 10, 10);
                spriteBatch.Draw(pillarTexture, capitalRect, new Color(240, 240, 240));

                Rectangle slotRect = pillar.GetSlotRectangle();

                if (!pillar.HasItem)
                {
                    spriteBatch.Draw(slotTexture, slotRect, Color.White);
                }

                if (pillar.HasItem)
                {
                    PillarItemType itemType = pillarItems != null && i < pillarItems.Length
                        ? pillarItems[i]
                        : PillarItemType.None;

                    if (itemType == PillarItemType.Maze && mazeItemSprite != null && mazeItemSprite.IsLoaded)
                    {
                        Vector2 slotCenter = new Vector2(slotRect.Center.X, slotRect.Center.Y);
                        Vector2 drawPos = new Vector2(
                            slotCenter.X - mazeItemSprite.Size.X / 2f,
                            slotCenter.Y - mazeItemSprite.Size.Y / 2f);
                        mazeItemSprite.Draw(spriteBatch, drawPos, isMoving: false, gameTime, Color.White);
                    }
                    else
                    {
                        Rectangle itemRect = slotRect;
                        itemRect.Inflate(-10, -10);
                        spriteBatch.Draw(pillarTexture, itemRect, pillar.ItemColor);
                    }
                }
                else
                {
                    DrawingHelpers.DrawRectangleOutline(spriteBatch, pillarTexture, slotRect, Color.DarkBlue);
                }
            }
        }

        public void DrawPortals(SpriteBatch spriteBatch, GameTime gameTime, Portal mazePortal, Portal minePortal, Portal mountainPortal)
        {
            float groundTop = GameConstants.BaseScreenSize.Y - GameConstants.GroundHeight;

            if (mazePortal.IsActive)
            {
                DrawingHelpers.DrawPortal(spriteBatch, portalTexture, mazePortal.Bounds, gameTime, mazePortal.BaseColor);
                string levelName = "Maze";
                Vector2 textSize = font.MeasureString(levelName);
                Vector2 textPos = new Vector2(
                    mazePortal.Position.X + mazePortal.Size.X / 2f - textSize.X / 2f,
                    mazePortal.Position.Y - textSize.Y - 5f);
                spriteBatch.DrawString(font, levelName, textPos, Color.White);
            }

            if (mountainPortal.IsActive)
            {
                DrawingHelpers.DrawPortal(spriteBatch, portalTexture, mountainPortal.Bounds, gameTime, mountainPortal.BaseColor);
                string levelName = "Mountain";
                Vector2 textSize = font.MeasureString(levelName);
                Vector2 textPos = new Vector2(
                    mountainPortal.Position.X + mountainPortal.Size.X / 2f - textSize.X / 2f,
                    mountainPortal.Position.Y - textSize.Y - 5f);
                spriteBatch.DrawString(font, levelName, textPos, Color.White);
            }

            if (minePortal.IsActive)
            {
                DrawingHelpers.DrawPortal(spriteBatch, portalTexture, minePortal.Bounds, gameTime, minePortal.BaseColor);
                string levelName = "Mine";
                Vector2 textSize = font.MeasureString(levelName);
                Vector2 textPos = new Vector2(
                    minePortal.Position.X + minePortal.Size.X / 2f - textSize.X / 2f,
                    groundTop - 80f - textSize.Y - 5f);
                spriteBatch.DrawString(font, levelName, textPos, Color.White);
            }
        }

        public void DrawUI(SpriteBatch spriteBatch, Texture2D itemTexture, bool hasAnyItem, PillarItemType currentCarriedItem, GameTime gameTime)
        {
            if (hasAnyItem && currentCarriedItem != PillarItemType.None)
            {
                Rectangle inventoryRect = new Rectangle(10, 10, 40, 40);

                spriteBatch.Draw(skyTexture, inventoryRect, Color.White);

                if (currentCarriedItem == PillarItemType.Maze && mazeItemSprite != null && mazeItemSprite.IsLoaded)
                {
                    Vector2 center = new Vector2(inventoryRect.Center.X, inventoryRect.Center.Y);
                    Vector2 drawPos = new Vector2(
                        center.X - mazeItemSprite.Size.X / 2f,
                        center.Y - mazeItemSprite.Size.Y / 2f);
                    mazeItemSprite.Draw(spriteBatch, drawPos, isMoving: false, gameTime, Color.White);
                }
                else
                {
                    Color itemColor = currentCarriedItem == PillarItemType.Mine 
                        ? Color.DeepSkyBlue 
                        : Color.Gold;
                    spriteBatch.Draw(itemTexture, inventoryRect, itemColor);
                }

                if (font != null)
                {
                    string inventoryText = "Item collected! Place it in a pillar.";
                    Vector2 inventoryTextPos = new Vector2(60f, 20f);
                    spriteBatch.DrawString(font, inventoryText, inventoryTextPos, Color.Gold);
                }
            }

            string title = hasAnyItem
                ? "Place the item in a pillar slot"
                : "Enter portal or insert the three items of Zeus";
            Vector2 titleSize = font.MeasureString(title);
            Vector2 titlePos = new Vector2((GameConstants.BaseScreenSize.X - titleSize.X) / 2f, 40f);
            spriteBatch.DrawString(font, title, titlePos, Color.Yellow);

            string instructions = hasAnyItem
                ? "Press E near an empty pillar to place the item"
                : "Press E near a portal to enter a level";
            Vector2 instructionsSize = font.MeasureString(instructions);
            Vector2 instructionsPos = new Vector2((GameConstants.BaseScreenSize.X - instructionsSize.X) / 2f, 70f);
            spriteBatch.DrawString(font, instructions, instructionsPos, Color.White);
        }
    }
}
