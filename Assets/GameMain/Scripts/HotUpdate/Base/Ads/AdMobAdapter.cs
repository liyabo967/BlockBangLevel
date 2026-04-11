using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

namespace GameMain.Scripts.HotUpdate.Base.Ads
{
    public class AdMobAdapter : BaseAdAdapter
    {
        public override AdPlatform Platform => AdPlatform.AdMob;

        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardedAd rewardedAd;

        public override void Initialize(AdConfig config, Action<bool> onComplete)
        {
            Config = config;
            
            List<string> testDeviceIds = new List<string>()
            {
                "41770E7F-5BC6-4322-8B86-98B4F6E5D1DD"
            };

            RequestConfiguration requestConfiguration = new RequestConfiguration();
            requestConfiguration.TestDeviceIds = testDeviceIds;
            MobileAds.SetRequestConfiguration(requestConfiguration);
            Debug.Log("AdMobAdapter Initialize, " + testDeviceIds[0]);
            MobileAds.Initialize(initStatus =>
            {
                IsInitialized = true;
                onComplete?.Invoke(true);
                Log("MobileAds Initialized");
                foreach (var keyValuePair in initStatus.getAdapterStatusMap())
                {
                    Debug.Log("Adapter, " + keyValuePair.Key + ":" + keyValuePair.Value.InitializationState);
                }
            });
        }

        public override void LoadAd(AdType type, string placementId = null)
        {
            switch (type)
            {
                case AdType.Banner:
                    LoadBanner();
                    break;
                case AdType.Interstitial:
                    LoadInterstitial();
                    break;
                case AdType.RewardedVideo:
                    LoadRewarded();
                    break;
            }
        }

        private void LoadBanner()
        {
            bannerView = new BannerView(Config.BannerId, AdSize.Banner, AdPosition.Bottom);
            bannerView.OnBannerAdLoaded += () => RaiseLoaded(new AdResult
            {
                Success = true, 
                AdType = AdType.Banner,
                AdNetwork = bannerView.GetResponseInfo().GetMediationAdapterClassName()
            });
            bannerView.OnBannerAdLoadFailed += (error) =>
            {
                RaiseLoadFailed(new AdResult()
                {
                    Success = false,
                    AdType = AdType.Banner,
                    Message = error.GetMessage()
                });
            };
            bannerView.LoadAd(new AdRequest());
        }

        private void LoadInterstitial()
        {
            InterstitialAd.Load(Config.InterstitialId, new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    RaiseLoadFailed(new AdResult()
                    {
                        Success = false,
                        AdType = AdType.Interstitial,
                        Message = error.GetMessage()
                    });
                    return;
                }

                interstitial = ad;
                interstitial.OnAdFullScreenContentClosed += () =>
                {
                    RaiseClosed(new AdResult() { Success = true, AdType = AdType.Interstitial });
                };
                RaiseLoaded(new AdResult
                {
                    Success = true, 
                    AdType = AdType.Interstitial,
                    AdNetwork = interstitial.GetResponseInfo().GetMediationAdapterClassName()
                });
            });
        }

        private void LoadRewarded()
        {
            RewardedAd.Load(Config.RewardedVideoId, new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    RaiseLoadFailed(new AdResult()
                    {
                        Success = false,
                        AdType = AdType.RewardedVideo,
                        Message = error.GetMessage()
                    });
                    return;
                }

                rewardedAd = ad;
                rewardedAd.OnAdPaid += (val) => RaiseRevenuePaid(new AdResult
                {
                    Revenue = val.Value / 1000000.0,
                    AdType = AdType.RewardedVideo
                });
                rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    RaiseClosed(new AdResult { Success = true, AdType = AdType.RewardedVideo });
                };
                RaiseLoaded(new AdResult
                {
                    Success = true, 
                    AdType = AdType.RewardedVideo,
                    AdNetwork = rewardedAd.GetResponseInfo().GetMediationAdapterClassName()
                });
            });
        }

        public override bool IsAdReady(AdType type)
        {
            return type switch
            {
                AdType.Interstitial => interstitial != null && interstitial.CanShowAd(),
                AdType.RewardedVideo => rewardedAd != null && rewardedAd.CanShowAd(),
                _ => false
            };
        }

        public override void ShowAd(AdType type, string placementId = null)
        {
            switch (type)
            {
                case AdType.Banner:
                    bannerView?.Show();
                    RaiseShown(new AdResult { Success = true, AdType = AdType.Banner });
                    break;
                case AdType.Interstitial:
                    interstitial?.Show();
                    break;
                case AdType.RewardedVideo:
                    rewardedAd?.Show(reward =>
                    {
                        Debug.LogError("show RewardedVideo completed");
                        RaiseRewarded(new AdResult { Success = true, AdType = AdType.RewardedVideo });
                    });
                    break;
            }
        }

        public override void HideBanner()
        {
            bannerView?.Hide();
        }
    }
}