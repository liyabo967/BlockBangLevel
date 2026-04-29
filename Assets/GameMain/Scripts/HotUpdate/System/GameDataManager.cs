// // ©2015 - 2026 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BlockPuzzleGameToolkit.Scripts.System
{
    public static class GameDataManager
    {
        public static int LevelNum;

        private static Level _level;

        public static bool isTestPlay = false;

        public static void ClearPlayerProgress()
        {
            PlayerPrefs.DeleteKey("Level");
            PlayerPrefs.Save();
        }

        public static void ClearALlData()
        {
            #if UNITY_EDITOR
            // clear variables ResourceObject from Resources/Variables
            var resourceObjects = Resources.LoadAll<ResourceObject>("Variables");
            foreach (var resourceObject in resourceObjects)
            {
                resourceObject.Set(0);
            }

            AssetDatabase.SaveAssets();

            PlayerPrefs.DeleteAll();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            #endif
        }

        public static void UnlockLevel(int currentLevel)
        {
            int savedLevel = UserDataManager.Instance.Level;
            if (savedLevel < currentLevel)
            {
                LevelNum = currentLevel;
                // PlayerPrefs.SetInt("Level", currentLevel);
                // PlayerPrefs.Save();
                UserDataManager.Instance.SetLevel(currentLevel);
            }
        }

        public static int GetLevelNum()
        {
            return UserDataManager.Instance.Level;
        }

        public static Level GetLevel()
        {
            if (_level != null && isTestPlay)
            {
                return _level;
            }

            if (_level != null)
            {
                return _level;
            }

            // _level = GetGameMode() == EGameMode.Classic ? Resources.Load<Level>("Misc/ClassicLevel") : Resources.Load<Level>("Levels/Level_" + GetLevelNum());
            if (GetGameMode() == EGameMode.Classic)
            {
                _level = Addressables.LoadAssetAsync<Level>("Assets/GameMain/Settings/Misc/ClassicLevel.asset").WaitForCompletion();
            }
            else
            {
                _level = Addressables.LoadAssetAsync<Level>($"Assets/GameMain/Settings/Levels/Level_{GetLevelNum()}.asset").WaitForCompletion();
            }
            return _level;
        }

        public static void SetLevel(Level level)
        {
            _level = level;
        }

        public static EGameMode GetGameMode()
        {
            return (EGameMode)UserDataManager.Instance.GameMode;
        }

        public static void SetGameMode(EGameMode gameMode)
        {
            // PlayerPrefs.SetInt("GameMode", (int)gameMode);
            // PlayerPrefs.Save();
            UserDataManager.Instance.SetGameMode((int)gameMode);
        }

        // public static void SetAllLevelsCompleted()
        // {
        //     var levels = Resources.LoadAll<Level>("Levels").Length;
        //     // PlayerPrefs.SetInt("Level", levels);
        //     // PlayerPrefs.Save();
        //     UserDataManager.Instance.SetLevel(levels);
        // }

        internal static bool HasMoreLevels()
        {
            int currentLevel = GetLevelNum();
            int totalLevels = Resources.LoadAll<Level>("Levels").Length;
            return currentLevel < totalLevels;
        }

        public static void SetLevelNum(int stateCurrentLevel)
        {
            LevelNum = stateCurrentLevel;
        }
    }
}