using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Rendering;
using MonoGame.Aseprite;

namespace ProjectZeus.Core.Levels.Maze
{
    /// <summary>
    /// Handles rendering for the maze level
    /// </summary>
    public class MazeRenderer
    {
        private readonly Texture2D solidTexture;
        private readonly Texture2D hedgeTexture;
        private readonly Texture2D sandTileTexture;
        private readonly AsepriteSprite minotaurSprite;
        private readonly AsepriteSprite grapesSprite;
        private readonly SpriteFont font;
        private readonly int cellSize;

        public MazeRenderer(Texture2D solidTexture, Texture2D hedgeTexture, Texture2D sandTileTexture,
            AsepriteSprite minotaurSprite, AsepriteSprite grapesSprite, SpriteFont font, int cellSize)
        {
            this.solidTexture = solidTexture;
            this.hedgeTexture = hedgeTexture;
            this.sandTileTexture = sandTileTexture;
            this.minotaurSprite = minotaurSprite;
            this.grapesSprite = grapesSprite;
            this.font = font;
            this.cellSize = cellSize;
        }

        public void DrawMaze(SpriteBatch spriteBatch, bool[,] walls, int mazeWidth, int mazeHeight, 
            int playerCellX, int playerCellY, int visibilityRadius)
        {
            for (int x = 0; x < mazeWidth; x++)
            {
                for (int y = 0; y < mazeHeight; y++)
                {
                    int dx = System.Math.Abs(x - playerCellX);
                    int dy = System.Math.Abs(y - playerCellY);
                    
                    if (dx <= visibilityRadius && dy <= visibilityRadius)
                    {
                        Rectangle cellRect = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                        
                        if (walls[x, y])
                        {
                            spriteBatch.Draw(hedgeTexture, cellRect, Color.White);
                        }
                        else
                        {
                            spriteBatch.Draw(sandTileTexture, cellRect, Color.White);
                        }
                    }
                }
            }
        }

        public void DrawItem(SpriteBatch spriteBatch, GameTime gameTime, Vector2 itemPosition, bool itemCollected,
            int playerCellX, int playerCellY, int visibilityRadius)
        {
            if (itemCollected) return;

            int itemCellX = (int)(itemPosition.X / cellSize);
            int itemCellY = (int)(itemPosition.Y / cellSize);
            int itemDx = System.Math.Abs(itemCellX - playerCellX);
            int itemDy = System.Math.Abs(itemCellY - playerCellY);
            
            if (itemDx <= visibilityRadius && itemDy <= visibilityRadius)
            {
                if (grapesSprite != null && grapesSprite.IsLoaded)
                {
                    Vector2 drawPos = new Vector2(
                        itemPosition.X - grapesSprite.Size.X / 2, 
                        itemPosition.Y - grapesSprite.Size.Y / 2);
                    grapesSprite.Draw(spriteBatch, drawPos, isMoving: false, gameTime, Color.White);
                }
            }
        }

        public void DrawMinotaur(SpriteBatch spriteBatch, GameTime gameTime, MinotaurController minotaurController,
            int playerCellX, int playerCellY, int visibilityRadius)
        {
            if (!minotaurController.IsActive) return;

            Vector2 minotaurPosition = minotaurController.Position;
            Vector2 minotaurVelocity = minotaurController.Velocity;
            Vector2 minotaurCollisionSize = minotaurController.CollisionSize;
            
            int minotaurCellX = (int)((minotaurPosition.X + minotaurCollisionSize.X / 2) / cellSize);
            int minotaurCellY = (int)((minotaurPosition.Y + minotaurCollisionSize.Y / 2) / cellSize);
            int minotaurDx = System.Math.Abs(minotaurCellX - playerCellX);
            int minotaurDy = System.Math.Abs(minotaurCellY - playerCellY);
            
            if (minotaurDx <= visibilityRadius && minotaurDy <= visibilityRadius)
            {
                if (minotaurSprite != null && minotaurSprite.IsLoaded)
                {
                    bool isMoving = minotaurVelocity.LengthSquared() > 0;
                    Vector2 scaledSpriteSize = minotaurSprite.Size * minotaurController.Scale;
                    
                    Rectangle destRect = new Rectangle(
                        (int)(minotaurPosition.X + minotaurCollisionSize.X / 2 - scaledSpriteSize.X / 2),
                        (int)(minotaurPosition.Y + minotaurCollisionSize.Y - scaledSpriteSize.Y),
                        (int)scaledSpriteSize.X,
                        (int)scaledSpriteSize.Y);
                    
                    SpriteEffects flip = minotaurVelocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    
                    var texture = minotaurSprite.GetFrameTexture(isMoving, gameTime, 10f);
                    if (texture != null)
                    {
                        spriteBatch.Draw(texture, destRect, null, Color.White, 0f, Vector2.Zero, flip, 0f);
                    }
                }
            }
        }

        public void DrawEntrance(SpriteBatch spriteBatch, Vector2 entrancePosition, bool itemCollected,
            int playerCellX, int playerCellY, int visibilityRadius)
        {
            if (!itemCollected) return;

            int entranceCellX = (int)(entrancePosition.X / cellSize);
            int entranceCellY = (int)(entrancePosition.Y / cellSize);
            int entranceDx = System.Math.Abs(entranceCellX - playerCellX);
            int entranceDy = System.Math.Abs(entranceCellY - playerCellY);
            
            if (entranceDx <= visibilityRadius && entranceDy <= visibilityRadius)
            {
                Rectangle entranceRect = new Rectangle((int)(entrancePosition.X - 14), (int)(entrancePosition.Y - 14), 28, 28);
                spriteBatch.Draw(solidTexture, entranceRect, new Color(0, 255, 0, 128));
            }
        }

        public void DrawCarriedItem(SpriteBatch spriteBatch, GameTime gameTime, Vector2 playerPosition, 
            Vector2 playerCollisionSize, bool itemCollected)
        {
            if (!itemCollected) return;

            if (grapesSprite != null && grapesSprite.IsLoaded)
            {
                Vector2 carriedCenter = new Vector2(
                    playerPosition.X + playerCollisionSize.X / 2f,
                    playerPosition.Y - 20f);

                Vector2 drawPos = new Vector2(
                    carriedCenter.X - grapesSprite.Size.X / 2f,
                    carriedCenter.Y - grapesSprite.Size.Y / 2f);

                grapesSprite.Draw(spriteBatch, drawPos, isMoving: false, gameTime, Color.White);
            }
        }

        public void DrawUI(SpriteBatch spriteBatch, Vector2 baseScreenSize, bool itemCollected, bool minotaurActive)
        {
            string instructions = "Navigate the maze! Press E near the golden item to collect it.";
            if (itemCollected)
                instructions = "Item collected! Return to the entrance (top-left) to exit.";
            
            spriteBatch.DrawString(font, instructions, new Vector2(10, 10), Color.Yellow);
            
            if (minotaurActive)
            {
                string warning = "MINOTAUR NEARBY!";
                Vector2 warningSize = font.MeasureString(warning);
                spriteBatch.DrawString(font, warning, new Vector2(baseScreenSize.X - warningSize.X - 10, 10), Color.Red);
            }
        }
    }
}
