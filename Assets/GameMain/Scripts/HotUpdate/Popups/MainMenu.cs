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

using System;
using System.Collections;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.GUI.Labels;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.System;
using DG.Tweening;
using Quester;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class MainMenu : UGuiForm
    {
        public CustomButton timedMode;
        public CustomButton classicMode;
        public CustomButton adventureMode;
        public CustomButton settingsButton;
        public CustomButton luckySpin;
        public GameObject playObject;
        public GameObject seasonTimeObject;
        public TextMeshProUGUI remainingTimeText;
        public GameObject adventureLock;
        public TextMeshProUGUI lockedText;
        public GameObject adventureRedPoint;
        public GameObject settingsRedPoint;
        public GameObject gameSettingBtn;
        
        private bool _enableTimer;
        private WaitForSeconds _waitForSeconds = new (1f);
        private bool _adventureUnlocked;
        private Sequence _adventureSequence;

        [SerializeField]
        private GameObject freeSpinMarker;

        [SerializeField]
        private Image background;
        
        [SerializeField]
        private GameObject fxPrefab;

        public Action OnAnimationEnded;

        private const string LastFreeSpinTimeKey = "LastFreeSpinTime";

        private void Start()
        {
            timedMode.onClick.AddListener(PlayTimedMode);
            classicMode.onClick.AddListener(PlayClassicMode);
            adventureMode.onClick.AddListener(PlayAdventureMode);
            settingsButton.onClick.AddListener(SettingsButtonClicked);
            luckySpin.onClick.AddListener(LuckySpinButtonClicked);
            UpdateFreeSpinMarker();
            GameDataManager.LevelNum = UserDataManager.Instance.Level;
            // var levelsCount = Resources.LoadAll<Level>("Levels").Length;
            luckySpin.gameObject.SetActive(GameManager.instance.GameSettings.enableLuckySpin);
            if (!GameManager.instance.GameSettings.enableTimedMode)
            {
                timedMode.gameObject.SetActive(false);
            }

            _adventureUnlocked = UserDataManager.Instance.AdventureState > 0;
            lockedText.text = GameEntry.Localization.GetString("#unlock_adventure", 700);
            adventureLock.SetActive(!_adventureUnlocked);
            seasonTimeObject.SetActive(_adventureUnlocked);
        }

        private void OnEnable()
        {
            _enableTimer = true;
            StartCoroutine(RefreshRemainingTime());
            
            if (!_adventureUnlocked)
            {
                if (UserDataManager.Instance.AdventureState == 1)
                {
                    _adventureUnlocked = true;
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        adventureLock.transform.DOScale(Vector3.zero, 0.3f)
                            .SetEase(Ease.InBack)
                            .OnComplete(() =>
                            {
                                adventureLock.SetActive(false);
                                PlayVfx(adventureMode.transform.position, () =>
                                {
                                    seasonTimeObject.SetActive(true);
                                    PlayAdventureButtonAnim();
                                });
                            });
                    });
                }
            }
        }

        public void RefreshUI()
        {
            gameSettingBtn.SetActive(!GameManager.instance.IsTutorialMode());
            adventureRedPoint.SetActive(UserDataManager.Instance.AdventureState == 1);
            settingsRedPoint.SetActive(UserDataManager.Instance.AdventureState == 1 && StateManager.instance.CurrentState == EScreenStates.Game);
        }

        private void PlayAdventureButtonAnim()
        {
            _adventureSequence = DOTween.Sequence();
            _adventureSequence.AppendInterval(2f);
            _adventureSequence.Append(
                adventureMode.transform.DOScale(1.15f, 0.15f));
            
            _adventureSequence.Append(
                adventureMode.transform.DOScale(1f, 0.15f));
            
            _adventureSequence.Append(
                adventureMode.transform.DOShakeRotation(
                    0.5f,
                    new Vector3(0, 0, 6),
                    15,
                    90));
            _adventureSequence.SetLoops(-1);
            
            // _adventureSequence.AppendInterval(2f);
            // _adventureSequence.Append(
            //     adventureMode.transform.DOPunchScale(
            //         Vector3.one * 0.25f,
            //         0.5f,
            //         8,
            //         0.8f));
            //
            // _adventureSequence.SetLoops(-1);
        }

        private void StopAdventureButtonAnim()
        {
            _adventureSequence?.Kill();
        }

        private bool CanUseFreeSpinToday()
        {
            if (!PlayerPrefs.HasKey(LastFreeSpinTimeKey))
            {
                return true;
            }

            var lastFreeSpinTimeStr = PlayerPrefs.GetString(LastFreeSpinTimeKey);
            var lastFreeSpinTime = DateTime.Parse(lastFreeSpinTimeStr);
            return DateTime.Now.Date > lastFreeSpinTime.Date;
        }

        private void UpdateFreeSpinMarker()
        {
            var isFreeSpinAvailable = CanUseFreeSpinToday();
            freeSpinMarker.SetActive(isFreeSpinAvailable);
        }

        private void PlayClassicMode()
        {
            GameManager.instance.SetGameMode(EGameMode.Classic);
            GameManager.instance.OpenMap();
        }

        private void PlayAdventureMode()
        {
            StopAdventureButtonAnim();
            GameManager.instance.SetGameMode(EGameMode.Adventure);
            GameManager.instance.OpenMap();
            adventureRedPoint.SetActive(false);
            UserDataManager.Instance.SetAdventureState(2);
        }

        private void PlayTimedMode()
        {
            GameManager.instance.SetGameMode(EGameMode.Timed);
            GameManager.instance.OpenMap();
        }

        private void SettingsButtonClicked()
        {
            GameEntry.UI.OpenUIForm(UIFormId.Settings);
        }

        private void LuckySpinButtonClicked()
        {
            GameEntry.UI.OpenUIForm(UIFormId.LuckySpin);
            UpdateFreeSpinMarker();
        }

        public void OnAnimationEnd(){
            OnAnimationEnded?.Invoke();
        }
        
        private IEnumerator RefreshRemainingTime()
        {
            while (_enableTimer)
            {
                var seconds = TimeManager.SeasonTime.seasonEndTime - TimeManager.GetCurrentTime();
                // Debug.Log($"seconds: {seconds}");
                remainingTimeText.text = FormatTime((int)seconds);
                yield return _waitForSeconds;
                if (seconds == 0)
                {
                    RefreshTime();
                }
            }
        }

        private void RefreshTime()
        {
            _enableTimer = false;
            TimeManager.SetSeasonTime(result =>
            {
                _enableTimer = true;
            });
        }
        
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
        
        private void PlayVfx(Vector3 targetPosition, Action callback)
        {
            var fx = PoolObject.GetObject(fxPrefab, targetPosition);
            fx.transform.localScale = Vector3.one;
            fx.transform.position = targetPosition;
            DOVirtual.DelayedCall(0.5f, () =>
            {
                PoolObject.Return(fx);
                callback?.Invoke();
            });
        }
    }
}