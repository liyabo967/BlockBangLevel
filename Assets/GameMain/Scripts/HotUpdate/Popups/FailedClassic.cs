// // ©2015 - 2026 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.System;
using DG.Tweening;
using GameAnalyticsSDK;
using GameMain.Scripts.HotUpdate.Services;
using Quester;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class FailedClassic : Failed
    {
        [SerializeField] private Sprite gold;
        [SerializeField] private Sprite green;
        [SerializeField] private Image buttonImage;
        
        [SerializeField] private GameObject bestTitle;
        [SerializeField] private GameObject loudspeaker;
        [SerializeField] private TutorialHand tutorialHand;
        [SerializeField] private TextMeshProUGUI exceedingTips;
        
        public GameObject failedStuff;
        public GameObject bestScoreStuff;

        public TextMeshProUGUI[] scoreText;
        public TextMeshProUGUI bestScoreText;
        protected BaseModeHandler modeHandler;
        
        private int _score;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            tutorialHand.gameObject.SetActive(false);
            exceedingTips.gameObject.SetActive(false);
            GameAnalyticsManager.SendDesignEvent("classic:end");
            modeHandler = FindObjectOfType<BaseModeHandler>(false);
            var score = modeHandler.score;
            var bestScore = UserDataManager.Instance.ClassicBestScore;
            scoreText[0].text = score.ToString();
            scoreText[1].text = score.ToString();
            bestScoreText.text = bestScore.ToString();
            _score = score;
            if (score >= bestScore && score >= 300)
            {
                PlayBestScoreAnimation();
                GameEntry.Sound.PlaySound(SoundId.SeasonSuccess);
                bestScoreStuff.SetActive(true);
                failedStuff.SetActive(false);
                buttonImage.sprite = gold;
                // exceedingTips.text = GameEntry.Localization.GetString("#better_than");
                
                if (score >= ClassicModeHandler.UnlockAdventureScore)
                {
                    var eventValues = new Dictionary<string, string>
                    {
                        { "score", score.ToString() }
                    };

                    AppsFlyer.sendEvent(
                        "classic_complete",
                        eventValues
                    );
                }
            }
            else
            {
                GameEntry.Sound.PlaySound(SoundId.ClassicFailure);
                failedStuff.SetActive(true);
                bestScoreStuff.SetActive(false);
                buttonImage.sprite = green;
            }
            // closeButton.gameObject.SetActive(true);
            GameAnalyticsManager.SendUserProgression(UserStage.Classic, GAProgressionStatus.Complete, null, score);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_score < 300)
            {
                StartCoroutine(AutoRetry(2.5f));
            }
        }

        private IEnumerator AutoRetry(float delay)
        {
            tutorialHand.gameObject.SetActive(true);
            tutorialHand.PlayAnim(retryButton.transform);
            yield return new WaitForSeconds(delay);
            Retry();
        }

        private void PlayBestScoreAnimation()
        {
            bestTitle.transform.localScale = Vector3.zero;
            bestTitle.transform.DOScale(1, 0.5f).OnComplete(() =>
            {
                var sequence = DOTween.Sequence();
                sequence.Append(loudspeaker.transform.DOScale(1.2f, 0.2f));
                sequence.Append(loudspeaker.transform.DOScale(1f, 0.2f));
                sequence.SetLoops(3, LoopType.Yoyo).OnComplete(() =>
                {
                    MobileReviewManager.Instance.RequestReview(EGameMode.Classic);
                });
            });
        }

        public void BackToHome()
        {
            GameAnalyticsManager.SendDesignEvent("classic:end:homepage");
            Close(true);
            GameManager.instance.MainMenu();
        }

        protected override void Retry()
        {
            if (UserDataManager.Instance.AdventureState == 1)
            {
                BackToHome();
            }
            else
            {
                base.Retry();
            }
        }
    }
}