using System;
using System.Collections;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.GUI;
using Quester;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class ClassicModeHandler : BaseModeHandler
    {
        private BackgroundChanger  _backgroundChanger;
        private bool _newRecordShown = false;
        private int _betterIndex = -1;
        private List<BetterItem> _betterList = new()
        {
            new BetterItem(200, 50),
            new BetterItem(300, 60),
            new BetterItem(500, 70),
            new BetterItem(800, 80),
            new BetterItem(1500, 85),
            new BetterItem(3000, 90),
            new BetterItem(5000, 91),
            new BetterItem(8000, 92),
            new BetterItem(10000, 93),
            new BetterItem(15000, 94),
            new BetterItem(20000, 95),
            new BetterItem(50000, 96),
            new BetterItem(100000, 97),
            new BetterItem(200000, 98),
            new BetterItem(500000, 99),
            new BetterItem(1000000, 100)
        };

        private void Start()
        {
            _backgroundChanger = FindFirstObjectByType<BackgroundChanger>();
        }

        protected override void LoadScores()
        {
            // Load current score from game state using the proper mode-specific loading
            var state = GameState.Load(EGameMode.Classic) as ClassicGameState;
            if (state != null)
            {
                score = state.score;
                scoreText.text = score.ToString();
            }
            else
            {
                score = 0;
                scoreText.text = "0";
            }
            
            bestScore = UserDataManager.Instance.ClassicBestScore;
            bestScore = bestScore > score ? bestScore : score;
            bestScoreText.text = bestScore.ToString();
        }

        protected override void SaveGameState()
        {
            var fieldManager = _levelManager.GetFieldManager();
            if (fieldManager != null)
            {
                var state = new ClassicGameState
                {
                    score = score,
                    bestScore = bestScore > score ? bestScore : score,
                    gameMode = EGameMode.Classic,
                    gameStatus = EventManager.GameStatus
                };
                GameState.Save(state, fieldManager);
            }
        }

        protected override void DeleteGameState()
        {
            GameState.Delete(EGameMode.Classic);
        }

        public override void OnLose()
        {
            if (score > bestScore)
            {
                UserDataManager.Instance.SetClassicBestScore(score);
                if (score >= 700)
                {
                    UserDataManager.Instance.SetAdventureState(1);
                }
            }

            base.OnLose();
        }

        protected override void ShowBanner(int newScore, bool isNewRecord)
        {
            StartCoroutine(ShowBannerCo(newScore, isNewRecord));
        }

        private IEnumerator ShowBannerCo(int newScore, bool isNewRecord)
        {
            yield return new WaitForSeconds(0.2f);
            var recordShown = false;
            if (isNewRecord)
            {
                recordShown = ShowNewRecord();
            }
            if (!recordShown)
            {
                ShowBetterBanner(newScore);
            }
        }
        
        private bool ShowNewRecord()
        {
            if (!_newRecordShown && UserDataManager.Instance.ClassicBestScore > 0)
            {
                // 每局游戏只显示一次新纪录
                _newRecordShown = true;
                GameEntry.UI.OpenUIForm(UIFormId.NewRecordBanner);
                _backgroundChanger?.ChangeBackground();
                return true;
            }
            return false;
        }

        private void ShowBetterBanner(int newScore)
        {
            var index = GetBetterIndex(newScore);
            if (index > _betterIndex)
            {
                _betterIndex = index;
                GameEntry.UI.OpenUIForm(UIFormId.BetterBanner, _betterList[index].Percent);
                _backgroundChanger?.ChangeBackground();
            }
        }

        private int GetBetterIndex(int newScore)
        {
            int index = -1;
            for (var i = 0; i < _betterList.Count; i++)
            {
                if (newScore >= _betterList[i].Score)
                {
                    index = i;
                }
            }
            return index;
        }

        private class BetterItem
        {
            private int _score;
            private int _percent;

            public int Score => _score;

            public int Percent => _percent;

            public BetterItem(int score, int percent)
            {
                _score = score;
                _percent = percent;
            }
        }
    }
}