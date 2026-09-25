using System;
using System.Collections;
using System.IO;
using BlockPuzzleGameToolkit.Scripts;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Map;
using BlockPuzzleGameToolkit.Scripts.System.Haptic;
using Cysharp.Threading.Tasks;
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
        [SerializeField]
        private PictureMap pictureMap;
        [SerializeField]
        private Image pictureFrame;
        [SerializeField]
        private RectTransform itemParent;
        [SerializeField]
        private RectTransform scrollContent;
        
        public Transform unlockedPieceParent;
        public Image unlockedPiece;
        public RectTransform scrollView;
        public PicturePiece itemPrefab;
        public Image focusImage;
        public Image fullImage;
        public Sprite redHighlight;
        
        
        private SeasonShape _seasonShape;
        // private Action<bool> _callback;
        private Sprite _sourceSprite;
        private Texture2D _sourceTexture;
        private static int _rows = 15;
        private static int _columns = 11;
        private static int _maxLevel;
        
        private int _cellWidth = 60;
        private int _cellHeight = 60;
        
        private int _currentLevel;
        private PicturePiece[,] _items;
        private float _xOffset;
        private float _yOffset;
        private Vector2Int _focusedPosition;
        private Sprite _focusedSprite;
        private Color _fullImageColor = new Color32(110, 110, 110, 255);
        
        // 原始图片大小
        private int _imgWidth = 420;
        private int _imgHeight = 780;
        private int _imgOffsetY = 0;
        private int _imageItemSize = 0;
        
        private bool _initialized;

        public Sprite SourceSprite => _sourceSprite;

        public static int MaxLevel => _maxLevel;

        private void Awake()
        {
            itemPrefab.gameObject.SetActive(false);
            focusImage.gameObject.SetActive(false);
            _currentLevel = UserDataManager.Instance.Level;

            SetSeasonShape(LoadSeasonShape());
            Init();
        }
        
        private SeasonShape LoadSeasonShape()
        {
            var shapeIndex = UserDataManager.Instance.CurrentSeasonShape.ToString("D3");
            return Addressables.LoadAssetAsync<SeasonShape>($"Assets/GameMain/SeasonShapes/SeasonShape_{shapeIndex}.asset").WaitForCompletion();
        }

        private void Init()
        {
            CalcItemSize();
            LoadImage();
            InitItems();
        }

        private void SetSeasonShape(SeasonShape seasonShape)
        {
            _seasonShape = seasonShape;
            _rows = _seasonShape.Matrix.rows.GetLength(0);
            _columns = _seasonShape.Matrix.rows[0].columns.GetLength(0);
            _maxLevel = _seasonShape.MaxLevel;
        }

        private void CalcItemSize()
        {
            var size = scrollContent.rect.width / _columns;
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
            
            scrollContent.sizeDelta = new Vector2(scrollContent.sizeDelta.x, _cellHeight * _rows);
            itemParent.sizeDelta = scrollContent.sizeDelta;
            if (scrollView.rect.height > scrollContent.rect.height)
            {
                // 这里使用的 300 是 prefab 中顶部底部 UI 使用的尺寸，50 是左右边界
                var offset = (scrollView.rect.height - scrollContent.rect.height) / 2;
                scrollView.offsetMin = new Vector2(50, 300 + offset);
                scrollView.offsetMax = new Vector2(-50, -300 - offset);
            }
        }

        public void ShowLevel(int level)
        {
            StartCoroutine(ShowLevelCo(level));
        }

        private IEnumerator ShowLevelCo(int level)
        {
            _currentLevel = level;
            yield return new WaitUntil(() => _initialized);
            if (_currentLevel <= _seasonShape.MaxLevel)
            {
                UpdateSprite();
                UpdateFocusPosition();
            }
            else
            {
                focusImage.gameObject.SetActive(false);
            }
        }
        
        private async UniTask InitItems()
        {
            _items = new PicturePiece[_rows, _columns];

            int index = 1;
            int count = 0;

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (!_seasonShape.Matrix.rows[i].columns[j])
                        continue;

                    var item = Instantiate(itemPrefab, itemParent);
                    item.gameObject.SetActive(false);
                    item.transform.localPosition = GetCellPosition(i, j);

                    var rect = item.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(
                        _cellWidth,
                        _cellHeight
                    );

                    item.name = $"item_{index}";
                    _items[i, j] = item;

                    index++;
                    count++;

                    // 每处理 10 个，交给下一帧
                    if (count >= 10)
                    {
                        count = 0;
                        await UniTask.Yield();
                    }
                }
            }
            // pictureFrame.transform.SetAsLastSibling();
            _initialized = true;
        }

        private void UpdateFocusPosition()
        {
            var focusRow = _focusedPosition.x;
            var focusColumn = _focusedPosition.y;
            focusImage.transform.localPosition = GetCellPosition(focusRow, focusColumn);
            focusImage.transform.SetAsLastSibling();
            focusImage.gameObject.SetActive(UserDataManager.Instance.Level <= _maxLevel);
        }

        public void ShowNextLevel(int nextLevel, Action<bool> callback)
        {
            unlockedPiece.gameObject.SetActive(true);
            // 重置碎片起始位置
            unlockedPiece.transform.SetParent(unlockedPieceParent);
            unlockedPiece.transform.localPosition = Vector3.zero;
            unlockedPiece.transform.localScale = Vector3.one;
            unlockedPiece.sprite = GetFocusedSprite();
            unlockedPiece.transform.SetParent(transform);
            
            var currentItem = _items[_focusedPosition.x, _focusedPosition.y];
            focusImage.gameObject.SetActive(false);
                
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.5f);
            sequence.AppendInterval(0.5f);
            var targetPos = GetCellPosition(_focusedPosition.x, _focusedPosition.y);
            sequence.Append(unlockedPiece.transform.DOLocalMove(targetPos, 0.3f));
            sequence.Join(unlockedPiece.transform.DOScale(new Vector3(0.3f, 0.3f, 0.3f), 0.3f));
            sequence.AppendCallback(() =>
            {
                unlockedPiece.gameObject.SetActive(false);
                unlockedPiece.transform.localPosition = Vector3.zero;
                currentItem.SetSprite(GetFocusedSprite());
                GameEntry.Sound.PlaySound(SoundId.Fragment);
                HapticFeedback.TriggerHapticFeedback(HapticFeedback.HapticForce.Heavy);
                Shake();
            });
            // sequence.Append(currentItem.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f));
            // sequence.Append(currentItem.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
            if (nextLevel <= _seasonShape.MaxLevel)
            {
                sequence.AppendCallback(() =>
                {
                    focusImage.gameObject.SetActive(true);
                    var nextFocused = GetFocusedPositionByLevel(nextLevel);
                    focusImage.transform.localPosition = GetCellPosition(nextFocused.x, nextFocused.y);
                    focusImage.transform.localScale = Vector3.zero;
                    _focusedPosition = nextFocused;
                });
                
                sequence.Append(focusImage.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f));
                sequence.Append(focusImage.transform.DOScale(Vector3.one, 0.2f));
            }
            sequence.OnComplete(() =>
            {
                PlayFullPictureAnim(callback);
            });
        }

        private Vector2Int GetFocusedPositionByLevel(int level)
        {
            int levelIndex = 0;
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (_seasonShape.Matrix.rows[i].columns[j])
                    {
                        levelIndex++;
                    }

                    if (levelIndex >= level)
                    {
                        return new Vector2Int(i, j);
                    }
                }
            }
            return Vector2Int.zero;
        }
        

        private void PlayFullPictureAnim(Action<bool> callback)
        {
            if (UserDataManager.Instance.Level > _maxLevel)
            {
                fullImage.color = Color.white;
                for (int i = 0; i < _rows; i++)
                {
                    for (int j = 0; j < _columns; j++)
                    {
                        var item = _items[i, j];
                        if (item != null)
                        {
                            item.FullImage(_cellWidth, _cellHeight);
                        }
                    }
                }
        
                CoroutineRunner.Instance.Delay(1f, () =>
                {
                    callback?.Invoke(true);
                });
            }
            else
            {
                callback?.Invoke(false);
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
                        item.gameObject.SetActive(true);
                        if (levelIndex < _currentLevel)
                        {
                            item.SetSprite(GetSprite(GetCroppedX(j), GetCroppedY(i), _imageItemSize, _imageItemSize));
                        }
                        else
                        {
                            if (levelIndex == _currentLevel)
                            {
                                _focusedPosition = new Vector2Int(i, j);
                            }
                            else
                            {
                                if (pictureMap.IsHardLevel(levelIndex))
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
            fullImage.DOColor(_fullImageColor, 2f);
        }

        private int GetCroppedX(int column)
        {
            return column * _imageItemSize;
        }
        
        private int GetCroppedY(int row)
        {
            return _sourceTexture.height - (row + 1) * _imageItemSize - _imgOffsetY;
        }

        public Sprite GetFocusedSprite()
        {
            return GetSprite(GetCroppedX(_focusedPosition.y), GetCroppedY(_focusedPosition.x), _imageItemSize, _imageItemSize);
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
        
        public void Shake()
        {
            transform.DOShakePosition(
                duration: 0.3f,
                strength: 50f,
                vibrato: 20,
                randomness: 90f,
                snapping: false,
                fadeOut: true
            );
        }
    }
}