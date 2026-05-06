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
using BlockPuzzleGameToolkit.Scripts.Audio;
using Quester;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [SerializeField]
        private Slider musicButton;

        [SerializeField]
        private Slider soundButton;

        [SerializeField]
        private AudioMixer mixer;

        [SerializeField]
        private string musicParameter = "musicVolume";

        [SerializeField]
        private string soundParameter = "soundVolume";

        private Dictionary<string, bool> _settingDict = new ();

        private void Start()
        {
            musicButton.onValueChanged.AddListener(ToggleMusic);
            soundButton.onValueChanged.AddListener(ToggleSound);
        }

        private void OnEnable()
        {
            UpdateButtonState(Constant.Settings.MusicEnabled);
            UpdateButtonState(Constant.Settings.SoundEnabled);
        }

        private void UpdateButtonState(string playerPrefKey)
        {
            var enabledState = GameEntry.Setting.GetBool(playerPrefKey, true);;
            if (playerPrefKey == Constant.Settings.SoundEnabled)
            {
                soundButton.value = enabledState ? 1 : 0;
            }
            else
            {
                musicButton.value = enabledState ? 1 : 0;
            }

            _settingDict[playerPrefKey] = enabledState;
        }

        private void ToggleMusic(float arg0)
        {
            var value = (int)arg0 == 1;
            ToggleState(Constant.Settings.MusicEnabled, value);
        }

        private void ToggleSound(float arg0)
        {
            var value = (int)arg0 == 1;
            ToggleState(Constant.Settings.SoundEnabled, value);
        }

        private void ToggleState(string key, bool enable)
        {
            Debug.Log($"Toggle state: {key}, {enable}");
            if (GameEntry.Setting.GetBool(key, true) != enable)
            {
                UpdateMixer(key == Constant.Settings.SoundEnabled ? soundParameter : musicParameter, enable);
                SoundBase.instance.PlaySound(SoundBase.instance.click);
                GameEntry.Setting.SetBool(key, enable);
                GameEntry.Setting.Save();
            }
        }

        private void UpdateMixer(string volumeParameter, bool enabledState)
        {
            float volumeValue = enabledState ? 0 : -80;
            mixer.SetFloat(volumeParameter, volumeValue);
        }
    }
}