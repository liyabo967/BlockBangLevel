using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockPuzzleGameToolkit.Scripts.System;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityGameFramework.Runtime;

namespace GameMain.Scripts.HotUpdate.Base.Ads
{
    public class AdManager : MonoSingleton<AdManager>
    {
        private IAdAdapter _adapter;
        public IAdAdapter Adapter => _adapter;
        
        private AdConfig _adConfig;
        private bool _initialized;
        private bool _consentInfoUpdateInProgress = false;
        private float _lastAdTime = 0;
        private Action<bool> _initializeCallback;
        
        // rewarded, The OnAdRewarded and OnAdClosed are asynchronous
        private bool _isRewarded = false;
        private bool _isRewardedClosed = false;
        private Action<bool> _onRewardedCallback;

        // 对外事件（业务层只订阅这里）
        public event Action<AdResult> OnRequest;
        public event Action<AdResult> OnLoaded;
        public event Action<AdResult> OnShown;
        public event Action<AdResult> OnShowFailed;
        public event Action<AdResult> OnClicked;
        public event Action<AdResult> OnRewarded;
        public event Action<AdResult> OnAdClosed;
        public event Action<AdResult> OnRevenuePaid;

        public void Init(Action<bool> callback)
        {
            _initializeCallback = callback;
            StartConsentFlow();
        }

        private void Initialize(Action<bool> onComplete = null)
        {
            var adSettingsPath = "Assets/GameMain/Settings/AdSettings/LevelPlay.asset";
            Addressables.LoadAssetAsync<AdSettings>(adSettingsPath).Completed += handle =>
            {
                var adSettings = handle.Result;
#if  UNITY_IOS
                _adConfig = adSettings.iOS;
#elif UNITY_ANDROID
                _adConfig = adSettings.Android;
#endif
                _adapter = AdAdapterFactory.Create(adSettings.Platform);
            
                _adapter.OnAdRequest += result =>
                {
                    OnRequest?.Invoke(result);
                };
                _adapter.OnAdLoaded += result =>
                {
                    Log.Info($"Ad Load success: {result.AdType}, {result.AdNetwork}");
                    OnLoaded?.Invoke(result);
                };
                _adapter.OnAdShown += result =>
                {
                    OnShown?.Invoke(result);
                };
                _adapter.OnAdShowFailed += result =>
                {
                    OnShowFailed?.Invoke(result);
                };
                _adapter.OnAdClicked += result =>
                {
                    OnClicked?.Invoke(result);
                };
                _adapter.OnAdRewarded += r =>
                {
                    OnRewardedHandler(r);
                    OnRewarded?.Invoke(r);
                };
                _adapter.OnAdClosed += r =>
                {
                    OnAdClosedHandler(r);
                    OnAdClosed?.Invoke(r);
                    // 关闭后自动预加载下一个
                    _adapter.LoadAd(r.AdType);
                };
                _adapter.OnAdRevenuePaid += r =>
                {
                    OnRevenuePaid?.Invoke(r);
                };
                _adapter.OnAdLoadFailed += r =>
                {
                    Log.Error($"Ad Load failed: {r.AdType}, {r.Message}");
                };

                _adapter.Initialize(_adConfig, success =>
                {
                    _initialized = success;
                    _initializeCallback?.Invoke(success);
                    if (success)
                    {
                        // 预加载常用广告
                        // _adapter.LoadAd(AdType.Banner);
                        // _adapter.LoadAd(AdType.Interstitial);
                        // _adapter.LoadAd(AdType.RewardedVideo);
                        StartCoroutine(DelayLoadAd());
                    }

                    onComplete?.Invoke(success);
                });
            };
            
        }

        private IEnumerator DelayLoadAd()
        {
            yield return new WaitForSeconds(1f);
            _adapter.LoadAd(AdType.Interstitial);
            yield return new WaitForSeconds(1f);
            _adapter.LoadAd(AdType.RewardedVideo);
        }

        public bool IsReady(AdType type) => _adapter?.IsAdReady(type) ?? false;

        public void LoadAd(AdType type)
        {
            if (_adapter != null)
            {
                _adapter.LoadAd(type);
            }
        }

        private bool CanShowInterstitial()
        {
#if UNITY_EDITOR
            return false;
#endif
            
            if (GameDataManager.GetLevelNum() < 10)
            {
                return false;
            }

            if (Time.time - _lastAdTime < 180f)
            {
                return false;
            }

            // if (failCount >= 3)
            //     return false;

            return true;
        }
        
