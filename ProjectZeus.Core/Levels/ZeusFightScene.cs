using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Entities;

namespace ProjectZeus.Core
{
    /// <summary>
    /// Placeholder for the Zeus fight scene. Currently draws a simple background.
    /// </summary>
    public class ZeusFightScene
    {
        public bool IsCompleted { get; private set; }

        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        private Texture2D solidTexture;
        private SpriteFont titleFont;

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

            int zeusWidth = 80;
            int zeusHeight = 120;
            int zeusMarginFromLeft = 40;
            Rectangle zeusRect = new Rectangle(
                zeusMarginFromLeft,
                groundRect.Y - zeusHeight,
                zeusWidth,
                zeusHeight);
            spriteBatch.Draw(solidTexture, zeusRect, new Color(220, 220, 240));

            player.Draw(gameTime, spriteBatch);

            if (titleFont != null)
            {
                string title = "Zeus Fight (placeholder scene)";
                Vector2 size = titleFont.MeasureString(title);
                Vector2 pos = new Vector2((baseScreenSize.X - size.X) / 2f, 40f);
                spriteBatch.DrawString(titleFont, title, pos, Color.Gold);
            }

            spriteBatch.End();
        }
    }
}
