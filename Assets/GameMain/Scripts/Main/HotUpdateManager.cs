using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HybridCLR;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public class HotUpdateManager : MonoBehaviour
    {
        private const string Server = "https://assets-1301567094.cos.ap-beijing.myqcloud.com/block-bang";
        private const string CheckNetworkServer = "https://www.apple.com";
        
        private const string HotUpdateDllName = "HotUpdate.dll.bytes";
        private const string LaunchPrefabKey = "Assets/GameMain/Prefabs/UnityGameFramework.prefab";
        
        private string _serverUrl;
        private string _platform;
        private string _hotUpdateDir;
        private string _hotUpdateStreamingDir;
        private string _aotDir;
        private string _aotStreamingDir;
        private VersionInfo _remoteVersion;
        private Assembly _hotUpdateAssembly;

        private bool _networkConnected;
        private bool _downloadError;
        
        private void Awake()
        {
            _platform = Util.GetPlatformName();
            _serverUrl = $"{Server}/{Application.version}/{_platform}";
            _hotUpdateDir = Path.Combine(Application.persistentDataPath, Application.version, "HotUpdate");
            _hotUpdateStreamingDir = Path.Combine(Application.streamingAssetsPath, "HotUpdate");
            _aotDir = Path.Combine(_hotUpdateDir, "AOT");
            _aotStreamingDir = Path.Combine(_hotUpdateStreamingDir, "AOT");
            
            CreateDirectory(_aotDir);
        }

        private void Start()
        {
            // PrintAssemblies();
            StartCoroutine(HotUpdateProcess());
        }

        private void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private IEnumerator CheckNetwork(Action<bool> callback)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                callback?.Invoke(false);
                yield break;
            }
            using var request = UnityWebRequest.Get(CheckNetworkServer);
            request.timeout = 5;
            yield return request.SendWebRequest();
            bool success = request.result == UnityWebRequest.Result.Success;
            callback?.Invoke(success);
        } 
        
        private IEnumerator HotUpdateProcess()
        {
            yield return StartCoroutine(CheckNetwork(result =>
            {
                _networkConnected = result;
                Debug.Log($"Network result: {_networkConnected}");
            }));
            
            if (!_networkConnected && PlayerPrefs.GetString("LaunchVersion") != Application.version)
            {
                // 第一次启动，如果网络没有连接，直接弹窗提示
                ShowNetworkError();
                yield break;
            }
            
            if (_networkConnected)
            {
                yield return StartCoroutine(CheckVersion());
            }
            if (_remoteVersion != null)
            {
                if (_remoteVersion.forceUpdate)
                {
                    ShowForceUpdateDlg();
                    yield break;
                }
                // check dll hash
                var localHash = "";
                yield return StartCoroutine(GetHotUpdateDllHash(result =>
                {
                    localHash = result;
                }));
                var sameHash = string.Equals(_remoteVersion.hotUpdateDll.hash, localHash);
                if (!sameHash)
                {
                    yield return DownloadHotUpdateDll(_remoteVersion);
                    if (_downloadError)
                    {
                        ShowDownloadError("HotUpdate");
                        yield break;
                    }
                }
                else
                {
                    Debug.Log("HotUpdate is already the latest version");
                }
                
                yield return DownloadMetadataDll(_remoteVersion);
                if (_downloadError)
                {
                    yield break;
                }
            }
            else
            {
                Debug.Log("remote check failed, load from local");
            }
            yield return StartCoroutine(LoadDlls());
            yield return StartCoroutine(UpdateCatalog());
            yield return StartCoroutine(LoadLaunchPrefab());
        }

        private IEnumerator DownloadHotUpdateDll(VersionInfo version)
        {
            string dllUrl = string.IsNullOrEmpty(version.hotUpdateDll.url) ? $"{_serverUrl}/HotUpdate/{HotUpdateDllName}" : version.hotUpdateDll.url;
            SetTipsByKey("#loading");
            yield return FileDownloader.Instance.Download(dllUrl, $"{_hotUpdateDir}/{HotUpdateDllName}", s =>
            {
                _downloadError = false;
            }, s =>
            {
                _downloadError = true;
            }, f =>
            {
                SetProgress(LoadingKey.HotUpdate, f);
            });
        }

        private IEnumerator DownloadMetadataDll(VersionInfo version)
        {
            string[] aotDlls = new string[]
            {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll"
            };
            var count = 0;
            foreach (var aotDll in aotDlls)
            {
                if (version.aotDllDict.TryGetValue(aotDll, out var aotDllInfo))
                {
                    var aotHash = string.Empty;
                    yield return GetAOTDllHash(aotDll, result =>
                    {
                        aotHash = result;
                    });
                    
                    if (!string.Equals(aotDllInfo.hash, aotHash))
                    {
                        string dllUrl = string.IsNullOrEmpty(aotDllInfo.url) ? $"{_serverUrl}/HotUpdate/AOT/{aotDll}.bytes" : aotDllInfo.url;
                        yield return FileDownloader.Instance.Download(dllUrl, $"{_aotDir}/{aotDll}.bytes", s =>
                        {
                            _downloadError = false;
                        }, s =>
                        {
                            _downloadError = true;
                        }, f =>
                        {
                            
                        });
                        
                        if (_downloadError)
                        {
                            ShowDownloadError(aotDll);
                            yield break;
                        }
                    }
                    else
                    {
                        Debug.Log($"AOT is already the latest version, {aotDll}");
                    }
                }

                count++;
                SetProgress(LoadingKey.AOT, count * 1.0f / aotDlls.Length);
            }
        }

        private void ShowDownloadError(string fileName)
        {
            var errorMsg = LocalLanguage.Instance.GetString("#check_network");
            var detail = $"{LocalLanguage.Instance.GetString("#download_fail")}: {fileName}";
            errorMsg += "\n" + detail;
            ShowDialog(errorMsg);
        }
        
        private void ShowNetworkError()
        {
            var errorMsg = LocalLanguage.Instance.GetString("#check_network");
            ShowDialog(errorMsg);
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

        private void ShowForceUpdateDlg()
        {
            var updateTitle = LocalLanguage.Instance.GetString("#tips");
            var updateTips = LocalLanguage.Instance.GetString("#update_tips");
            AotDialogUI.Instance.Show(new AotDialogUI.AotDialogParams()
            {
                title = updateTitle,
                message = updateTips,
                confirmText = LocalLanguage.Instance.GetString("#update"),
                callback = () => {
                    AppStoreUtil.OpenStore();
                }
            });
        }

        private IEnumerator LoadDlls()
        {
            yield return StartCoroutine(LoadMetadataDll());
            yield return StartCoroutine(LoadHotUpdateDll());
        }

        private IEnumerator LoadMetadataDll()
        {
            // 需要加载的 AOT DLL 列表
            string[] aotDlls = new string[]
            {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll"
            };
            
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            string aotDir = _aotDir;
            if (!CheckMetadata(aotDir, aotDlls))
            {
                aotDir =_aotStreamingDir;
            }
            
            foreach (string dllName in aotDlls)
            {
                string dllPath = Path.Combine(aotDir, $"{dllName}.bytes");

                yield return StartCoroutine(FileLoader.LoadFile(dllPath, bytes =>
                {
                    if (bytes == null)
                    {
                        Debug.LogError($"AOT dll loaded failed: {dllPath}");
                    }
                    else
                    {
                        LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(bytes, mode);
                        if (err != LoadImageErrorCode.OK)
                        {
                            Log.Error($"AOT metadata loaded failed: {dllName}, errorCode: {err}");
                        }
                    }
                }));
            }
        }
        
        private IEnumerator LoadHotUpdateDll()
        {
            string dllPath = Path.Combine(_hotUpdateDir, HotUpdateDllName);
            if (!File.Exists(dllPath))
            {
                dllPath = Path.Combine(_hotUpdateStreamingDir, HotUpdateDllName);
            }

            yield return StartCoroutine(FileLoader.LoadFile(dllPath, bytes =>
            {
                if (bytes == null)
                {
                    Debug.LogError($"HotUpdate dll loaded failed: {dllPath}");
                }
                else
                {
                    _hotUpdateAssembly = Assembly.Load(bytes);
                    // Debug.Log($"HotUpdate dll loaded success: {_hotUpdateAssembly.FullName}");
                }
            }));
        }

        private IEnumerator GetHotUpdateDllHash(Action<string> callback)
        {
            string dllPath = Path.Combine(_hotUpdateDir, HotUpdateDllName);
            if (!File.Exists(dllPath))
            {
                dllPath =  Path.Combine(_hotUpdateStreamingDir, HotUpdateDllName);
            }
            return FileLoader.LoadFile(dllPath, bytes =>
            {
                callback?.Invoke(Util.CalculateMD5(bytes));
            });
        }
        
        private IEnumerator GetAOTDllHash(string dllName, Action<string> callback)
        {
            var dllPath = Path.Combine(_aotDir, $"{dllName}.bytes");
            if (!File.Exists(dllPath))
            {
                dllPath = Path.Combine(_aotStreamingDir, $"{dllName}.bytes");
            }
            return FileLoader.LoadFile(dllPath, bytes =>
            {
                callback?.Invoke(Util.CalculateMD5(bytes));
            });
        }

        private bool CheckMetadata(string dir, string[] dllNames)
        {
            foreach (var dllName in dllNames)
            {
                if (!File.Exists(Path.Combine(dir, $"{dllName}.bytes")))
                {
                    return false;
                }
            }
            return true;
        }

        private IEnumerator UpdateCatalog()
        {
            yield return InitAddressables();
            if (!_networkConnected)
            {
                yield break;
            }
            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            yield return checkHandle;
            if (checkHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"CheckForCatalogUpdates failed: {checkHandle.Status}");
                yield break;
            }

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
            
            var sizeHandle = Addressables.GetDownloadSizeAsync(LaunchPrefabKey);
            yield return sizeHandle;

            long downloadSize = sizeHandle.Result;
            if (downloadSize > 0)
            {
                var downloadSizeStr = Util.FormatSize(downloadSize);
                Log.Info($"需要下载资源大小: {downloadSizeStr}");
                SetTipsByKey("#loading");
                var downloadHandle = Addressables.DownloadDependenciesAsync(LaunchPrefabKey);
                yield return downloadHandle;
                Log.Info("资源下载完成");
            }
            else
            {
                Debug.Log("没有需要下载的资源");
            }
        } 
        
        private IEnumerator LoadLaunchPrefab()
        {
            // 加载启动预制体
            Debug.Log("LoadLaunchPrefab");
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(LaunchPrefabKey);
            yield return new WaitUntil(() => handle.IsDone);
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                var msg = LocalLanguage.Instance.GetString("#download_fail");
                msg += "\n" + handle.OperationException.Message;
                ShowDialog(msg);
            }
            else
            {
                Launch(handle.Result);
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

        private void Launch(GameObject launchPrefab)
        {
            GameObject instance = Instantiate(launchPrefab);
            instance.transform.position = Vector3.zero;
            Log.Info("LoadLaunchPrefab Finish");
        }
        
        /// <summary>
        /// 检查版本
        /// </summary>
        private IEnumerator CheckVersion()
        {
            string versionUrl = $"{_serverUrl}/version.json";
            Log.Info($"CheckVersion: {versionUrl}");
            SetTipsByKey("#check_update");
            using (UnityWebRequest request = UnityWebRequest.Get(versionUrl))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();
                Debug.Log($"CheckVersion result: {request.result}");
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    // _remoteVersion = JsonUtility.FromJson<VersionInfo>(json);
                    _remoteVersion = JsonConvert.DeserializeObject<VersionInfo>(json);
                }
                else
                {
                    Debug.Log($"CheckVersion Error: {request.error}");
                }
            }
        }
        
        private void ShowDialog(string msg)
        {
            var message = msg;
            // message += $"\n{LocalLanguage.Instance.GetString("#check_network")}";
            // message += $"\n{msg}";
            var title = LocalLanguage.Instance.GetString("#tips");
            AotDialogUI.Instance.Show(new AotDialogUI.AotDialogParams()
            {
                title = title,
                message = message,
                confirmText = LocalLanguage.Instance.GetString("#retry"),
                callback = () =>
                {
                    StartCoroutine(Retry());
                }
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
        
        private enum LoadingKey
        {
            HotUpdate,
            AOT
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