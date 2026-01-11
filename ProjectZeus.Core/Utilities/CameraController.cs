using Microsoft.Xna.Framework;

namespace ProjectZeus.Core.Utilities
{
    /// <summary>
    /// Reusable camera controller for side-scrolling levels
    /// </summary>
    public class CameraController
    {
        private Vector2 cameraOffset;
        private readonly float screenWidth;
        private readonly float screenHeight;
        private readonly float worldWidth;
        private readonly float worldHeight;

        public Vector2 CameraOffset => cameraOffset;

        public CameraController(float screenWidth, float screenHeight, float worldWidth, float worldHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;
            this.cameraOffset = Vector2.Zero;
        }

        /// <summary>
        /// Updates camera to follow player horizontally with smooth lerp
        /// </summary>
        public void FollowPlayerHorizontal(Vector2 playerPosition, float horizontalOffset = 0.3f, float smoothing = 0.1f)
        {
            float targetCameraX = playerPosition.X - screenWidth * horizontalOffset;
            targetCameraX = MathHelper.Clamp(targetCameraX, 0, worldWidth - screenWidth);
            cameraOffset.X = MathHelper.Lerp(cameraOffset.X, targetCameraX, smoothing);
            cameraOffset.Y = 0; // No vertical scrolling
        }

        /// <summary>
        /// Updates camera to follow player vertically with smooth lerp
        /// </summary>
        public void FollowPlayerVertical(Vector2 playerPosition, float verticalOffset = 0.6f, float smoothing = 0.1f)
        {
            float targetCameraY = playerPosition.Y - screenHeight * verticalOffset;
            targetCameraY = MathHelper.Clamp(targetCameraY, 0, worldHeight - screenHeight);
            cameraOffset.Y = MathHelper.Lerp(cameraOffset.Y, targetCameraY, smoothing);
            cameraOffset.X = 0; // No horizontal scrolling
        }

        /// <summary>
        /// Sets camera to show bottom of level
        /// </summary>
        public void SetToBottom()
        {
            cameraOffset = new Vector2(0, worldHeight - screenHeight);
        }

        /// <summary>
        /// Resets camera to origin
        /// </summary>
        public void Reset()
        {
            cameraOffset = Vector2.Zero;
        }

        /// <summary>
        /// Gets camera transformation matrix for rendering
        /// </summary>
        public Matrix GetTransform()
        {
            return Matrix.CreateTranslation(-cameraOffset.X, -cameraOffset.Y, 0);
        }
    }
}
