// NetworkConfig.cs - 网络配置
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkConfig", menuName = "Network/Network Config")]
public class NetworkConfig : ScriptableObject
{
    [Header("基础配置")]
    public string baseUrl = "";
    public int defaultTimeout = 30;
    public int defaultRetryCount = 0;
    public int maxConcurrentRequests = 5;

    [Header("请求头配置")]
    public List<HeaderConfig> defaultHeaders = new List<HeaderConfig>();

    [Header("重试配置")]
    public bool enableRetry = true;
    public float retryDelay = 1f;
    public RetryStrategy retryStrategy = RetryStrategy.Linear;

    [Header("日志配置")]
    public bool enableLogging = true;
    public LogLevel logLevel = LogLevel.Info;

    [Header("缓存配置")]
    public bool enableCache = false;
    public int cacheExpireTime = 300; // 秒

    public enum RetryStrategy
    {
        Linear,     // 线性延迟
        Exponential // 指数退避
    }

    public enum LogLevel
    {
        None,
        Error,
        Warning,
        Info,
        Debug
    }

    [Serializable]
    public class HeaderConfig
    {
        public string key;
        public string value;
    }
}