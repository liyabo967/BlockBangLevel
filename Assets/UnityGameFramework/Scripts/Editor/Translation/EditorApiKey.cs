using UnityEngine;

// 广告配置
[CreateAssetMenu(fileName = "EditorApiKey", menuName = "GameMain/EditorApiKey")]
public class EditorApiKey : ScriptableObject
{
    public string OpenRouterKey;
    public string OpenAiKey;
}
