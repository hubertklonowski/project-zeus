using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Levels;

namespace ProjectZeus.Core.Game
{
    /// <summary>
    /// Manages game scene transitions and state
    /// </summary>
    public class SceneManager
    {
        public enum GameScene
        {
            PillarRoom,
            MineLevel,
            MazeLevel,
            MountainLevel,
            ZeusFight
        }

        public GameScene CurrentScene { get; set; } = GameScene.PillarRoom;
        public bool HasCollectedMazeItem { get; set; }
        public bool HasCollectedMineItem { get; set; }
        public bool HasCollectedMountainItem { get; set; }

        public bool HasAnyItem => HasCollectedMazeItem || HasCollectedMineItem || HasCollectedMountainItem;

        public void ResetItems()
        {
            HasCollectedMazeItem = false;
            HasCollectedMineItem = false;
            HasCollectedMountainItem = false;
        }

        public void TransitionToScene(GameScene scene)
        {
            CurrentScene = scene;
        }

        public void HandlePillarRoomTransitions(AdonisPlayer player, PillarRoom pillarRoom, 
            MineLevel mineLevel, MountainLevel mountainLevel, InputManager input)
        {
            // Check maze portal
            if (pillarRoom.MazePortal.Intersects(player.Bounds) && !HasCollectedMazeItem)
            {
                CurrentScene = GameScene.MazeLevel;
                return;
            }

            // Check mine portal
            if (pillarRoom.MinePortal.Intersects(player.Bounds) && !HasCollectedMineItem)
            {
                CurrentScene = GameScene.MineLevel;
                mineLevel.Enter();
                return;
            }

            // Check mountain portal (requires E key)
            if (input.IsKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.E) && 
                pillarRoom.MountainPortal.Intersects(player.Bounds) && 
                !HasCollectedMountainItem)
            {
                CurrentScene = GameScene.MountainLevel;
                mountainLevel.Reset();
                float groundTop = GameConstants.BaseScreenSize.Y - GameConstants.GroundHeight;
                player.Position = new Vector2(60f, groundTop - player.Size.Y);
                player.Velocity = Vector2.Zero;
                player.IsOnGround = true;
                return;
            }

            // Check if all items inserted - transition to Zeus fight
            if (pillarRoom.AllItemsInserted)
            {
                CurrentScene = GameScene.ZeusFight;
                float fightGroundTop = GameConstants.BaseScreenSize.Y * 0.7f;
                player.Position = new Vector2(GameConstants.BaseScreenSize.X - 100f, fightGroundTop - player.Size.Y);
                player.Velocity = Vector2.Zero;
                player.IsOnGround = true;
            }
        }
    }
}
