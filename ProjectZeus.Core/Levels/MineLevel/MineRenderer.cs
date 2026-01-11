using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectZeus.Core.Levels.MineLevel
{
    /// <summary>
    /// Handles rendering for the mine level
    /// </summary>
    public class MineRenderer
    {
        private readonly Texture2D solidTexture;

        public MineRenderer(Texture2D solidTexture)
        {
            this.solidTexture = solidTexture;
        }

        public void DrawBackground(SpriteBatch spriteBatch, float cameraOffsetX, float screenWidth, float screenHeight)
        {
            Rectangle bgRect = new Rectangle((int)cameraOffsetX, 0, (int)screenWidth + 100, (int)screenHeight);
            spriteBatch.Draw(solidTexture, bgRect, new Color(20, 15, 30));

            for (int x = (int)(cameraOffsetX / 200) * 200; x < cameraOffsetX + screenWidth + 200; x += 200)
            {
                int patchY = 100 + (x / 200 % 3) * 80;
                Rectangle patch = new Rectangle(x, patchY, 150, 100);
                spriteBatch.Draw(solidTexture, patch, new Color(15, 10, 25));
            }
        }

        public void DrawRails(SpriteBatch spriteBatch, float groundTop, float worldWidth, float cameraOffsetX, float screenWidth)
        {
            int railY1 = (int)(groundTop - 8);
            int railY2 = (int)(groundTop - 3);

            Rectangle leftRail = new Rectangle(0, railY1, (int)worldWidth, 3);
            spriteBatch.Draw(solidTexture, leftRail, new Color(100, 100, 110));

            Rectangle rightRail = new Rectangle(0, railY2, (int)worldWidth, 3);
            spriteBatch.Draw(solidTexture, rightRail, new Color(100, 100, 110));

            for (int x = 0; x < worldWidth; x += 25)
            {
                if (x >= cameraOffsetX - 30 && x <= cameraOffsetX + screenWidth + 30)
                {
                    Rectangle tie = new Rectangle(x, railY1 - 2, 15, 12);
                    spriteBatch.Draw(solidTexture, tie, new Color(101, 67, 33));
                }
            }
        }

        public void DrawTorches(SpriteBatch spriteBatch, GameTime gameTime, float groundTop, float worldWidth, float cameraOffsetX, float screenWidth)
        {
            float flickerTime = (float)gameTime.TotalGameTime.TotalSeconds;

            for (int x = 100; x < worldWidth; x += 300)
            {
                if (x >= cameraOffsetX - 50 && x <= cameraOffsetX + screenWidth + 50)
                {
                    Rectangle torchPost = new Rectangle(x, (int)(groundTop - 45), 8, 45);
                    spriteBatch.Draw(solidTexture, torchPost, new Color(101, 67, 33));

                    Rectangle bracket = new Rectangle(x - 3, (int)(groundTop - 50), 14, 8);
                    spriteBatch.Draw(solidTexture, bracket, new Color(80, 50, 25));

                    float flicker = (float)System.Math.Sin(flickerTime * 8f + x * 0.1f) * 0.2f + 0.8f;
                    Rectangle flame = new Rectangle(x - 4, (int)(groundTop - 65), 16, 15);
                    spriteBatch.Draw(solidTexture, flame, new Color((byte)(255 * flicker), (byte)(200 * flicker), 50));

                    Rectangle innerFlame = new Rectangle(x - 1, (int)(groundTop - 62), 10, 10);
                    spriteBatch.Draw(solidTexture, innerFlame, new Color((byte)(255 * flicker), (byte)(255 * flicker), 100));
                }
            }
        }
    }
}
