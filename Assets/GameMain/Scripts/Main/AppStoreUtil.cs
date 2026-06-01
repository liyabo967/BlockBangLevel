using UnityEngine;

namespace GameMain
{
    public static class AppStoreUtil
    {
        // Android Google Play 包名
        private const string AndroidPackageName = "com.quester.game.blockbang";

        // iOS App Store ID
        private const string IOSAppId = "6749655294";

        public static void OpenStore()
        {
#if UNITY_ANDROID
        string url = $"market://details?id={AndroidPackageName}";

        try
        {
            Application.OpenURL(url);
        }
        catch
        {
            Application.OpenURL(
                $"https://play.google.com/store/apps/details?id={AndroidPackageName}");
        }

#elif UNITY_IOS
            Application.OpenURL(
                $"itms-apps://itunes.apple.com/app/id{IOSAppId}");

#else
        Application.OpenURL("https://xjoy.games");
#endif
        }
    }
}