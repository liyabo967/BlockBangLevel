using System;

namespace UnityGameFramework.Runtime
{
    public class DataStorageComponent : GameFrameworkComponent
    {
        private IDataStorage _storage;

        private void Start()
        {
            // 默认使用 JSON 文件存储，可随时切换
            _storage = new JsonFileStorage("SaveData");
        }

        /// <summary>切换存储后端</summary>
        public void SetStorage(IDataStorage storage)
        {
            _storage?.Flush();
            _storage = storage;
        }

        public void Save<T>(string key, T data) => _storage.Save(key, data);
        public T Load<T>(string key, T defaultValue = default) => _storage.Load(key, defaultValue);
        public bool HasKey(string key) => _storage.HasKey(key);
        public void DeleteKey(string key) => _storage.DeleteKey(key);
        public void DeleteAll() => _storage.DeleteAll();
        public void Flush() => _storage.Flush();
    }
}