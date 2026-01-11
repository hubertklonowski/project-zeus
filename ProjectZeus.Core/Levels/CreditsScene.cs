using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Credits scene displayed after game completion
    /// </summary>
    public class CreditsScene
    {
        private readonly Vector2 baseScreenSize = new Vector2(800, 480);
        private Texture2D solidTexture;
        private SpriteFont font;
        private float displayTimer;
        private const float DisplayDuration = 100f;
        private const float MinimumDisplayTime = 0.5f; // Prevent immediate skip from last key press
        
        private KeyboardState previousKeyState;
        
        public bool IsComplete { get; private set; }
        
        public CreditsScene()
        {
            IsComplete = false;
            displayTimer = 0f;
        }
        
        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            solidTexture = new Texture2D(graphicsDevice, 1, 1);
            solidTexture.SetData(new[] { Color.White });
            this.font = font;
        }
        
        public void Reset()
        {
            IsComplete = false;
            displayTimer = 0f;
            previousKeyState = Keyboard.GetState();
        }
        
        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            displayTimer += dt;
            
            // Only allow skipping with SPACE after minimum display time
            // Use key press detection to prevent holding from previous scene
            bool spacePressed = keyboardState.IsKeyDown(Keys.Space) && !previousKeyState.IsKeyDown(Keys.Space);
            
            if (spacePressed && displayTimer >= MinimumDisplayTime)
            {
                IsComplete = true;
            }
            
            // Auto-complete after duration
            if (displayTimer >= DisplayDuration)
            {
                IsComplete = true;
            }
            
            previousKeyState = keyboardState;
        }
        
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(new Color(20, 20, 40));
            
            if (solidTexture == null || font == null)
                return;
            
            // Use LinearClamp for better text quality when scaling
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, null);
            
            // Draw background gradient effect
            Rectangle bgRect = new Rectangle(0, 0, (int)baseScreenSize.X, (int)baseScreenSize.Y);
            spriteBatch.Draw(solidTexture, bgRect, new Color(20, 20, 40));
            
            // Title
            string title = "PROJECT ZEUS";
            float titleScale = 2.5f;
            Vector2 titleSize = font.MeasureString(title) * titleScale;
            Vector2 titlePos = new Vector2((baseScreenSize.X - titleSize.X) / 2f, 80);
            
            // Draw title with shadow for depth
            spriteBatch.DrawString(font, title, titlePos + new Vector2(3, 3), Color.Black, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, title, titlePos, Color.Gold, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
            
            // Credits
            float creditsY = 220;
            float lineSpacing = 50;
            float creditsScale = 1.5f;
            
            // Programmer credit
            string programmerLabel = "Programmer";
            string programmerName = "Hubert K³onowski";
            DrawCreditLine(spriteBatch, programmerLabel, programmerName, creditsY, creditsScale);
            
            // Artist credit
            creditsY += lineSpacing;
            string artistLabel = "Artist";
            string artistName = "Justyna Witkowska";
            DrawCreditLine(spriteBatch, artistLabel, artistName, creditsY, creditsScale);
            
            // Skip hint
            string skipHint = "Press SPACE to continue";
            Vector2 skipSize = font.MeasureString(skipHint);
            Vector2 skipPos = new Vector2((baseScreenSize.X - skipSize.X) / 2f, baseScreenSize.Y - 50);
            
            // Fade in/out effect for skip hint (only show after minimum display time)
            if (displayTimer >= MinimumDisplayTime)
            {
                float alpha = (float)Math.Sin(displayTimer * 3) * 0.3f + 0.7f;
                spriteBatch.DrawString(font, skipHint, skipPos, Color.White * alpha);
            }
            
            spriteBatch.End();
        }
        
        private void DrawCreditLine(SpriteBatch spriteBatch, string label, string name, float y, float scale)
        {
            Vector2 labelSize = font.MeasureString(label) * scale;
            Vector2 nameSize = font.MeasureString(name) * scale;
            
            float centerX = baseScreenSize.X / 2f;
            float labelX = centerX - labelSize.X - 20;
            float nameX = centerX + 20;
            
            Vector2 labelPos = new Vector2(labelX, y);
            Vector2 namePos = new Vector2(nameX, y);
            
            // Draw label
            spriteBatch.DrawString(font, label, labelPos + new Vector2(2, 2), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, label, labelPos, Color.LightGray, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            
            // Draw name
            spriteBatch.DrawString(font, name, namePos + new Vector2(2, 2), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, name, namePos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
