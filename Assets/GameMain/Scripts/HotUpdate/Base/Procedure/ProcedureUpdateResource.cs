

using System.Collections;
using System.Collections.Generic;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityGameFramework.Runtime;

namespace Quester
{
    public class ProcedureUpdateResource : ProcedureBase
    {
        private bool _checkResourceCompleted = false;
        
        private const string RemoteLabel = "remote";

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            Log.Info("ProcedureUpdateResource OnEnter");
            base.OnEnter(procedureOwner);
            CoroutineRunner.Instance.StartCo(CheckAndUpdate());
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!_checkResourceCompleted)
            {
                return;
            }
            GameEntry.Event.Fire(this,
                ProgressEventArgs.Create(ProgressEventArgs.ProgressKey.UpdateResource, 1f));
            ChangeState<ProcedurePreload>(procedureOwner);
        }

        private IEnumerator CheckAndUpdate()
        {
            // 热更已经检查过 catalog，可以直接检查下载大小
            AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(RemoteLabel);
            yield return sizeHandle;

            long size = sizeHandle.Result;

            if (size > 0)
            {
                Debug.Log($"需要下载资源 {size / 1024f / 1024f} MB");

                // 下载依赖
                AsyncOperationHandle downloadHandle =
                    Addressables.DownloadDependenciesAsync(RemoteLabel);

                while (!downloadHandle.IsDone)
                {
                    float progress = downloadHandle.PercentComplete;
                    OnProgress(progress);
                    // Debug.Log($"下载进度 {progress * 100}%");
                    yield return null;
                }

                Debug.Log($"资源下载结束: {downloadHandle.Status == AsyncOperationStatus.Succeeded}");
                DownloadCompleted(downloadHandle.Status == AsyncOperationStatus.Succeeded);
            }
            else
            {
                Debug.Log("没有需要更新的资源");
                _checkResourceCompleted = true;
            }
        }

        private float _lastProgress = 0;
        private void OnProgress(float progress)
        {
            if (progress > _lastProgress + 0.01)
            {
                _lastProgress = progress;
                GameEntry.Event.Fire(this,
                    ProgressEventArgs.Create(ProgressEventArgs.ProgressKey.UpdateResource, progress));
            }
        }
            

        private void DownloadCompleted(bool success)
        {
            _checkResourceCompleted = success;
            if (!success)
            {
                var msg = GameEntry.Localization.GetString("#download_fail") 
                          + "\n" + GameEntry.Localization.GetString("#network_check_tips");
                DialogParams dialogParams = new DialogParams()
                {
                    Title = GameEntry.Localization.GetString("#tips"),
                    Message = msg,
                    Mode = 1,
                    ConfirmText = GameEntry.Localization.GetString("#retry"),
                    OnClickConfirm = Retry
                };
                GameEntry.UI.OpenDialog(dialogParams);
            }
        }

        private void Retry(object obj)
        {
            CoroutineRunner.Instance.StartCo(CheckAndUpdate());
        }
    }
}