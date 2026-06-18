using System;
using System.Collections;
using AppsFlyerSDK;
using Facebook.Unity;
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
            InitFacebook();
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
                    Debug.Log("Firebase.FirebaseApp is available.");
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    var app = Firebase.FirebaseApp.DefaultInstance;
                    Firebase.Analytics.FirebaseAnalytics.LogEvent("game_start");
                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                } else {
                    UnityEngine.Debug.LogError(System.String.Format(
                        "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
#endif
        }

        private void InitFacebook()
        {
            if (FB.IsInitialized) {
                FB.ActivateApp();
            } else {
                //Handle FB.Init
                FB.Init(FB.ActivateApp);
            }
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
            CoroutineRunner.Instance.Delay(2f, () =>
            {
                GameEntry.Event.Fire(this, ProgressEventArgs.Create(ProgressEventArgs.ProgressKey.InitSdk, 0.5f));
            });
            CoroutineRunner.Instance.Delay(4f, () =>
            {
                _completed = true;
                GameEntry.Event.Fire(this, ProgressEventArgs.Create(ProgressEventArgs.ProgressKey.InitSdk, 1f));
            });
            SubscribeAdEvent();
            AdManager.Instance.Init((success) =>
            {
                _completed = true;
                InitAppsFlyer();
                GameEntry.Event.Fire(this, ProgressEventArgs.Create(ProgressEventArgs.ProgressKey.InitSdk, 1f));
            });
        }

        private void InitAppsFlyer()
        {
            string devKey = "kq7EtcBRk5FyRBA86KTvPa";
            string appID = "com.quester.game.blockbang";
#if UNITY_ANDROID
            appID = "com.quester.game.blockbang";
#elif UNITY_IOS
            appID = "6749655294";
#endif
            AppsFlyer.initSDK(devKey, appID);
            AppsFlyer.OnRequestResponse += AppsFlyerOnRequestResponse;
            AppsFlyerConsent consent = AppsFlyerConsent.ForGDPRUser(true, true);
            AppsFlyer.setConsentData(consent);
            AppsFlyer.setIsDebug(true);
            AppsFlyer.startSDK();
        }
        
        private void AppsFlyerOnRequestResponse(object sender, EventArgs e)
        {
            var args = e as AppsFlyerRequestEventArgs;
            Debug.Log("AppsFlyerOnRequestResponse, status code " + args.statusCode);
            AppsFlyer.AFLog("AppsFlyerOnRequestResponse", " status code " + args.statusCode);
        }

        private void SubscribeAdEvent()
        {
            AdManager.Instance.OnRequest += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.Request, result.AdNetwork);
            };
            
            AdManager.Instance.OnLoaded += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.Loaded, result.AdNetwork);
            };
            
            AdManager.Instance.OnShown += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.Show, result.AdNetwork);
            };
            
            AdManager.Instance.OnShowFailed += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.FailedShow, result.AdNetwork);
            };
            
            AdManager.Instance.OnClicked += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.Clicked, result.AdNetwork);
            };

            AdManager.Instance.OnRewarded += result =>
            {
                GameAnalyticsManager.SendAdEvent(GetAdType(result), GAAdAction.RewardReceived, result.AdNetwork);
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