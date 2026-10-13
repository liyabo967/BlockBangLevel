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
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.GUI
{
    public class OutlineField : MonoBehaviour
    {
        [SerializeField]
        private Image imageToFade;

        [SerializeField]
        private Image imageToFade2;
        
        private Color _targetColor;
        private Sequence imgSequence;

        public void PlayAnim(Color color)
        {
            _targetColor = color;
            imageToFade.color = color;
            imageToFade2.color = color;
            
            _targetColor.a = 0.1f;
            imageToFade.color = _targetColor;
            if (imgSequence == null)
            {
                imgSequence = DOTween.Sequence();
                imgSequence.Append(imageToFade.DOFade(1f, 1f));
                imgSequence.Append(imageToFade.DOFade(0.1f, 1f));
                imgSequence.SetLoops(-1, LoopType.Yoyo);
            }
            
            imageToFade2.gameObject.SetActive(true);
            imageToFade2.transform.localScale = Vector3.one;
            var sequence2 = DOTween.Sequence();
            sequence2.Append(imageToFade2.transform.DOScale(1.1f, 0.3f));
            sequence2.AppendInterval(0.1f);
            sequence2.Append(imageToFade2.DOFade(0f, 0.2f)).OnComplete(() =>
            {
                imageToFade2.gameObject.SetActive(false);
            });
        }

        private void OnDisable()
        {
            imgSequence.Kill();
            imgSequence = null;
        }
    }
}