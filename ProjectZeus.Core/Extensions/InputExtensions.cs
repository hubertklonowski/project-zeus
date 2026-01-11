using Microsoft.Xna.Framework.Input;

namespace ProjectZeus.Core.Extensions
{
    /// <summary>
    /// Extension methods for input handling
    /// </summary>
    public static class InputExtensions
    {
        /// <summary>
        /// Gets horizontal movement input (-1 for left, 1 for right, 0 for none)
        /// </summary>
        public static float GetHorizontalInput(this KeyboardState keyboardState)
        {
            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                move += 1f;
            return move;
        }

        /// <summary>
        /// Gets vertical movement input (-1 for up, 1 for down, 0 for none)
        /// </summary>
        public static float GetVerticalInput(this KeyboardState keyboardState)
        {
            float move = 0f;
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
                move -= 1f;
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
                move += 1f;
            return move;
        }

        /// <summary>
        /// Checks if jump key is pressed
        /// </summary>
        public static bool IsJumpPressed(this KeyboardState keyboardState)
        {
            return keyboardState.IsKeyDown(Keys.Space) || 
                   keyboardState.IsKeyDown(Keys.Up) || 
                   keyboardState.IsKeyDown(Keys.W);
        }

        /// <summary>
        /// Checks if a key was just pressed (down now but not in previous state)
        /// </summary>
        public static bool WasJustPressed(this KeyboardState currentState, KeyboardState previousState, Keys key)
        {
            return currentState.IsKeyDown(key) && !previousState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if action/interact key (E) was just pressed
        /// </summary>
        public static bool WasActionPressed(this KeyboardState currentState, KeyboardState previousState)
        {
            return currentState.WasJustPressed(previousState, Keys.E);
        }
    }
}
