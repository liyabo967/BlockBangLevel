using System;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;

namespace GameMain.Scripts.HotUpdate.Services
{
    public class MobileReviewManager : MonoSingleton<MobileReviewManager>
    {
        private const int ReviewInterval = 7 * 24 * 3600;
        
        public void RequestReview(EGameMode gameMode)
        {
            var isIntervalReady =  DateTimeOffset.Now.ToUnixTimeSeconds() - UserDataManager.Instance.LastRateTimestamp >= ReviewInterval;
            var scoreReady = gameMode != EGameMode.Classic
                             || UserDataManager.Instance.ClassicBestScore >= 5000;
            
            if (isIntervalReady && scoreReady)
            {
                GameAnalyticsManager.SendDesignEvent("game:review");
                UserDataManager.Instance.SetLastRateTimestamp(DateTimeOffset.Now.ToUnixTimeSeconds());
                MobileReview.Instance.RequestReview();
            }
        }
    }
}