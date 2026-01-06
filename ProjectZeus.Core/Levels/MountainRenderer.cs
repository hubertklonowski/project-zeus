using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Handles rendering for the mountain level
    /// </summary>
    public class MountainRenderer
    {
        private readonly Vector2 baseScreenSize;
        private Texture2D solidTexture;

        public MountainRenderer(Vector2 baseScreenSize)
        {
            this.baseScreenSize = baseScreenSize;
        }

        public void LoadContent(Texture2D solidTexture)
        {
            this.solidTexture = solidTexture;
        }

        public void DrawBackground(SpriteBatch spriteBatch)
        {
            Color skyColor = new Color(135, 206, 235);
            Color mountainColor = new Color(139, 137, 137);
            Color snowColor = new Color(255, 250, 250);

            spriteBatch.Draw(solidTexture, new Rectangle(0, 0, (int)baseScreenSize.X, (int)baseScreenSize.Y), skyColor);

            int mountainBase = (int)(baseScreenSize.Y * 0.6f);
            int mountainHeight = (int)(baseScreenSize.Y * 0.4f);
            int leftPeak = (int)(baseScreenSize.X * 0.3f);
            int rightPeak = (int)(baseScreenSize.X * 0.7f);

            DrawTriangle(spriteBatch, new Vector2(0, mountainBase), new Vector2(leftPeak, mountainBase - mountainHeight), 
                new Vector2(baseScreenSize.X * 0.5f, mountainBase), mountainColor);
            DrawTriangle(spriteBatch, new Vector2(baseScreenSize.X * 0.5f, mountainBase), new Vector2(rightPeak, mountainBase - mountainHeight), 
                new Vector2(baseScreenSize.X, mountainBase), mountainColor);

            int snowLine = (int)(baseScreenSize.Y * 0.35f);
            DrawTriangle(spriteBatch, new Vector2(leftPeak - 50, snowLine), new Vector2(leftPeak, mountainBase - mountainHeight), 
                new Vector2(leftPeak + 50, snowLine), snowColor);
            DrawTriangle(spriteBatch, new Vector2(rightPeak - 50, snowLine), new Vector2(rightPeak, mountainBase - mountainHeight), 
                new Vector2(rightPeak + 50, snowLine), snowColor);
        }

        public void DrawPlatforms(SpriteBatch spriteBatch, List<Platform> platforms)
        {
            Color platformColor = new Color(101, 67, 33);
            foreach (var platform in platforms)
            {
                spriteBatch.Draw(solidTexture, platform.Bounds, platformColor);
            }
        }

        public void DrawGoat(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            Color goatColor = new Color(200, 200, 200);
            Rectangle goatRect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            spriteBatch.Draw(solidTexture, goatRect, goatColor);
        }

        public void DrawRocks(SpriteBatch spriteBatch, List<Rock> rocks)
        {
            Color rockColor = new Color(128, 128, 128);
            foreach (var rock in rocks)
            {
                if (rock.Active)
                {
                    Rectangle rockRect = new Rectangle((int)rock.Position.X, (int)rock.Position.Y, (int)rock.Size.X, (int)rock.Size.Y);
                    spriteBatch.Draw(solidTexture, rockRect, rockColor);
                }
            }
        }

        public void DrawItem(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            Color itemColor = Color.Gold;
            Rectangle itemRect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            spriteBatch.Draw(solidTexture, itemRect, itemColor);
            DrawRectangleOutline(spriteBatch, itemRect, Color.Yellow);
        }

        public void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            int thickness = 2;
            spriteBatch.Draw(solidTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(solidTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(solidTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(solidTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        private void DrawTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            int minX = (int)MathHelper.Min(p1.X, MathHelper.Min(p2.X, p3.X));
            int maxX = (int)MathHelper.Max(p1.X, MathHelper.Max(p2.X, p3.X));
            int minY = (int)MathHelper.Min(p1.Y, MathHelper.Min(p2.Y, p3.Y));
            int maxY = (int)MathHelper.Max(p1.Y, MathHelper.Max(p2.Y, p3.Y));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (IsPointInTriangle(new Vector2(x, y), p1, p2, p3))
                    {
                        spriteBatch.Draw(solidTexture, new Rectangle(x, y, 1, 1), color);
                    }
                }
            }
        }

        private bool IsPointInTriangle(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float d1 = Sign(p, p1, p2);
            float d2 = Sign(p, p2, p3);
            float d3 = Sign(p, p3, p1);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }
    }
}
