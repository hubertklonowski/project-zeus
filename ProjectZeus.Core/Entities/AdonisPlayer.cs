using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Platformer2D;
using ProjectZeus.Core.Constants;

namespace ProjectZeus.Core.Entities
{
    /// <summary>
    /// Animated player character using Adonis sprite sheets
    /// </summary>
    public class AdonisPlayer
    {
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private AnimationPlayer sprite;
        private SpriteEffects flip = SpriteEffects.None;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public bool IsOnGround { get; set; }
        public Vector2 Size => GameConstants.PlayerSize;

        public Rectangle Bounds => new Rectangle(
            (int)Position.X, 
            (int)Position.Y, 
            (int)Size.X, 
            (int)Size.Y);

        public AdonisPlayer()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            IsOnGround = false;
        }

        public void LoadContent(ContentManager content)
        {
            idleAnimation = new Animation(content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
            runAnimation = new Animation(content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            
            sprite.PlayAnimation(idleAnimation);
        }

        public void Update(GameTime gameTime)
        {
            if (IsOnGround)
            {
                if (System.Math.Abs(Velocity.X) > 0.02f)
                {
                    sprite.PlayAnimation(runAnimation);
                }
                else
                {
                    sprite.PlayAnimation(idleAnimation);
                }
            }
            else
            {
                sprite.PlayAnimation(jumpAnimation);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            Vector2 bottomCenter = new Vector2(Position.X + Size.X / 2f, Position.Y + Size.Y);
            sprite.Draw(gameTime, spriteBatch, bottomCenter, flip);
        }
    }
}
