using System;
using System.Collections;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Data;
using DG.Tweening;
using Quester;
using Quester;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Quester.UI
{
    public class CollectionDlg : UGuiForm
    {
        public RectTransform contentTransform;
        public CollectionItem itemPrefab;
        public Transform parent;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            var pictureList = UserDataManager.Instance.PictureList;
            var startIndex = parent.childCount - 1;
            for (int i = startIndex; i < pictureList.Count; i++)
            {
                var pictureData = pictureList[i];
                var data = pictureData.Split("_");
                var item =  Instantiate(itemPrefab, parent);
                item.gameObject.SetActive(true);
                item.SetData(int.Parse(data[0]), int.Parse(data[1]));
            }
            // contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, 60);
            int itemCount = parent.childCount - 1;
            var height = Math.Max(330, 330 * Math.Ceiling(itemCount / 3.0));
            contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, (int)height);
        }
    }
}