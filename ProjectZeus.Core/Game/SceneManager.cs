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
            ZeusFight,
            Credits
        }

        private AdonisPlayer player;
        private PillarRoom pillarRoom;
        private MineLevel mineLevel;
        private MazeLevel mazeLevel;
        private MountainLevel mountainLevel;
        private ZeusFightScene zeusFightScene;
        private CreditsScene creditsScene;

        // When true, the player is acting as the goat at the top of the mountain
        // and should be able to throw rocks like the original goat.
        public bool IsPlayerGoatOnMountain { get; private set; }

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
        public CreditsScene CreditsScene => creditsScene;

        public SceneManager(AdonisPlayer player, PillarRoom pillarRoom, MineLevel mineLevel, 
            MazeLevel mazeLevel, MountainLevel mountainLevel, ZeusFightScene zeusFightScene, CreditsScene creditsScene)
        {
            this.player = player;
            this.pillarRoom = pillarRoom;
            this.mineLevel = mineLevel;
            this.mazeLevel = mazeLevel;
            this.mountainLevel = mountainLevel;
            this.zeusFightScene = zeusFightScene;
            this.creditsScene = creditsScene;
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
            mineLevel.Reset();
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
                IsPlayerGoatOnMountain = false;
            }
            else if (mountainLevel.ShouldShowCredits)
            {
                // Show credits after 1 minute as goat
                CurrentScene = GameScene.Credits;
                creditsScene?.Reset();
            }
        }

        public void ReplaceMazeLevel(MazeLevel newMazeLevel)
        {
            mazeLevel = newMazeLevel;
        }

        public void ReplaceZeusFightScene(ZeusFightScene newZeusFightScene)
        {
            zeusFightScene = newZeusFightScene;
        }

        /// <summary>
        /// Handles transition from Zeus fight to mountain level when the
        /// player is turned into a goat and stomp sequence has finished.
        /// </summary>
        public void HandleZeusFightCompletion(System.Action<Vector2> setPlayerPosition)
        {
            if (zeusFightScene == null)
                return;
            
            // Check if we should show credits (victory)
            if (zeusFightScene.ShouldShowCredits)
            {
                CurrentScene = GameScene.Credits;
                creditsScene?.Reset();
                return;
            }
            
            // Check if we should transition to mountain as goat
            if (zeusFightScene.ShouldStartMountainAsGoat)
            {
                // Switch to the mountain level.
                CurrentScene = GameScene.MountainLevel;
                IsPlayerGoatOnMountain = true;

                Vector2 playerSize = player?.Size ?? new Vector2(32, 48);
                Vector2 spawnPos = Vector2.Zero;

                if (mountainLevel != null)
                {
                    // Place player where the original goat was standing on the top platform.
                    spawnPos = mountainLevel.GetGoatSpawnPosition(playerSize);
                    mountainLevel.PlayerIsGoat = true;
                }

                setPlayerPosition?.Invoke(spawnPos);
            }
        }
    }
}
