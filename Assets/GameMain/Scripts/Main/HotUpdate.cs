using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using HybridCLR;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityGameFramework.Runtime;

namespace GameMain
{
    /// <summary>
    /// 高级 DLL 加载器
    /// 支持版本检查、MD5 校验、补充元数据加载、断点续传等功能
    /// </summary>
    public class HotUpdate : MonoBehaviour
    {
        private string server = "https://assets-1301567094.cos.ap-beijing.myqcloud.com/block-bang-level";
        // private string server = "http://localhost:8080";
        private string _serverUrl;
        
        [Header("热更新配置")]
        [SerializeField] private bool checkVersion = true;
        [SerializeField] private bool verifyMD5 = true;
        [SerializeField] private bool loadAOTMetadata = true;
        
        [Header("调试选项")]
        [SerializeField] private bool showDebugLog = true;

        private string _platform;
        private string _hotUpdateDir;
        private VersionInfo _remoteVersion;
        private Assembly _hotUpdateAssembly;

        private void Awake()
        {
            _serverUrl =
                $"{server}/{Application.version}/{Util.GetPlatformName()}";
        }

        private void Start()
        {
            // PrintAssemblies();
            _platform = GetPlatformName();
            _hotUpdateDir = Path.Combine(Application.persistentDataPath, "HotUpdate");
            
            if (!Directory.Exists(_hotUpdateDir))
            {
                Directory.CreateDirectory(_hotUpdateDir);
            }
            
            // Debug.Log("=== 开始热更新流程 ===");
            // Debug.Log($"平台: {_platform}");
            // Debug.Log($"保存路径: {_hotUpdateDir}");
            StartCoroutine(HotUpdateProcess());
        }
        
        private void PrintAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Debug.Log($"assemblies.Length: {assemblies.Length}");
            int i = 1;
            foreach (var asm in assemblies)
            {
                Debug.Log(i + ", " + asm?.GetName()?.Name);
                i++;
            }
        }

        /// <summary>
        /// 热更新完整流程
        /// </summary>
        private IEnumerator HotUpdateProcess()
        {
            string dllPath = Path.Combine(_hotUpdateDir, "HotUpdate.dll");
            
            // 1. 检查版本
            if (checkVersion)
            {
                yield return StartCoroutine(CheckVersion());
                
                if (_remoteVersion != null)
                {
                    // 2. 下载热更新 DLL
                    bool needDownload = true;
            
                    if (File.Exists(dllPath) && _remoteVersion != null && verifyMD5)
                    {
                        string localMD5 = CalculateMD5(dllPath);
                        if (localMD5 == _remoteVersion.hotUpdateDll.md5)
                        {
                            Log.Info("本地 DLL 已是最新版本，跳过下载");
                            needDownload = false;
                        }
                    }
            
                    if (needDownload)
                    {
                        var hotUpdateCompleted = false;
                        string dllUrl = _remoteVersion != null 
                            ? _remoteVersion.hotUpdateDll.url 
                            : $"{_serverUrl}/HotUpdate.dll";
                    
                        SetTipsByKey("#download_update");
                        yield return FileDownloader.Instance.Download(dllUrl, dllPath, s =>
                        {
                            hotUpdateCompleted = true;
                        }, s =>
                        {
                            ShowDialog(s);
                            Debug.LogError($"download fail: {s}");
                        }, f =>
                        {
                            SetProgress(LoadingKey.HotUpdate, f);
                        });

                        if (!hotUpdateCompleted)
                        {
                            yield break;
                        }
                    }
                    else
                    {
                        Debug.Log("HotUpdate 无需更新");
                    }

                    // 3. 加载补充元数据 DLL
                    if (loadAOTMetadata)
                    {
                        yield return StartCoroutine(LoadMetadataForAOTAssemblies());
                    }
                }
                else
                {
                    Log.Error("版本检查失败，使用本地版本");
                }
            }
            LoadAssemblyAndLaunch(dllPath);
        }

