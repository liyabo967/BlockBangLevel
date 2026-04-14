using System;
using System.Collections;
using System.IO;
using BlockPuzzleGameToolkit.Scripts.Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using Random = UnityEngine.Random;

namespace Quester
{
    public class PictureComponent : MonoBehaviour
    {
        public Image parent;
        public Image itemPrefab;
        public Image focusImage;

        private Action<bool> _callback;
        private Sprite _sourceSprite;
        private Texture2D _sourceTexture;
        private static int _rows = 11;
        private static int _columns = 8;
        private static int _maxLevel = _rows * _columns;
        
        private int _cellWidth = 60;
        private int _cellHeight = 60;
        
        private int _currentLevel = 1;
        private Image[,] _items;
        private float _xOffset;
        private float _yOffset;
        private Color _itemOriginColor;
        
        // 原始图片大小
        private int _imgWidth = 420;
        private int _imgHeight = 780;
        private int _imgOffsetX = 0;
        private int _imgOffsetY = 0;
        private int _imageItemSize = 0;

        public Sprite SourceSprite => _sourceSprite;

        public static int MaxLevel => _maxLevel;

        private void Awake()
        {
            _maxLevel = _rows * _columns;
            focusImage.gameObject.SetActive(false);
            ColorUtility.TryParseHtmlString("#44499A", out _itemOriginColor);
        }

        private void Start()
        {
            InitSize();
        }

        private void InitSize()
        {
            var padding = 50;
            var rootRect = GetComponent<RectTransform>().rect;
            var ratio = _columns * 1.0f / _rows;
            if (rootRect.width / rootRect.height > ratio)
            {
                // 以高度为准
                var size = (rootRect.height - padding * 2) / _rows;
                _cellWidth = (int)size;
                _cellHeight = _cellWidth;
            }
            else
            {
                var size = (rootRect.width - padding * 2) / _columns;
                _cellWidth = (int)size;
                _cellHeight = _cellWidth;
            }
            
            var pictureWidth = _columns * _cellWidth;
            var pictureHeight = _rows * _cellHeight;
            RectTransform rt = parent.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(pictureWidth, pictureHeight);
            _xOffset = _cellWidth * 0.5f - pictureWidth * 0.5f;
            _yOffset = _cellHeight * 0.5f - pictureHeight * 0.5f;
            focusImage.GetComponent<RectTransform>().sizeDelta = new Vector2(_cellWidth, _cellHeight);
            // 根据 prefab 大小为 60 的情况进行字体大小调整
            itemPrefab.GetComponentInChildren<Text>().fontSize = (int)(_cellWidth * 1.0f / 60 * 25);
        }

        private void CalcOffset()
        {
            var ratio = _columns * 1.0f / _rows;
            // 计算图片偏移
            if (_imgWidth * 1.0f / _imgHeight > ratio)
            {
                _imgOffsetX = (int)((_imgWidth - _imgHeight * ratio) / 2);
                _imgOffsetY = 0;
                _imageItemSize = _imgHeight / _rows;
            }
            else
            {
                _imgOffsetY = (int)((_imgHeight - _imgWidth / ratio) / 2);
                _imgOffsetX = 0;
                _imageItemSize = _imgWidth / _columns;
            }
        }

        public void ShowPicture(int level, Action<bool> onFinish = null)
        {
            StartCoroutine(ShowPictureCo(level, onFinish));
        }

        private IEnumerator ShowPictureCo(int level, Action<bool> onFinish = null)
        {
            _callback = onFinish;
            _currentLevel = level;
            if (_items == null)
            {
                yield return null;
                _items =  new Image[_rows, _columns];
                var index = 1;
                for (int i = 0; i < _rows; i++)
                {
                    for (int j = 0; j < _columns; j++)
                    {
                        var item = Instantiate(itemPrefab, parent.transform);
                        item.gameObject.SetActive(true);
                        item.transform.localPosition = GetCellPosition(i, j);
                        item.GetComponent<RectTransform>().sizeDelta = new Vector2(_cellWidth - 2, _cellHeight - 2);
                        item.transform.GetChild(0).GetComponent<Text>().text = (i * _columns + j + 1).ToString();
                        item.transform.name = $"item_{index}";
                        _items[i, j] = item;
                        index++;
                    }

                    yield return null;
                }
                
                LoadImage();
            }
            else
            {
                UpdateSprite();
                UpdateFocusPosition();
            }
        }

