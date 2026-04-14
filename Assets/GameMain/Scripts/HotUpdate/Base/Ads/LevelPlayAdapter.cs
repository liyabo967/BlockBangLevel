using System;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace GameMain.Scripts.HotUpdate.Base.Ads
{
    public class LevelPlayAdapter : BaseAdAdapter
    {
        public override AdPlatform Platform => AdPlatform.LevelPlay;
        
        private LevelPlayRewardedAd _rewardedAd;
        private LevelPlayInterstitialAd _interstitialAd;
        private LevelPlayBannerAd _bannerAd;
        
        public override void Initialize(AdConfig config, Action<bool> onComplete)
        {
            Debug.Log("LevelPlay Initialize");
            Config = config;
            LevelPlay.SetMetaData("is_test_suite", "enable");
            LevelPlay.OnInitSuccess += configuration =>
            {
                SdkInitializationCompletedEvent(configuration);
                onComplete.Invoke(true);
            };
            LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
            // SDK init
            LevelPlay.Init(config.AppId);
        }

        private void SdkInitializationCompletedEvent(LevelPlayConfiguration  levelPlayConfiguration)
        {
            Debug.Log("LevelPlayAdapter initialization success");
            CreateAds();
            LevelPlay.LaunchTestSuite();
        }

        private void SdkInitializationFailedEvent(LevelPlayInitError error)
        {
            Debug.LogError($"LevelPlayAdapter initialization failed, {error.ErrorCode}, {error.ErrorMessage}");
        }
        

        public override void LoadAd(AdType type, string placementId = null)
        {
            switch (type)
            {
                case AdType.Banner:
                    LoadBannerAd();
                    break;
                case AdType.Interstitial:
                    LoadInterstitialAd();
                    break;
                case AdType.RewardedVideo:
                    LoadRewarded();
                    break;
            }
        }
        
        public override void ShowAd(AdType type, string placementId = null)
        {
            switch (type)
            {
                case AdType.Banner:
                    ShowBannerAd();
                    break;
                case AdType.Interstitial:
                    ShowInterstitialAd();
                    break;
                case AdType.RewardedVideo:
                    ShowRewarded();
                    break;
            }
        }

        public override bool IsAdReady(AdType type)
        {
            return type switch
            {
                AdType.Interstitial => _interstitialAd != null && _interstitialAd.IsAdReady(),
                AdType.RewardedVideo => _rewardedAd != null && _rewardedAd.IsAdReady(),
                _ => false
            };
        }
        

        public override void HideBanner()
        {
            HideBannerAd();
        }

        private void CreateAds()
        {
            CreateBannerAd();
            CreateRewardedAd();
            CreateInterstitialAd();
        }

        #region Rewarded
        
        private void CreateRewardedAd() {
            // Create RewardedAd instance
            _rewardedAd = new LevelPlayRewardedAd(Config.RewardedId);

            // Subscribe RewardedAd events
            _rewardedAd.OnAdLoaded += RewardedOnAdLoadedEvent;
            _rewardedAd.OnAdLoadFailed += RewardedOnAdLoadFailedEvent;
            _rewardedAd.OnAdDisplayed += RewardedOnAdDisplayedEvent;
            _rewardedAd.OnAdDisplayFailed += RewardedOnAdDisplayFailedEvent;
            _rewardedAd.OnAdClicked += RewardedOnAdClickedEvent;
            _rewardedAd.OnAdClosed += RewardedOnAdClosedEvent;
            _rewardedAd.OnAdRewarded += RewardedOnAdRewardedEvent;
            _rewardedAd.OnAdInfoChanged += RewardedOnAdInfoChangedEvent;
        }
        private void LoadRewarded() {
            // Load or reload RewardedAd
            _rewardedAd.LoadAd();
        }
        
        private void ShowRewarded() {
            // Show RewardedAd, check if the ad is ready before showing
            if (_rewardedAd.IsAdReady()) {
                _rewardedAd.ShowAd();
            }
        }
        // Implement RewardedAd events
        private void RewardedOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            RaiseLoaded(new AdResult
            {
                Success = true, 
                AdType = AdType.RewardedVideo,
                AdNetwork = adInfo.AdNetwork
            });
        }

        private void RewardedOnAdLoadFailedEvent(LevelPlayAdError error)
        {
            RaiseLoadFailed(new AdResult()
            {
                Success = false,
                AdType = AdType.RewardedVideo,
                Message = error.ErrorMessage
            });
        }
        void RewardedOnAdClickedEvent(LevelPlayAdInfo adInfo) { }

        void RewardedOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            RaiseShown(new AdResult { Success = true, AdType = AdType.RewardedVideo });
        }

        void RewardedOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            RaiseShown(new AdResult { Success = false, AdType = AdType.RewardedVideo, Message = error.ErrorMessage});
        }

        void RewardedOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            RaiseClosed(new AdResult { Success = true, AdType = AdType.RewardedVideo });
        }

        void RewardedOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward adReward)
        {
            RaiseRewarded(new AdResult { Success = true, AdType = AdType.RewardedVideo });
        }
        void RewardedOnAdInfoChangedEvent(LevelPlayAdInfo adInfo) { }

        #endregion

        
        #region Interstitial

        void CreateInterstitialAd() {
            //Create InterstitialAd instance
            _interstitialAd= new LevelPlayInterstitialAd(Config.InterstitialId);

            //Subscribe InterstitialAd events
            _interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
            _interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
            _interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
            _interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
            _interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
            _interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
            _interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;
        }
        
        void LoadInterstitialAd() {
            //Load or reload InterstitialAd 	
            _interstitialAd.LoadAd();
        }
        
        void ShowInterstitialAd() {
            //Show InterstitialAd, check if the ad is ready before showing
            if (_interstitialAd.IsAdReady()) {
                _interstitialAd.ShowAd();
            }
        }
  
        //Implement InterstitialAd events
        void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            RaiseLoaded(new AdResult
            {
                Success = true, 
                AdType = AdType.Interstitial,
                AdNetwork = adInfo.AdNetwork
            });
        }

        void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
        {
            RaiseLoadFailed(new AdResult()
            {
                Success = false,
                AdType = AdType.Interstitial,
                Message = error.ErrorMessage
            });
        }
        void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo) { }

        void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            RaiseShown(new AdResult { Success = true, AdType = AdType.Interstitial });
        }

        void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            RaiseShown(new AdResult { Success = false, AdType = AdType.Interstitial, Message = error.ErrorMessage});
        }
        void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo) { }
        void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo) { }

        #endregion


        #region Banner

        private void CreateBannerAd() {
            // Create ad configuration - optional
            var adConfig = new LevelPlayBannerAd.Config.Builder()
                .SetSize(LevelPlayAdSize.BANNER)
                .SetPosition(LevelPlayBannerPosition.BottomCenter)
                .SetDisplayOnLoad(true)
                .SetRespectSafeArea(true)
                .Build();
        
            // Create banner instance
            _bannerAd = new LevelPlayBannerAd(Config.BannerId, adConfig);
            // Subscribe BannerAd events
            _bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
            _bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
            _bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
            _bannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
            _bannerAd.OnAdClicked += BannerOnAdClickedEvent;
            _bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
            _bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
            _bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;
        }
        
        public void LoadBannerAd() {
            //Load the banner ad 
            _bannerAd.LoadAd();
        }
        
        public void ShowBannerAd() {
            //Show the banner ad, call this method only if you turned off the auto show when you created this banner instance.
            _bannerAd.ShowAd();
        }
        
        public void HideBannerAd() {
            //Hide banner
            _bannerAd.HideAd();
        }
        
        public void DestroyBannerAd() {
            //Destroy banner
            _bannerAd.DestroyAd();
        }
        
        //Implement BannerAd Events
        private void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            RaiseLoaded(new AdResult
            {
                Success = true, 
                AdType = AdType.Banner,
                AdNetwork = adInfo.AdNetwork
            });
        }

        private void BannerOnAdLoadFailedEvent(LevelPlayAdError error)
        {
            RaiseLoadFailed(new AdResult()
            {
                Success = false,
                AdType = AdType.Banner,
                Message = error.ErrorMessage
            });
        }
        private void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo) {}
        private void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo) {}
        private void BannerOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error){}
        private void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo) {}
        private void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo) {}
        private void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo) {}

        #endregion
    }
}