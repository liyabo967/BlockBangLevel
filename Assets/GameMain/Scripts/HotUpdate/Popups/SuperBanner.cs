using DG.Tweening;
using Quester;
using UnityEngine;

namespace GameMain.Scripts.HotUpdate.Popups
{
    public class SuperBanner : UGuiForm
    {
        public CanvasGroup canvasGroup;

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
                _canvas.sortingLayerName = "FX";
                _canvas.sortingOrder = 100;
            }
        }

        protected void Hide()
        {
            canvasGroup.DOFade(0, 0.3f).OnComplete(() =>
            {
                Close(true);
            });
        }
    }
}