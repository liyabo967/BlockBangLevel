using DG.Tweening;
using UnityEngine;

namespace Quester
{
    public class Processing : UGuiForm
    {
        public GameObject loading;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            StartAnim();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            StopAnim();
        }

        private void StartAnim()
        { 
            loading.transform.DORotate(
                    new Vector3(0, 0, 360), // 旋转一圈
                    1f,                     // 用时 1 秒
                    RotateMode.FastBeyond360
                )
                .SetEase(Ease.Linear)       // 匀速（很关键）
                .SetLoops(-1, LoopType.Restart);
        }

        private void StopAnim()
        {
            loading.transform.DOKill();
        }
    }
}