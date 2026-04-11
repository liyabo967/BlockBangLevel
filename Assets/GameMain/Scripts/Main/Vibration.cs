using UnityEngine;
using System.Collections;

public static class Vibration
{

#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
    public static AndroidJavaClass unityPlayer;
    public static AndroidJavaObject currentActivity;
    public static AndroidJavaObject vibrator;
#endif

    public static void Vibrate()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        vibrator.Call("vibrate");
#endif
    }
    
    public static void Vibrate(long milliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        vibrator.Call("vibrate", milliseconds);
#endif
    }

    public static void Vibrate(long[] pattern, int repeat)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        vibrator.Call("vibrate", pattern, repeat);
#endif
    }

    public static bool HasVibrator()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return true;
#endif
        return false;
    }

    public static void Cancel()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        vibrator.Call("cancel");
#endif
    }
}