        public void ShowInterstitial()
        {
            if (!_initialized)
            {
                return;
            }
            if (IsReady(AdType.Interstitial))
            {
                if (CanShowInterstitial())
                {
                    _adapter.ShowAd(AdType.Interstitial);
                    _lastAdTime = Time.time;
                }
            }
            else
            {
                _adapter.LoadAd(AdType.Interstitial);
            }
        }

        public void ShowRewarded(Action<bool> onResult)
        {
            if (!_initialized)
            {
                return;
            }
            if (!IsReady(AdType.RewardedVideo))
            {
                _adapter.LoadAd(AdType.RewardedVideo);
                onResult?.Invoke(false);
                return;
            }

            _isRewarded = false;
            _isRewardedClosed = false;
            _onRewardedCallback = onResult;
            _adapter.ShowAd(AdType.RewardedVideo);
        }

        private void OnRewardedHandler(AdResult result)
        {
            _isRewarded = true;
            TryToReward();
        }

        private void OnAdClosedHandler(AdResult result)
        {
            if (result.AdType == AdType.RewardedVideo)
            {
                _isRewardedClosed = true;
                TryToReward();
            }
        }

        private void TryToReward()
        {
            // Debug.Log($"Try to reward:, closed:{_isRewardedClosed}, rewarded:{_isRewarded}");
            if (_isRewardedClosed && _isRewarded)
            {
                _onRewardedCallback?.Invoke(true);
                _isRewardedClosed = false;
                _isRewarded = false;
            }
        }

        public void RemoveAds()
        {
            HideBanner();
        }

        public void ShowBanner()
        {
            if (!_initialized)
            {
                return;
            }
            if (_adapter != null)
            {
                if (_adapter.IsAdReady(AdType.Banner))
                {
                    _adapter?.ShowAd(AdType.Banner);
                }
                else
                {
                    _adapter?.LoadAd(AdType.Banner);
                }
            }
        }

        public void HideBanner()
        {
            if (!_initialized)
            {
                return;
            }
            _adapter?.HideBanner();
        }

        public void ReconsiderUMPConsent()
        {
#if UMP_AVAILABLE && (UNITY_ANDROID || UNITY_IOS)
            ConsentInformation.Reset();
#endif
            StartConsentFlow();
        }
        
        private void StartConsentFlow()
        {
            if (_consentInfoUpdateInProgress)
            {
                return;
            }
            
            _consentInfoUpdateInProgress = true;

            // Skip consent if disabled in settings
            // if (GameManager.instance.GameSettings.skipConsentPopup)
            // {
            //     Debug.Log("Consent popup disabled in settings - skipping consent flow");
            //     Initialize();
            //     _consentInfoUpdateInProgress = false;
            //     return;
            // }

#if UMP_AVAILABLE && (UNITY_ANDROID || UNITY_IOS)
            var request = new ConsentRequestParameters();
            
            if (Debug.isDebugBuild || Application.isEditor)
            {
                var debugSettings = new ConsentDebugSettings
                {
                    DebugGeography = DebugGeography.EEA
                };
                
                var testDeviceIds = new List<string>();
                // testDeviceIds.Add("YOUR-TEST-DEVICE-ID-HERE"); // Uncomment and add your device ID if needed
                
                if (testDeviceIds.Count > 0)
                {
                    debugSettings.TestDeviceHashedIds = testDeviceIds;
                }
                
                request.ConsentDebugSettings = debugSettings;
            }

            ConsentInformation.Update(request, OnConsentInfoUpdated);
#else
            Initialize();
            consentInfoUpdateInProgress = false;
#endif
        }
        
        public bool IsPrivacyOptionsRequired()
        {
#if UMP_AVAILABLE
            return ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
#else
            return false;
#endif
        }
        
#if UMP_AVAILABLE
        private void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                Debug.LogError($"Consent info update error: {consentError}");
                _consentInfoUpdateInProgress = false;
                // Initialize ads even if consent update failed
                Initialize();
                return;
            }

            Debug.Log($"Consent status: {ConsentInformation.ConsentStatus}");

            ConsentForm.LoadAndShowConsentFormIfRequired(OnConsentFormDismissed);
        }

        private void OnConsentFormDismissed(FormError formError)
        {
            _consentInfoUpdateInProgress = false;

            if (formError != null)
            {
                Debug.LogError($"Consent form error: {formError}");
            }
            else
            {
                Debug.Log("Consent form completed");
                Debug.Log($"Final consent status: {ConsentInformation.ConsentStatus}");
                Debug.Log($"Can request personalized ads: {ConsentInformation.CanRequestAds()}");
            }

            // Initialize ads after consent is handled (whether accepted or denied)
            Initialize();
        }
#endif
    }
}