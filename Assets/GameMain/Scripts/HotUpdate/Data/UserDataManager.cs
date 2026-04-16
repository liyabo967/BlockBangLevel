using System;
using System.Collections.Generic;
using Quester;

namespace BlockPuzzleGameToolkit.Scripts.Data
{
    public class UserDataManager : MonoSingleton<UserDataManager>
    {
        private const string UserKey = "user";
        private UserData _userData;
        
        public int Level => _userData.level;
        public int Coins => _userData.coins;
        public int Score => _userData.score;
        public int TimedBestScore => _userData.timedBestScore;
        public string DailyBonusDay => _userData.dailyBonusDay;
        public int RewardStreak => _userData.rewardStreak;
        public bool TutorialCompleted => _userData.tutorialCompleted;
        public int GameMode => _userData.gameMode;
        public List<string> PictureList => _userData.pictureList;
        
        public void Load()
        {
            _userData = GameEntry.Storage.Load(UserKey, _userData);
            if (_userData == null)
            {
                _userData = new UserData();
                InitData();
            }

            _userData.level = 80;
        }

        private void InitData()
        {
            _userData.level = 1;
            _userData.coins = 10;
            _userData.rewardStreak = -1;
            _userData.dailyBonusDay = "1900-01-01 00:00:00";
        }

        // 序列化数据，但是不写入磁盘
        private void Save()
        {
            GameEntry.Storage.Save(UserKey, _userData);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
        }

        protected override void OnApplicationQuit()
        {
            Save();
            base.OnApplicationQuit();
        }
        
        public void SetLevel(int level)
        {
            _userData.level = level;
        }

        public void SetCoins(int coins)
        {
            _userData.coins = coins;
        }

        public void SetScore(int score)
        {
            _userData.score = score;
        }

        public void SetTimedBestScore(int timedBestScore)
        {
            _userData.timedBestScore = timedBestScore;
        }

        /// <summary>
        /// 关于金币分数的特殊处理，后面再处理 scriptableObject的问题
        /// </summary>
        /// <param name="dataName"></param>
        public int GetData(string dataName)
        {
            var result = 0;
            switch (dataName)
            {
                case "Coins":
                    result = _userData.coins;
                    break;
                case "Score":
                    result = _userData.score;
                    break;
                case "TimedBestScore":
                    result = _userData.timedBestScore;
                    break;
            }
            return result;
        }

        public void SetData(string dataName, int dataValue)
        {
            switch (dataName)
            {
                case "Coins":
                    _userData.coins = dataValue;
                    break;
                case "Score":
                    _userData.score = dataValue;
                    break;
                case "TimedBestScore":
                    _userData.timedBestScore = dataValue;
                    break;
            }
        }

        public void SetRewardStreak(int rewardStreak)
        {
            _userData.rewardStreak = rewardStreak;
        }

        public void SetGameMode(int gameMode)
        {
            _userData.gameMode = gameMode;
            Save();
        }

        public void SetTutorialCompleted()
        {
            _userData.tutorialCompleted = true;
        }

        public void SetDailyBonusDay(string day)
        {
            _userData.dailyBonusDay = day;
        }

        public void SetLastPlayedMode(string mode)
        {
            _userData.lastPlayedMode = mode;
        }

        public void SetPurchasedProductId(string productId)
        {
            if (!_userData.purchasedIdList.Contains(productId))
            {
                _userData.purchasedIdList.Add(productId);
                Save();
            }
        }

        public bool IsPurchasedProductId(string productId)
        {
            return _userData.purchasedIdList.Contains(productId);
        }
        
    }
}