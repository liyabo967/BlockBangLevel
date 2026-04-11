//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using GameFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Quester
{
    public class DialogForm : UGuiForm
    {
        [SerializeField]
        private TextMeshProUGUI titleText = null;

        [SerializeField]
        private TextMeshProUGUI messageText = null;

        [SerializeField]
        private GameObject[] modeObjects = null;

        [SerializeField]
        private TextMeshProUGUI[] confirmTexts = null;

        [SerializeField]
        private TextMeshProUGUI[] cancelTexts = null;

        [SerializeField]
        private TextMeshProUGUI[] otherTexts = null;

        private int _dialogMode = 1;
        private bool _pauseGame = false;
        private object _userData = null;
        private Action<object> _onClickConfirm = null;
        private Action<object> _onClickCancel = null;
        private Action<object> _onClickOther = null;

        public int DialogMode
        {
            get
            {
                return _dialogMode;
            }
        }

        public bool PauseGame
        {
            get
            {
                return _pauseGame;
            }
        }

        public object UserData
        {
            get
            {
                return _userData;
            }
        }

        public void OnConfirmButtonClick()
        {
            Close();

            if (_onClickConfirm != null)
            {
                _onClickConfirm(_userData);
            }
        }

        public void OnCancelButtonClick()
        {
            Close();

            if (_onClickCancel != null)
            {
                _onClickCancel(_userData);
            }
        }

        public void OnOtherButtonClick()
        {
            Close();

            if (_onClickOther != null)
            {
                _onClickOther(_userData);
            }
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);

            DialogParams dialogParams = (DialogParams)userData;
            if (dialogParams == null)
            {
                Log.Warning("DialogParams is invalid.");
                return;
            }

            _dialogMode = dialogParams.Mode;
            RefreshDialogMode();

            titleText.text = dialogParams.Title;
            messageText.text = dialogParams.Message;

            _pauseGame = dialogParams.PauseGame;
            RefreshPauseGame();

            _userData = dialogParams.UserData;

            RefreshConfirmText(dialogParams.ConfirmText);
            _onClickConfirm = dialogParams.OnClickConfirm;

            RefreshCancelText(dialogParams.CancelText);
            _onClickCancel = dialogParams.OnClickCancel;

            RefreshOtherText(dialogParams.OtherText);
            _onClickOther = dialogParams.OnClickOther;
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnClose(bool isShutdown, object userData)
#else
        protected internal override void OnClose(bool isShutdown, object userData)
#endif
        {
            if (_pauseGame)
            {
                GameEntry.Base.ResumeGame();
            }

            _dialogMode = 1;
            titleText.text = string.Empty;
            messageText.text = string.Empty;
            _pauseGame = false;
            _userData = null;

            RefreshConfirmText(string.Empty);
            _onClickConfirm = null;

            RefreshCancelText(string.Empty);
            _onClickCancel = null;

            RefreshOtherText(string.Empty);
            _onClickOther = null;

            base.OnClose(isShutdown, userData);
        }

        private void RefreshDialogMode()
        {
            for (int i = 1; i <= modeObjects.Length; i++)
            {
                modeObjects[i - 1].SetActive(i == _dialogMode);
            }
        }

        private void RefreshPauseGame()
        {
            if (_pauseGame)
            {
                GameEntry.Base.PauseGame();
            }
        }

        private void RefreshConfirmText(string confirmText)
        {
            if (string.IsNullOrEmpty(confirmText))
            {
                confirmText = GameEntry.Localization.GetString("Dialog.ConfirmButton");
            }

            for (int i = 0; i < confirmTexts.Length; i++)
            {
                confirmTexts[i].text = confirmText;
            }
        }

        private void RefreshCancelText(string cancelText)
        {
            if (string.IsNullOrEmpty(cancelText))
            {
                cancelText = GameEntry.Localization.GetString("Dialog.CancelButton");
            }

            for (int i = 0; i < cancelTexts.Length; i++)
            {
                cancelTexts[i].text = cancelText;
            }
        }

        private void RefreshOtherText(string otherText)
        {
            if (string.IsNullOrEmpty(otherText))
            {
                otherText = GameEntry.Localization.GetString("Dialog.OtherButton");
            }

            for (int i = 0; i < otherTexts.Length; i++)
            {
                otherTexts[i].text = otherText;
            }
        }
    }
}
