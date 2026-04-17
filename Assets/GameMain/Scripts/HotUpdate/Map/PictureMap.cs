using System;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.Popups;
using DG.Tweening;
using Quester;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Map
{
    public class PictureMap : MonoBehaviour
    {
        public TextMeshProUGUI levelText;
        public CustomButton levelButton;
        public CustomButton backButton;
        public CustomButton collectionButton;
        public TextMeshProUGUI completedText;
        
        private PictureComponent _pictureComponent;
        private int _currentLevel = 0;

        private void Awake()
        {
            _pictureComponent = GetComponentInChildren<PictureComponent>();
            backButton.onClick.AddListener(Back);
            levelButton.onClick.AddListener(PlayGame);
            collectionButton.onClick.AddListener(OpenCollection);
            _currentLevel = UserDataManager.Instance.Level;
            collectionButton.gameObject.SetActive(UserDataManager.Instance.PictureList.Count > 0);
        }

        private void OnEnable()
        {
            levelText.text = GameEntry.Localization.GetString("#level_n", UserDataManager.Instance.Level);
            var seasonCompleted = UserDataManager.Instance.Level > PictureComponent.MaxLevel;
            levelButton.gameObject.SetActive(!seasonCompleted);
            completedText.gameObject.SetActive(seasonCompleted);
            if (UserDataManager.Instance.Level > _currentLevel)
            {
                _pictureComponent.ShowPicture(_currentLevel, PictureCompleted);
            }
        }

        private void Start()
        {
            _pictureComponent.ShowPicture(UserDataManager.Instance.Level);
        }

        private void PlayGame()
        {
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
        
        private void PictureCompleted(bool completed)
        {
            backButton.gameObject.SetActive(true);
            levelButton.gameObject.SetActive(!completed);
            completedText.gameObject.SetActive(completed);

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
                };
            }
        }
    }
}