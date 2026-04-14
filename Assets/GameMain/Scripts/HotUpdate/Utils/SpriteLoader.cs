using UnityEngine;
using System.IO;


public static class SpriteLoader
{
    /// <summary>
    /// 从本地路径加载 Sprite
    /// </summary>
    /// <param name="path">图片路径</param>
    /// <param name="pivot">锚点位置，默认 (0.5, 0.5) 中心</param>
    /// <param name="pixelsPerUnit">每单位像素数，默认 100</param>
    public static Sprite LoadFromFile(string path, Vector2? pivot = null, float pixelsPerUnit = 100f)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("文件不存在: " + path);
            return null;
        }

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        Vector2 actualPivot = pivot ?? new Vector2(0.5f, 0.5f);

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), actualPivot, pixelsPerUnit);
    }

    /// <summary>
    /// 异步加载 Sprite
    /// </summary>
    public static System.Collections.IEnumerator LoadFromFileAsync(string path, System.Action<Sprite> onComplete)
    {
        string url = "file://" + path;

        using (UnityEngine.Networking.UnityWebRequest request =
               UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                onComplete?.Invoke(sprite);
            }
            else
            {
                Debug.LogError("加载失败: " + request.error);
                onComplete?.Invoke(null);
            }
        }
    }
}