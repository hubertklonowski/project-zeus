using System.Collections.Generic;
using ProjectZeus.Core.Levels;

namespace ProjectZeus.Core.Levels.ZeusFight
{
    public class ZeusItemManager
    {
        private const int MaxItemAttempts = 3;
        private PillarItemType currentPlacedItem = PillarItemType.None;
        private readonly HashSet<PillarItemType> usedItems = new HashSet<PillarItemType>();

        public PillarItemType CurrentPlacedItem => currentPlacedItem;
        
        public void MarkItemAsUsed(PillarItemType item)
        {
            usedItems.Add(item);
        }

        public void ClearCurrentItem()
        {
            currentPlacedItem = PillarItemType.None;
        }

        public void CycleNextItem()
        {
            if (currentPlacedItem == PillarItemType.None)
            {
                if (!usedItems.Contains(PillarItemType.Mountain))
                    currentPlacedItem = PillarItemType.Mountain;
                else if (!usedItems.Contains(PillarItemType.Mine))
                    currentPlacedItem = PillarItemType.Mine;
                else if (!usedItems.Contains(PillarItemType.Maze))
                    currentPlacedItem = PillarItemType.Maze;
                return;
            }

            PillarItemType nextItem = currentPlacedItem;
            int attempts = 0;

            do
            {
                if (nextItem == PillarItemType.Mountain)
                    nextItem = PillarItemType.Mine;
                else if (nextItem == PillarItemType.Mine)
                    nextItem = PillarItemType.Maze;
                else if (nextItem == PillarItemType.Maze)
                    nextItem = PillarItemType.Mountain;

                attempts++;
                if (attempts > MaxItemAttempts) return;
            }
            while (usedItems.Contains(nextItem));

            currentPlacedItem = nextItem;
        }

        public void CyclePreviousItem()
        {
            if (currentPlacedItem == PillarItemType.None)
            {
                if (!usedItems.Contains(PillarItemType.Maze))
                    currentPlacedItem = PillarItemType.Maze;
                else if (!usedItems.Contains(PillarItemType.Mine))
                    currentPlacedItem = PillarItemType.Mine;
                else if (!usedItems.Contains(PillarItemType.Mountain))
                    currentPlacedItem = PillarItemType.Mountain;
                return;
            }

            PillarItemType previousItem = currentPlacedItem;
            int attempts = 0;

            do
            {
                if (previousItem == PillarItemType.Mountain)
                    previousItem = PillarItemType.Maze;
                else if (previousItem == PillarItemType.Maze)
                    previousItem = PillarItemType.Mine;
                else if (previousItem == PillarItemType.Mine)
                    previousItem = PillarItemType.Mountain;

                attempts++;
                if (attempts > MaxItemAttempts) return;
            }
            while (usedItems.Contains(previousItem));

            currentPlacedItem = previousItem;
        }
    }
}
