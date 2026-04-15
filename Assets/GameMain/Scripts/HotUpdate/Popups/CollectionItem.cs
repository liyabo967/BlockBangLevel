using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Quester;
using Quester;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Quester.UI
{
    public class CollectionItem : MonoBehaviour
    {
        public Image image;
        
        private int _year;
        private int _week;
        
        private void Start()
        {
            var assetAsync = Addressables.LoadAssetAsync<Sprite>($"Assets/GameMain/Arts/Pictures/{_year}/{_week}.jpg");
            assetAsync.Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    image.sprite = handle.Result;
                }
                else
                {
                    Debug.LogError(handle.OperationException);
                }
            };
        }

        public void SetData(int year, int week)
        {
            _year = year;
            _week = week;
        }
    }
}