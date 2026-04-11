using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingAotUI : MonoBehaviour
{
    public static LoadingAotUI Instance;

    public Slider progressBar;
    public TMP_Text tipsText;
    public TMP_Text progressText;

    private int _progress;
    private string _loadingStr;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PlayAnim();
    }

    public void SetTips(string tips)
    {
        tipsText.text = tips;
    }

    public void SetProgress(float progress)
    {
        progressBar.value = progress;
        int p = Mathf.RoundToInt(progress * 100);
        progressText.text = $"{p}%";
    }

    private void PlayAnim()
    {
        DOTween.To(
            () => 0f,
            x =>
            {
                progressBar.value = x;
                progressText.text = $"{Mathf.RoundToInt(x * 100)}%";;
            },
            0.1f,
            0.5f
        );
    }
}