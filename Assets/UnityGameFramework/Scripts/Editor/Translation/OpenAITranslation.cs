using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAITranslation
{
    // [MenuItem("Tools/Translation")]
    static async void Translate()
    {
        string result = await AskAsync("正在更新");
        Debug.Log($"openAI result: {result}");
    }
    
    private const string ApiKey = "";
    private const string Url = "https://api.openai.com/v1/responses";

    private static async Task<string> Translate(string message)
    {
        var prompt = $"翻译一下\"{message}\"，分别使用中文，繁体中文，英语，日语，韩语，法语，德语，西班牙语，葡萄语语，俄语，意大利语，泰语，越南语，印尼语，阿拉伯语进行翻译，翻译结果直接使用'\t'分隔";
        return await AskAsync(prompt);
    }
    
    private static async Task<string> AskAsync(string prompt)
    {
        var requestBody = new OpenAIRequest()
        {
            model = "gpt-4o-mini",
            input = prompt
        };

        string json = JsonUtility.ToJson(requestBody);
        Debug.Log(json);
        using var request = new UnityWebRequest(Url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {ApiKey}");

        var op = request.SendWebRequest();
        while (!op.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"OpenAI Error: {request.error}\n{request.downloadHandler.text}");
            return null;
        }

        return request.downloadHandler.text;
    }

    private class OpenAIRequest
    {
        public string model;
        public string input;
    }
}
