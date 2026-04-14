namespace UnityGameFramework.Runtime
{
    // IDataStorage.cs
    public interface IDataStorage
    {
        void Save<T>(string key, T data);
        T Load<T>(string key, T defaultValue = default);
        bool HasKey(string key);
        void DeleteKey(string key);
        void DeleteAll();
        void Flush(); // 强制写入磁盘
    }
}