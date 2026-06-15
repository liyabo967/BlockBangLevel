using BlockPuzzleGameToolkit.Scripts.Enums;
using Facebook.Unity;
using Firebase.Analytics;
using GameAnalyticsSDK;
using Quester;

public class FBEventManager
{
    public static void SendAppEvent(string eventName)
    {
        if (FB.IsInitialized)
        {
            FB.LogAppEvent(eventName);
        }
        else
        {
            FB.Init(FB.ActivateApp);
        }
    }
}