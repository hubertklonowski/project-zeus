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
        private const float LevelSpacing = 70f; // Reduced spacing for achievable jumps
        private const float MinPlatformWidth = 65f;
        private const float MaxPlatformWidth = 80f;
        private const float ScreenWidth = 800f;
        
        // Moving platform constants
        private const float MovingPlatformSpeed = 50f; // Pixels per second
        
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
        
        public static (List<Platform> staticPlatforms, List<MovingPlatform> movingPlatforms) BuildPlatforms(Vector2 screenSize)
        {
            var staticPlatforms = new List<Platform>();
            var movingPlatforms = new List<MovingPlatform>();
            float baseY = WorldHeight - GroundHeight;
            
            // Ground at bottom of the extended world
            staticPlatforms.Add(new Platform
            {
                Position = new Vector2(0, baseY),
                Size = new Vector2(screenSize.X, GroundHeight),
                Color = new Color(100, 80, 60)
            });
            
            // Generate platform levels with zigzag pattern
            int totalLevels = 28;
            float currentY = baseY - 55f; // Start first level above ground
            
            for (int level = 0; level < totalLevels; level++)
            {
                // Determine color based on screen section
                int colorIndex = level / 5;
                if (colorIndex >= LevelColors.Length)
                    colorIndex = LevelColors.Length - 1;
                Color levelColor = LevelColors[colorIndex];
                
                // Every 5th level (starting at level 4) is a moving platform level
                bool isMovingPlatformLevel = (level % 5) == 4 && level > 3;
                
                if (isMovingPlatformLevel)
                {
                    // Create a moving platform
                    var movingPlatform = GenerateMovingPlatform(level, currentY, levelColor);
                    movingPlatforms.Add(movingPlatform);
                }
                else
                {
                    // Generate static platforms - zigzag pattern with stepping stones
                    var levelPlatforms = GenerateLevelPlatforms(level, currentY, levelColor);
                    staticPlatforms.AddRange(levelPlatforms);
                }
                
                // Move up to next level with slight variation
                float spacing = LevelSpacing + GetHeightVariation(level);
                currentY -= spacing;
            }
            
            // Add the final goat platform at the top - increased width for goat and item
            staticPlatforms.Add(new Platform
            {
                Position = new Vector2(screenSize.X / 2f - 200f, baseY - 2120),
                Size = new Vector2(400, PlatformHeight),
                Color = LevelColors[^1]
            });
            
            return (staticPlatforms, movingPlatforms);
        }
        
        /// <summary>
        /// Generates a moving platform for the specified level
        /// </summary>
        private static MovingPlatform GenerateMovingPlatform(int level, float y, Color color)
        {
            float platformWidth = 90f; // Wide for easier landing
            
            // Moving platforms travel a shorter distance (middle portion of screen)
            bool startsOnLeft = (level % 2) == 0;
            
            Vector2 startPos;
            Vector2 endPos;
            
            if (startsOnLeft)
            {
                startPos = new Vector2(150f, y);
                endPos = new Vector2(ScreenWidth - platformWidth - 150f, y);
            }
            else
            {
                startPos = new Vector2(ScreenWidth - platformWidth - 150f, y);
                endPos = new Vector2(150f, y);
            }
            
            return new MovingPlatform
            {
                Position = startPos,
                StartPosition = startPos,
                EndPosition = endPos,
                Size = new Vector2(platformWidth, PlatformHeight),
                Color = new Color(180, 160, 140),
                Speed = MovingPlatformSpeed,
                Progress = 0f,
                MovingToEnd = true
            };
        }
        
        /// <summary>
        /// Gets height variation to make levels feel less uniform
        /// </summary>
        private static float GetHeightVariation(int level)
        {
            return (level % 4) switch
            {
                0 => 0f,
                1 => 5f,
                2 => -3f,
                3 => 8f,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Generates platforms for a single level with zigzag pattern
        /// </summary>
        private static List<Platform> GenerateLevelPlatforms(int level, float y, Color color)
        {
            var platforms = new List<Platform>();
            
            // Create a zigzag pattern: left-center-right-center-left...
            // This requires diagonal jumps but not impossible ones
            int pattern = level % 4;
            float mainPlatformWidth = GetPlatformWidth(level);
            float mainPlatformX;
            
            switch (pattern)
            {
                case 0: // Left side
                    mainPlatformX = 80f + GetHorizontalVariation(level);
                    break;
                case 1: // Center-left
                    mainPlatformX = 250f + GetHorizontalVariation(level);
                    break;
                case 2: // Center-right
                    mainPlatformX = 450f + GetHorizontalVariation(level);
                    break;
                case 3: // Right side
                default:
                    mainPlatformX = ScreenWidth - mainPlatformWidth - 80f - GetHorizontalVariation(level);
                    break;
            }
            
            // Clamp to screen bounds
            mainPlatformX = MathHelper.Clamp(mainPlatformX, 40f, ScreenWidth - mainPlatformWidth - 40f);
            
            // Add slight Y variation
            float yOffset = GetVerticalOffset(level);
            
            // Add main platform
            platforms.Add(new Platform
            {
                Position = new Vector2(mainPlatformX, y + yOffset),
                Size = new Vector2(mainPlatformWidth, PlatformHeight),
                Color = color
            });
            
            // Add a helper stepping stone on alternating levels to assist with longer jumps
            if (level % 2 == 1)
            {
                float stepWidth = 50f;
                float stepX;
                
                // Position stepping stone between current and next expected platform position
                if (pattern == 0 || pattern == 1)
                {
                    // Going right, place step to the right of main
                    stepX = mainPlatformX + mainPlatformWidth + 80f;
                }
                else
                {
                    // Going left, place step to the left of main
                    stepX = mainPlatformX - stepWidth - 80f;
                }
                
                stepX = MathHelper.Clamp(stepX, 50f, ScreenWidth - stepWidth - 50f);
                
                platforms.Add(new Platform
                {
                    Position = new Vector2(stepX, y + 10f),
                    Size = new Vector2(stepWidth, PlatformHeight),
                    Color = color
                });
            }
            
            return platforms;
        }
        
        /// <summary>
        /// Gets horizontal variation to prevent perfectly aligned platforms
        /// </summary>
        private static float GetHorizontalVariation(int level)
        {
            int pattern = level % 5;
            return pattern switch
            {
                0 => 0f,
                1 => 20f,
                2 => -15f,
                3 => 25f,
                4 => -10f,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Gets vertical offset for slight height variation
        /// </summary>
        private static float GetVerticalOffset(int level)
        {
            int pattern = level % 4;
            return pattern switch
            {
                0 => 0f,
                1 => -5f,
                2 => 3f,
                3 => -8f,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Gets platform width with variation
        /// </summary>
        private static float GetPlatformWidth(int level)
        {
            int pattern = level % 3;
            return pattern switch
            {
                0 => 65f,
                1 => 75f,
                2 => 80f,
                _ => 70f
            };
        }
        
        /// <summary>
        /// Gets the Y position of the top platform where the goat patrols
        /// </summary>
        public static float GetTopPlatformY()
        {
            return WorldHeight - GroundHeight - 2120;
        }
    }
}
