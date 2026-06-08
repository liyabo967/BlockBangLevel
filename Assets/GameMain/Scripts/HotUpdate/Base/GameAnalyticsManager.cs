using BlockPuzzleGameToolkit.Scripts.Enums;
using Firebase.Analytics;
using GameAnalyticsSDK;
using Quester;

public class GameAnalyticsManager
{
    public static void SendLevelProgression(int level, GAProgressionStatus status, int score = 0)
    {
        var progression01 = $"{TimeManager.SeasonTime.year}_{TimeManager.SeasonTime.week}";
        GameAnalytics.NewProgressionEvent(status, progression01, level.ToString(), score);
    }
    
    public static void SendUserProgression(UserStage stage, GAProgressionStatus status, string progression2 = "", int score = 0)
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
}