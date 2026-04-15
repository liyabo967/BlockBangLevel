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
            _userData.pictureList = new();
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
    }
}