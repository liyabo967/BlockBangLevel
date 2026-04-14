using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class PlayerPrefsStorage : IDataStorage
    {
        public void Save<T>(string key, T data)
        {
            // 用 JSON 序列化，统一处理所有类型
            string json = JsonUtility.ToJson(new Wrapper<T> { value = data });
            PlayerPrefs.SetString(key, json);
        }

        public T Load<T>(string key, T defaultValue = default)
        {
            if (!PlayerPrefs.HasKey(key)) return defaultValue;
            try
            {
                string json = PlayerPrefs.GetString(key);
                return JsonUtility.FromJson<Wrapper<T>>(json).value;
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool HasKey(string key) => PlayerPrefs.HasKey(key);
        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        public void DeleteAll() => PlayerPrefs.DeleteAll();
        public void Flush() => PlayerPrefs.Save();

        // JsonUtility 不能直接序列化基础类型，用包装类解决
        [System.Serializable]
        private class Wrapper<T> { public T value; }
    }


}