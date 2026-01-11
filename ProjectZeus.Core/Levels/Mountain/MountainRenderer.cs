using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Levels;
using ProjectZeus.Core.Rendering;

namespace ProjectZeus.Core.Levels.Mountain
{
    /// <summary>
    /// Handles rendering for the mountain level
    /// </summary>
    public class MountainRenderer
    {
        private readonly Texture2D solidTexture;
        private readonly AsepriteSprite goatSprite;
        private readonly AsepriteSprite rockSprite;
        private readonly SpriteFont font;
        private readonly Vector2 baseScreenSize;

        public MountainRenderer(Texture2D solidTexture, AsepriteSprite goatSprite, AsepriteSprite rockSprite, 
            SpriteFont font, Vector2 baseScreenSize)
        {
            this.solidTexture = solidTexture;
            this.goatSprite = goatSprite;
            this.rockSprite = rockSprite;
            this.font = font;
            this.baseScreenSize = baseScreenSize;
        }

        public void DrawBackground(SpriteBatch spriteBatch, Vector2 cameraOffset, float worldHeight)
        {
            Rectangle skyRect = new Rectangle(0, (int)cameraOffset.Y, (int)baseScreenSize.X, (int)baseScreenSize.Y);
            spriteBatch.Draw(solidTexture, skyRect, new Color(135, 206, 235));

            Rectangle groundRect = new Rectangle(0, (int)(worldHeight - 40), (int)baseScreenSize.X, 40);
            spriteBatch.Draw(solidTexture, groundRect, new Color(101, 67, 33));
        }

        public void DrawPlatforms(SpriteBatch spriteBatch, List<Platform> platforms, List<MovingPlatform> movingPlatforms)
        {
            foreach (var platform in platforms)
            {
                spriteBatch.Draw(solidTexture, platform.Bounds, new Color(139, 69, 19));
                Rectangle topLayer = new Rectangle(platform.Bounds.X, platform.Bounds.Y, platform.Bounds.Width, 5);
                spriteBatch.Draw(solidTexture, topLayer, new Color(101, 67, 33));
            }

            foreach (var movingPlatform in movingPlatforms)
            {
                spriteBatch.Draw(solidTexture, movingPlatform.Bounds, new Color(169, 169, 169));
                Rectangle topLayer = new Rectangle(movingPlatform.Bounds.X, movingPlatform.Bounds.Y, movingPlatform.Bounds.Width, 5);
                spriteBatch.Draw(solidTexture, topLayer, new Color(128, 128, 128));
            }
        }

        public void DrawGoat(SpriteBatch spriteBatch, GameTime gameTime, Vector2 goatPosition, Vector2 goatVelocity, 
            Vector2 goatSize, bool playerIsGoat)
        {
            if (goatSprite != null && goatSprite.IsLoaded)
            {
                SpriteEffects flip = goatVelocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                bool isMoving = Math.Abs(goatVelocity.X) > 0 || playerIsGoat;
                goatSprite.Draw(spriteBatch, goatPosition, isMoving, gameTime, Color.White, 10f, flip);
            }
            else
            {
                spriteBatch.Draw(solidTexture, new Rectangle((int)goatPosition.X, (int)goatPosition.Y, 
                    (int)goatSize.X, (int)goatSize.Y), Color.Gray);
            }
        }

        public void DrawRocks(SpriteBatch spriteBatch, GameTime gameTime, List<Rock> rocks)
        {
            foreach (var rock in rocks)
            {
                if (rockSprite != null && rockSprite.IsLoaded)
                {
                    Vector2 drawPos = new Vector2(
                        rock.Position.X - rockSprite.Size.X / 2,
                        rock.Position.Y - rockSprite.Size.Y / 2);
                    
                    Rectangle destRect = new Rectangle(
                        (int)drawPos.X, (int)drawPos.Y,
                        (int)rockSprite.Size.X, (int)rockSprite.Size.Y);
                    
                    spriteBatch.Draw(rockSprite.GetFrameTexture(0), destRect, null, Color.White, 
                        rock.Rotation, Vector2.Zero, SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(solidTexture, new Rectangle((int)rock.Position.X, (int)rock.Position.Y, 
                        (int)rock.Size.X, (int)rock.Size.Y), Color.DarkGray);
                }
            }
        }

        public void DrawItem(SpriteBatch spriteBatch, Vector2 itemPosition, Vector2 itemSize, bool itemCollected)
        {
            if (!itemCollected)
            {
                Rectangle itemRect = new Rectangle((int)itemPosition.X, (int)itemPosition.Y, 
                    (int)itemSize.X, (int)itemSize.Y);
                spriteBatch.Draw(solidTexture, itemRect, Color.Gold);
                
                Rectangle glowRect = itemRect;
                glowRect.Inflate(5, 5);
                spriteBatch.Draw(solidTexture, glowRect, new Color(255, 215, 0, 100));
            }
        }

        public void DrawUI(SpriteBatch spriteBatch, bool itemCollected, bool playerIsGoat)
        {
            string instructions = itemCollected 
                ? "Item collected! Walk off the left edge to return to the hub."
                : "Climb to the top and collect the item. Watch out for falling rocks!";
            
            if (playerIsGoat)
                instructions = "You are now the goat! Press E to throw rocks.";
            
            spriteBatch.DrawString(font, instructions, new Vector2(10, 10), Color.Yellow);
        }
    }
}
