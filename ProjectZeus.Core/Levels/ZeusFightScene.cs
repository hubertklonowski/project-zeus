using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;
using MonoGame.Aseprite;

namespace ProjectZeus.Core
{
    /// <summary>
    /// Zeus fight scene with the boss character
    /// </summary>
    public class ZeusFightScene
    {
        public bool IsCompleted { get; private set; }

        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        private Texture2D solidTexture;
        private SpriteFont titleFont;
        private AsepriteSprite zeusSprite;
        
        private Vector2 zeusPosition;

        public ZeusFightScene()
        {
            IsCompleted = false;
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
        }

        public void Update(GameTime gameTime)
        {
            // TODO: Add Zeus fight logic here in the future.
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

            // Draw Zeus using sprite or fallback
            if (zeusSprite != null && zeusSprite.IsLoaded)
            {
                // Zeus is stationary for now (no logic yet)
                bool isMoving = false;
                zeusSprite.Draw(spriteBatch, zeusPosition, isMoving, gameTime, Color.White, 10f, SpriteEffects.None);
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
                spriteBatch.Draw(solidTexture, zeusRect, new Color(220, 220, 240));
            }

            player.Draw(gameTime, spriteBatch);

            spriteBatch.End();
        }
    }
}
