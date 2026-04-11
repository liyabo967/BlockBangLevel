using System;

namespace GameFramework.Resource
{
    public interface ICustomResourceManager
    {
        void Init(Action<bool> complete);
        
        void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, int priority, object userData);

        void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData);

        void LoadAsset(string assetName, Type assetType, int priority, GameFramework.Resource.LoadAssetCallbacks loadAssetCallbacks, object userData);

        void UnloadAsset(object asset);
    }
}