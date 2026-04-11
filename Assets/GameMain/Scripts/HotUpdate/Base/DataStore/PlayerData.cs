using System;

namespace GameMain.Scripts.HotUpdate.Base.DataStore
{
    [Serializable]
    public class PlayerData
    {
        public int level = 1;
        public int coin = 0;
        public bool soundEnabled;
    }
}