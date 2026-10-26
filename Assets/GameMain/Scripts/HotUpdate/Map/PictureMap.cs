using System;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.Popups;
using DG.Tweening;
using GameMain;
using GameMain.Scripts.HotUpdate.Services;
using Quester;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Map
{
    public class PictureMap : MonoBehaviour
    {
        [SerializeField] private TutorialHand tutorialHand;
        [SerializeField] private Sprite red;
        [SerializeField] private Sprite green;
        
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI tipsText;
        public CustomButton levelButton;
        public CustomButton backButton;
        public CustomButton collectionButton;
        public TextMeshProUGUI completedText;
        public ParticleSystem starParticle;
        
        private PictureComponent _pictureComponent;
        private SeasonShape _seasonShape;
        private int _currentLevel = 0;
        private Sequence _tutorialSequence;

        public SeasonShape SeasonShape => _seasonShape;

        private void Awake()
        {
            LoadSeasonShape();
            _pictureComponent = GetComponentInChildren<PictureComponent>();
            backButton.onClick.AddListener(Back);
            levelButton.onClick.AddListener(PlayGame);
            collectionButton.onClick.AddListener(OpenCollection);
            _currentLevel = UserDataManager.Instance.Level;
            collectionButton.gameObject.SetActive(UserDataManager.Instance.PictureList.Count > 0);
        }

        private void LoadSeasonShape()
        {
            var shapeIndex = UserDataManager.Instance.CurrentSeasonShape.ToString("D3");
            _seasonShape = Addressables.LoadAssetAsync<SeasonShape>($"Assets/GameMain/SeasonShapes/SeasonShape_{shapeIndex}.asset").WaitForCompletion();
        }

        private void OnEnable()
        {
            levelText.text = GameEntry.Localization.GetString("#level_n", UserDataManager.Instance.Level);
            var seasonCompleted = UserDataManager.Instance.Level > _seasonShape.MaxLevel;
            levelButton.gameObject.SetActive(!seasonCompleted);
            completedText.gameObject.SetActive(seasonCompleted);
            tipsText.gameObject.SetActive(!seasonCompleted);
            levelButton.GetComponent<Image>().sprite = IsHardLevel(UserDataManager.Instance.Level) ? red : green;
            starParticle.gameObject.SetActive(seasonCompleted);
            if (UserDataManager.Instance.Level > _currentLevel)
            {
                backButton.gameObject.SetActive(false);
                levelButton.gameObject.SetActive(false);
                completedText.gameObject.SetActive(false);
                _pictureComponent.ShowNextLevel(UserDataManager.Instance.Level, PictureCompleted);
                _currentLevel = UserDataManager.Instance.Level;
            }

            if (UserDataManager.Instance.Level == 1)
            {
                PlayTutorialAnim();
            }
        }

        private void Start()
        {
            _pictureComponent.ShowLevel(UserDataManager.Instance.Level);
        }
        
        public bool IsHardLevel(int level)
        {
            return level % 5 == 0 || level == _seasonShape.MaxLevel;;
        }

        private void PlayGame()
        {
            tutorialHand.gameObject.SetActive(false);
            SceneLoader.instance.StartGameScene(UserDataManager.Instance.Level);
        }

        private void Back()
        {
            SceneLoader.instance.GoMain();
        }

        private void OpenCollection()
        {
            GameEntry.UI.OpenUIForm(UIFormId.CollectionDlg);
        }

        private void PlayTutorialAnim()
        {
            Debug.Log("PlayTutorialAnim: " + (tutorialHand == null));
            tutorialHand.gameObject.SetActive(true);
            tutorialHand.PlayAnim(levelButton.transform);
        }
        
        private void PictureCompleted(bool completed)
        {
            backButton.gameObject.SetActive(true);
            levelButton.gameObject.SetActive(!completed);
            
            if (completed)
            {
                collectionButton.gameObject.SetActive(true);
                GameObject go = new GameObject("FullImage");
                Image img = go.AddComponent<Image>();
                img.sprite = _pictureComponent.SourceSprite;
                go.transform.SetParent(_pictureComponent.transform, false);
                RectTransform imgRectTransform = img.rectTransform;
                imgRectTransform.sizeDelta = new Vector2(420, 780);
                imgRectTransform.anchoredPosition = Vector2.zero;
            
                img.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 1f);
                img.transform.DOMove(collectionButton.transform.position, 1f).onComplete += () =>
                {
                    img.gameObject.SetActive(false);
                    completedText.gameObject.SetActive(true);
                };
                GameEntry.Sound.PlaySound(SoundId.SeasonSuccess);
            }
            
            if (_currentLevel == 6)
            {
                OpenRateDlg(0.2f);
            }
        }

        private void OpenRateDlg(float delay)
        {
            CoroutineRunner.Instance.Delay(delay, () =>
            {
                MobileReviewManager.Instance.RequestReview(EGameMode.Adventure);
                // GameEntry.UI.OpenUIForm(UIFormId.RateDlg);
            });
        }

        private void OnDisable()
        {
            _tutorialSequence?.Kill();
            _tutorialSequence = null;
        }
    }
}