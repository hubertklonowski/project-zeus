using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace ProjectZeus.Core.Game
{
    /// <summary>
    /// Manages keyboard and gamepad input state
    /// </summary>
    public class InputManager
    {
        public GamePadState GamePadState { get; private set; }
        public KeyboardState KeyboardState { get; private set; }
        public KeyboardState PreviousKeyboardState { get; private set; }
        public TouchCollection TouchState { get; private set; }

        public void Update()
        {
            PreviousKeyboardState = KeyboardState;
            GamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState = Keyboard.GetState();
            TouchState = TouchPanel.GetState();
        }

        public bool IsKeyJustPressed(Keys key)
        {
            return KeyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key);
        }

        public float GetHorizontalMovement()
        {
            float move = 0f;
            if (KeyboardState.IsKeyDown(Keys.Left) || KeyboardState.IsKeyDown(Keys.A))
                move -= 1f;
            if (KeyboardState.IsKeyDown(Keys.Right) || KeyboardState.IsKeyDown(Keys.D))
                move += 1f;
            return move;
        }

        public bool IsJumpPressed()
        {
            return KeyboardState.IsKeyDown(Keys.Space) || KeyboardState.IsKeyDown(Keys.Up);
        }
    }
}
