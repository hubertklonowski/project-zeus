using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ProjectZeus.Core.Levels
{
    /// <summary>
    /// Builds the mountain platform structure for the mountain climbing level
    /// </summary>
    public static class MountainPlatformBuilder
    {
        // Total world height for the mountain level (approximately 4.5 screens)
        public const float WorldHeight = 2160f;
        
        // Platform generation constants
        private const float GroundHeight = 20f;
        private const float PlatformHeight = 15f;
        private const float LevelSpacing = 70f; // Vertical spacing between platform levels
        private const int PlatformsPerLevel = 5;
        private const float MinPlatformWidth = 70f;
        private const float MaxPlatformWidth = 85f;
        private const float ScreenWidth = 800f;
        
        // Color progression as player climbs (gets lighter toward top)
        private static readonly Color[] LevelColors =
        [
            new Color(120, 100, 80),  // Screen 1
            new Color(125, 105, 85),  // Screen 2
            new Color(130, 110, 90),  // Screen 3
            new Color(135, 115, 95),  // Screen 4
            new Color(140, 120, 100), // Screen 5
            new Color(145, 125, 105), // Near summit
            new Color(150, 130, 110)  // Summit
        ];
        
        public static List<Platform> BuildPlatforms(Vector2 screenSize)
        {
            var platforms = new List<Platform>();
            float baseY = WorldHeight - GroundHeight;
            
            // Ground at bottom of the extended world
            platforms.Add(new Platform
            {
                Position = new Vector2(0, baseY),
                Size = new Vector2(screenSize.X, GroundHeight),
                Color = new Color(100, 80, 60)
            });
            
            // Generate platform levels procedurally
            int totalLevels = 30;
            float currentY = baseY - 50f; // Start first level above ground
            
            for (int level = 0; level < totalLevels; level++)
            {
                // Determine color based on screen section
                int colorIndex = level / 6; // Change color every 6 levels
                if (colorIndex >= LevelColors.Length)
                    colorIndex = LevelColors.Length - 1;
                Color levelColor = LevelColors[colorIndex];
                
                // Generate platforms for this level
                int platformCount = GetPlatformCount(level, totalLevels);
                var levelPlatforms = GenerateLevelPlatforms(level, currentY, platformCount, levelColor);
                platforms.AddRange(levelPlatforms);
                
                // Move up to next level with slight variation
                float spacing = LevelSpacing + GetHeightVariation(level);
                currentY -= spacing;
            }
            
            // Add the final goat platform at the top
            platforms.Add(new Platform
            {
                Position = new Vector2(screenSize.X / 2f - 150f, baseY - 2120),
                Size = new Vector2(300, PlatformHeight),
                Color = LevelColors[^1] // Last color
            });
            
            return platforms;
        }
        
        /// <summary>
        /// Gets the number of platforms for a given level (fewer near top for challenge)
        /// </summary>
        private static int GetPlatformCount(int level, int totalLevels)
        {
            // Reduced platform count across all levels for increased difficulty
            if (level >= totalLevels - 2)
                return 2; // Was 3
            if (level >= totalLevels - 4)
                return 3; // Was 4
            return 3; // Was 5 - reduced everywhere
        }
        
        /// <summary>
        /// Gets height variation to make levels feel less uniform
        /// </summary>
        private static float GetHeightVariation(int level)
        {
            // Use a simple pattern for variation instead of random (for consistency)
            return (level % 3) switch
            {
                0 => 0f,
                1 => 5f,
                2 => -5f,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Generates platforms for a single level with horizontal distribution
        /// </summary>
        private static List<Platform> GenerateLevelPlatforms(int level, float y, int count, Color color)
        {
            var platforms = new List<Platform>();
            
            // Calculate horizontal spacing based on platform count
            float usableWidth = ScreenWidth - 60f; // Leave margins on sides
            float sectionWidth = usableWidth / count;
            
            for (int i = 0; i < count; i++)
            {
                // Calculate base X position with offset pattern
                float baseX = 30f + (i * sectionWidth);
                
                // Add horizontal offset based on level and platform index for variety
                float offsetPattern = GetHorizontalOffset(level, i);
                float x = baseX + offsetPattern;
                
                // Clamp to screen bounds
                x = MathHelper.Clamp(x, 30f, ScreenWidth - MaxPlatformWidth - 30f);
                
                // Vary platform width
                float width = GetPlatformWidth(level, i);
                
                // Add slight Y variation for visual interest
                float yOffset = GetVerticalOffset(level, i);
                
                platforms.Add(new Platform
                {
                    Position = new Vector2(x, y + yOffset),
                    Size = new Vector2(width, PlatformHeight),
                    Color = color
                });
            }
            
            return platforms;
        }
        
        /// <summary>
        /// Gets horizontal offset for platform variety using deterministic pattern
        /// </summary>
        private static float GetHorizontalOffset(int level, int platformIndex)
        {
            // Create a wave-like pattern that varies by level
            int pattern = (level + platformIndex) % 5;
            return pattern switch
            {
                0 => 0f,
                1 => 20f,
                2 => -10f,
                3 => 15f,
                4 => -15f,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Gets vertical offset for slight height variation within a level
        /// </summary>
        private static float GetVerticalOffset(int level, int platformIndex)
        {
            // Alternate slight vertical offsets
            int pattern = (level * 3 + platformIndex) % 4;
            return pattern switch
            {
                0 => 0f,
                1 => -5f,
                2 => 5f,
                3 => -10f,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Gets platform width with variation
        /// </summary>
        private static float GetPlatformWidth(int level, int platformIndex)
        {
            // Vary width based on position
            int pattern = (level + platformIndex * 2) % 4;
            return pattern switch
            {
                0 => 70f,
                1 => 75f,
                2 => 80f,
                3 => 85f,
                _ => 75f
            };
        }
        
        /// <summary>
        /// Gets the Y position of the top platform where the goat patrols
        /// </summary>
        public static float GetTopPlatformY()
        {
            return WorldHeight - GroundHeight - 2120; // baseY - 2120
        }
    }
}
