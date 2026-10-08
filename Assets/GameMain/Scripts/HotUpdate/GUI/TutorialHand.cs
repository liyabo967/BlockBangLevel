using System;
using DG.Tweening;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.GUI
{
    public class TutorialHand : MonoBehaviour
    {
        [SerializeField] private GameObject circle;


        private void OnEnable()
        {
            PlayCircleAnim();
        }

        private void OnDisable()
        {
            circle.transform.DOKill();
        }

        private void PlayCircleAnim()
        {
            var sequence = DOTween.Sequence();
            // sequence.Append(transform.DORotate(new Vector3(10, 20, 0), 0.5f));
            // sequence.Append(transform.DORotate(new Vector3(0, 0, 0), 0.5f));
            sequence.AppendInterval(1.5f);
            sequence.Append(
                circle.transform.DOScale(1.3f, 0.5f));
            sequence.Append(
                circle.transform.DOScale(1f, 0.3f));
            sequence.AppendInterval(0.5f);
            sequence.SetLoops(-1);
        }
    }
}