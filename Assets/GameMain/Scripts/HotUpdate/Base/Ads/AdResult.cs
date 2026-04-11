namespace GameMain.Scripts.HotUpdate.Base.Ads
{
    public class AdResult
    {
        public bool Success;
        public string Message;
        public string PlacementId;
        public AdType AdType;
        public string AdNetwork;
        public double Revenue; // 广告收益（用于上报）
    }
}