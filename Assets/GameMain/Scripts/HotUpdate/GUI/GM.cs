using System;
using BlockPuzzleGameToolkit.Scripts.Data;
using GoogleMobileAds.Api;
using Quester;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.GUI
{
    public class GM : MonoSingleton<GM>
    {
        [SerializeField]
        private LongPressButton longPressButton;
        [Header("Login")]
        [SerializeField]
        private GameObject loginDlg;
        [SerializeField]
        private Button loginBtn;
        [SerializeField]
        private Button closeLoginBtn;
        [SerializeField]
        private TMP_InputField passwordInput;
        [Header("GM")]
        [SerializeField]
        private Button gmCloseBtn;
        [SerializeField]
        private GameObject gmDlg;
        [SerializeField]
        private TMP_InputField levelInput;
        [SerializeField]
        private Button levelOkBtn;
        
        [SerializeField]
        private TMP_InputField scoreInput;
        [SerializeField]
        private Button scoreOkBtn;
        
        [SerializeField]
        private Button unlockAdventureBtn;
        [SerializeField]
        private Button testAdMobBtn;
        
        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            longPressButton.onLongPress.AddListener(ShowLoginDialog);
            loginBtn.onClick.AddListener(CheckPassword);
            closeLoginBtn.onClick.AddListener(CloseLoginDialog);
            gmCloseBtn.onClick.AddListener(CloseGm);
            levelOkBtn.onClick.AddListener(EditLevel);
            scoreOkBtn.onClick.AddListener(EditScore);
            unlockAdventureBtn.onClick.AddListener(UnlockAdventure);
            testAdMobBtn.onClick.AddListener(OpenAdMobInspector);
        }

        private void ShowLoginDialog()
        {
            loginDlg.SetActive(true);
        }

        private void CheckPassword()
        {
            if (passwordInput.text == "010101")
            {
                loginDlg.SetActive(false);
                gmDlg.SetActive(true);
            }
        }

        private void CloseLoginDialog()
        {
            loginDlg.SetActive(false);
        }

        private void CloseGm()
        {
            gmDlg.SetActive(false);
        }

        public void PauseMusic()
        {
            GameEntry.Sound.PauseMusic();
        }

        public void ResumeMusic()
        {
            GameEntry.Sound.ResumeMusic();
        }

        private void EditLevel()
        {
            if (int.TryParse(levelInput.text, out int level))
            {
                UserDataManager.Instance.SetLevel(level);
            }
        }

        private void EditScore()
        {
            if (int.TryParse(scoreInput.text, out int score))
            {
                UserDataManager.Instance.SetClassicBestScore(score);
            }
        }

        public void UnlockAdventure()
        {
            UserDataManager.Instance.SetAdventureState(1);
        }
        
        public void OpenAdMobInspector()
        {
            MobileAds.OpenAdInspector((AdInspectorError error) =>
            {
                if (error != null)
                {
                    Debug.LogError($"code: {error.GetCode()}");
                    Debug.LogError(error.GetCause()?.GetMessage());
                    Debug.LogError(error.GetMessage());
                }
            });
        }
    }
}