using UnityEngine;
using GameAnalyticsSDK;

public class SDKManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameAnalytics.Initialize();
    }
}
