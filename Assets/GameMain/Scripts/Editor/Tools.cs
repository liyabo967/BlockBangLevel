using System.IO;
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
        
        [MenuItem("Tools/BatchLevel")]
        static void BatchCopy()
        {
            string sourceFolder = "Assets/GameMain/Settings/Levels/Group_0";
            string destFolder = "Assets/GameMain/Settings/Levels/Group_4";
        
            // 确保目标文件夹存在
            if (!AssetDatabase.IsValidFolder(destFolder))
            {
                string[] folders = destFolder.Split('/');
                string currentPath = folders[0];
            
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
            
            for (int i = 1; i <= 100; i++)
            {
                var levelNum = i * 2;
                levelNum = Random.Range(levelNum - 1, levelNum + 1);
                string sourceFile = Path.Combine(sourceFolder, $"Level_{levelNum}.asset");
                string newFileName = $"Level_{i}.asset";
                string destPath = Path.Combine(destFolder, newFileName);
                if (AssetDatabase.CopyAsset(sourceFile, destPath))
                {
                    Debug.Log($"复制成功: {levelNum} -> {newFileName}");
                }
            }
        
            AssetDatabase.Refresh();
            Debug.Log("批量复制完成");
        }
        
        
        void RenameAssetWithMainObject(string assetPath, string newName)
        {
            // 加载资源
            Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset == null)
            {
                Debug.LogError($"无法加载资源: {assetPath}");
                return;
            }
        
            string oldName = asset.name;
            string extension = Path.GetExtension(assetPath);
            string directory = Path.GetDirectoryName(assetPath);
            string newFilePath = Path.Combine(directory, newName + extension);
        
            // 1. 先修改 Main Object 名称
            asset.name = newName;
            EditorUtility.SetDirty(asset);
        
            // 2. 再重命名文件
            string error = AssetDatabase.RenameAsset(assetPath, newName);
        
            if (string.IsNullOrEmpty(error))
            {
                Debug.Log($"成功重命名: {oldName} -> {newName}");
            }
            else
            {
                Debug.LogError($"重命名失败: {error}");
                // 回滚 Main Object 名称
                asset.name = oldName;
                EditorUtility.SetDirty(asset);
            }
        
            // 保存所有修改
            AssetDatabase.SaveAssets();
        }
    
        private static string GetRelativePath(string absolutePath)
        {
            if (absolutePath.StartsWith(Application.dataPath))
            {
                return "Assets" + absolutePath.Substring(Application.dataPath.Length);
            }
            return absolutePath;
        }
    }
}