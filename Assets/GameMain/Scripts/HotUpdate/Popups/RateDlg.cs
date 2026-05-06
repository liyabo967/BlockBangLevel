using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quester
{
    public class RateDlg : UGuiForm
    {
        public Button rateBtn;
        public Button feedbackBtn;
        public List<Button> starBtnList;
        public TextMeshProUGUI rateTips;
        public TextMeshProUGUI feedbackTips;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            rateBtn.onClick.AddListener(OnClickRate);
            feedbackBtn.onClick.AddListener(OnClickFeedback);
            for (var i = 0; i < starBtnList.Count; i++)
            {
                int index = i;
                starBtnList[i].onClick.AddListener(() =>
                {
                    OnClickStar(index);
                });
            }
        }

        private void OnClickStar(int index)
        {
            for (var i = 0; i < starBtnList.Count; i++)
            {
                var star =  starBtnList[i];
                var starIcon =  star.transform.GetChild(0); 
                starIcon.gameObject.SetActive(i <= index);
            }

            var positive = index >= 3;
            rateBtn.gameObject.SetActive(positive);
            rateTips.gameObject.SetActive(positive);
            feedbackBtn.gameObject.SetActive(!positive);
            feedbackTips.gameObject.SetActive(!positive);
        }

        private void OnClickRate()
        {
#if UNITY_ANDROID
            Application.OpenURL("market://details?id=com.quester.game.blockbang");
#elif UNITY_IPHONE
            Application.OpenURL("itms-apps://itunes.apple.com/app/id6749655294");
#endif
            Close();
        }

        private void OnClickFeedback()
        {
            EmailUtils.SendEMail(UserDataManager.Instance.Level);
            Close();
        }
    }
}