        private void UpdateFocusPosition()
        {
            var focusRow = (_currentLevel - 1) / _columns;
            focusRow = Mathf.Min(focusRow, _rows - 1);
            var focusColumn = (_currentLevel - 1) % _columns;
            focusImage.transform.localPosition = GetCellPosition(focusRow, focusColumn);
            focusImage.transform.SetAsLastSibling();
            focusImage.gameObject.SetActive(true);

            if (UserDataManager.Instance.Level > _currentLevel)
            {
                var currentItem = _items[focusRow, focusColumn];
                var nextRow = focusRow ;
                var nextColumn = focusColumn + 1 >= _columns ? 0 : focusColumn + 1;
                if (nextColumn == 0)
                {
                    nextRow =  focusRow + 1;
                }
                var nextLocalPosition = GetCellPosition(nextRow, nextColumn);
                focusImage.gameObject.SetActive(false);
                
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(0.5f);
                sequence.Append(currentItem.transform.DOScale(Vector3.zero, 0.5f));
                sequence.AppendCallback(() =>
                {
                    currentItem.transform.GetChild(0).GetComponent<Text>().text = "";
                    currentItem.sprite = GetSprite(focusRow, focusColumn);
                    currentItem.color = Color.white;
                });
                sequence.Append(currentItem.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f));
                sequence.Append(currentItem.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
                if (_currentLevel < _rows * _columns)
                {
                    sequence.AppendCallback(() =>
                    {
                        focusImage.gameObject.SetActive(true);
                    });
                    sequence.Append(focusImage.transform.DOLocalMove(nextLocalPosition, 0.5f));
                }
                sequence.onComplete += PlayFullPictureAnim;
            }
            else
            {
                focusImage.gameObject.SetActive(UserDataManager.Instance.Level <= _maxLevel);
            }
        }
        

        private void PlayFullPictureAnim()
        {
            if (UserDataManager.Instance.Level > _maxLevel)
            {
                var maxDuration = 1f;
                for (int i = 0; i < _rows; i++)
                {
                    for (int j = 0; j < _columns; j++)
                    {
                        var item = _items[i, j];
                        RectTransform rt = item.rectTransform;
                        rt.DOSizeDelta(new Vector2(_cellWidth, _cellHeight), Random.Range(0.5f, maxDuration));
                    }
                }

                CoroutineRunner.Instance.Delay(1f, () =>
                {
                    _callback?.Invoke(true);
                });
            }
            else
            {
                _callback?.Invoke(false);
            }
        }

        private Vector3 GetCellPosition(int row, int column)
        {
            return new Vector3(column * _cellWidth + _xOffset, row * _cellHeight + _yOffset, 0);
        }

        private void UpdateSprite()
        {
            if (_items != null)
            {
                var fullImage = _currentLevel > _maxLevel;
                for (int i = 0; i < _rows; i++)
                {
                    for (int j = 0; j < _columns; j++)
                    {
                        var item = _items[i, j];
                        if (i * _columns + j + 1 < _currentLevel)
                        {
                            item.color = Color.white;
                            item.sprite = GetSprite(i, j);
                            item.transform.GetChild(0).GetComponent<Text>().text = "";
                            if (fullImage)
                            {
                                item.rectTransform.sizeDelta = new Vector2(_cellWidth, _cellHeight);
                            }
                        }
                    }
                }
            }
        }

        private void LoadImage()
        {
            var filePath = $"{Application.persistentDataPath}/pictures/{TimeManager.SeasonTime.year}/{TimeManager.SeasonTime.week}.jpg";
            Debug.Log(filePath);
            if (File.Exists(filePath))
            {
                // 异步加载
                StartCoroutine(SpriteLoader.LoadFromFileAsync(filePath, (sprite) =>
                {
                    LoadImageCompleted(sprite);
                }));
            }
            else
            {
                // 从本地包加载
                var assetAsync = Addressables.LoadAssetAsync<Sprite>($"Assets/GameMain/Sprites/pictures/0.jpg");
                assetAsync.Completed += handle =>
                {
                    Log.Info($"LoadAssetAsync Completed: {handle.Status}");
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        LoadImageCompleted(handle.Result);
                    }
                    else
                    {
                        if (handle.OperationException != null)
                        {
                            Log.Error($"异常信息: {handle.OperationException.Message}");
                            Log.Error($"堆栈跟踪: {handle.OperationException.StackTrace}");
                        }
                    }
                };
            }
        }

        private void LoadImageCompleted(Sprite sprite)
        {
            _sourceSprite = sprite;
            _sourceTexture = _sourceSprite.texture;
            _imgWidth =  _sourceSprite.texture.width;
            _imgHeight = _sourceSprite.texture.height;
            Debug.Log($"LoadImageCompleted: {_imgWidth}");
            Debug.Log($"LoadImageCompleted: {_imgHeight}");
            CalcOffset();
            UpdateSprite();
            UpdateFocusPosition();
        }

        private Sprite GetSprite(int row, int column)
        {
            // 2️⃣ 从原纹理中取像素
            Color[] pixels = _sourceTexture.GetPixels(
                column * _imageItemSize + _imgOffsetX,
                row * _imageItemSize + _imgOffsetY,
                _imageItemSize,
                _imageItemSize
            );

            // 3️⃣ 创建一个新的 Texture2D 并写入像素
            Texture2D croppedTexture = new Texture2D(_imageItemSize, _imageItemSize);
            croppedTexture.SetPixels(pixels);
            croppedTexture.filterMode = FilterMode.Point;
            croppedTexture.wrapMode = TextureWrapMode.Clamp;
            croppedTexture.Apply();

            // 4️⃣ 创建新的 Sprite
            Sprite newSprite = Sprite.Create(
                croppedTexture,
                new Rect(0, 0, croppedTexture.width, croppedTexture.height),
                new Vector2(0.5f, 0.5f) // pivot 中心点
            );
            return newSprite;
        }
    }
}