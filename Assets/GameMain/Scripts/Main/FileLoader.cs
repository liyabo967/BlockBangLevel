namespace GameMain
{
    using System;
    using System.Collections;
    using System.IO;
    using UnityEngine;
    using UnityEngine.Networking;

    public static class FileLoader
    {
        public static IEnumerator LoadFile(string filePath, Action<byte[]> callback)
        {
            byte[] bytes = null;
            if (filePath.StartsWith("jar") || filePath.StartsWith("http"))
            {
                // Special case to access StreamingAsset content on Android and Web
                UnityWebRequest request = UnityWebRequest.Get(filePath);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    bytes = request.downloadHandler.data;
                }
                else
                {
                    Debug.LogError($"FileLoader, Failed to load StreamingAsset: {request.error}");
                }
            }
            else
            {
                // Regular file path on most platforms and in Editor
                if (System.IO.File.Exists(filePath))
                {
                    bytes = System.IO.File.ReadAllBytes(filePath);
                }
                else
                {
                    Debug.LogError($"FileLoader, File not found at: {filePath}");
                }
            }

            // Send the loaded content back to the caller
            callback?.Invoke(bytes);
        }
    }
}