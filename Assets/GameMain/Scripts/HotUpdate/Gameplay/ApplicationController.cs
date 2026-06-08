using System;
using Quester;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    private void OnApplicationPause(bool pauseStatus)
    {
        // Debug.Log("ApplicationController::OnApplicationPause: " + pauseStatus);
        // if (pauseStatus)
        // {
        //     GameEntry.Sound.PauseMusic();
        // }
        // else
        // {
        //     GameEntry.Sound.ResumeMusic();
        // }
    }

    private void OnApplicationQuit()
    {
        
    }
}
