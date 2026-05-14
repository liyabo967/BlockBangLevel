using System.Collections;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
using Firebase.Extensions;
using GameAnalyticsSDK;
using GameMain.Scripts.HotUpdate.Base.Ads;
using UnityEngine;

namespace Quester
{
    public class ProcedureSdk : ProcedureBase
    {
        private bool _completed;
        
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            InitFirebase();
            InitATT();
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!_completed)
            {
                return;
            }
            ChangeState<ProcedurePreload>(procedureOwner);
        }

        private void InitFirebase()
        {
#if !UNITY_EDITOR
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available) {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    var app = Firebase.FirebaseApp.DefaultInstance;

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                } else {
                    UnityEngine.Debug.LogError(System.String.Format(
                        "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
#endif
        }

        private void InitATT()
        {
            ATTManager.Instance.RequestAuthorization(status =>
            {
                InitAd();
            });
        }
        
        private void InitAd()
        {
            CoroutineRunner.Instance.Delay(7f, () =>
            {
                GameEntry.Event.Fire(this, ProgressEventArgs.Create(ProgressEventArgs.ProgressKey.InitSdk, 0.5f));
            });
            CoroutineRunner.Instance.Delay(15f, () =>
            {
                _completed = true;
                GameEntry.Event.Fire(this, ProgressEventArgs.Create(ProgressEventArgs.ProgressKey.InitSdk, 1f));
            });
            SubscribeAdEvent();
            AdManager.Instance.Init((success) =>
            {
                _completed = true;
                GameEntry.Event.Fire(this, ProgressEventArgs.Create(ProgressEventArgs.ProgressKey.InitSdk, 1f));
            });
        }

        private void SubscribeAdEvent()
        {
            AdManager.Instance.OnRequest += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.Request);
            };
            
            AdManager.Instance.OnLoaded += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.Loaded);
            };
            
            AdManager.Instance.OnShown += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.Show);
            };
            
            AdManager.Instance.OnShowFailed += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.FailedShow);
            };
            
            AdManager.Instance.OnClicked += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.Clicked);
            };

            AdManager.Instance.OnRewarded += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.RewardReceived);
            };
        }

        private GAAdType GetAdType(AdResult result)
        {
            switch (result.AdType)
            {
                case AdType.Banner:
                    return GAAdType.Banner;
                case AdType.Interstitial:
                    return GAAdType.Interstitial;
                case AdType.RewardedVideo:
                    return GAAdType.RewardedVideo;
            }
            return GAAdType.Undefined;
        }
    }
}