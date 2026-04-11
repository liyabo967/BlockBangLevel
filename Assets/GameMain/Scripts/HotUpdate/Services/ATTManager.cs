using System;
using System.Collections;
using GameMain.Scripts.HotUpdate.Base.Ads;
using UnityEngine;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
using UnityEngine.iOS;
#endif


/// <summary>
/// iOS 14.5+ App Tracking Transparency 授权管理
/// </summary>
public class ATTManager : MonoSingleton<ATTManager>
{
    private Action<ATTStatus> _onCompleted;
    
    public enum ATTStatus
    {
        NotDetermined = 0, // 用户还没做选择
        Restricted = 1, // 系统限制（比如家长控制）
        Denied = 2, // 用户拒绝
        Authorized = 3, // 用户同意
        NotSupported = 99 // 非 iOS 或 iOS < 14.5
    }

    private void Start()
    {
        RequestAuthorization(status =>
        {
            AdManager.Instance.Init();
        });
    }

    /// <summary>
    /// 获取当前 ATT 授权状态（不弹窗）
    /// </summary>
    public ATTStatus GetStatus()
    {
#if UNITY_IOS && !UNITY_EDITOR
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            return (ATTStatus)(int)status;
#else
        return ATTStatus.NotSupported;
#endif
    }

    /// <summary>
    /// 请求 ATT 授权（会弹窗，仅在 NotDetermined 状态下弹）
    /// </summary>
    /// <param name="onComplete">授权结果回调</param>
    public void RequestAuthorization(Action<ATTStatus> onComplete)
    {
        Debug.Log("RequestAuthorization");
        _onCompleted =  onComplete;
#if UNITY_IOS && !UNITY_EDITOR
            var current = GetStatus();

            // 只有 NotDetermined 才会弹窗，其他状态直接返回当前值
            if (current != ATTStatus.NotDetermined)
            {
                Debug.Log($"[ATT] Already determined: {current}");
                _onCompleted?.Invoke(current);
                return;
            }

            Debug.Log("[ATT] Requesting authorization...");
            ATTrackingStatusBinding.RequestAuthorizationTracking();
            StartCoroutine(WaitForATTResponse());
#else
        Debug.Log("[ATT] Not iOS, skip");
        _onCompleted?.Invoke(ATTStatus.NotSupported);
#endif
    }
    
    private IEnumerator WaitForATTResponse()
    {
#if UNITY_IOS && !UNITY_EDITOR
        ATTrackingStatusBinding.AuthorizationTrackingStatus status =
            ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

        while (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            yield return new WaitForSeconds(0.5f);
            status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        }

        Debug.Log("ATT Result: " + status);

        OnATTFinished(status);
#endif
        yield return new WaitForEndOfFrame();
    }

    // =========================
    // 完成回调
    // =========================
    void OnATTFinished(ATTrackingStatusBinding.AuthorizationTrackingStatus status)
    {
        var attStatus = ATTStatus.Denied;
#if UNITY_IOS && !UNITY_EDITOR
        if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED)
        {
            string idfa = Device.advertisingIdentifier;
            Debug.Log("IDFA: " + idfa);
            attStatus = ATTStatus.Authorized;
        }
        else
        {
            Debug.Log("ATT Denied or Restricted");
            attStatus = ATTStatus.Denied;
        }
#endif
        _onCompleted?.Invoke(attStatus);
    }
}