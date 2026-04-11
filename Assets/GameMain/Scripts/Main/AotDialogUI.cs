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

    public void Show(string message, Action callback = null)
    {
        panel.SetActive(true);
        messageText.text = message;
        _onButtonClick = callback;
    }

    private void OnConfirm()
    {
        panel.SetActive(false);
        _onButtonClick?.Invoke();
    }

}
