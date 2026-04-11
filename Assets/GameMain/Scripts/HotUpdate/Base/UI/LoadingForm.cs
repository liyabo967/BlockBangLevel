using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Event;
using GameMain;
using Quester;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingForm : UGuiForm
{
    public static LoadingForm Instance;

    public Slider progressBar;
    public TMP_Text tipsText;
    public TMP_Text progressText;

    private float _targetProgress;
    private string _loadingStr;
    private Tween _progressTween;
    
    // 不同阶段的进度条占比
    private Dictionary<ProgressEventArgs.ProgressKey, float> _progressWeightDict = new()
    {
        { ProgressEventArgs.ProgressKey.HotUpdate , 0.3f},
        { ProgressEventArgs.ProgressKey.PreLoadDataTable , 0.4f},
        { ProgressEventArgs.ProgressKey.UpdateResource , 0.2f},
        { ProgressEventArgs.ProgressKey.Preload , 0.1f}
    };

    private Dictionary<ProgressEventArgs.ProgressKey, float> _currentProgressDict = new();
    private ProgressEventArgs.ProgressKey _currentProgressKey;
    

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetCanvas();
        SetTips(LocalLanguage.Instance.GetString("#loading"));
        GameEntry.Event.Subscribe(ProgressEventArgs.EventId, OnProgressEventArgs);
    }
    
    private void SetCanvas()
    {
        var mainCamera = Camera.main;
        if (mainCamera)
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCamera;
        }
    }

    private void OnDestroy()
    {
        GameEntry.Event.Unsubscribe(ProgressEventArgs.EventId, OnProgressEventArgs);
    }

    private void OnProgressEventArgs(object sender, GameEventArgs args)
    {
        if (args is ProgressEventArgs progressArgs)
        {
            var currentProgress = _currentProgressDict.GetValueOrDefault(progressArgs.Key, 0);
            if (progressArgs.Progress > currentProgress)
            {
                SetProgressTips(progressArgs.Key);
                _currentProgressDict[progressArgs.Key] = progressArgs.Progress;

                // 热更新占比
                var totalProgress = _progressWeightDict[ProgressEventArgs.ProgressKey.HotUpdate];
                foreach (var keyValuePair in _currentProgressDict)
                {
                    var p = _progressWeightDict[keyValuePair.Key] * _currentProgressDict[keyValuePair.Key];
                    totalProgress += p;
                }
                SetProgress(totalProgress);
            }
            else
            {
                Debug.LogError($"{progressArgs.Key}, {progressArgs.Progress}");
            }
        }
    }

    private void SetProgressTips(ProgressEventArgs.ProgressKey key)
    {
        if (key != _currentProgressKey)
        {
            _currentProgressKey = key;
            SetTips(GameEntry.Localization.GetString(GetLanguageKey(key)));
        }
    }

    private string GetLanguageKey(ProgressEventArgs.ProgressKey progressKey)
    {
        string key = "#loading";
        // switch (progressKey)
        // {
        //     case ProgressEventArgs.ProgressKey.PreLoadDataTable:
        //         key = "#load_data_table";
        //         break;
        //     case ProgressEventArgs.ProgressKey.UpdateResource:
        //         key = "#update_resource";
        //         break;
        //     case ProgressEventArgs.ProgressKey.Preload:
        //         key = "#loading";
        //         break;
        // }
        return key;
    }

    public void SetTips(string tips)
    {
        tipsText.text = tips;
    }
    
    private void SetProgress(float progress)
    {
        if (progress < 1 && progress < _targetProgress + 0.1f)
        {
            return;
        }

        _targetProgress = progress;
        if (_progressTween != null)
        {
            _progressTween.Kill();
        }
        _progressTween = DOTween.To(
            () => progressBar.value,
            x =>
            {
                progressBar.value = x;
                progressText.text = $"{Mathf.RoundToInt(x * 100)}%";;
            },
            progress,
            0.5f
        );
        _progressTween.onComplete = () =>
        {
            if (progress >= 1f)
            {
                GameEntry.Event.Fire(this, PreloadSuccessEventArgs.Create());
            }
        };
    }

    public void CloseSelf()
    {
        StartCoroutine(CloseCo(0.5f));
    }

    private IEnumerator CloseCo(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        gameObject.SetActive(false);
    }
}