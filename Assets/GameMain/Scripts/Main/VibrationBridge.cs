using System.Runtime.InteropServices;

public class VibrationBridge
{
        
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _TriggerHapticFeedback(int force);
#endif

    public static void TriggerHapticFeedback(int force)
    {
#if UNITY_IOS
        _TriggerHapticFeedback(force);
#endif
    }
}