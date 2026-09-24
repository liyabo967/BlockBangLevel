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
using UnityEngine.VFX;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class LevelWin : UGuiForm
    {
        [SerializeField] private GameObject title;
        [SerializeField] private Image pieceImage;
        [SerializeField] private Image pieceFrame;
        [SerializeField] private ParticleSystem winParticle;
        [SerializeField] private ParticleSystem unlockParticle;
        
        private PictureComponent _pictureComponent;
        private Sprite _originalSprite;
        private Canvas _canvas;
        private ParticleSystemRenderer _winParticleRenderer;
        private ParticleSystemRenderer _unlockParticleRenderer;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            _originalSprite = pieceImage.sprite;
            _canvas = GetComponent<Canvas>(); 
            _winParticleRenderer = winParticle.GetComponent<ParticleSystemRenderer>();
            _unlockParticleRenderer = unlockParticle.GetComponent<ParticleSystemRenderer>();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _winParticleRenderer.sortingOrder = _canvas.sortingOrder;
            _unlockParticleRenderer.sortingOrder = _canvas.sortingOrder;
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
            pieceFrame.gameObject.SetActive(false);
            PlayImageAnim();
        }

        protected virtual void OnEnable()
        {
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
                unlockParticle.gameObject.SetActive(true);
                pieceImage.sprite = _pictureComponent.GetFocusedSprite();
                pieceFrame.gameObject.SetActive(true);
            });
            // sequence.AppendInterval(0.5f);
            sequence.Append(pieceImage.transform.DOScale(
                new Vector3(1, 1, 1),
                0.5f
            ));
            sequence.AppendInterval(0.5f).OnComplete(OnPieceAnimationFinished);
        }

        private void OnPieceAnimationFinished()
        {
            // Debug.LogError("OnPieceAnimationFinished");
            Close(true);
        }
        
        public override void AfterShowAnimation()
        {
            base.AfterShowAnimation();
            PlayTextAnim();
        }

        private void PlayTextAnim()
        {
            title.transform.localScale = Vector3.zero;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(title.transform.DOScale(
                new Vector3(1.1f, 1.1f, 1.1f),
                0.3f
            ));
            sequence.Append(title.transform.DOScale(
                Vector3.one,
                0.3f
            ));
            sequence.Append(title.transform.DOScale(
                new Vector3(1.05f, 1.05f, 1.05f),
                0.3f
            ));
            sequence.Append(title.transform.DOScale(
                Vector3.one,
                0.3f
            ));
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