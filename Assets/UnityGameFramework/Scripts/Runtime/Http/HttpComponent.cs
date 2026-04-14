// NetworkManager.cs - 核心管理器
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityGameFramework.Runtime;

public class HttpComponent : GameFrameworkComponent, INetworkClient
{
    [SerializeField] private NetworkConfig config;
    private List<IRequestInterceptor> requestInterceptors = new List<IRequestInterceptor>();
    private List<IResponseInterceptor> responseInterceptors = new List<IResponseInterceptor>();
    private Queue<RequestTask> requestQueue = new Queue<RequestTask>();
    private int currentConcurrentRequests = 0;
    private Dictionary<string, CacheEntry> cache = new Dictionary<string, CacheEntry>();

    private class RequestTask
    {
        public NetworkRequest request;
        public Action<NetworkResponse> callback;
    }

    private class CacheEntry
    {
        public string data;
        public float expireTime;
    }

    protected override void Awake()
    {
        base.Awake();
        // config = Resources.Load<NetworkConfig>("NetworkConfig");
    }

    // 注册拦截器
    public void RegisterRequestInterceptor(IRequestInterceptor interceptor)
    {
        if (!requestInterceptors.Contains(interceptor))
            requestInterceptors.Add(interceptor);
    }

    public void RegisterResponseInterceptor(IResponseInterceptor interceptor)
    {
        if (!responseInterceptors.Contains(interceptor))
            responseInterceptors.Add(interceptor);
    }

    public void UnregisterRequestInterceptor(IRequestInterceptor interceptor)
    {
        requestInterceptors.Remove(interceptor);
    }

    public void UnregisterResponseInterceptor(IResponseInterceptor interceptor)
    {
        responseInterceptors.Remove(interceptor);
    }

    // 发送请求
    public void SendRequest(NetworkRequest request, Action<NetworkResponse> callback)
    {
        StartCoroutine(SendRequestCoroutine(request, callback));
    }

    public IEnumerator SendRequestCoroutine(NetworkRequest request, Action<NetworkResponse> callback)
    {
        // 应用配置
        ApplyConfig(request);

        // 应用拦截器
        foreach (var interceptor in requestInterceptors)
        {
            request = interceptor.OnRequest(request);
        }

        // 检查缓存
        if (request.useCache && config.enableCache)
        {
            string cacheKey = GenerateCacheKey(request);
            if (cache.ContainsKey(cacheKey) && cache[cacheKey].expireTime > Time.time)
            {
                var cachedResponse = new NetworkResponse
                {
                    success = true,
                    statusCode = 200,
                    data = cache[cacheKey].data,
                    originalRequest = request
                };
                callback?.Invoke(cachedResponse);
                yield break;
            }
        }

        // 等待并发数限制
        while (currentConcurrentRequests >= config.maxConcurrentRequests)
        {
            yield return null;
        }

        currentConcurrentRequests++;
        float startTime = Time.time;
        int retryAttempt = 0;
        NetworkResponse response = null;

        while (retryAttempt <= request.retryCount)
        {
            yield return StartCoroutine(ExecuteRequest(request, (result) =>
            {
                response = result;
            }));

            response.elapsedTime = Time.time - startTime;

            if (response.success || retryAttempt >= request.retryCount)
            {
                break;
            }

            if (config.enableRetry)
            {
                retryAttempt++;
                float delay = config.retryStrategy == NetworkConfig.RetryStrategy.Exponential
                    ? config.retryDelay * Mathf.Pow(2, retryAttempt - 1)
                    : config.retryDelay * retryAttempt;
                yield return new WaitForSeconds(delay);
            }
            else
            {
                break;
            }
        }

        currentConcurrentRequests--;

        // 应用响应拦截器
        foreach (var interceptor in responseInterceptors)
        {
            response = interceptor.OnResponse(response);
        }

        // 缓存响应
        if (response.success && request.useCache && config.enableCache)
        {
            string cacheKey = GenerateCacheKey(request);
            cache[cacheKey] = new CacheEntry
            {
                data = response.data,
                expireTime = Time.time + config.cacheExpireTime
            };
        }

        // LogResponse(response);
        callback?.Invoke(response);
    }

    private void ApplyConfig(NetworkRequest request)
    {
        if (config == null) return;

        // 应用基础URL
        if (!string.IsNullOrEmpty(config.baseUrl) && !request.url.StartsWith("http"))
        {
            request.url = config.baseUrl + request.url;
        }

        // 应用默认超时
        if (request.timeout <= 0)
        {
            request.timeout = config.defaultTimeout;
        }

        // 应用默认重试次数
        if (request.retryCount <= 0 && config.enableRetry)
        {
            request.retryCount = config.defaultRetryCount;
        }

        // 应用默认请求头
        if (config.defaultHeaders != null)
        {
            foreach (var header in config.defaultHeaders)
            {
                if (!request.headers.ContainsKey(header.key))
                {
                    request.headers[header.key] = header.value;
                }
            }
        }
    }

