using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace GameMain.Editor
{
    public class AppsFlyerPostProcess
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {

            if (target == BuildTarget.iOS)
            {
                string plistPath = pathToBuiltProject + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                PlistElementDict rootDict = plist.root;
                rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
                rootDict.SetBoolean("AppsFlyerShouldSwizzle", true);
                /*** To add more keys :
                 ** rootDict.SetString("<your key>", "<your value>");
                 ***/

                File.WriteAllText(plistPath, plist.WriteToString());

                Debug.Log("Info.plist updated with NSAdvertisingAttributionReportEndpoint");
            }

        }
    }
}