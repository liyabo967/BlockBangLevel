using System;
using System.Diagnostics;
using System.IO;
using HybridCLR.Editor.Commands;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GameMain.Editor
{
    public class ToolsMenu
    {
        private const string Server = "https://assets-1301567094.cos.ap-beijing.myqcloud.com/block-bang-level";
        // private const string Server = "http://localhost:8080";
        
        [MenuItem("HybridCLR/Build Helper/BuildAndCopyFiles")]
        static void BuildAndCopyFiles()
        {
            // 生成热更文件
            CompileDllCommand.CompileDllActiveBuildTarget();
            
            // 计算 MD5
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            string targetPlatform = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
            string  dllPath = $"{projectRoot}/HybridCLRData/HotUpdateDlls/{targetPlatform}/HotUpdate.dll";
            var md5 = Util.CalculateMD5(dllPath);
            Debug.Log($"MD5: {md5}");
            
            // 生成版本文件
            VersionInfo versionInfo = new VersionInfo();
            versionInfo.version = Application.version;
            versionInfo.buildTime = DateTime.Now.ToString("yyyyMMddHHmm");
            versionInfo.hotUpdateDll = new DllInfo()
            {
                name = "HotUpdate.dll",
                md5 = md5,
                url = $"{Server}/{Application.version}/{targetPlatform}/HotUpdate.dll"
            };
            var targetDir = $"{Directory.GetParent(Application.dataPath)?.FullName}/ServerData/{Application.version}/{targetPlatform}";
            var jsonPath = $"{targetDir}/version.json";
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(versionInfo));
            
            // 复制热更文件
            string dstFile = Path.Combine(targetDir, "HotUpdate.dll");
            File.Copy(dllPath, dstFile, true);
            
            // 复制补充元数据
            string metaPath = $"{projectRoot}/HybridCLRData/AssembliesPostIl2CppStrip/{targetPlatform}";
            var metaDlls = new string[]
            {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll"
            };
            var aotDir = $"{targetDir}/AOT";
            if (!Directory.Exists(aotDir))
            {
                Directory.CreateDirectory(aotDir);
            }
            foreach (var metaDll in metaDlls)
            {
                File.Copy($"{metaPath}/{metaDll}", $"{aotDir}/{metaDll}", true);
            }
            
            // 打开目录
            Application.OpenURL($"file://{projectRoot}/ServerData/{Application.version}");
        }
        
        
        #region 数据结构

        [Serializable]
        private class VersionInfo
        {
            public string version;
            public string buildTime;
            public DllInfo hotUpdateDll;
        }

        [Serializable]
        private class DllInfo
        {
            public string name;
            public string md5;
            public long size;
            public string url;
        }

        #endregion
    }
}