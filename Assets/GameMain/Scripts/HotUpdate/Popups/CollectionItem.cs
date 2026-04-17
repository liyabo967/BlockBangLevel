using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            var filePath = $"{Application.persistentDataPath}/Pictures/{_year}/{_week}.jpg";
            // Debug.Log(filePath);
            if (File.Exists(filePath))
            {
                // 异步加载
                StartCoroutine(SpriteLoader.LoadFromFileAsync(filePath, (sprite) =>
                {
                    image.sprite = sprite;
                }));
            }
            else
            {
                var assetAsync = Addressables.LoadAssetAsync<Sprite>($"Assets/GameMain/Sprites/Pictures/0.jpg");
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
        }

        public void SetData(int year, int week)
        {
            _year = year;
            _week = week;
        }
    }
}