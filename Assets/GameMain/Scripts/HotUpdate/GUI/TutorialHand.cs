using System;
using DG.Tweening;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.GUI
{
    public class TutorialHand : MonoBehaviour
    {
        [SerializeField] private GameObject circle;

        private float _xOffset = 50f;      // 向左移动的距离
        private float _yOffset = -50f;      // 向上移动的距离

        private Transform _target;
        
        private Sequence _circleSequence;
        private Sequence _handSequence;
        private Sequence _targetSequence;
        
        public void PlayAnim(Transform target)
        {
            _target = target;
            PlayCircleAnim();
            PlayHandMove();
            PlayTargetAnim();
        }

        private void OnDisable()
        {
            _circleSequence?.Kill();
            _handSequence?.Kill();
            _targetSequence?.Kill();
            
            _circleSequence = null;
            _handSequence = null;
            _targetSequence = null;
        }

        private void PlayCircleAnim()
        {
            _circleSequence = DOTween.Sequence();
            _circleSequence.AppendInterval(1.5f);
            _circleSequence.Append(
                circle.transform.DOScale(1.3f, 0.5f));
            _circleSequence.Append(
                circle.transform.DOScale(1f, 0.3f));
            _circleSequence.AppendInterval(0.5f);
            _circleSequence.SetLoops(-1);
        }

        private void PlayHandMove()
        {
            var endPos = transform.localPosition;
            var startPos = new Vector3(endPos.x + _xOffset, endPos.y + _yOffset, endPos.z);
            transform.localPosition = startPos;
            
            _handSequence = DOTween.Sequence();
            _handSequence.AppendInterval(1f);
            _handSequence.Append(transform.DOLocalMove(endPos, 1f));
            _handSequence.AppendInterval(2f);
            _handSequence.Append(transform.DOLocalMove(startPos, 0.5f));
            _handSequence.SetLoops(-1);
        }

        private void PlayTargetAnim()
        {
            _targetSequence = DOTween.Sequence();
            _targetSequence.AppendInterval(1.5f);
            _targetSequence.Append(
               _target.transform.DOScale(1.15f, 0.5f));
            _targetSequence.Append(
                _target.transform.DOScale(1f, 0.5f));
            _targetSequence.AppendInterval(2f);
            _targetSequence.SetLoops(-1);
        }
    }
}