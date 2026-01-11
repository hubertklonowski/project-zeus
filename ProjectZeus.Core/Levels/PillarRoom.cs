using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Constants;
using PillarEntity = ProjectZeus.Core.Entities.Pillar;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Levels.Pillar;
using MonoGame.Aseprite;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Main hub room with three pillars where items must be placed
    /// </summary>
    public class PillarRoom
    {
        private PillarEntity[] pillars;
        private Portal mazePortal;
        private Portal minePortal;
        private Portal mountainPortal;
        
        private Texture2D pillarTexture;
        private Texture2D skyTexture;
        private SpriteFont font;
        private AsepriteSprite mazeItemSprite;
        private PillarRoomRenderer renderer;

        public PillarItemType CurrentCarriedItem { get; set; } = PillarItemType.None;
        private PillarItemType[] pillarItems;

        public PillarEntity[] Pillars => pillars;
        public Portal MazePortal => mazePortal;
        public Portal MinePortal => minePortal;
        public Portal MountainPortal => mountainPortal;
        public bool AllItemsInserted => AreAllItemsInserted();

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            this.font = font;
            
            pillarTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, new Color(230, 230, 230));
            Texture2D slotTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, new Color(200, 200, 255));
            skyTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, new Color(135, 206, 235));
            
            var vaseSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Vase);
            Texture2D portalTexture = null;
            if (vaseSprite != null && vaseSprite.IsLoaded)
            {
                portalTexture = vaseSprite.GetFrameTexture(0);
            }

            mazeItemSprite = AsepriteSprite.Load(graphicsDevice, AssetPaths.Grapes);

            SetupPillars();
            SetupPortals();
            
            renderer = new PillarRoomRenderer(pillarTexture, slotTexture, skyTexture, portalTexture, font, mazeItemSprite);
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
                new PillarEntity 
                { 
                    Position = new Vector2(centerX - spacing, groundY), 
                    Size = pillarSize, 
                    SlotSize = slotSize, 
                    SlotOffsetY = slotOffsetY,
                    HasItem = false,
                    ItemColor = Color.Gold
                },
                new PillarEntity 
                { 
                    Position = new Vector2(centerX, groundY), 
                    Size = pillarSize, 
                    SlotSize = slotSize, 
                    SlotOffsetY = slotOffsetY,
                    HasItem = false,
                    ItemColor = Color.DeepSkyBlue
                },
                new PillarEntity 
                { 
                    Position = new Vector2(centerX + spacing, groundY), 
                    Size = pillarSize, 
                    SlotSize = slotSize, 
                    SlotOffsetY = slotOffsetY,
                    HasItem = false,
                    ItemColor = Color.MediumVioletRed
                }
            };

            pillarItems = new PillarItemType[pillars.Length];
            for (int i = 0; i < pillarItems.Length; i++)
            {
                pillarItems[i] = PillarItemType.None;
            }
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
            if (CurrentCarriedItem == PillarItemType.None)
                return false;

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
                    pillarItems[i] = CurrentCarriedItem;
                    CurrentCarriedItem = PillarItemType.None;
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

            if (pillarItems != null)
            {
                for (int i = 0; i < pillarItems.Length; i++)
                {
                    pillarItems[i] = PillarItemType.None;
                }
            }

            CurrentCarriedItem = PillarItemType.None;
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
            renderer.DrawBackground(spriteBatch, gameTime);
            renderer.DrawPillars(spriteBatch, pillars, pillarItems, gameTime);
            renderer.DrawPortals(spriteBatch, gameTime, mazePortal, minePortal, mountainPortal);
        }

        public void DrawUI(SpriteBatch spriteBatch, Texture2D itemTexture, bool hasAnyItem)
        {
            renderer.DrawUI(spriteBatch, itemTexture, hasAnyItem, CurrentCarriedItem, default);
        }
    }
}
