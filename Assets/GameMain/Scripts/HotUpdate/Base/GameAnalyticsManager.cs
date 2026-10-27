using BlockPuzzleGameToolkit.Scripts.Enums;
using Firebase.Analytics;
using GameAnalyticsSDK;
using Quester;

public class GameAnalyticsManager
{
    /// <summary>
    /// 统计关卡的成功率
    /// </summary>
    /// <param name="level"></param>
    /// <param name="status"></param>
    /// <param name="score"></param>
    public static void SendLevelProgression(int level, GAProgressionStatus status, int score = 0)
    {
        var progression01 = "group_1";
        GameAnalytics.NewProgressionEvent(status, progression01, level.ToString(), score);
    }
    
    /// <summary>
    /// 统计用户漏斗收据
    /// </summary>
    /// <param name="stage"></param>
    /// <param name="status"></param>
    /// <param name="progression2"></param>
    /// <param name="score"></param>
    public static void SendUserProgression(UserStage stage, GAProgressionStatus status, string progression2 = null, int score = 0)
    {
        GameAnalytics.NewProgressionEvent(status, stage.ToString(), progression2, score);
    }

    public static void SendAdEvent(GAAdType adType, GAAdAction action, string adNetwork, GAAdError error = GAAdError.Undefined)
    {
        if (string.IsNullOrEmpty(adNetwork))
        {
            adNetwork = "levelplay";
        }
        GameAnalytics.NewAdEvent(action, adType, adNetwork, "default", error);
    }

    public static void SendDesignEvent(string eventName)
    {
        GameAnalytics.NewDesignEvent(eventName);
    }
}