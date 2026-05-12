using UnityEngine;
using GameAnalyticsSDK;

public class SDKManager : MonoBehaviour
{
    void Start()
    {
        GameAnalytics.Initialize();
    }
}
