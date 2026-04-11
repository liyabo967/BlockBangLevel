namespace GameMain.Scripts.HotUpdate.Base.DataStore
{
    using System.IO;
    using UnityEngine;

    public class JsonDataStore : IDataStore
    {
        private string GetPath(string key)
        {
            return Path.Combine(Application.persistentDataPath, key + ".json");
        }

        public void Save<T>(string key, T data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetPath(key), json);
        }

        public T Load<T>(string key, T defaultValue = default)
        {
            string path = GetPath(key);
            if (!File.Exists(path))
                return default;

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }

        public bool Exists(string key)
        {
            return File.Exists(GetPath(key));
        }

        public void Delete(string key)
        {
            string path = GetPath(key);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}