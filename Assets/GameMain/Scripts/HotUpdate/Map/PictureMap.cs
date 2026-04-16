using System;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.Popups;
using Quester;
using TMPro;
using UnityEngine;

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
        }

        private void OnEnable()
        {
            levelText.text = GameEntry.Localization.GetString("#level_n", UserDataManager.Instance.Level);
            var seasonCompleted = UserDataManager.Instance.Level > PictureComponent.MaxLevel;
            levelButton.gameObject.SetActive(!seasonCompleted);
            completedText.gameObject.SetActive(seasonCompleted);
            if (UserDataManager.Instance.Level > _currentLevel)
            {
                _pictureComponent.ShowPicture(_currentLevel);
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
    }
}