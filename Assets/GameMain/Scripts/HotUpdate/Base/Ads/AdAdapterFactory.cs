namespace GameMain.Scripts.HotUpdate.Base.Ads
{
    public static class AdAdapterFactory
    {
        public static IAdAdapter Create(AdPlatform platform)
        {
            return platform switch
            {
                AdPlatform.AdMob => new AdMobAdapter(),
                AdPlatform.LevelPlay => new LevelPlayAdapter(),
                // AdPlatform.IronSource => new IronSourceAdapter(),
                // AdPlatform.TopOn => new TopOnAdapter(),
                // AdPlatform.Max => new MaxAdapter(),
                _ => throw new System.NotSupportedException($"Platform {platform} not supported")
            };
        }
    }
}