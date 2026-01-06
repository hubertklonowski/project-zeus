using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Constants;

namespace ProjectZeus.Core.Rendering
{
    /// <summary>
    /// Helper methods for drawing common game elements
    /// </summary>
    public static class DrawingHelpers
    {
        /// <summary>
        /// Draws an animated portal with pulsing colors
        /// </summary>
        public static void DrawPortal(SpriteBatch spriteBatch, Texture2D portalTexture, Rectangle portalRect, GameTime gameTime, Color baseColor)
        {
            if (portalTexture == null)
                return;

            float portalTime = (float)gameTime.TotalGameTime.TotalSeconds;
            float pulse = (float)(Math.Sin(portalTime * GameConstants.PortalPulseFrequency) * GameConstants.PortalPulseAmplitude + GameConstants.PortalPulseOffset);

            // If we have a 1x1 white texture (fallback), draw the old multi-layer portal effect
            if (portalTexture.Width == 1 && portalTexture.Height == 1)
            {
                float fastPulse = (float)Math.Sin(portalTime * GameConstants.PortalPulseFrequency * 2f) * 0.5f + 0.5f;

                Color portalColor1 = new Color(
                    (byte)(GameConstants.PortalOuterRed * pulse), 
                    (byte)(GameConstants.PortalOuterGreen * pulse), 
                    (byte)(GameConstants.PortalOuterBlue * pulse));
                Color portalColor2 = new Color(
                    (byte)(GameConstants.PortalInnerRed * fastPulse), 
                    (byte)(GameConstants.PortalInnerGreen * fastPulse), 
                    (byte)(GameConstants.PortalInnerBlue * fastPulse));

                spriteBatch.Draw(portalTexture, portalRect, baseColor * pulse);

                Rectangle innerRect = portalRect;
                innerRect.Inflate(-8, -8);
                spriteBatch.Draw(portalTexture, innerRect, portalColor1);

                Rectangle coreRect = portalRect;
                coreRect.Inflate(-16, -16);
                spriteBatch.Draw(portalTexture, coreRect, portalColor2);

                DrawRectangleOutline(spriteBatch, portalTexture, portalRect, 
                    new Color(GameConstants.PortalOuterRed, GameConstants.PortalOuterGreen, GameConstants.PortalOuterBlue));
            }
            else
            {
                // For actual sprite textures like vase.aseprite, draw them with consistent sizing and ground placement
                
                // Define a standard vase size that works well for all portals
                const int standardVaseWidth = 48;
                const int standardVaseHeight = 64;
                
                // Calculate the ground level (bottom of the portal rect represents ground level)
                int groundY = portalRect.Bottom;
                
                // Position vase on the ground, centered horizontally within the portal area
                Rectangle spriteRect = new Rectangle(
                    portalRect.X + (portalRect.Width - standardVaseWidth) / 2,
                    groundY - standardVaseHeight,
                    standardVaseWidth,
                    standardVaseHeight);

                // Add a subtle pulsing effect to the sprite color
                Color spriteColor = Color.White * (0.8f + pulse * 0.2f);
                spriteBatch.Draw(portalTexture, spriteRect, spriteColor);
            }
        }

        /// <summary>
        /// Draws a rectangle outline
        /// </summary>
        public static void DrawRectangleOutline(SpriteBatch spriteBatch, Texture2D texture, Rectangle rect, Color color)
        {
            if (texture == null)
                return;

            const int outlineThickness = 2;
            
            spriteBatch.Draw(texture, new Rectangle(rect.X, rect.Y, rect.Width, outlineThickness), color);
            spriteBatch.Draw(texture, new Rectangle(rect.X, rect.Bottom - outlineThickness, rect.Width, outlineThickness), color);
            spriteBatch.Draw(texture, new Rectangle(rect.X, rect.Y, outlineThickness, rect.Height), color);
            spriteBatch.Draw(texture, new Rectangle(rect.Right - outlineThickness, rect.Y, outlineThickness, rect.Height), color);
        }

        /// <summary>
        /// Creates a simple 1x1 solid color texture
        /// </summary>
        public static Texture2D CreateSolidTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++)
                data[i] = color;
            texture.SetData(data);
            return texture;
        }
    }
}
