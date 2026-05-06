using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Quester;
using UnityEngine;
using UnityEngine.Networking;

public class EmailUtils : MonoBehaviour
{
    public static void SendEMail(int level)
    {
        string head = "My+suggestion+is:\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";

        string infoTitle = "Please do not delete the important info below!" + "\n";
        string gameVersion = "Version:  " + Application.version + "\n";
        string systemInfo = "System+Info:  " + SystemInfo.operatingSystem + "\n";
        string deviceInfo = "Device+Info:  " + SystemInfo.deviceModel + "\n";
        string timezone = "Timezone:  " + TimeZoneInfo.Local.StandardName + "\n";
        string screenResolution = "Screen+Resolution:  " + Screen.currentResolution.ToString() + "\n";
        string userLevel = "UserLevel:  " + level.ToString() + "\n";
        string systemLanguage = "System+Language:  " + Application.systemLanguage.ToString() + "\n";
        string uID = "UID:  " + SystemInfo.deviceUniqueIdentifier + "\n";
        string country = "Country:  "+ RegionInfo.CurrentRegion + "\n";
        string faceBookID = "FBID:  " + "\n";  //无FaceBook信息
        string appleID = "APPLEID:  " + "\n";  //无苹果ID信息
        string time = "Time:  "+ DateTime.Now + "\n";

        string mainMessage = head + infoTitle + gameVersion + systemInfo + deviceInfo + timezone + screenResolution + userLevel  + systemLanguage + uID + country + faceBookID + appleID + time;

        string email = "support@xjoy.games";
        string subject = MyEscapeURL("Block Bang! + Feedback");
        string body = MyEscapeURL(mainMessage);
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }
    
    
    public static string MyEscapeURL(string url)
    {
        //%20是空格在url中的编码，这个方法将url中非法的字符转换成%20格式
        return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }
}
