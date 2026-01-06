using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Builds the mountain platform structure for the mountain climbing level
    /// </summary>
    public static class MountainPlatformBuilder
    {
        public static List<Platform> BuildPlatforms(Vector2 screenSize)
        {
            var platforms = new List<Platform>();
            
            // Ground at bottom
            platforms.Add(new Platform
            {
                Position = new Vector2(0, screenSize.Y - 20),
                Size = new Vector2(screenSize.X, 20),
                Color = new Color(100, 80, 60)
            });
            
            // COMPLEX MAZE STRUCTURE - Much larger and more challenging
            // The maze forces players to navigate left and right, with dead ends and tricky jumps
            
            // ===== LEVEL 1 - Starting platforms (Y: 410-430) =====
            platforms.Add(new Platform { Position = new Vector2(50, 430), Size = new Vector2(80, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(200, 420), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(350, 425), Size = new Vector2(75, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(500, 420), Size = new Vector2(80, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(660, 430), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            
            // ===== LEVEL 2 - Lower maze (Y: 360-380) =====
            platforms.Add(new Platform { Position = new Vector2(100, 375), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(10, 360), Size = new Vector2(60, 15), Color = new Color(120, 100, 80) }); // Dead end on left
            platforms.Add(new Platform { Position = new Vector2(250, 365), Size = new Vector2(80, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(410, 370), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(560, 365), Size = new Vector2(75, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(720, 370), Size = new Vector2(60, 15), Color = new Color(120, 100, 80) }); // Dead end on right
            
            // ===== LEVEL 3 - Mid-lower maze (Y: 305-325) =====
            platforms.Add(new Platform { Position = new Vector2(30, 315), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(170, 310), Size = new Vector2(80, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(320, 305), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(470, 315), Size = new Vector2(75, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(620, 310), Size = new Vector2(80, 15), Color = new Color(120, 100, 80) });
            
            // ===== LEVEL 4 - Middle maze (Y: 250-270) =====
            platforms.Add(new Platform { Position = new Vector2(80, 265), Size = new Vector2(75, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(230, 255), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(380, 250), Size = new Vector2(85, 15), Color = new Color(120, 100, 80) }); // Central platform
            platforms.Add(new Platform { Position = new Vector2(530, 260), Size = new Vector2(75, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(680, 255), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            
            // ===== LEVEL 5 - Mid-upper maze (Y: 195-215) =====
            platforms.Add(new Platform { Position = new Vector2(50, 210), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(190, 200), Size = new Vector2(80, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(340, 195), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(480, 205), Size = new Vector2(75, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(630, 200), Size = new Vector2(80, 15), Color = new Color(120, 100, 80) });
            
            // ===== LEVEL 6 - Upper maze (Y: 140-160) =====
            platforms.Add(new Platform { Position = new Vector2(100, 155), Size = new Vector2(75, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(250, 145), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(390, 140), Size = new Vector2(80, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(540, 150), Size = new Vector2(75, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(690, 145), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            
            // ===== LEVEL 7 - High platforms (Y: 85-105) =====
            platforms.Add(new Platform { Position = new Vector2(70, 100), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(210, 90), Size = new Vector2(75, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(700, 90), Size = new Vector2(80, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(560, 95), Size = new Vector2(75, 15), Color = new Color(120, 100, 80) });
            
            // ===== LEVEL 8 - Near top (Y: 50-65) =====
            platforms.Add(new Platform { Position = new Vector2(140, 60), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(280, 55), Size = new Vector2(65, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(420, 50), Size = new Vector2(70, 15), Color = new Color(120, 100, 80) });
            platforms.Add(new Platform { Position = new Vector2(560, 55), Size = new Vector2(65, 15), Color = new Color(120, 100, 80) });
            
            // ===== TOP LEVEL - Goat platform (Y: 30) =====
            // Top platform where goat patrols - center of screen
            platforms.Add(new Platform
            {
                Position = new Vector2(screenSize.X / 2f - 150f, 30),
                Size = new Vector2(300, 15),
                Color = new Color(140, 120, 100)
            });
            
            return platforms;
        }
    }
}
