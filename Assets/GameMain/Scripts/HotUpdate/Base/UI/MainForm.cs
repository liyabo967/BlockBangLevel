using System.Collections;
using DG.Tweening;
using Quester;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain.Scripts.HotUpdate.UI
{
    public class MainForm : UGuiForm
    {
        public Image logoImg;
        public Text remainingTimeText;

        private Sequence _breathingSeq;
        private bool _enableTimer = false;
        private WaitForSeconds _waitForSeconds = new(1f);

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            CreateAnim();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            _enableTimer = true;
            PlayAnim();
        }

        private void CreateAnim()
        {
            _breathingSeq = DOTween.Sequence();
            _breathingSeq.Append(logoImg.transform.DOScale(1.1f, 2f));
            _breathingSeq.Join(logoImg.DOFade(0.8f, 2f));
            _breathingSeq.SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo).Pause();
        }
        private void PlayAnim()
        {
            _breathingSeq.Play();
        }

        private void StopAnim()
        {
            _breathingSeq.Pause();
            _breathingSeq.Rewind(); // 回到初始状态（可选）
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            _enableTimer = false;
            StopAnim();
        }

        public void StartAdventure()
        {
            GameEntry.UI.OpenUIForm(UIFormId.MapDlg);
        }

        public void StartClassic()
        {
            
        }
        
        // private IEnumerator RefreshRemainingTime()
        // {
        //     while (_enableTimer)
        //     {
        //         var seconds = GlobalVariables.SeasonTime.seasonEndTime - TimeUtil.GetCurrentTime();
        //         remainingTimeText.text = FormatTime((int)seconds);
        //         yield return _waitForSeconds;
        //         if (seconds == 0)
        //         {
        //             RefreshTime();
        //         }
        //     }
        // }

        // private void RefreshTime()
        // {
        //     _enableTimer = false;
        //     SeasonTimeUtil.SetSeasonTime(result =>
        //     {
        //         _enableTimer = true;
        //     });
        // }
        
        private string FormatTime(int seconds)
        {
            int days = seconds / 3600 / 24;
            int hours = seconds / 3600;
            if (days > 0)
            {
                // return $"{days}d {hours % 24}h {seconds % 60}s";
                return $"{days}d {hours % 24}h";
            }
            else if (hours > 0)
            {
                return $"{hours}h {seconds % 3600 / 60}m";
            }
            else
            {
                return $"{seconds / 60}m {seconds % 60}s";
            }
        }
    }
}