using System;
using GameMain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AotDialogUI : MonoBehaviour
{
    public GameObject panel;
    
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI confirmText;
    public Button closeButton;
    
    public static AotDialogUI Instance;

    private Action _onButtonClick;
    
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        titleText.text = LocalLanguage.Instance.GetString("#tips");
        confirmText.text = LocalLanguage.Instance.GetString("#retry");
        closeButton.onClick.AddListener(OnConfirm);
    }

    public void Show(AotDialogParams dialogParams)
    {
        panel.SetActive(true);
        if (!string.IsNullOrEmpty(dialogParams.title))
        {
            titleText.text = dialogParams.title;
        }
        messageText.text = dialogParams.message;
        confirmText.text = dialogParams.confirmText;
        _onButtonClick = dialogParams.callback;
    }

    private void OnConfirm()
    {
        panel.SetActive(false);
        _onButtonClick?.Invoke();
    }

    public class AotDialogParams
    {
        public string title;
        public string message;
        public string confirmText;
        public Action callback;
    }
}
