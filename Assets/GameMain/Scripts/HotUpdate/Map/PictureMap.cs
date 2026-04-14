using System;
using BlockPuzzleGameToolkit.Scripts.Data;
using Quester;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Map
{
    public class PictureMap : MonoBehaviour
    {
        private PictureComponent _pictureComponent;

        private void Awake()
        {
            _pictureComponent = GetComponentInChildren<PictureComponent>();
        }

        private void Start()
        {
            _pictureComponent.ShowPicture(UserDataManager.Instance.Level);
        }
    }
}