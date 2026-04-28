using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 文件下载器 —— 支持重试、进度回调、超时设置
/// </summary>
public class FileDownloader : MonoSingleton<FileDownloader>
{
    // ── 默认配置 ──────────────────────────────────────
    private const int   DEFAULT_MAX_RETRIES  = 3;
    private const float DEFAULT_TIMEOUT      = 30f;   // 秒
    private const float DEFAULT_RETRY_DELAY  = 2f;    // 重试前等待秒数

    // ══════════════════════════════════════════════════
    //  公开入口
    // ══════════════════════════════════════════════════

    /// <summary>
    /// 下载文件到指定本地路径
    /// </summary>
    /// <param name="url">远程地址</param>
    /// <param name="savePath">本地保存完整路径</param>
    /// <param name="onSuccess">成功回调（本地路径）</param>
    /// <param name="onFailure">最终失败回调（错误信息）</param>
    /// <param name="onProgress">进度回调 0~1（可选）</param>
    /// <param name="maxRetries">最大重试次数（默认 3）</param>
    /// <param name="timeout">单次超时秒数（默认 30s）</param>
    /// <param name="retryDelay">重试间隔秒数（默认 2s）</param>
    public IEnumerator Download(
        string url,
        string savePath,
        Action<string>  onSuccess  = null,
        Action<string>  onFailure  = null,
        Action<float>   onProgress = null,
        int   maxRetries = DEFAULT_MAX_RETRIES,
        float timeout    = DEFAULT_TIMEOUT,
        float retryDelay = DEFAULT_RETRY_DELAY)
    {
        yield return StartCoroutine(DownloadRoutine(
            url, savePath,
            onSuccess, onFailure, onProgress,
            maxRetries, timeout, retryDelay));
    }

    // ══════════════════════════════════════════════════
    //  核心协程
    // ══════════════════════════════════════════════════

    private IEnumerator DownloadRoutine(
        string url,
        string savePath,
        Action<string>  onSuccess,
        Action<string>  onFailure,
        Action<float>   onProgress,
        int   maxRetries,
        float timeout,
        float retryDelay)
    {
        int  attempt   = 0;
        bool succeeded = false;
        string lastError = string.Empty;

        while (attempt <= maxRetries && !succeeded)
        {
            if (attempt > 0)
            {
                Debug.Log($"[Downloader] 第 {attempt} 次重试，等待 {retryDelay}s… (url={url})");
                yield return new WaitForSeconds(retryDelay);
            }

            attempt++;
            Debug.Log($"[Downloader] 开始下载 (尝试 {attempt}/{maxRetries + 1}) → {url}");

            using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            req.downloadHandler = new DownloadHandlerFile(savePath)
            {
                removeFileOnAbort = true   // 中途失败自动删除残留文件
            };
            req.timeout = (int)timeout;

            var op = req.SendWebRequest();

            // 等待下载，同时持续上报进度
            while (!op.isDone)
            {
                onProgress?.Invoke(req.downloadProgress);
                yield return null;
            }

            // ── 结果判断 ──────────────────────────────
            if (req.result == UnityWebRequest.Result.Success)
            {
                onProgress?.Invoke(1f);
                Debug.Log($"[Downloader] 下载成功 → {savePath}");
                onSuccess?.Invoke(savePath);
                succeeded = true;
            }
            else
            {
                lastError = $"[HTTP {req.responseCode}] {req.error}";
                Debug.LogWarning($"[Downloader] 第 {attempt} 次失败：{lastError}");
            }
        }

        if (!succeeded)
        {
            string msg = $"下载失败，已重试 {maxRetries} 次。最后错误：{lastError}";
            Debug.LogError($"[Downloader] {msg}");
            onFailure?.Invoke(msg);
        }
    }
}
