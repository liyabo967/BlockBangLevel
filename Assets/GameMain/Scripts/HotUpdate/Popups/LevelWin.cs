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

using System.Collections.Generic;
using AppsFlyerSDK;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using GameAnalyticsSDK;
using GameMain.Scripts.HotUpdate.Base.Ads;
using Quester;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class LevelWin : UGuiForm
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image pieceImage;
        [SerializeField] private ParticleSystem particleSystem;

        private PictureComponent _pictureComponent;
        private Sprite _originalSprite;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            _originalSprite = pieceImage.sprite;
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (_pictureComponent == null)
            {
                _pictureComponent = FindFirstObjectByType<PictureComponent>(FindObjectsInactive.Include);
            }
            // AppsFlyer
            Dictionary<string, string> eventValues = new Dictionary<string, string>();
            eventValues.Add(AFInAppEvents.LEVEL, (UserDataManager.Instance.Level - 1).ToString());
            AppsFlyer.sendEvent(AFInAppEvents.LEVEL_ACHIEVED, eventValues);
            UserDataManager.Instance.AddWinCount();
            pieceImage.sprite = _originalSprite;
            PlayImageAnim();
        }

        protected virtual void OnEnable()
        {
            messageText.transform.localScale = Vector3.zero;
            GameEntry.Sound.PlaySound(SoundId.Win);
            if (UserDataManager.Instance.Level > PictureComponent.MaxLevel)
            {
                UserDataManager.Instance.AddPicture($"{TimeManager.SeasonTime.year}_{TimeManager.SeasonTime.week}");
            }
        }

        private void PlayImageAnim()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(pieceImage.transform.DORotate(
                new Vector3(0, 360, 0),
                1f,
                RotateMode.FastBeyond360
            ));
            sequence.Append(pieceImage.transform.DOScale(
                new Vector3(0, 0, 0),
                0.5f
            ));
            sequence.AppendCallback(() =>
            {
                // particleSystem.gameObject.SetActive(true);
                // particleSystem.Play();
                pieceImage.sprite = _pictureComponent.GetFocusedSprite();
            });
            // sequence.AppendInterval(0.5f);
            sequence.Append(pieceImage.transform.DOScale(
                new Vector3(1, 1, 1),
                0.5f
            )).OnComplete(OnPieceAnimationFinished);
        }

        private void OnPieceAnimationFinished()
        {
            // Debug.LogError("OnPieceAnimationFinished");
            Close(true);
        }
        
        public override void AfterShowAnimation()
        {
            if (messageText != null)
            {
                messageText.transform.DOScale(Vector3.one, 0.2f);
            }
            base.AfterShowAnimation();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            EventManager.GameStatus = EGameState.Win;
            if (!UserDataManager.Instance.NoAdsPurchased)
            {
                AdManager.Instance.ShowInterstitial();
            }
        }
    }
    
}