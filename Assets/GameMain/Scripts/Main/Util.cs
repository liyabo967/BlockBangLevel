using System;
using System.IO;
using UnityEngine;

namespace GameMain
{
    public class Util
    {
        /// <summary>
        /// 计算文件 MD5
        /// </summary>
        public static string CalculateMD5(string filePath)
        {
            // Debug.Log($"File.Exists: {File.Exists(filePath)}, filePath: {filePath}");
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
        }
        
        public static string GetPlatformName()
        {
#if UNITY_STANDALONE_WIN
            return "StandaloneWindows64";
#elif UNITY_STANDALONE_OSX
            return "StandaloneOSX";
#elif UNITY_IOS
            return "iOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "Unknown";
#endif
        }
    }
}