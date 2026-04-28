using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class JsonFileStorage : IDataStorage
    {
        private readonly string _rootDir;
        private Dictionary<string, string> _cache;
        private bool _dirty;
        private readonly bool _useEncryption = true;
        
        private readonly AesEncryptor _aesEncryptor;
        
        private static readonly byte[] SALT = new byte[]
        {
            0x4A, 0x7B, 0x9C, 0x2D, 0xE5, 0xF1, 0x83, 0x6A,
            0xB4, 0x1F, 0x88, 0xD3, 0x5E, 0x29, 0xC7, 0x99
        };

        private const string Password = "GameEncrypted";

        public JsonFileStorage(string saveFolder = "SaveData")
        {
            _rootDir = Path.Combine(Application.persistentDataPath, saveFolder);
            Directory.CreateDirectory(_rootDir);
            _aesEncryptor = new AesEncryptor(Password, SALT);
            Load();
        }

        private void Load()
        {
            _cache = new Dictionary<string, string>();
            string[] jsonFiles = Directory.GetFiles(_rootDir, "*.json", SearchOption.AllDirectories);
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    string json = File.ReadAllText(jsonFile);
                    if (_useEncryption)
                    {
                        json = _aesEncryptor.Decrypt(json);
                    }
                
                    _cache[Path.GetFileNameWithoutExtension(jsonFile)] = json;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[JsonFileStorage] Load failed: {e.Message}");
                }
            }
        }

        public void Save<T>(string key, T data)
        {
            _cache[key] = JsonConvert.SerializeObject(data);
            var json = _cache[key];
            if (_useEncryption)
            {
                json = _aesEncryptor.Encrypt(json);
            }
            File.WriteAllText(GetPath(key), json);
        }

        public T Load<T>(string key, T defaultValue = default)
        {
            if (!_cache.TryGetValue(key, out string json)) return defaultValue;
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool HasKey(string key) => _cache.ContainsKey(key);

        public void DeleteKey(string key)
        {
            if (_cache.Remove(key))
            {
                _dirty = true;
            }
            File.Delete(GetPath(key));
        }

        public void DeleteAll()
        {
            _cache.Clear();
            _dirty = true;
        }

        private string GetPath(string key)
        {
            return Path.Combine(_rootDir, key + ".json");
        }

        public void Flush()
        {
            if (!_dirty) return;
            
            foreach (var kv in _cache)
            {
                var json = kv.Value;
                File.WriteAllText(GetPath(kv.Key), json);
            }
            _dirty = false;
        }
    }
}