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

using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using DG.Tweening;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay.FX
{
    public class BonusAnimation : MonoBehaviour
    {
        public AnimationCurve curveX;
        public AnimationCurve curveY;
        public Vector2 targetPos;
        private Vector3 originPos;
        private Bonus bonusItem;
        public GameObject sparklePrefab;

        public TweenCallback<BonusItemTemplate> OnFinish;

        private void Awake()
        {
            bonusItem = GetComponent<Bonus>();
        }

        private void OnEnable()
        {
            transform.localScale = Vector3.one;
        }

        public void MoveTo(int index)
        {
            // Debug.LogError($"move to: {index}");
            transform.localScale = Vector3.one * 2f;
            originPos = transform.position;
            Vector2 direction = ((Vector2)transform.position - targetPos).normalized;
            Vector3 backwardPos = transform.position + (Vector3)(direction * 0.5f);
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(backwardPos, 0.2f).SetEase(Ease.OutQuad));
            sequence.Join(transform.DOScale(new Vector3(5f, 5f, 5f), 0.2f));
            sequence.AppendInterval(0.1f + index * 0.05f);
            sequence.Append(transform.DOMove(new Vector3(targetPos.x, targetPos.y, transform.position.z), 0.2f).SetEase(Ease.Linear));
            // sequence.Join(transform.DORotate(new Vector3(0, 0, 90), 0.3f));
            sequence.OnComplete(Finish);
        }

        private void Finish()
        {
            PoolObject.GetObject(sparklePrefab).transform.position = transform.position;
            OnFinish(bonusItem.bonusItemTemplate);
            PoolObject.Return(gameObject);
        }

        public void Fill(BonusItemTemplate getBonusItem)
        {
            bonusItem.FillIcon(getBonusItem);
        }
    }
}