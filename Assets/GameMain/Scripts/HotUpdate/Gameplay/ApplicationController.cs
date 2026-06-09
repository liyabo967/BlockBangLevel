using System;
using Facebook.Unity;
using Quester;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    private void OnApplicationPause(bool pauseStatus)
    {
        // Debug.Log("ApplicationController::OnApplicationPause: " + pauseStatus);
        if (!pauseStatus) {
            //app resume
            if (FB.IsInitialized) {
                FB.ActivateApp();
            } else {
                //Handle FB.Init
                FB.Init( () => {
                    FB.ActivateApp();
                });
            }
        }
    }

    private void OnApplicationQuit()
    {
        
    }
}
