using Microsoft.Xna.Framework;

namespace ProjectZeus.Core.Entities
{
    /// <summary>
    /// Represents a pillar with an item slot in the hub room
    /// </summary>
    public class Pillar
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 SlotSize { get; set; }
        public float SlotOffsetY { get; set; }
        public bool HasItem { get; set; }
        public Color ItemColor { get; set; }

        public Rectangle GetPillarRectangle()
        {
            return new Rectangle(
                (int)(Position.X - Size.X / 2f),
                (int)(Position.Y - Size.Y),
                (int)Size.X,
                (int)Size.Y);
        }

        public Rectangle GetSlotRectangle()
        {
            Rectangle pillarRect = GetPillarRectangle();
            return new Rectangle(
                pillarRect.X + (pillarRect.Width - (int)SlotSize.X) / 2,
                pillarRect.Y - (int)SlotOffsetY - (int)SlotSize.Y,
                (int)SlotSize.X,
                (int)SlotSize.Y);
        }
    }
}
