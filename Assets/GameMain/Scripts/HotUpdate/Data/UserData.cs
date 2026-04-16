using System.Collections.Generic;

namespace BlockPuzzleGameToolkit.Scripts.Data
{
    public class UserData
    {
        public int level;
        public int coins;
        public int score;
        public int timedBestScore;
        public int rewardStreak;
        public int gameMode;
        public bool tutorialCompleted;
        public string dailyBonusDay;
        public string lastPlayedMode;
        public List<string> pictureList;
        public List<string> purchasedIdList;
    }
}