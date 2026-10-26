using UnityEngine;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

public static class CountryHelper
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern IntPtr GetDeviceCountryCodeNative();
#endif

    /// <summary>
    /// 获取设备的国家/地区代码，例如 JP、CN、US。
    /// 获取失败时返回空字符串。
    /// </summary>
    public static string GetCountryCode()
    {
        try
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var localeClass =
                new AndroidJavaClass("java.util.Locale"))
            using (var locale =
                localeClass.CallStatic<AndroidJavaObject>("getDefault"))
            {
                return locale.Call<string>("getCountry") ?? "";
            }

#elif UNITY_IOS && !UNITY_EDITOR
            IntPtr ptr = GetDeviceCountryCodeNative();

            if (ptr == IntPtr.Zero)
                return "";

            return Marshal.PtrToStringAnsi(ptr) ?? "";

#else
            // Editor 或其他平台：使用 .NET 当前区域
            return RegionInfoCountryCode();
#endif
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                $"GetCountryCode failed: {e.Message}");
            return "";
        }
    }

    private static string RegionInfoCountryCode()
    {
        try
        {
            return System.Globalization.RegionInfo
                .CurrentRegion.TwoLetterISORegionName;
        }
        catch
        {
            return "";
        }
    }
    
    public static string GetCountryName()
    {
        string countryCode = GetCountryCode();

        if (string.IsNullOrEmpty(countryCode))
            return "";

        try
        {
            return new RegionInfo(countryCode).EnglishName;
        }
        catch
        {
            return countryCode;
        }
    }
}