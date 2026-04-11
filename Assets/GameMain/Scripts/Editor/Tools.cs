using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameMain.Editor
{
    public class Tools
    {
        [MenuItem("Tools/Clear/PlayerPrefs")]
        public static void ClearAll()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("All PlayerPrefs have been cleared!");
        }

        [MenuItem("Tools/Clear/Addressables")]
        public static async void ClearAddressables()
        {
            var key = "remote";
            var handle = Addressables.ClearDependencyCacheAsync(key, false);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("缓存清理成功: " + key);
            }
            else
            {
                Debug.LogError("缓存清理失败: " + key);
            }
            handle.Release();
            Caching.ClearCache();
        }
    }
}