    private IEnumerator ExecuteRequest(NetworkRequest request, Action<NetworkResponse> callback)
    {
        UnityWebRequest www = CreateUnityWebRequest(request);

        if (www == null)
        {
            callback?.Invoke(new NetworkResponse
            {
                success = false,
                error = "Unsupported HTTP method",
                originalRequest = request
            });
            yield break;
        }

        www.timeout = request.timeout;

        // 设置请求头
        foreach (var header in request.headers)
        {
            www.SetRequestHeader(header.Key, header.Value);
        }

        LogRequest(request);

        yield return www.SendWebRequest();
        Debug.Log("response: " + www.downloadHandler.text);
        NetworkResponse response = CreateResponse(www, request);
        www.Dispose();

        callback?.Invoke(response);
    }

    private UnityWebRequest CreateUnityWebRequest(NetworkRequest request)
    {
        switch (request.method)
        {
            case NetworkRequest.HttpMethod.GET:
                return UnityWebRequest.Get(request.url);

            case NetworkRequest.HttpMethod.POST:
                // var postRequest = UnityWebRequest.PostWwwForm(request.url, request.body);
                var postRequest = new UnityWebRequest(request.url, "POST");
                if (!string.IsNullOrEmpty(request.body))
                {
                    postRequest.SetRequestHeader("Content-Type", "application/json");
                    postRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.body));
                    postRequest.downloadHandler = new DownloadHandlerBuffer();
                }
                else if (request.bytes != null)
                {
                    WWWForm form = new WWWForm();
                    form.AddBinaryData("file", request.bytes);
                    postRequest = UnityWebRequest.Post(request.url, form);
                }
                return postRequest;

            case NetworkRequest.HttpMethod.PUT:
                var putRequest = UnityWebRequest.Put(request.url, request.body);
                if (!string.IsNullOrEmpty(request.body))
                {
                    putRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.body));
                    putRequest.downloadHandler = new DownloadHandlerBuffer();
                }
                return putRequest;

            case NetworkRequest.HttpMethod.DELETE:
                return UnityWebRequest.Delete(request.url);

            case NetworkRequest.HttpMethod.PATCH:
                var patchRequest = new UnityWebRequest(request.url, "PATCH");
                if (!string.IsNullOrEmpty(request.body))
                {
                    patchRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.body));
                    patchRequest.downloadHandler = new DownloadHandlerBuffer();
                }
                return patchRequest;

            case NetworkRequest.HttpMethod.HEAD:
                return UnityWebRequest.Head(request.url);

            default:
                return UnityWebRequest.Get(request.url);
        }
    }

    private NetworkResponse CreateResponse(UnityWebRequest www, NetworkRequest request)
    {
        NetworkResponse response = new NetworkResponse
        {
            originalRequest = request,
            statusCode = (int)www.responseCode
        };

        if (www.result == UnityWebRequest.Result.Success)
        {
            response.success = true;
            response.data = www.downloadHandler?.text;
        }
        else
        {
            response.success = false;
            response.error = www.error;
            response.errorMessage = www.downloadHandler?.text;
            response.data = www.downloadHandler?.text;
        }

        // 获取响应头
        if (www.GetResponseHeaders() != null)
        {
            foreach (var header in www.GetResponseHeaders())
            {
                response.headers[header.Key] = header.Value;
            }
        }

        return response;
    }

    private string GenerateCacheKey(NetworkRequest request)
    {
        return $"{request.method}_{request.url}_{request.body}";
    }

    private void LogRequest(NetworkRequest request)
    {
        if (config == null || !config.enableLogging) return;

        if (config.logLevel >= NetworkConfig.LogLevel.Debug)
        {
            Debug.Log($"[Network] {request.method} {request.url}");
        }
    }

    private void LogResponse(NetworkResponse response)
    {
        if (config == null || !config.enableLogging) return;

        if (response.success)
        {
            if (config.logLevel >= NetworkConfig.LogLevel.Info)
            {
                Debug.Log($"[Network] Success: {response.statusCode} - {response.originalRequest.url} ({response.elapsedTime:F2}s)");
            }
        }
        else
        {
            if (config.logLevel >= NetworkConfig.LogLevel.Error)
            {
                Debug.LogError($"[Network] Error: {response.statusCode} - {response.originalRequest.url} - {response.error}");
            }
        }
    }

    // 清除缓存
    public void ClearCache()
    {
        cache.Clear();
    }

    // 便捷方法
    public void Get(string url, Action<NetworkResponse> callback, Dictionary<string, string> headers = null)
    {
        var request = new NetworkRequest
        {
            url = url,
            method = NetworkRequest.HttpMethod.GET,
            headers = headers ?? new Dictionary<string, string>()
        };
        SendRequest(request, callback);
    }

    public void Post(string url, string jsonData, Action<NetworkResponse> callback, Dictionary<string, string> headers = null)
    {
        var request = new NetworkRequest
        {
            url = url,
            method = NetworkRequest.HttpMethod.POST,
            body = jsonData,
            headers = headers ?? new Dictionary<string, string>()
        };
        SendRequest(request, callback);
    }
    
    public void Post(string url, byte[] bytes, Action<NetworkResponse> callback, Dictionary<string, string> headers = null)
    {
        var request = new NetworkRequest
        {
            url = url,
            method = NetworkRequest.HttpMethod.POST,
            bytes = bytes,
            headers = headers ?? new Dictionary<string, string>()
        };
        SendRequest(request, callback);
    }

    // public void Post<T>(string url, T data, Action<NetworkResponse> callback, Dictionary<string, string> headers = null)
    // {
    //     string json = JsonConvert.SerializeObject(data);
    //     Post(url, json, callback, headers);
    // }
}