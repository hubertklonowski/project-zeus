using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using ProjectZeus.Core.Constants;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Levels;
using ProjectZeus.Core.Rendering;
using ProjectZeus.Core.Utilities;

namespace ProjectZeus.Core.Game
{
    /// <summary>
    /// Handles content loading for the game
    /// </summary>
    public class GameContentLoader
    {
        public void LoadGameContent(
            GraphicsDevice graphicsDevice,
            ContentManager content,
            out SpriteBatch spriteBatch,
            out SpriteFont hudFont,
            out Texture2D playerTexture,
            out VirtualGamePad virtualGamePad,
            out AdonisPlayer player,
            out PillarRoom pillarRoom,
            out MineLevel mineLevel,
            out MazeLevel mazeLevel,
            out MountainLevel mountainLevel,
            out ZeusFightScene zeusFightScene,
            out CreditsScene creditsScene,
            Matrix globalTransformation)
        {
            content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(graphicsDevice);
            hudFont = content.Load<SpriteFont>("Fonts/Hud");

            virtualGamePad = new VirtualGamePad(GameConstants.BaseScreenSize, globalTransformation, 
                content.Load<Texture2D>("Sprites/VirtualControlArrow"));

            if (!OperatingSystem.IsIOS())
            {
                try
                {
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(content.Load<Song>("Sounds/Music"));
                }
                catch { }
            }

            playerTexture = DrawingHelpers.CreateSolidTexture(graphicsDevice, 1, 1, new Color(255, 220, 180));

            player = new AdonisPlayer();
            player.LoadContent(graphicsDevice);

            pillarRoom = new PillarRoom();
            pillarRoom.LoadContent(graphicsDevice, hudFont);

            mineLevel = new MineLevel();
            mineLevel.LoadContent(graphicsDevice, hudFont);

            mazeLevel = new MazeLevel();
            mazeLevel.LoadContent(graphicsDevice, hudFont);

            mountainLevel = new MountainLevel();
            mountainLevel.LoadContent(graphicsDevice, hudFont);

            zeusFightScene = new ZeusFightScene();
            zeusFightScene.LoadContent(graphicsDevice, hudFont);

            creditsScene = new CreditsScene();
            creditsScene.LoadContent(graphicsDevice, hudFont);
        }
    }
}
