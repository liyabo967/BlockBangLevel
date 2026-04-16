using System;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.System;
using Quester;

namespace BlockPuzzleGameToolkit.Scripts.Data
{
    public class GameStateManager : MonoSingleton<GameStateManager>
    {
        private ClassicGameState _classicState;
        private TimedGameState _timedState;
        
        public GameState Load(EGameMode gameMode)
        {
            GameState state = null;
            switch (gameMode)
            {
                case EGameMode.Classic:
                    state = GameEntry.Storage.Load(nameof(EGameMode.Classic), _classicState);
                    break;
                case EGameMode.Timed:
                    state = GameEntry.Storage.Load(nameof(EGameMode.Timed), _timedState);
                    break;
            }
            return state;
        }

        // 序列化数据，但是不写入磁盘
        private void Save()
        {
            // GameEntry.Storage.Save(nameof(EGameMode.Classic), _classicState);
            // GameEntry.Storage.Save(nameof(EGameMode.Timed), _timedState);
        }

        public void SaveState(EGameMode gameMode, GameState state)
        {
            switch (gameMode)
            {
                case EGameMode.Classic:
                    GameEntry.Storage.Save(nameof(EGameMode.Classic), state);
                    break;
                case EGameMode.Timed:
                    GameEntry.Storage.Save(nameof(EGameMode.Timed), state);
                    break;
            }
        }

        public void Delete(EGameMode gameMode)
        {
            GameEntry.Storage.DeleteKey(gameMode.ToString());
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
        }

        protected override void OnApplicationQuit()
        {
            Save();
            base.OnApplicationQuit();
        }
        
    }
}