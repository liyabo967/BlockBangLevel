using System.Runtime.InteropServices;

public class VibrationBridge
{
        
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _TriggerHapticFeedback(int force);
    [DllImport("__Internal")]
    private static extern void _PlayImpactNative(float intensity, float sharpness, float duration);
#endif

    public static void TriggerHapticFeedback(int force)
    {
#if UNITY_IOS
        _TriggerHapticFeedback(force);
#endif
    }

    public static void ContinuousVibration()
    {
        _PlayImpactNative(0.5f, 0.5f, 0.5f);
    }
}