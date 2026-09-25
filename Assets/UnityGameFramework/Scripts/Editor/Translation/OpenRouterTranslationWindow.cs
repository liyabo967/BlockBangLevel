using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityGameFramework.Scripts.Editor.Translation.OpenRouter;

namespace UnityGameFramework.Scripts.Editor.Translation
{
    public class OpenRouterTranslationWindow : EditorWindow
    {
        private const int TranslatedLineColumns = 42;
        
        private CancellationTokenSource _cts;
        private string _key = "";
        private string _originalContent = "";
        private string _tips = "";
        private TextAsset _txtFile;
        private string _filePath;
        private bool _skipTranslatedLine = true;
        
        private bool _translating = false;
        private float _errorLines = 0;
        private float _timeoutLines = 0;
        
        private const string API_URL = "https://openrouter.ai/api/v1/chat/completions";
        private string API_KEY = "";

        // 创建窗口的菜单项
        [MenuItem("Tools/Translation")]
        public static void ShowWindow()
        {
            // 创建窗口实例并显示
            OpenRouterTranslationWindow window = GetWindow<OpenRouterTranslationWindow>("Translation");
            window.minSize = new Vector2(300, 150); // 设置最小尺寸
        }

        private void OnEnable()
        {
            var apiKey = Resources.Load<EditorApiKey>("EditorApiKey");
            API_KEY = apiKey.OpenRouterKey;
        }

        // 绘制窗口 GUI
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _txtFile = (TextAsset)EditorGUILayout.ObjectField(
                "目标文件",
                _txtFile,
                typeof(TextAsset),
                false
            );

            if (EditorGUI.EndChangeCheck())
            {
                if (_txtFile != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(_txtFile);
                    _filePath = Path.Combine(
                        Application.dataPath,
                        assetPath.Substring("Assets".Length + 1)
                    );
                }
            }
            
            GUILayout.Label("请输入信息", EditorStyles.boldLabel);
            _key = EditorGUILayout.TextField("Key", _key);
            _originalContent = EditorGUILayout.TextField("原文", _originalContent);
            EditorGUILayout.LabelField(_tips);
            GUILayout.Space(10);

            if (GUILayout.Button("开始翻译"))
            {
                if (!CheckFile())
                {
                    return;
                }
                if (!_translating)
                {
                    _translating = true;
                    TranslateAndWrite();
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "正在翻译，请稍等...", "确定");
                }
            }
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            // EditorGUILayout.Space(10);
            _skipTranslatedLine = EditorGUILayout.Toggle("跳过已翻译的行", _skipTranslatedLine);
            
