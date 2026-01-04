using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Main hub room with three pillars where items must be placed
    /// </summary>
    public class PillarRoom
    {
        private Pillar[] pillars;
        private Portal mazePortal;
        private Portal minePortal;
        private Portal mountainPortal;
        
        private Texture2D pillarTexture;
        private Texture2D slotTexture;
        private Texture2D skyTexture;
        private Texture2D portalTexture;
        private SpriteFont font;

        public Pillar[] Pillars => pillars;
        public Portal MazePortal => mazePortal;
        public Portal MinePortal => minePortal;
        public Portal MountainPortal => mountainPortal;
        public bool AllItemsInserted => AreAllItemsInserted();

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            this.font = font;
            
            pillarTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, new Color(230, 230, 230));
            slotTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, new Color(200, 200, 255));
            skyTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, new Color(135, 206, 235));
            portalTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, Color.White);

            SetupPillars();
            SetupPortals();
        }

        private void SetupPillars()
        {
            float centerX = GameConstants.BaseScreenSize.X / 2f;
            float groundY = GameConstants.BaseScreenSize.Y;
            float spacing = 200f;

            Vector2 pillarSize = new Vector2(60f, 140f);
            Vector2 slotSize = new Vector2(80f, 60f);
            float slotOffsetY = 10f;

            pillars = new[]
            {
                new Pillar 
                { 
                    Position = new Vector2(centerX - spacing, groundY), 
                    Size = pillarSize, 
                    SlotSize = slotSize, 
                    SlotOffsetY = slotOffsetY,
                    HasItem = false,
                    ItemColor = Color.Gold
                },
                new Pillar 
                { 
                    Position = new Vector2(centerX, groundY), 
                    Size = pillarSize, 
                    SlotSize = slotSize, 
                    SlotOffsetY = slotOffsetY,
                    HasItem = false,
                    ItemColor = Color.DeepSkyBlue
                },
                new Pillar 
                { 
                    Position = new Vector2(centerX + spacing, groundY), 
                    Size = pillarSize, 
                    SlotSize = slotSize, 
                    SlotOffsetY = slotOffsetY,
                    HasItem = false,
                    ItemColor = Color.MediumVioletRed
                }
            };
        }

        private void SetupPortals()
        {
            float groundTop = GameConstants.BaseScreenSize.Y - GameConstants.GroundHeight;
            Vector2 portalSize = new Vector2(60, 80);

            float mazePortalX = (pillars[0].Position.X + pillars[1].Position.X) / 2f - portalSize.X / 2f;
            float mazePortalY = groundTop - portalSize.Y;
            mazePortal = new Portal(new Vector2(mazePortalX, mazePortalY), portalSize, new Color(100, 50, 200));

            float minePortalWidth = 80f;
            float minePortalHeight = 130f;
            float xBetween = (pillars[1].Position.X + pillars[2].Position.X) / 2f;
            float minePortalX = xBetween - minePortalWidth / 2f;
            float minePortalY = groundTop - minePortalHeight;
            minePortal = new Portal(new Vector2(minePortalX, minePortalY), 
                new Vector2(minePortalWidth, minePortalHeight), new Color(100, 50, 200));

            float mountainPortalX = pillars[2].GetPillarRectangle().Right + 40f;
            float mountainPortalY = groundTop - portalSize.Y;
            mountainPortal = new Portal(new Vector2(mountainPortalX, mountainPortalY), portalSize, new Color(100, 50, 200));
        }

        public bool TryInsertItem(Vector2 playerPosition, Vector2 playerSize)
        {
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 
                (int)playerSize.X, (int)playerSize.Y);

            for (int i = 0; i < pillars.Length; i++)
            {
                if (pillars[i].HasItem)
                    continue;

                Rectangle slotRect = pillars[i].GetSlotRectangle();
                Rectangle interactionRect = slotRect;
                interactionRect.Inflate(20, 20);

                if (playerRect.Intersects(interactionRect))
                {
                    pillars[i].HasItem = true;
                    return true;
                }
            }

            return false;
        }

        public void ResetItems()
        {
            foreach (var pillar in pillars)
            {
                pillar.HasItem = false;
            }
        }

        private bool AreAllItemsInserted()
        {
            foreach (var pillar in pillars)
            {
                if (!pillar.HasItem)
                    return false;
            }
            return true;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime, bool hasItem)
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

            for (int i = 0; i < pillars.Length; i++)
            {
                Pillar pillar = pillars[i];
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
                spriteBatch.Draw(slotTexture, slotRect, Color.White);

                if (pillar.HasItem)
                {
                    Rectangle itemRect = slotRect;
                    itemRect.Inflate(-10, -10);
                    spriteBatch.Draw(pillarTexture, itemRect, pillar.ItemColor);
                }

                DrawingHelpers.DrawRectangleOutline(spriteBatch, pillarTexture, slotRect, Color.DarkBlue);
            }

            if (mazePortal.IsActive && !hasItem)
            {
                DrawingHelpers.DrawPortal(spriteBatch, portalTexture, mazePortal.Bounds, gameTime, mazePortal.BaseColor);
            }

            if (mountainPortal.IsActive)
            {
                DrawingHelpers.DrawPortal(spriteBatch, portalTexture, mountainPortal.Bounds, gameTime, mountainPortal.BaseColor);
            }

            if (minePortal.IsActive)
            {
                DrawingHelpers.DrawPortal(spriteBatch, portalTexture, minePortal.Bounds, gameTime, minePortal.BaseColor);
            }
        }

        public void DrawUI(SpriteBatch spriteBatch, Texture2D itemTexture, bool hasAnyItem)
        {
            if (hasAnyItem)
            {
                Rectangle inventoryRect = new Rectangle(10, 10, 40, 40);
                spriteBatch.Draw(itemTexture, inventoryRect, Color.Gold);
                DrawingHelpers.DrawRectangleOutline(spriteBatch, pillarTexture, inventoryRect, Color.Yellow);

                if (font != null)
                {
                    string inventoryText = "Item collected! Place it in a pillar.";
                    Vector2 inventoryTextSize = font.MeasureString(inventoryText);
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
                : "Walk into a portal to enter a level";
            Vector2 instructionsSize = font.MeasureString(instructions);
            Vector2 instructionsPos = new Vector2((GameConstants.BaseScreenSize.X - instructionsSize.X) / 2f, 70f);
            spriteBatch.DrawString(font, instructions, instructionsPos, Color.White);
        }
    }
}
