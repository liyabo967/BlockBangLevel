using System;
using UnityEngine;

namespace GameMain.Scripts.HotUpdate.Base.Ads
{
    // 广告配置
    [CreateAssetMenu(fileName = "AdSettings", menuName = "GameMain/AdSettings")]
    public class AdSettings : ScriptableObject
    {
        public AdPlatform Platform;
        public AdConfig Android;
        public AdConfig iOS;
        public bool TestMode = true;
    }
    
    [Serializable]
    public class AdConfig
    {
        public string AppId;
        public string BannerId;
        public string InterstitialId;
        public string RewardedId;
    }
}