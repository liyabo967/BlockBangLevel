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
using GameFramework;
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
        
        private void Start()
        {
            musicButton.onValueChanged.AddListener(ToggleMusic);
            soundButton.onValueChanged.AddListener(ToggleSound);
        }

        private void OnEnable()
        {
            UpdateButtonState(Constant.Setting.MusicGroup);
            UpdateButtonState(Constant.Setting.SoundGroup);
        }

        private void UpdateButtonState(string groupName)
        {
            var mute = GameEntry.Setting.GetBool(Utility.Text.Format(Constant.Setting.SoundGroupMuted, groupName), false);;
            if (groupName == Constant.Setting.SoundGroup)
            {
                soundButton.value = mute ? 0 : 1;
            }
            else
            {
                musicButton.value = mute ? 0 : 1;
            }
        }

        private void ToggleMusic(float arg0)
        {
            var value = (int)arg0 == 1;
            ToggleState(Constant.Setting.MusicGroup, value);
        }

        private void ToggleSound(float arg0)
        {
            var value = (int)arg0 == 1;
            ToggleState(Constant.Setting.SoundGroup, value);
        }

        private void ToggleState(string group, bool enable)
        {
            Debug.Log($"Toggle state: {group}, {enable}");
            GameEntry.Sound.PlaySound(SoundId.Click);
            GameEntry.Sound.Mute(group, !enable);
        }
    }
}