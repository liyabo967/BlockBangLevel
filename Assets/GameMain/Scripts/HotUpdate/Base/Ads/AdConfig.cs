using System;
using UnityEngine;

namespace GameMain.Scripts.HotUpdate.Base.Ads
{
    // 广告配置
    [CreateAssetMenu(fileName = "AdConfig", menuName = "GameMain/AdConfig")]
    public class AdConfig : ScriptableObject
    {
        public AdPlatform Platform;
        public string AppId;
        public string BannerId;
        public string InterstitialId;
        public string RewardedVideoId;
        public bool TestMode = true;
    }
}