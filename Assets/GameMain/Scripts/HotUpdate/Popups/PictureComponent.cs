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
    public class PictureComponent : MonoBehaviour
    {
        public RectTransform scrollView;
        public PicturePiece itemPrefab;
        public Image focusImage;
        public Image fullImage;
        public Sprite redHighlight;

        private RectTransform _parent;
        private SeasonShape _seasonShape;
        private Action<bool> _callback;
        private Sprite _sourceSprite;
        private Texture2D _sourceTexture;
        private static int _rows = 15;
        private static int _columns = 11;
        private static int _maxLevel = _rows * _columns;
        
        private int _cellWidth = 60;
        private int _cellHeight = 60;
        
        private int _currentLevel = 1;
        private PicturePiece[,] _items;
        private float _xOffset;
        private float _yOffset;
        private int _focusRow;
        private int _focusColumn;
        private Color _fullImageColor = new Color32(70, 70, 70, 255);
        
        
        // 原始图片大小
        private int _imgWidth = 420;
        private int _imgHeight = 780;
        private int _imgOffsetY = 0;
        
        private int _imageItemSize = 0;

        public Sprite SourceSprite => _sourceSprite;

        public static int MaxLevel => _maxLevel;

        private void Awake()
        {
            _parent = GetComponent<RectTransform>();
            itemPrefab.gameObject.SetActive(false);
            _maxLevel = _rows * _columns;
            focusImage.gameObject.SetActive(false);
        }

        private void Start()
        {
            LoadSeasonShape();
            InitSize();
        }

        private void LoadSeasonShape()
        {
            var shapeIndex = UserDataManager.Instance.CurrentSeasonShape.ToString("D3");
            shapeIndex = "001";
            _seasonShape = Addressables.LoadAssetAsync<SeasonShape>($"Assets/GameMain/SeasonShapes/SeasonShape_{shapeIndex}.asset").WaitForCompletion();
            _rows = _seasonShape.Matrix.rows.GetLength(0);
            _columns = _seasonShape.Matrix.rows[0].columns.GetLength(0);
        }

        private void InitSize()
        {
            var size = _parent.rect.width / _columns;
            _cellWidth = (int)size;
            _cellHeight = _cellWidth;
            
            // Debug.LogError("rootRect.width: " + rootRect.width);
            // Debug.LogError("rootRect.height: " + rootRect.height);
            // Debug.LogError("_cellWidth: " + _cellWidth);
            // Debug.LogError("_cellHeight: " + _cellHeight);
            
            // var pictureWidth = _columns * _cellWidth;
            // var pictureHeight = _rows * _cellHeight;
            // RectTransform rt = parent;
            // rt.sizeDelta = new Vector2(pictureWidth, pictureHeight);
            _xOffset = _cellWidth * 0.5f;
            _yOffset = _cellHeight * 0.5f;
            focusImage.GetComponent<RectTransform>().sizeDelta = new Vector2(_cellWidth * 1.0f, _cellHeight * 1.0f);
            
            _parent.sizeDelta = new Vector2(_parent.sizeDelta.x, _cellHeight * _rows);
            if (scrollView.rect.height > _parent.rect.height)
            {
                // 这里使用的 300 是 prefab 中顶部底部 UI 使用的尺寸，50 是左右边界
                var offset = (scrollView.rect.height - _parent.rect.height) / 2;
                scrollView.offsetMin = new Vector2(50, 300 + offset);
                scrollView.offsetMax = new Vector2(-50, -300 - offset);
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
                _items =  new PicturePiece[_rows, _columns];
                var index = 1;
                var padding = _cellWidth * 0.1f;
                padding = 0;
                for (int i = 0; i < _rows; i++)
                {
                    for (int j = 0; j < _columns; j++)
                    {
                        if (!_seasonShape.Matrix.rows[i].columns[j])
                        {
                            continue;
                        }
                        var item = Instantiate(itemPrefab, _parent);
                        item.gameObject.SetActive(true);
                        item.transform.localPosition = GetCellPosition(i, j);
                        
                        item.GetComponent<RectTransform>().sizeDelta = new Vector2(_cellWidth - padding, _cellHeight - padding);
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
            var focusRow = _focusRow;
            var focusColumn = _focusColumn;
            focusImage.transform.localPosition = GetCellPosition(focusRow, focusColumn);
            focusImage.transform.SetAsLastSibling();
            focusImage.gameObject.SetActive(true);

            if (UserDataManager.Instance.Level > _currentLevel)
            {
                var currentItem = _items[focusRow, focusColumn];
                // var nextRow = focusRow ;
                // var nextColumn = focusColumn + 1 >= _columns ? 0 : focusColumn + 1;
                // if (nextColumn == 0)
                // {
                //     nextRow =  focusRow + 1;
                // }
                var nextLocalPosition = GetNextLevelPosition(_currentLevel);
                focusImage.gameObject.SetActive(false);
                
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(0.5f);
                sequence.Append(currentItem.transform.DOScale(Vector3.zero, 0.5f));
                sequence.AppendCallback(() =>
                {
                    currentItem.SetSprite(GetSprite(GetCroppedX(focusColumn), GetCroppedY(focusRow), _imageItemSize, _imageItemSize));
                    GameEntry.Sound.PlaySound(SoundId.Fragment);
                });
                sequence.Append(currentItem.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f));
                sequence.Append(currentItem.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
                if (_currentLevel < _seasonShape.MaxLevel)
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

        private Vector3 GetNextLevelPosition(int currentLevel)
        {
            if (currentLevel == _seasonShape.MaxLevel)
            {
                return Vector3.zero;
            }

            int levelIndex = 0;
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (_seasonShape.Matrix.rows[i].columns[j])
                    {
                        levelIndex++;
                    }

                    if (levelIndex > currentLevel)
                    {
                        return GetCellPosition(i, j);
                    }
                }
            }
            return Vector3.zero;
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
                        item.FullImage(_cellWidth, _cellHeight);
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
            return new Vector3(column * _cellWidth + _xOffset, -row * _cellHeight - _yOffset, 0);
        }

        private void UpdateSprite()
        {
            if (_items != null)
            {
                int levelIndex = 0;
                
                for (int i = 0; i < _rows; i++)
                {
                    for (int j = 0; j < _columns; j++)
                    {
                        var item = _items[i, j];
                        if (!_seasonShape.Matrix.rows[i].columns[j])
                        {
                            continue;
                        }
                        levelIndex++;
                        if (levelIndex < _currentLevel)
                        {
                            item.SetSprite(GetSprite(GetCroppedX(j), GetCroppedY(i), _imageItemSize, _imageItemSize));
                        }
                        else
                        {
                            if (levelIndex == _currentLevel)
                            {
                                _focusRow = i;
                                _focusColumn = j;
                            }
                            else
                            {
                                if (levelIndex % 5 == 0)
                                {
                                    item.SetSprite(redHighlight);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadImage()
        {
            var filePath = $"{Application.persistentDataPath}/Pictures/{TimeManager.SeasonTime.year}/{TimeManager.SeasonTime.week}.jpg";
            // Debug.Log(filePath);
            if (false)
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
                var assetAsync = Addressables.LoadAssetAsync<Sprite>($"Assets/GameMain/Sprites/Pictures/0.jpg");
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
            _imageItemSize = _sourceSprite.texture.width / _columns;
            _imgOffsetY = (_imgHeight - (int)(_imgWidth * _rows * 1.0f / _columns)) / 2;
            // Debug.Log($"LoadImageCompleted: {_imgWidth}");
            // Debug.Log($"LoadImageCompleted: {_imgHeight}");
            fullImage.sprite = GetSprite(0, _imgOffsetY, _imageItemSize * _columns, _imageItemSize * _rows);
            fullImage.color = _fullImageColor;
            UpdateSprite();
            UpdateFocusPosition();
        }

        private int GetCroppedX(int column)
        {
            return column * _imageItemSize;
        }
        
        private int GetCroppedY(int row)
        {
            return _sourceTexture.height - (row + 1) * _imageItemSize - _imgOffsetY;
        }

        private Sprite GetSprite(int x, int y, int blockWidth, int blockHeight)
        {
            // 2️⃣ 从原纹理中取像素
            Color[] pixels = _sourceTexture.GetPixels(x, y, blockWidth, blockHeight);
            // 3️⃣ 创建一个新的 Texture2D 并写入像素
            Texture2D croppedTexture = new Texture2D(blockWidth, blockHeight);
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