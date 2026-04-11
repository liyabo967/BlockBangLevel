using GameFramework;

public static class AssetUtility
{
    // private static CustomAssetConfigSo _customAssetConfig;

    public static string GetPictureAsset(int year, int week)
    {
        return $"Assets/GameMain/Arts/Pictures/{year}/{week}.jpg";
    }

    public static string GetConfigAsset(string assetName, bool fromBytes)
    {
        return Utility.Text.Format("Assets/GameMain/Configs/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
    }

    public static string GetElementTextureAsset(string assetName, string extension = "png")
    {
        return Utility.Text.Format("Assets/GameMain/Arts/Game/Texture/{0}.{1}", assetName, extension);
    }

    public static string GetDataTableAsset(string assetName, bool fromBytes)
    {
        return Utility.Text.Format("Assets/GameMain/DataTables/{0}.{1}", assetName,
            fromBytes ? "bytes" : "txt");
    }


    public static string GetFontAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Fonts/{0}.ttf", assetName);
    }

    public static string GetSceneAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Scenes/{0}.unity", assetName);
    }

    public static string GetMusicAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Audio/Music/{0}.mp3", assetName);
    }

    public static string GetSoundAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Audio/Sound/{0}", assetName);
    }

    public static string GetUISoundAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Audio/UISound/{0}.ogg", assetName);
    }

    public static string GetEntityAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Entities/{0}.prefab", assetName);
    }

    public static string GetUIFormAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Prefabs/GUI/Popups/{0}.prefab", assetName);
    }

    public static string GetUIItemAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/UI/UIItems/{0}.prefab", assetName);
    }

    public static string GetElementAsset(string assetName)
    {
        return Utility.Text.Format("Assets/XMtileMap/Tiles/Elements/Prefabs/{0}.prefab", assetName);
    }

    public static string GetMaterialAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Materials/{0}.mat", assetName);
    }

    public static string GetUIAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Art/UI/{0}.png", assetName);
    }

    public static string GetElementIconAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Sprites/Elements/{0}.png", assetName);
    }

    public static string GetEntityIconAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Entities/{0}.png", assetName);
    }

    public static string GetTimelineAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Art/TimeLines/{0}.prefab", assetName);
    }

    public static string GetCommonUIAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/UI/Common/{0}.prefab", assetName);
    }
}