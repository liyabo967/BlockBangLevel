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

using BlockPuzzleGameToolkit.Scripts.Gameplay;
using DG.Tweening;
using GameMain.Scripts.HotUpdate.Popups;
using Quester;
using TMPro;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    [ExecuteAlways]
    public class NewRecord : SuperBanner
    {
        public GameObject rays;
        public TextMeshProUGUI scoreText;

        private void OnEnable()
        {
            scoreText.text = FindFirstObjectByType<ClassicModeHandler>().bestScore.ToString();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            canvasGroup.alpha = 1;
            StartAnim();
            GameEntry.Sound.PlaySound(SoundId.SeasonSuccess);
            Invoke(nameof(Hide), 1f);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            StopAnim();
        }

        private void StartAnim()
        { 
            rays.transform.DORotate(new Vector3(0, 0, 360), 3f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
            
            scoreText.transform.localScale = new Vector3(3, 3, 3);
            scoreText.transform.DOScale(1, 0.3f);
        }

        private void StopAnim()
        {
            rays.transform.DOKill();
        }
    }
}