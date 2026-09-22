using System;
using System.Collections;
using System.IO;
using BlockPuzzleGameToolkit.Scripts;
using BlockPuzzleGameToolkit.Scripts.Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using Random = UnityEngine.Random;

namespace Quester
{
    public class PicturePiece : MonoBehaviour
    {
        public Image bg;
        public Image image;

        public void SetSprite(Sprite sprite)
        {
            image.sprite = sprite;
            image.color = Color.white;
        }

        public void FullImage(int width, int height)
        {
            image.rectTransform.DOSizeDelta(new Vector2(width, height), Random.Range(0.5f, 1f));
        }
    }
}