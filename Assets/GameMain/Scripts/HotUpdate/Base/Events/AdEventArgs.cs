using GameFramework;
using GameFramework.Event;

namespace Quester
{
    public class AdEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(AdEventArgs).GetHashCode();
        
        private AdType _adType;
        
        private AdEventType _adEventType;
        
        public override int Id
        {
            get { return EventId; }
        }

        public AdType ADType => _adType;

        public AdEventType ADEventType => _adEventType;

        public static AdEventArgs Create(AdType adType, AdEventType adEventType)
        {
            AdEventArgs eventArgs = ReferencePool.Acquire<AdEventArgs>();
            eventArgs._adType = adType;
            eventArgs._adEventType = adEventType;
            return eventArgs;
        }

        /// <summary>
        /// 清理加载数据表成功事件。
        /// </summary>
        public override void Clear()
        {
            
        }
        
        public enum AdType
        {
            Interstitial,
            Rewarded,
            Banner
        }
        
        public enum AdEventType
        {
            RewardedVideoDisplayed,
            RewardedVideoClosed,
            RewardedVideoRewarded,
            InterstitialVideoDisplayed,
            InterstitialVideoClosed,
        }
    }

    
}