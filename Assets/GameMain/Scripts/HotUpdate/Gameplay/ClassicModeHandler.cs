using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class ClassicModeHandler : BaseModeHandler
    {
        public Image rhombusImage;

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
    }
}