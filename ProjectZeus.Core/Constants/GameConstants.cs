using Microsoft.Xna.Framework;

namespace ProjectZeus.Core.Constants
{
    /// <summary>
    /// Global constants used throughout the game
    /// </summary>
    public static class GameConstants
    {
        public static readonly Vector2 BaseScreenSize = new Vector2(800, 480);
        
        // Physics constants
        public const float MoveSpeed = 180f;
        public const float JumpVelocity = -560f;
        public const float Gravity = 900f;
        
        // Ground constants
        public const float GroundHeight = 20f;
        
        // Player constants
        public static readonly Vector2 PlayerSize = new Vector2(32, 48);
        
        // Portal animation constants
        public const int PortalOuterRed = 255;
        public const int PortalOuterGreen = 100;
        public const int PortalOuterBlue = 255;
        public const int PortalInnerRed = 200;
        public const int PortalInnerGreen = 50;
        public const int PortalInnerBlue = 200;
        
        public const float PortalPulseFrequency = 3f;
        public const float PortalPulseAmplitude = 0.3f;
        public const float PortalPulseOffset = 0.7f;
    }
}
