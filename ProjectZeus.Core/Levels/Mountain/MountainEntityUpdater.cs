using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Levels;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Extensions;

namespace ProjectZeus.Core.Levels.Mountain
{
    /// <summary>
    /// Handles entity updates for the mountain level
    /// </summary>
    public class MountainEntityUpdater
    {
        private readonly Random random;
        private const float ThrowAngleVariation = 40f;
        private const float ThrowAngleOffset = 20f;

        public MountainEntityUpdater(Random random)
        {
            this.random = random;
        }

        public void UpdateMovingPlatforms(List<MovingPlatform> movingPlatforms, float deltaTime)
        {
            foreach (var platform in movingPlatforms)
            {
                platform.Update(deltaTime);
            }
        }

        public void UpdateGoat(ref Vector2 goatPosition, ref Vector2 goatVelocity, ref float goatThrowTimer, 
            Rectangle topPlatformBounds, float deltaTime, float goatMoveSpeed, float goatThrowInterval,
            List<Rock> rocks, AsepriteSprite rockSprite, Vector2 goatSize)
        {
            goatPosition += goatVelocity * deltaTime;
            
            float topPlatformLeft = topPlatformBounds.X;
            float topPlatformRight = topPlatformBounds.Right;
            
            if (goatPosition.X <= topPlatformLeft || goatPosition.X + goatSize.X >= topPlatformRight)
            {
                goatVelocity = new Vector2(-goatVelocity.X, 0f);
                goatPosition = new Vector2(
                    MathHelper.Clamp(goatPosition.X, topPlatformLeft, topPlatformRight - goatSize.X),
                    goatPosition.Y);
            }
            
            goatThrowTimer -= deltaTime;
            if (goatThrowTimer <= 0)
            {
                ThrowRock(goatPosition, goatSize, rocks, rockSprite);
                goatThrowTimer = goatThrowInterval;
            }
        }

        public void UpdatePlayerGoat(KeyboardState keyboardState, ref float playerRockThrowCooldown, 
            float deltaTime, Vector2 goatPosition, Vector2 goatSize, List<Rock> rocks, AsepriteSprite rockSprite,
            float playerRockThrowDelay)
        {
            playerRockThrowCooldown -= deltaTime;
            
            if (keyboardState.IsKeyDown(Keys.E) && playerRockThrowCooldown <= 0)
            {
                ThrowRock(goatPosition, goatSize, rocks, rockSprite);
                playerRockThrowCooldown = playerRockThrowDelay;
            }
        }

        public void UpdateRocks(List<Rock> rocks, float deltaTime, float worldHeight)
        {
            for (int i = rocks.Count - 1; i >= 0; i--)
            {
                rocks[i].Position += rocks[i].Velocity * deltaTime;
                rocks[i].Rotation += rocks[i].RotationSpeed * deltaTime;
                
                if (rocks[i].Position.Y > worldHeight + 50)
                {
                    rocks.RemoveAt(i);
                }
            }
        }

        private void ThrowRock(Vector2 goatPosition, Vector2 goatSize, List<Rock> rocks, AsepriteSprite rockSprite)
        {
            float angleVariation = (float)(random.NextDouble() * ThrowAngleVariation - ThrowAngleVariation / 2);
            float throwAngle = MathHelper.ToRadians(90 + angleVariation + ThrowAngleOffset);
            
            float rockSpeed = 300f;
            Vector2 rockVelocity = new Vector2(
                (float)Math.Cos(throwAngle) * rockSpeed,
                (float)Math.Sin(throwAngle) * rockSpeed);
            
            Vector2 rockSize = rockSprite != null && rockSprite.IsLoaded 
                ? rockSprite.Size 
                : new Vector2(20, 20);
            
            rocks.Add(new Rock
            {
                Position = new Vector2(goatPosition.X + goatSize.X / 2, goatPosition.Y + goatSize.Y),
                Size = rockSize,
                Velocity = rockVelocity,
                Rotation = 0f,
                RotationSpeed = (float)(random.NextDouble() * 2 - 1) * 5f
            });
        }
    }
}
