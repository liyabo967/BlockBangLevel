namespace GameMain.Scripts.HotUpdate.Base.DataStore
{
    public interface IDataStore
    {
        void Save<T>(string key, T data);
        T Load<T>(string key, T defaultValue = default);
        bool Exists(string key);
        void Delete(string key);
    }
}