            if (GUILayout.Button("翻译全部"))
            {
                if (!CheckFile())
                {
                    return;
                }
                if (!_translating)
                {
                    _cts = new CancellationTokenSource();
                    TranslateAll(_cts.Token);
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "正在翻译，请稍等...", "确定");
                }
            }
            
            if (_translating && GUILayout.Button("停止翻译"))
            {
                _translating = false; 
                _cts?.Cancel();
            }
        }

        private void SetTips(string tips)
        {
            _tips = tips;
            Repaint();
        }

        private bool CheckFile()
        {
            if (_txtFile == null)
            {
                SetTips("请选择输出文件");
                return false;
            }
            AssetDatabase.Refresh();
            return true;
        }

        private async void TranslateAndWrite()
        {
            try
            {
                var content = await Translation(_key, _originalContent);
                WriteToFile(content);
                SetTips("翻译完成，已写入文件");
                _translating = false;
            }
            catch (Exception e)
            {
                _translating = false;
                SetTips($"翻译异常，{e.Message}");
                Debug.LogException(e);
            }
        }
        
        private async void TranslateAll(CancellationToken token)
        {
            _timeoutLines = 0;
            _errorLines = 0;
            _translating = true;
            string tempPath = _filePath + ".tmp";

            using (var reader = new StreamReader(_filePath, Encoding.UTF8))
            using (var writer = new StreamWriter(tempPath, false, Encoding.UTF8))
            {
                long lineNumber = 1;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#") || token.IsCancellationRequested)
                    {
                        writer.WriteLine(line);
                        continue;
                    }
                    
                    var values = line.Split('\t');
                    if (values.Length < 3 || values.Length > TranslatedLineColumns)
                    {
                        SetTips($"翻译异常， line: {lineNumber}, length: {values.Length}");
                        _translating = false;
                        return;
                    }
                    
                    if (_skipTranslatedLine && IsTranslated(values))
                    {
                        writer.WriteLine(line);
                        continue;
                    }
                    
                    string originalContent = values[2];
                    if (values.Length >= 5 && !string.IsNullOrEmpty(values[4]))
                    {
                        originalContent = values[4];
                    }
                    var content = await Translation(values[1], originalContent);
                    if (string.IsNullOrEmpty(content))
                    {
                        _timeoutLines++;
                        Debug.Log($"{values[1]} Timeout");
                        writer.WriteLine(line);
                    }
                    else if (content.Split('\t').Length != TranslatedLineColumns)
                    {
                        _errorLines++;
                        writer.WriteLine(content);
                        Debug.LogWarning($"Translate Fail: {values[1]}");
                        // Debug.LogWarning($"Translate content: {content}");
                    }
                    else
                    {
                        writer.WriteLine(content);
                    }
                    lineNumber++;
                }
            }
            // 替换原文件
            File.Delete(_filePath);
            File.Move(tempPath, _filePath);
            
            Debug.Log($"Translate completed, Timeout: {_timeoutLines}, errors: {_errorLines}");
            SetTips($"翻译完成，已写入文件, Timeout: {_timeoutLines}, Errors: {_errorLines}");
            _translating = false;

            if (_timeoutLines > 0 && !_cts.IsCancellationRequested)
            {
                RetryTranslateAll();
            }
        }

        private void RetryTranslateAll()
        {
            Debug.LogError("RetryTranslateAll ------------------------");
            _cts = new CancellationTokenSource();
            TranslateAll(_cts.Token);
        }

        private bool IsTranslated(string[] values)
        {
            if (values.Length < TranslatedLineColumns)
            {
                return false;
            }

            for (var i = 1; i < values.Length; i++)
            {
                if (string.IsNullOrEmpty(values[i]))
                {
                    return false;
                }
            }
            return true;
        }
        
        private async Task<string> Translation(string key, string  originalContent)
        {
            SetTips($"正在翻译：{key}, {originalContent}");
            string result = await Translate(originalContent);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            var response = JsonConvert.DeserializeObject<OpenRouterResponse>(result);
            var originContent = response.choices[0].message.content;
            // Debug.Log($"originContent: {originContent.Trim()}");
            var content = WrapTranslation($"{key}", originalContent, originContent.Trim());
            // Debug.Log(content);
            return content;
        }

        private static string WrapTranslation(string key, string originalContent, string content)
        {
            return $"\t{key}\t" + content;
        }
        
        private async Task<string> Translate(string message)
        {
            var prompt = $"翻译一下\"{message}\"，分别使用简体中文，繁体中文，英语，南非荷兰语，阿拉伯语，巴斯克语，白俄罗斯语，保加利亚语，加泰罗尼亚语，捷克语，丹麦语，荷兰语，爱沙尼亚语，法罗语，芬兰语，法语，德语，希腊语，匈牙利语，冰岛语，印尼语，意大利语，日语，韩语，拉脱维亚语，立陶宛语，挪威语，波兰语，葡萄牙语，罗马尼亚语，俄语，斯洛伐克语，斯洛文尼亚语，西班牙语，瑞典语，泰语，土耳其语，乌克兰语，越南语，印地语，进行翻译，首字母大写，翻译结果直接使用'\t'分隔";
            return await AskAsync(prompt);
        }

        private async Task<string> AskAsync(string content)
        {
            var requestBody = new OpenRouterRequest();
            requestBody.model = "openai/gpt-oss-120b";
            requestBody.stream = false;
            // requestBody.model = "openai/gpt-4o-mini";
            // requestBody.model = "openrouter/free";
            requestBody.messages.Add(new RequestMessage()
            {
                role = "user",
                content = content
            });

            var jsonBody = JsonConvert.SerializeObject(requestBody);
            // Debug.Log(jsonBody);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            UnityWebRequest request = new UnityWebRequest(API_URL, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Authorization", "Bearer " + API_KEY);
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;

            var op = request.SendWebRequest();
            
            while (!op.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                
                // Debug.Log("HTTP Code: " + request.responseCode);
                // Debug.Log("Result: " + request.downloadHandler.text);
                SetTips("翻译完成");
            }
            else
            {
                var error = $"Error: {request.responseCode}, {request.error}";
                Debug.LogError(error);
                return null;
            }
            return Encoding.UTF8.GetString(request.downloadHandler.data);
        }

        private void WriteToFile(string content, bool append = true)
        {
            // 确保目录存在
            try
            {
                var filePath = _filePath;
                string dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (append)
                {
                    File.AppendAllText(filePath, content + "\n", Encoding.UTF8);
                }
                else
                {
                    File.WriteAllText(filePath, content, Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        
        private void RemoveOldLines()
        {
            var path = _filePath;
            string tempPath = path + ".tmp";
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            using (StreamWriter writer = new StreamWriter(tempPath, false, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || !line.StartsWith("#"))
                        continue;
                    writer.WriteLine(line);
                }
            }

            File.Delete(path);
            File.Move(tempPath, path);
        }

        #region request data

        private class OpenRouterRequest
        {
            public string model;
            public bool stream;
            public List<RequestMessage> messages = new List<RequestMessage>();
        }

        private class RequestMessage
        {
            public string role;
            public string content;
        }

        #endregion
    }
}