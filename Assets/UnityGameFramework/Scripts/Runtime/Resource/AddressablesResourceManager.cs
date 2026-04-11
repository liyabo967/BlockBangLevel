using System;
using System.Collections.Generic;
using GameFramework.Resource;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityGameFramework.Runtime
{
    public class AddressablesResourceManager : MonoBehaviour, ICustomResourceManager
    {
        private Dictionary<object, AsyncOperationHandle> _operationHandles = new ();
        
        private void Awake()
        {
            GFAdapterResourceManager.Instance.CustomResourceManager = this;
        }

        public async void Init(Action<bool> complete)
        {
            var initHandle = Addressables.InitializeAsync();
            await initHandle.Task;
            complete.Invoke(initHandle.Status == AsyncOperationStatus.Succeeded);
        }

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, int priority, object userData)
        {
            throw new NotImplementedException();
        }

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            throw new NotImplementedException();
        }

        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            var handle = Addressables.LoadAssetAsync<UnityEngine.Object>(assetName);
            handle.Completed += op =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _operationHandles[handle.Result] = handle;
                    loadAssetCallbacks.LoadAssetSuccessCallback(assetName, handle.Result, 0, userData);
                }
                else
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.AssetError, handle.Status.ToString(), userData);
                }
            };
        }

        public void UnloadAsset(object asset)
        {
            if (_operationHandles.TryGetValue(asset, out var handle))
            {
                Addressables.ReleaseInstance(handle);
            }
        }
    }
}