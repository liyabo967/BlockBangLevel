using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Android;

namespace BlockPuzzleGameToolkit.Scripts.System.Haptic
{
    public class HapticFeedback : MonoSingleton<HapticFeedback>
    {
        public enum HapticForce
        {
            Light,
            Medium,
            Heavy,
            Continuous
        }

        private const string VibrationPrefKey = "VibrationLevel";
        
        private static bool IsSystemSupported()
        {
            #if UNITY_IOS
            return SystemInfo.supportsVibration;
            #elif UNITY_ANDROID
            return SystemInfo.supportsVibration && Permission.HasUserAuthorizedPermission("android.permission.VIBRATE");
            #else
            return false;
            #endif
        }

        private static bool TryHapticFeedback(HapticForce force)
        {
            if (!IsSystemSupported())
                return false;

            try
            {
                #if UNITY_IOS
                if (force == HapticForce.Continuous)
                {
                    VibrationBridge.ContinuousVibration();
                }
                else
                {
                    VibrationBridge.TriggerHapticFeedback((int)force);
                }
                #elif UNITY_ANDROID
                long[] pattern = force switch
                {
                    HapticForce.Light => new long[] { 0, 10 },
                    HapticForce.Medium => new long[] { 0, 30 },
                    HapticForce.Heavy => new long[] { 0, 100 },
                    HapticForce.Continuous => new long[] { 0, 500 },
                    _ => new long[] { 0, 10 }
                };
                VibrationAndroid.Vibrate(pattern, -1);
                #endif
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Haptic feedback failed: {e.Message}");
                return false;
            }
        }

        public static void TriggerHapticFeedback(HapticForce force)
        {
            if (IsVibrationDisabled())
                return;

#if UNITY_EDITOR
            return;
#endif
            TryHapticFeedback(force);
        }

        private static bool IsVibrationDisabled()
        {
            return PlayerPrefs.HasKey(VibrationPrefKey) && PlayerPrefs.GetFloat(VibrationPrefKey) <= 0;
        }
    }
}