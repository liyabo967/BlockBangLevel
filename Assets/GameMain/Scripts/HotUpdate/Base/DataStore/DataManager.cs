using Quester;

namespace GameMain.Scripts.HotUpdate.Base.DataStore
{
    public class DataManager : Singleton<DataManager>
    {
        public PlayerData PlayerData { get; private set; }
        
        private IDataStore _dataStore;
        private const string PlayerKey = "player";

        protected override void OnInit()
        {
            base.OnInit();
            // 在这里切换存储方案
            _dataStore = new JsonDataStore();
        }

        public void Save<T>(string key, T data)
        {
            _dataStore.Save(key, data);
        }

        public void Load()
        {
            PlayerData = _dataStore.Load<PlayerData>(PlayerKey);
        }

        public bool Exists(string key)
        {
            return _dataStore.Exists(key);
        }
    }
}