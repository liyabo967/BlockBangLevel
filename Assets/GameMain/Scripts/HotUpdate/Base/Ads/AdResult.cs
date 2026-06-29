namespace GameMain.Scripts.HotUpdate.Base.Ads
{
    public class AdResult
    {
        public bool Success;
        public string Message;
        public string PlacementId;
        public AdType AdType;
        public string AdUnitId;
        public string AdNetwork;
        public string Currency;
        public double Revenue; // 广告收益（用于上报）
    }
}