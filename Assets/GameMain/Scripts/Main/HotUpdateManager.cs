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
        
        private const string HotUpdateDllName = "HotUpdate.dll.bytes";
        
        private string _serverUrl;
        private string _platform;
        private string _hotUpdateDir;
        private string _hotUpdateStreamingDir;
        private string _aotDir;
        private string _aotStreamingDir;
        private VersionInfo _remoteVersion;
        private Assembly _hotUpdateAssembly;

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
            CreateDirectory(_aotStreamingDir);
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
        
        private IEnumerator HotUpdateProcess()
        {
            yield return StartCoroutine(CheckVersion());
            if (_remoteVersion != null)
            {
                if (_remoteVersion.forceUpdate)
                {
                    ShowForceUpdateDlg();
                    yield break;
                }
                // check dll hash
                var sameHash = string.Equals(_remoteVersion.hotUpdateDll.hash, GetHotUpdateDllHash());
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
            LoadDlls();
            StartCoroutine(LoadLaunchPrefab());
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
                    if (!string.Equals(aotDllInfo.hash, GetAOTDllHash(aotDll)))
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
            Debug.LogError($"download fail: {errorMsg}");
        }

        private string GetAOTDllHash(string dllName)
        {
            var dllPath = Path.Combine(_aotDir, $"{dllName}.bytes");
            if (!File.Exists(dllPath))
            {
                dllPath = Path.Combine(_aotStreamingDir, $"{dllName}.bytes");
            }
            return Util.CalculateMD5(dllPath);
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
                    Debug.Log("OpenStore");
                    AppStoreUtil.OpenStore();
                }
            });
        }

        private void LoadDlls()
        {
            LoadMetadataDll();
            LoadHotUpdateDll();
        }

        private void LoadMetadataDll()
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
                
                if (File.Exists(dllPath))
                {
                    byte[] dllBytes = File.ReadAllBytes(dllPath);
                    LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                    if (err != LoadImageErrorCode.OK)
                    {
                        Log.Error($"AOT metadata loaded failed: {dllName}, errorCode: {err}");
                    }
                }
                else
                {
                    Debug.LogError($"AOT dll not exist: {dllPath}");
                }
            }
        }
        
        private void LoadHotUpdateDll()
        {
            try
            {
                string dllPath = Path.Combine(_hotUpdateDir, HotUpdateDllName);
                if (!File.Exists(dllPath))
                {
                    dllPath = Path.Combine(_hotUpdateStreamingDir, HotUpdateDllName);
                }
                byte[] dllBytes = File.ReadAllBytes(dllPath);
                _hotUpdateAssembly = Assembly.Load(dllBytes);
                Debug.Log($"HotUpdate dll loaded success: {_hotUpdateAssembly.FullName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"HotUpdate dll loaded failed: {e.Message}\n{e.StackTrace}");
            }
        }

        private string GetHotUpdateDllHash()
        {
            string dllPath = Path.Combine(_hotUpdateDir, HotUpdateDllName);
            if (!File.Exists(dllPath))
            {
                dllPath =  Path.Combine(_hotUpdateStreamingDir, HotUpdateDllName);
            }
            return Util.CalculateMD5(dllPath);
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
                var downloadSizeStr = Util.FormatSize(downloadSize);
                Log.Info($"需要下载资源大小: {downloadSizeStr}");
                SetTipsByKey("#loading");
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
                request.timeout = 10;
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    // _remoteVersion = JsonUtility.FromJson<VersionInfo>(json);
                    _remoteVersion = JsonConvert.DeserializeObject<VersionInfo>(json);
                }
                else
                {
                    Debug.LogError($"CheckVersion Error: {request.error}");
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