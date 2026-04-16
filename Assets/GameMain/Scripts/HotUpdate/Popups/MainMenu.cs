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
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.System;
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
        public TextMeshProUGUI remainingTimeText;
        
        private bool _enableTimer;
        private WaitForSeconds _waitForSeconds = new (1f);

        [SerializeField]
        private GameObject freeSpinMarker;

        [SerializeField]
        private Image background;

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
            var levelsCount = Resources.LoadAll<Level>("Levels").Length;
            luckySpin.gameObject.SetActive(GameManager.instance.GameSettings.enableLuckySpin);
            if(!GameManager.instance.GameSettings.enableTimedMode)
                timedMode.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _enableTimer = true;
            StartCoroutine(RefreshRemainingTime());
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
            GameManager.instance.SetGameMode(EGameMode.Adventure);
            GameManager.instance.OpenMap();
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
    }
}