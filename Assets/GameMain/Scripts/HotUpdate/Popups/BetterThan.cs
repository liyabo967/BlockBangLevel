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
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using DG.Tweening;
using GameMain.Scripts.HotUpdate.Popups;
using Quester;
using TMPro;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class BetterThan : SuperBanner
    {
        public GameObject rays;
        public GameObject contentBg;
        public TextMeshProUGUI percentText;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            canvasGroup.alpha = 1;
            StartAnim();
            int percent = (int)userData;
            percentText.text = $"{percent}%";
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
            
            contentBg.transform.localScale = new Vector3(0, 1, 1);
            contentBg.transform.DOScaleX(1, 0.5f)
                .SetEase(Ease.OutCubic);

            percentText.transform.localScale = new Vector3(3, 3, 3);
            percentText.transform.DOScale(1, 0.5f);
        }
        
        private void StopAnim()
        {
            rays.transform.DOKill();
        }
    }
}