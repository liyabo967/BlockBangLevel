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
        public GameObject emptyTips;
        public GridLayoutGroup gridLayoutGroup;

        private int _columns = 3;
        private int _rowSpacing = 20;
        // 图片的高度
        private int _imageHeight = 0;

        private void Awake()
        {
            var imageRect = itemPrefab.transform.GetChild(0).GetComponent<RectTransform>().rect;
            _imageHeight = (int)imageRect.height;
        }

        private void Start()
        {
            var rectTransform = gridLayoutGroup.GetComponent<RectTransform>();
            // 一行显示 4 个
            var width = rectTransform.rect.width / _columns;
            gridLayoutGroup.cellSize = new Vector2(width, gridLayoutGroup.cellSize.y);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            var pictureList = UserDataManager.Instance.PictureList;
            
            
            emptyTips.SetActive(pictureList.Count == 0);
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
            int rows = Mathf.CeilToInt((float)itemCount / _columns);
            var height = _imageHeight * rows + _rowSpacing * (rows - 1);
            Debug.Log($"rows: {rows}, height: {height}");
            contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, height);
        }
    }
}