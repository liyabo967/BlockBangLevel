#if UNITY_IOS
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Xml;
using UnityEngine;

public static class ATTPostProcess
{
    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        var root = plist.root;

        // 添加 ATT 说明文案
        root.SetString("NSUserTrackingUsageDescription",
            "Your data will be used to provide you a better and personalized ad experience.");

        // AdMob 需要这个（iOS 14+）
        root.SetString("GADApplicationIdentifier", "ca-app-pub-1495494109347043~9970644385");

        // SKAdNetwork 配置（广告归因需要，SDK 文档里会给完整列表）
        var skAdNetworkItems = root.CreateArray("SKAdNetworkItems");
        AddNetwork(skAdNetworkItems);
        // AddSKAdNetworkId(skAdNetworkItems, "cstr6suwn9.skadnetwork"); // AdMob
        plist.WriteToFile(plistPath);
    }

    private static void AddSKAdNetworkId(PlistElementArray array, string id)
    {
        var dict = array.AddDict();
        dict.SetString("SKAdNetworkIdentifier", id);
        // Debug.Log($"add id: {id}");
    }

    private static void AddNetwork(PlistElementArray array)
    {
        string admobFilePath = FindAdMobSKAdFile();

        if (!string.IsNullOrEmpty(admobFilePath))
        {
            var ids = ParseSKAdNetworkIds(admobFilePath);

            foreach (var id in ids)
            {
                AddSKAdNetworkId(array, id);
            }

            Debug.Log($"[SKAdNetwork] 已添加 {ids.Count} 个 AdMob ID");
        }
        else
        {
            Debug.LogWarning("未找到 GoogleMobileAdsSKAdNetworkItems 文件！");
        }
    }
    
    static string FindAdMobSKAdFile()
    {
        string[] guids = AssetDatabase.FindAssets("GoogleMobileAdsSKAdNetworkItems");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (path.EndsWith(".xml") || path.EndsWith(".plist"))
            {
                return path;
            }
        }

        return null;
    }
    
    static List<string> ParseSKAdNetworkIds(string filePath)
    {
        List<string> ids = new List<string>();

        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);

        XmlNodeList stringNodes = doc.GetElementsByTagName("SKAdNetworkIdentifier");

        foreach (XmlNode node in stringNodes)
        {
            string value = node.InnerText;

            if (value.EndsWith(".skadnetwork"))
            {
                ids.Add(value);
            }
        }

        return ids;
    }
}
#endif