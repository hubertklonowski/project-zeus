using System;
using System.Collections.Generic;
using ProjectZeus.Core.Levels;

namespace ProjectZeus.Core.Levels.ZeusFight
{
    public struct DialogueMapping
    {
        public string Dialogue;
        public PillarItemType ExpectedItem;
        
        public DialogueMapping(string dialogue, PillarItemType expectedItem)
        {
            Dialogue = dialogue;
            ExpectedItem = expectedItem;
        }
    }

    public class ZeusGameStateManager
    {
        private const float TimeLimit = 10f;
        
        public DialogueMapping[] DialogueMappings { get; private set; }
        public int CurrentDialogueIndex { get; private set; }
        public int CorrectAnswers { get; private set; }
        public float RemainingTime { get; private set; }
        public bool TimerActive { get; private set; }
        public bool VictoryAchieved { get; private set; }
        public bool PlayerTransformedToGoat { get; private set; }
        public bool ZeusStomp { get; private set; }
        public float StompAnimationTime { get; private set; }

        public ZeusGameStateManager()
        {
            DialogueMappings = new[]
            {
                new DialogueMapping("κρι-κρι", PillarItemType.Mountain),
                new DialogueMapping("χρυσός του Μίδα", PillarItemType.Mine),
                new DialogueMapping("κρασί του Διονύσου", PillarItemType.Maze)
            };
            ShuffleDialogues();
            TimerActive = true;
            RemainingTime = TimeLimit;
        }

        public void ShuffleDialogues()
        {
            Random rng = new Random();
            int n = DialogueMappings.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (DialogueMappings[k], DialogueMappings[n]) = (DialogueMappings[n], DialogueMappings[k]);
            }
        }

        public void UpdateTimer(float dt)
        {
            if (TimerActive && !PlayerTransformedToGoat)
            {
                RemainingTime -= dt;
                if (RemainingTime <= 0)
                {
                    TriggerTransformation();
                }
            }
        }

        public void UpdateStompAnimation(float dt)
        {
            if (ZeusStomp)
            {
                StompAnimationTime += dt;
            }
        }

        public void ValidateSacrifice(PillarItemType currentItem, ZeusItemManager itemManager)
        {
            PillarItemType expectedItem = DialogueMappings[CurrentDialogueIndex].ExpectedItem;

            if (currentItem == expectedItem)
            {
                CorrectAnswers++;
                itemManager.MarkItemAsUsed(currentItem);
                itemManager.ClearCurrentItem();

                if (CorrectAnswers >= 3)
                {
                    VictoryAchieved = true;
                    TimerActive = false;
                }
                else
                {
                    CurrentDialogueIndex++;
                    RemainingTime = TimeLimit;
                    TimerActive = true;
                }
            }
            else
            {
                TriggerTransformation();
            }
        }

        private void TriggerTransformation()
        {
            ZeusStomp = true;
            PlayerTransformedToGoat = true;
            StompAnimationTime = 0f;
            TimerActive = false;
        }
    }
}
