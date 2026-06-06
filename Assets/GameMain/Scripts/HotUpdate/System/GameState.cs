using System;
using System.IO;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using Newtonsoft.Json;
using Quester;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.System
{
    [Serializable]
    public abstract class GameState
    {
        public EGameState gameStatus;
        public EGameMode gameMode;
        public int score;
        public LevelRowSaveData[] levelRows;
        public DateTime quitTime;
        public int bestScore;

        public static void Save(GameState state, FieldManager field)
        {
            if (state == null) return;
            
            if (field != null)
            {
                var cells = field.GetAllCells();
                state.levelRows = new LevelRowSaveData[cells.GetLength(0)];
                
                for (var i = 0; i < cells.GetLength(0); i++)
                {
                    state.levelRows[i] = new LevelRowSaveData(cells.GetLength(1));
                    for (var j = 0; j < cells.GetLength(1); j++)
                    {
                        if (cells[i, j].item != null && !cells[i, j].IsEmpty())
                        {
                            if (cells[i, j].item.itemTemplate == null)
                            {
                                Debug.LogError($"GameState SaveData: {i},{j}, null template");
                                continue;
                            }
                            var cellData = new CellSaveData();
                            state.levelRows[i].cells[j] = cellData;
                            cellData.isFilled = true;
                            cellData.itemTemplateId = cells[i, j].item.itemTemplate.templateId;
                            cellData.hasBonusItem = cells[i, j].HasBonusItem();
                            cellData.isDisabled = cells[i, j].IsDisabled();
                        }
                    }
                }
            }
            
            state.quitTime = DateTime.Now;
            
            // GameStateManager.Instance.SaveState(state.gameMode, state);
            // var json = JsonConvert.SerializeObject(state);
            string key = state.gameMode.ToString();
            GameEntry.Storage.Save(key, state);
            UserDataManager.Instance.SetLastPlayedMode(state.gameMode.ToString());
        }

        public static GameState Load(EGameMode gameMode)
        {
            string key = gameMode.ToString();
            GameState state = null;
            switch (gameMode)
            {
                case EGameMode.Classic:
                    state = GameEntry.Storage.Load<ClassicGameState>(key);
                    break;
                case EGameMode.Timed:
                    state = GameEntry.Storage.Load<TimedGameState>(key);
                    break;
            }
            return state;
        }

        public static GameState Load()
        {
            // Legacy loading for backward compatibility
            if (PlayerPrefs.HasKey("GameState"))
            {
                var json = PlayerPrefs.GetString("GameState");
                var tempState = JsonUtility.FromJson<LegacyGameState>(json);
                
                // Convert to appropriate state based on gameMode
                switch (tempState.gameMode)
                {
                    case EGameMode.Classic:
                        var classicState = new ClassicGameState();
                        // CopyBaseProperties(tempState, classicState);
                        return classicState;
                    case EGameMode.Timed:
                        var timedState = new TimedGameState();
                        // CopyBaseProperties(tempState, timedState);
                        timedState.remainingTime = tempState.remainingTime;
                        return timedState;
                    default:
                        return null;
                }
            }
            return null;
        }

        // private static void CopyBaseProperties(LegacyGameState source, GameState target)
        // {
        //     target.gameStatus = source.gameStatus;
        //     target.currentLevel = source.currentLevel;
        //     target.gameMode = source.gameMode;
        //     target.score = source.score;
        //     target.levelRows = source.levelRows;
        //     target.quitTime = source.quitTime;
        //     target.bestScore = source.bestScore;
        // }

        public static void Delete(EGameMode gameMode)
        {
            // PlayerPrefs.DeleteKey("GameState_" + gameMode);
            // PlayerPrefs.Save();
            GameStateManager.Instance.Delete(gameMode);
        }

        public static void Delete()
        {
            // Delete legacy key
            PlayerPrefs.DeleteKey("GameState");
            
            // Delete all game mode specific keys
            foreach (EGameMode mode in Enum.GetValues(typeof(EGameMode)))
            {
                PlayerPrefs.DeleteKey("GameState_" + mode);
            }
            
            PlayerPrefs.Save();
        }
    }

    [Serializable]
    public class LevelRowSaveData
    {
        public CellSaveData[] cells;
        public LevelRowSaveData(int size)
        {
            cells = new CellSaveData[size];
        }
    }

    [Serializable]
    public class CellSaveData
    {
        public bool isFilled;
        public int itemTemplateId;
        public bool hasBonusItem;
        public bool isDisabled;
        public bool isHighlighted;
    }

    [Serializable]
    public class ClassicGameState : GameState
    {
        public int level;

        public ClassicGameState()
        {
            gameMode = EGameMode.Classic;
        }
    }

    [Serializable]
    public class TimedGameState : GameState
    {
        public float remainingTime;

        public TimedGameState()
        {
            gameMode = EGameMode.Timed;
            remainingTime = 180f; // Default duration if not set
            score = 0;
            bestScore = 0;
        }

        public void SetBestScore(int newScore)
        {
            if (remainingTime <= 0 && newScore > bestScore)
            {
                bestScore = newScore;
            }
        }
    }

    [Serializable]
    public class LegacyGameState
    {
        // For backwards compatibility when loading old saved states
        public EGameState gameStatus;
        public int currentLevel;
        public EGameMode gameMode;
        public int score;
        public int remainingTime;
        public LevelRow[] levelRows;
        public DateTime quitTime;
        public int bestScore;
    }
}
