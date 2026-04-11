using System;
using Quester;
using UnityEngine;

namespace GameMain.Scripts.HotUpdate.UI
{
    public class UIRoot : MonoBehaviour
    {
        private Canvas _canvas;
        
        private void Awake()
        {
            _canvas =  GetComponent<Canvas>();
            SetCanvas();
        }

        private void SetCanvas()
        {
            var mainCamera = Camera.main;
            if (mainCamera)
            {
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = mainCamera;
                // _canvas.sortingLayerName = "UI";
            }
        }

        private void Start()
        {
            GameEntry.Event.Subscribe(SceneLoadedSuccessEventArgs.EventId, OnSceneLoadedSuccess);
        }

        private void OnDestroy()
        {
            GameEntry.Event.Unsubscribe(SceneLoadedSuccessEventArgs.EventId, OnSceneLoadedSuccess);
        }

        private void OnSceneLoadedSuccess(object sender, EventArgs eventArgs)
        {
            if (eventArgs is SceneLoadedSuccessEventArgs sceneLoadedSuccessEventArgs)
            {
                Debug.Log($"OnSceneLoadedSuccess: {sceneLoadedSuccessEventArgs.SceneAssetName}");
                SetCanvas();
            }
        }
    }
}