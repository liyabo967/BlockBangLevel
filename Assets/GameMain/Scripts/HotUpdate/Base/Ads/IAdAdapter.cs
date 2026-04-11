using System;

namespace GameMain.Scripts.HotUpdate.Base.Ads
{
    public interface IAdAdapter
    {
        AdPlatform Platform { get; }
        bool IsInitialized { get; }

        void Initialize(AdConfig config, Action<bool> onComplete);

        // 加载
        void LoadAd(AdType type, string placementId = null);
        bool IsAdReady(AdType type);

        // 展示
        void ShowAd(AdType type, string placementId = null);
        void HideBanner();

        // 事件回调
        event Action<AdResult> OnAdLoaded;
        event Action<AdResult> OnAdLoadFailed;
        event Action<AdResult> OnAdShown;
        event Action<AdResult> OnAdShowFailed;
        event Action<AdResult> OnAdClicked;
        event Action<AdResult> OnAdClosed;
        event Action<AdResult> OnAdRewarded;  // 激励广告专用
        event Action<AdResult> OnAdRevenuePaid; // 收益回调
    }
}