        private void DownloadHotUpdateDllCompleted(string dllPath)
        {
            // 验证 MD5
            if (verifyMD5 && _remoteVersion != null)
            {
                string downloadedMD5 = CalculateMD5(dllPath);
                if (downloadedMD5 != _remoteVersion.hotUpdateDll.md5)
                {
                    Log.Error($"MD5 校验失败！期望: {_remoteVersion.hotUpdateDll.md5}, 实际: {downloadedMD5}");
                    File.Delete(dllPath);
                }
                Debug.Log("MD5 校验通过");
            }
        }

        private void LoadAssemblyAndLaunch(string dllPath)
        {
            if (!VerifyDlls())
            {
                ShowDialog(LocalLanguage.Instance.GetString("#verify_dll_fail"));
                return;
            }
            SetTipsByKey("#launch_update");
            // 加载热更新 DLL
            LoadHotUpdateAssembly(dllPath);
            // 启动热更新代码
            StartHotUpdateCode();
        }

        private bool VerifyDlls()
        {
            string dllPath = Path.Combine(_hotUpdateDir, "HotUpdate.dll");
            if (!File.Exists(dllPath))
            {
                return false;
            }
            string[] aotDlls = new string[]
            {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll"
            };
            foreach (var aotDll in aotDlls)
            {
                var aotPath = Path.Combine(_hotUpdateDir, "AOT", aotDll);
                if (!File.Exists(aotPath))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查版本
        /// </summary>
        private IEnumerator CheckVersion()
        {
            string versionUrl = $"{_serverUrl}/version.json";
            Log.Info($"检查版本: {versionUrl}");
            SetTipsByKey("#check_update");
            using (UnityWebRequest request = UnityWebRequest.Get(versionUrl))
            {
                request.timeout = 10;
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    _remoteVersion = JsonUtility.FromJson<VersionInfo>(json);
                }
                else
                {
                    Debug.LogError($"版本检查失败: {request.error}");
                }
            }
        }

        /// <summary>
        /// 加载补充元数据 DLL
        /// 为 AOT 泛型提供元数据支持
        /// </summary>
        private IEnumerator LoadMetadataForAOTAssemblies()
        {
            Debug.Log("=== 开始加载补充元数据 ===");
            
            // 需要加载的 AOT DLL 列表
            string[] aotDlls = new string[]
            {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll"
            };
            
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            int i = 0;
            var downloadFailed = false;
            foreach (string dllName in aotDlls)
            {
                i++;
                string dllPath = Path.Combine(_hotUpdateDir, "AOT", dllName);
                
                // 如果本地不存在，尝试下载
                if (!File.Exists(dllPath))
                {
                    string dllUrl = $"{_serverUrl}/AOT/{dllName}";
                    string aotDir = Path.Combine(_hotUpdateDir, "AOT");
                    if (!Directory.Exists(aotDir))
                    {
                        Directory.CreateDirectory(aotDir);
                    }
                    
                    yield return StartCoroutine(
                        FileDownloader.Instance.Download(dllUrl, dllPath, s =>
                        {
                            
                        }, s =>
                        {
                            // SetTipsByKey("#download_fail", s);
                            downloadFailed = true;
                            Debug.LogError($"download fail: {s}");
                        }, f =>
                        {
                            var p = ((i - 1) * 1.0f + f) / aotDlls.Length;
                            SetProgress(LoadingKey.AOT, p);
                        }));
                }
                
                if (File.Exists(dllPath))
                {
                    byte[] dllBytes = File.ReadAllBytes(dllPath);
                    LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                    
                    if (err == LoadImageErrorCode.OK)
                    {
                        Log.Info($"加载 AOT 元数据成功: {dllName}");
                    }
                    else
                    {
                        Log.Error($"加载 AOT 元数据失败: {dllName}, 错误码: {err}");
                    }
                }
                else
                {
                    Log.Warning($"找不到 AOT DLL: {dllName}");
                }
            }

            if (downloadFailed)
            {
                ShowDialog(LocalLanguage.Instance.GetString("#download_fail"));
            }
            
            Log.Info("=== 补充元数据加载完成 ===");
        }

        /// <summary>
        /// 加载热更新程序集
        /// </summary>
        private void LoadHotUpdateAssembly(string dllPath)
        {
            Debug.Log("=== 开始加载热更新程序集 ===");
            try
            {
                byte[] dllBytes = File.ReadAllBytes(dllPath);
                _hotUpdateAssembly = Assembly.Load(dllBytes);
                Debug.Log($"热更新程序集加载成功: {_hotUpdateAssembly.FullName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"热更新程序集加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 启动热更新代码
        /// </summary>
        private void StartHotUpdateCode()
        {
            // if (hotUpdateAssembly == null)
            // {
            //     Log.Error("热更新程序集未加载，无法启动");
            //     return;
            // }
            
            Debug.Log("=== 启动热更新代码 ===");
            
            try
            {
                StartCoroutine(LoadLaunchPrefab());
            }
            catch (Exception e)
            {
                Log.Error($"启动热更新代码失败: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private IEnumerator InitAddressables()
        {
            var initHandle = Addressables.InitializeAsync(false);
            yield return initHandle;

            if (initHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Log.Error("Addressables 初始化失败");
            }
            else
            {
                Log.Info("Addressables 初始化完成");
            }
            initHandle.Release();
        }
        
        private IEnumerator LoadLaunchPrefab()
        {
            yield return InitAddressables();
            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            yield return checkHandle;

            List<string> catalogs = checkHandle.Result;
            checkHandle.Release();
            if (catalogs != null && catalogs.Count > 0)
            {
                Log.Info("发现 Catalog 更新");
                var updateHandle = Addressables.UpdateCatalogs(catalogs);
                yield return updateHandle;
                Log.Info("Catalog 更新完成");
            }
            else
            {
                Log.Info("Catalog 没有更新");
            }
            
            var prefabKey = "Assets/GameMain/Prefabs/UnityGameFramework.prefab";
            var sizeHandle = Addressables.GetDownloadSizeAsync(prefabKey);
            yield return sizeHandle;

            long downloadSize = sizeHandle.Result;
            if (downloadSize > 0)
            {
                Log.Info($"需要下载资源大小: {downloadSize / 1024f / 1024f} MB");
                yield return Addressables.DownloadDependenciesAsync(prefabKey);
                Log.Info("资源下载完成");
            }
            else
            {
                Log.Info("没有需要下载的资源");
            }
            
            // 加载启动预制体
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(prefabKey);
            yield return new WaitUntil(() => handle.IsDone);

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                ShowDialog(handle.OperationException.Message);
            }
            else
            {
                // 实例化预制体
                GameObject instance = Instantiate(handle.Result);
                instance.transform.position = Vector3.zero;
                Log.Info("LoadLaunchPrefab Finish");
            }
        }

        /// <summary>
        /// 计算文件 MD5
        /// </summary>
        private string CalculateMD5(string filePath)
        {
            return Util.CalculateMD5(filePath);
        }

        private void ShowDialog(string msg)
        {
            var message = LocalLanguage.Instance.GetString("#download_fail");
            message += $"\n{LocalLanguage.Instance.GetString("#check_network")}";
            // message += $"\n{msg}";
            AotDialogUI.Instance.Show(message, () =>
            {
                StartCoroutine(Retry());
            });
        }

        private IEnumerator Retry()
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(HotUpdateProcess());
        }

        private void SetTipsByKey(string key, string msg = null)
        {
            var tips = LocalLanguage.Instance.GetString(key);
            if (!string.IsNullOrEmpty(msg))
            {
                tips = $"{tips}:{msg}";
            }
            LoadingAotUI.Instance.SetTips(tips);
        }

        private void SetProgress(LoadingKey key, float progress)
        {
            // 热更新总进度为 20%，HotUpdate 10%，AOT 10%
            float p = 0;
            if (key == LoadingKey.HotUpdate)
            {
                p = progress / 10;
            }
            else if (key == LoadingKey.AOT)
            {
                p = 0.1f + progress / 10;
            }

            // 起步是 10%，UI显示更友好，尤其是网络差的时候
            p += 0.1f;
            LoadingAotUI.Instance.SetProgress(p);
        }

        /// <summary>
        /// 获取平台名称
        /// </summary>
        private string GetPlatformName()
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


        private enum LoadingKey
        {
            HotUpdate,
            AOT
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


