using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Levels;

namespace ProjectZeus.Core.Game
{
    /// <summary>
    /// Manages game scene transitions and level states
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

        private AdonisPlayer player;
        private PillarRoom pillarRoom;
        private MineLevel mineLevel;
        private MazeLevel mazeLevel;
        private MountainLevel mountainLevel;
        private ZeusFightScene zeusFightScene;

        public GameScene CurrentScene { get; set; } = GameScene.PillarRoom;
        
        public bool HasCollectedMazeItem { get; set; }
        public bool HasCollectedMineItem { get; set; }
        public bool HasCollectedMountainItem { get; set; }

        public AdonisPlayer Player => player;
        public PillarRoom PillarRoom => pillarRoom;
        public MineLevel MineLevel => mineLevel;
        public MazeLevel MazeLevel => mazeLevel;
        public MountainLevel MountainLevel => mountainLevel;
        public ZeusFightScene ZeusFightScene => zeusFightScene;

        public SceneManager(AdonisPlayer player, PillarRoom pillarRoom, MineLevel mineLevel, 
            MazeLevel mazeLevel, MountainLevel mountainLevel, ZeusFightScene zeusFightScene)
        {
            this.player = player;
            this.pillarRoom = pillarRoom;
            this.mineLevel = mineLevel;
            this.mazeLevel = mazeLevel;
            this.mountainLevel = mountainLevel;
            this.zeusFightScene = zeusFightScene;
        }

        public void TransitionToScene(GameScene scene)
        {
            CurrentScene = scene;
        }

        public void HandleMazeLevelCompletion(GraphicsDevice graphicsDevice, SpriteFont hudFont, System.Action resetPlayerAction)
        {
            if (!mazeLevel.IsCompleted) return;

            CurrentScene = GameScene.PillarRoom;
            HasCollectedMazeItem = mazeLevel.HasItem;
            if (HasCollectedMazeItem)
            {
                pillarRoom.MazePortal.IsActive = false;
                pillarRoom.CurrentCarriedItem = PillarItemType.Maze;
            }
            mazeLevel = new MazeLevel();
            mazeLevel.LoadContent(graphicsDevice, hudFont);
            resetPlayerAction?.Invoke();
        }

        public void HandleMineLevelCompletion(System.Action resetPlayerAction)
        {
            if (mineLevel.IsActive) return;

            CurrentScene = GameScene.PillarRoom;
            if (mineLevel.HasCollectedItem)
            {
                HasCollectedMineItem = true;
                pillarRoom.MinePortal.IsActive = false;
                pillarRoom.CurrentCarriedItem = PillarItemType.Mine;
            }
            resetPlayerAction?.Invoke();
        }

        public void HandleMountainLevelCompletion(System.Action resetPlayerAction, System.Action respawnAction)
        {
            if (mountainLevel.PlayerDied)
            {
                respawnAction?.Invoke();
            }
            else if (mountainLevel.ItemWasCollected)
            {
                CurrentScene = GameScene.PillarRoom;
                HasCollectedMountainItem = true;
                pillarRoom.MountainPortal.IsActive = false;
                pillarRoom.CurrentCarriedItem = PillarItemType.Mountain;
                resetPlayerAction?.Invoke();
            }
        }
    }
}
