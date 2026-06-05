using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using HybridCLR.Editor.Commands;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GameMain.Editor
{
    public class HybridCLRMenu
    {
        private const string Server = "https://assets-1301567094.cos.ap-beijing.myqcloud.com/block-bang";
        // private const string Server = "http://localhost:8080";
        
        [MenuItem("HybridCLR/Build Helper/BuildAndCopyFiles")]
        static void BuildAndCopy()
        {
            // 生成热更文件
            CompileDllCommand.CompileDllActiveBuildTarget();
            
            // clear folder
            // ClearFolder("Assets/StreamingAssets/aa");
            ClearFolder("Assets/StreamingAssets/HotUpdate");
            
            string server = "https://assets-1301567094.cos.ap-beijing.myqcloud.com/block-bang";
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            string targetPlatform = EditorUserBuildSettings.activeBuildTarget.ToString();
            
            // copy aa
            // string src = $"ServerData/{Application.version}/{targetPlatform}";
            // string dst = $"Assets/StreamingAssets/";
            // Directory.CreateDirectory(dst);
            // FileUtil.CopyFileOrDirectory(src, $"{dst}");
            
            // copy HotUpdate to StreamingAssets And ServerData
            string hotUpdatePath = $"HybridCLRData/HotUpdateDlls/{targetPlatform}/HotUpdate.dll";
            string targetHotUpdateDir = "Assets/StreamingAssets/HotUpdate";
            
            Directory.CreateDirectory(targetHotUpdateDir);
            string dstFile = Path.Combine(targetHotUpdateDir, "HotUpdate.dll.bytes");
            // To StreamingAssets
            File.Copy(hotUpdatePath, dstFile, true);
            // To ServerData
            string serverDataDir = $"ServerData/{Application.version}/{targetPlatform}";
            string serverDataHotUpdateDir = $"{serverDataDir}/HotUpdate";
            if (!Directory.Exists(serverDataHotUpdateDir))
            {
                Directory.CreateDirectory(serverDataHotUpdateDir);
            }
            File.Copy(hotUpdatePath, $"{serverDataHotUpdateDir}/HotUpdate.dll.bytes", true);
            
            // copy aot metadata
            Dictionary<string, DllInfo> dllInfoDict = new ();
            string dllPath = $"HybridCLRData/AssembliesPostIl2CppStrip/{targetPlatform}";
            var dlls = new string[]
            {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll"
            };
            var aotDir = $"Assets/StreamingAssets/HotUpdate/AOT";
            var serverDataAOTDir = $"{serverDataHotUpdateDir}/AOT";
            if (!Directory.Exists(aotDir))
            {
                Directory.CreateDirectory(aotDir);
            }
            if (!Directory.Exists(serverDataAOTDir))
            {
                Directory.CreateDirectory(serverDataAOTDir);
            }
            foreach (var dll in dlls)
            {
                var dllInfo = new DllInfo()
                {
                    hash = Util.CalculateMD5($"{dllPath}/{dll}")
                };
                dllInfoDict[dll] = dllInfo;
                // To StreamingAssets
                File.Copy($"{dllPath}/{dll}", $"{aotDir}/{dll}.bytes", true);
                // To ServerData
                File.Copy($"{dllPath}/{dll}", $"{serverDataAOTDir}/{dll}.bytes", true);
            }
            
            // generate version.json
            // 生成版本文件
            VersionInfo versionInfo = new VersionInfo();
            versionInfo.forceUpdate = false;
            versionInfo.version = Application.version;
            versionInfo.buildTime = DateTime.Now.ToString("yyyyMMddHHmm");
            versionInfo.hotUpdateDll = new DllInfo()
            {
                name = "HotUpdate.dll.bytes",
                hash = Util.CalculateMD5(hotUpdatePath),
                url = $"{server}/{Application.version}/{targetPlatform}/HotUpdate/HotUpdate.dll.bytes"
            };
            versionInfo.aotDllDict = dllInfoDict;
            var jsonPath = $"{serverDataDir}/version.json";
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(versionInfo));
            
            AssetDatabase.Refresh();
            // 打开目录
            Application.OpenURL($"file://{projectRoot}/ServerData/{Application.version}");
            Debug.Log("Copy Done");
        }

        private static void ClearFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.Log($"Directory not found: {folderPath}");
                return;
            }

            // 删除文件
            foreach (var file in Directory.GetFiles(folderPath))
            {
                File.Delete(file);
            }

            // 删除子目录
            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                Directory.Delete(dir, true);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("HybridCLR/Build Helper/PrintHash")]
        static void PrintHash()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "HotUpdate/HotUpdate.dll.bytes");
            Debug.Log(Util.CalculateMD5(path));
        }

        #region 数据结构

        [Serializable]
        private class VersionInfo
        {
            public bool forceUpdate;
            public string version;
            public string buildTime;
            public DllInfo hotUpdateDll;
            public Dictionary<string, DllInfo> aotDllDict;
        }

        [Serializable]
        private class DllInfo
        {
            public string name;
            public string hash;
            public long size;
            public string url;
        }

        #endregion
    }
}