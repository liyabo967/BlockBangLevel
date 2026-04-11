using System;
using UnityEngine;

namespace GameMain.Scripts.HotUpdate.Base.Ads
{
    public abstract class BaseAdAdapter : IAdAdapter
    {
        public abstract AdPlatform Platform { get; }
        public bool IsInitialized { get; protected set; }

        protected AdConfig Config;

        public event Action<AdResult> OnAdLoaded;
        public event Action<AdResult> OnAdLoadFailed;
        public event Action<AdResult> OnAdShown;
        public event Action<AdResult> OnAdShowFailed;
        public event Action<AdResult> OnAdClicked;
        public event Action<AdResult> OnAdClosed;
        public event Action<AdResult> OnAdRewarded;
        public event Action<AdResult> OnAdRevenuePaid;

        public abstract void Initialize(AdConfig config, Action<bool> onComplete);
        public abstract void LoadAd(AdType type, string placementId = null);
        public abstract bool IsAdReady(AdType type);
        public abstract void ShowAd(AdType type, string placementId = null);
        public abstract void HideBanner();

        // 子类调用这些方法触发事件
        protected void RaiseLoaded(AdResult r) => OnAdLoaded?.Invoke(r);
        protected void RaiseLoadFailed(AdResult r) => OnAdLoadFailed?.Invoke(r);
        protected void RaiseShown(AdResult r) => OnAdShown?.Invoke(r);
        protected void RaiseShowFailed(AdResult r) => OnAdShowFailed?.Invoke(r);
        protected void RaiseClicked(AdResult r) => OnAdClicked?.Invoke(r);
        protected void RaiseClosed(AdResult r) => OnAdClosed?.Invoke(r);
        protected void RaiseRewarded(AdResult r) => OnAdRewarded?.Invoke(r);
        protected void RaiseRevenuePaid(AdResult r) => OnAdRevenuePaid?.Invoke(r);

        protected void Log(string msg) => Debug.Log($"[{Platform}] {msg}");
    }
}