using System;
using BlockPuzzleGameToolkit.Scripts.Data;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.Popups;
using GameMain.Scripts.HotUpdate.Base.Ads;
using Quester;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData
{
    // [CreateAssetMenu(fileName = "ClassicStateHandler", menuName = "BlockPuzzleGameToolkit/Levels/ClassicStateHandler")]
    public class ClassicLevelStateHandler : LevelStateHandler
    {

        private bool OnPreFailCondition(LevelManager levelManager)
        {
            if (levelManager.ClassicModeHandler.IsNewRecord)
            {
                return false;
            }

            if (levelManager.ClassicModeHandler.IsRewarded)
            {
                return false;
            }

            if (levelManager.ClassicModeHandler.score < 500)
            {
                return false;
            }

            if (!AdManager.Instance.IsReady(AdType.RewardedVideo))
            {
                return false;
            }

            var rewardInterval = DateTimeOffset.Now.ToUnixTimeSeconds() - UserDataManager.Instance.FirstLaunchTimestamp;
            if (rewardInterval < 3600)
            {
                return false;
            }
            
            return true;
        }
        
        private protected override void HandlePreFailed(LevelManager levelManager)
        {
            var level = levelManager.GetCurrentLevel();
            var preFailedPopup = level.levelType.preFailedPopup;
            levelManager.timerManager?.StopTimer();

            levelManager.StartCoroutine(levelManager.EndAnimations(() =>
            {
                if (preFailedPopup != UIFormId.Undefined && OnPreFailCondition(levelManager))
                {
                    GameEntry.UI.OpenUIForm(preFailedPopup, this);
                    levelManager.ClearEmptyCells();
                    // MenuManager.instance.ShowPopup(preFailedPopup, levelManager.ClearEmptyCells, result =>
                    // {
                    //     if (result == EPopupResult.Continue)
                    //     {
                    //         levelManager.cellDeck.UpdateCellDeckAfterFail();
                    //         EventManager.GameStatus = EGameState.Playing;
                    //     }
                    // });
                }
                else
                {
                    EventManager.GameStatus = EGameState.Failed;
                }
            }));
        }
    }
}