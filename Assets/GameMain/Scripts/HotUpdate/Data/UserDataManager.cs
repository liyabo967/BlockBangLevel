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
        
        public string DailyBonusDay => _userData.dailyBonusDay;
        
        public int RewardStreak => _userData.rewardStreak;
        
        public bool TutorialCompleted => _userData.tutorialCompleted;
        
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
            _userData.pictureList = new();
            _userData.purchasedIdList = new();
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