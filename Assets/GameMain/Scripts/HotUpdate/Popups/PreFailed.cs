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

using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.System;
using DG.Tweening;
using Quester;
using TMPro;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class PreFailed : UGuiForm
    {
        public TextMeshProUGUI continuePrice;
        public TextMeshProUGUI timerText;
        public CustomButton continueButton;
        public CustomButton rewardButton;
        public TextMeshProUGUI timeLeftText;
        protected int timer;
        protected int price;
        protected bool hasContinued = false;
        protected EPopupResult result;

        private object _userData;
        private LevelManager _levelManager;
        private int _extraSeconds;

        protected virtual void OnEnable()
        {
            _levelManager = FindAnyObjectByType<LevelManager>();
            price = GameManager.instance.GameSettings.continuePrice;
            continuePrice.text = price.ToString();
            continueButton.onClick.AddListener(Continue);
            
            InitializeTimer();
            
            timerText.text = timer.ToString();
            SoundBase.instance.PlaySound(SoundBase.instance.warningTime);
            rewardButton.gameObject.SetActive(GameManager.instance.GameSettings.enableAds);
            if(GameDataManager.GetLevel().enableTimer && timeLeftText != null)
            {
                timeLeftText.gameObject.SetActive(true);
                _extraSeconds = 0;
                if (_levelManager.timerManager.RemainingTime < 5)
                {
                    _extraSeconds = GameManager.instance.GameSettings.continueTimerBonus;
                }
                else
                {
                    _extraSeconds = GameManager.instance.GameSettings.continueTimerBonus / 2;
                }
                var extraString = $"<color=yellow>{_extraSeconds}</color>";
                timeLeftText.text = GameEntry.Localization.GetString("#continue_with_extra_seconds", extraString);
            }
        }

        protected virtual void InitializeTimer()
        {
            timer = GameManager.instance.GameSettings.failedTimerStart;
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            _userData = userData;
            continueButton.interactable = true;
            rewardButton.interactable = true;
            hasContinued = false;
            // Start the timer only after the popup animation is complete
            InvokeRepeating(nameof(UpdateTimer), 1, 1);
        }

        protected virtual void UpdateTimer()
        {
            // Only decrement timer if this popup is active and not already expired
            if (timer > 0)
            {
                timer--;
                SaveTimerState();
            }

            timerText.text = timer.ToString();
            if (timer <= 0)
            {
                continueButton.interactable = false;
                rewardButton.interactable = false;
                hasContinued = true;

                CancelInvoke(nameof(UpdateTimer));
                Close(true);
                EventManager.GameStatus = EGameState.Failed;
            }
        }

        protected virtual void SaveTimerState() { }

        public void PauseTimer()
        {
            CancelInvoke(nameof(UpdateTimer));
        }

        protected virtual void Continue()
        {
            if (timer <= 0 || hasContinued)
            {
                return;
            }

            var coinsResource = ResourceManager.instance.GetResource("Coins");
            if (coinsResource.Consume(price))
            {
                hasContinued = true;
                continueButton.interactable = false;
                rewardButton.interactable = false;
                                
                CancelInvoke(nameof(UpdateTimer));
                ShowCoinsSpendFX(continueButton.transform.position);
                OnContinue();
            }
            else
            {
                PauseTimer();
            }
        }

        public void OnContinue()
        {
            DOTween.Kill(this);
            DOVirtual.DelayedCall(0.5f, ContinueGame);
        }

        public void ContinueGame()
        {
            result = EPopupResult.Continue; 
            EventManager.GameStatus = EGameState.Playing; 
            Close();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);

            var levelManager = _levelManager;
            var level = levelManager.GetCurrentLevel();
            if (result == EPopupResult.Continue)
            {
                if (_userData is ClassicLevelStateHandler)
                {
                    levelManager.cellDeck.UpdateCellDeckAfterFail();
                    EventManager.GameStatus = EGameState.Playing;
                }
                else if (_userData is AdventureLevelStateHandler)
                {
                    if (level.enableTimer)
                    {
                        float newTime = levelManager.timerManager.RemainingTime + _extraSeconds;
                        levelManager.timerManager.InitializeTimer(newTime);
                    }
                    levelManager.cellDeck.UpdateCellDeckAfterFail();
                    EventManager.GameStatus = EGameState.Playing;
                }
                else if (userData is TimedLevelStateHandler)
                {
                    levelManager.cellDeck.UpdateCellDeckAfterFail();
                    levelManager.timerManager?.InitializeTimer(levelManager.timerManager.RemainingTime );
                    EventManager.GameStatus = EGameState.Playing;
                }
            }
        }
    }
}