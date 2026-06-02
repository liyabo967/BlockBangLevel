using System.Collections.Generic;

namespace BlockPuzzleGameToolkit.Scripts.Data
{
    public class UserData
    {
        public int level;
        public int coins;
        // public int score;
        public int classicBestScore;
        public int group;
        public int levelGroup;
        public int timedBestScore;
        public int rewardStreak;
        public int gameMode;
        public bool tutorialCompleted;
        public int adventureState;
        public string dailyBonusDay;
        public string lastPlayedMode;
        public int winStreak;
        public int failStreak;
        public int firstSeason;
        public int currentSeason;
        public long lastRateTimestamp;
        public bool noAdsPurchased;
        public List<string> pictureList = new ();
        public List<string> purchasedIdList = new ();
    }
}