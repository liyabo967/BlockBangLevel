using GoogleMobileAds.Api;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.GUI
{
    public class GM : MonoSingleton<GM>
    {
        public void OpenAdInspector()
        {
            MobileAds.OpenAdInspector((AdInspectorError error) =>
            {
                if (error != null)
                {
                    Debug.LogError($"code: {error.GetCode()}");
                    Debug.LogError(error.GetCause().GetMessage());
                    Debug.LogError(error.GetMessage());
                }
            });
        }
    }
}