using GameAnalyticsSDK;
using Quester;

public class GameAnalyticsManager
{
    public static void SendLevelProgression(int level, GAProgressionStatus status)
    {
        var season = $"{TimeManager.SeasonTime.year}_{TimeManager.SeasonTime.week}";
        GameAnalytics.NewProgressionEvent(status, season, level);
    }

    public static void SendAdEvent(GAAdType adType, GAAdAction action)
    {
        GameAnalytics.NewAdEvent(action, adType, "LevelPlay", "");
    }
}