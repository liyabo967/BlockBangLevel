using GameAnalyticsSDK;
using Quester;

public class GameAnalyticsManager
{
    public static void SendLevelProgression(int level, GAProgressionStatus status, int score = 0)
    {
        var season = $"{TimeManager.SeasonTime.year}_{TimeManager.SeasonTime.week}";
        GameAnalytics.NewProgressionEvent(status, season, level.ToString(), score);
    }

    public static void SendAdEvent(GAAdType adType, GAAdAction action)
    {
        GameAnalytics.NewAdEvent(action, adType, "LevelPlay", "");
    }
}