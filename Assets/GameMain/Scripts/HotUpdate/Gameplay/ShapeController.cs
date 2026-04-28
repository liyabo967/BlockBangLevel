
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using UnityEngine;
using UnityGameFramework.Runtime;
using Debug = UnityEngine.Debug;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class ShapeController
    {
        private static int _rowSize;
        private static int _columnSize;
        private static bool[,] _capturedBoardStatus;
        private static FieldManager _fieldManager;

        public static List<ShapeTemplate> GetPerfectShapeList(FieldManager field, ShapeTemplate[] shapesToConsider)
        {
            _fieldManager = field;
            _rowSize = field.cells.GetLength(0);
            _columnSize = field.cells.GetLength(1);
            CaptureBoardData();

            return GetPerfectShapes(shapesToConsider.ToList());
        }
        
        private static void CaptureBoardData()
        {
            _capturedBoardStatus ??= new bool[_rowSize, _columnSize];
            for (int i = 0; i < _rowSize; i++)
            {
                for (int j = 0; j < _columnSize; j++)
                {
                    _capturedBoardStatus[i, j] = IsFilled(i, j);
                }
            }
        }
        
        private static List<ShapeTemplate> GetPerfectShapes(List<ShapeTemplate> shapeList)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var scoreDict = new Dictionary<int, PerfectInfo>();
            foreach (var shapeInfo in shapeList)
            {
                // if (shapeInfo.weight <= 0)
                // {
                //     scoreDict[shapeInfo.id] = new PerfectInfo(shapeInfo.id);
                //     continue;
                // }
                scoreDict[shapeInfo.id] = GetPerfectInfo(shapeInfo);
            }
            shapeList.Sort((x, y) =>
            {
                return scoreDict[y.id].score.CompareTo(scoreDict[x.id].score);
            });
            sw.Stop();
            Log.Info($"GetPerfectShapes 运行耗时: {sw.ElapsedMilliseconds} ms");
            var result = new List<ShapeTemplate>();
            foreach (var shapeInfo in shapeList)
            {
                var perfectInfo = scoreDict[shapeInfo.id];
                if (CheckCapturedBoardPosition(shapeInfo, perfectInfo.placeRow, perfectInfo.placeCol))
                {
                    result.Add(shapeInfo);
                    // PrintCapturedBoard();
                    UpdateBoardStatus(perfectInfo);
                    // PrintCapturedBoard();
                    Log.Info($"PerfectInfo, id: {shapeInfo.id}, {perfectInfo.score}, place: {perfectInfo.placeRow}, {perfectInfo.placeCol}");
                    if (result.Count >= 3)
                    {
                        break;
                    }
                }
                else
                {
                    Log.Info($"Skip-------: {perfectInfo.id}, pos: {perfectInfo.placeRow}, {perfectInfo.placeCol}");
                }
            }
            return result;
        }
        
        
        private static PerfectInfo GetPerfectInfo(ShapeTemplate item)
        {
            var maxScore = -1;
            var score = 0;
            var iSkip = _rowSize - item.rowCount;
            var jSkip = _columnSize - item.columnCount;
            var perfectInfo = new PerfectInfo();
            perfectInfo.id = item.id;
            perfectInfo.shapeTemplate = item;
            for (int i = 0; i < _rowSize; i++)
            {
                for (int j = 0; j < _columnSize; j++)
                {
                    if (i > iSkip || j > jSkip)
                    {
                        continue;
                    }

                    // Log.Info($"CheckItem, {item.id}: {i}, {j}");
                    if (!CheckBoardPosition(item, i, j))
                    {
                        continue;
                    }

                    score = 0;
                    var boardData = GetBoolData(item, i, j);
                    if (!IsPerfect(item.boolValues, boardData))
                    {
                        Log.Info($"PerfectInfo, id: {item.id}, IsPerfect: {i}, {j}, print data");
                        for (var i1 = 0; i1 < item.boolValues.Length; i1++)
                        {
                            Log.Info($"PerfectInfo, id: {item.id}, item.boolValues: {item.boolValues[i1]}");
                        }
                        for (var i1 = 0; i1 < boardData.Length; i1++)
                        {
                            Log.Info($"PerfectInfo, id: {item.id}, boardData: {boardData[i1]}");
                        }
                        
                        continue;
                    }
                    score += item.rowCount * item.columnCount;
                    if (item.isRect)
                    {
                        score = Mathf.Max(item.rowCount, item.columnCount); 
                    }
                    Log.Info($"PerfectInfo, id: {item.id}, IsPerfect: {i}, {j}, score: {score}");

                    var around = GetAround(item, i, j);
                    score += around;
                    // if (item.isRect && around == 0)
                    // {
                    //     score = 0; 
                    // }
                    Log.Info($"PerfectInfo, id: {item.id}, GetAround: {i}, {j}, around: {around}, score: {score}");
                    if (score > maxScore)
                    {
                        Log.Info($"PerfectInfo, id: {item.id}, Found Position: {i}, {j}, current: {score}, max: {maxScore}");
                        maxScore = score;
                        perfectInfo.placeRow = i;
                        perfectInfo.placeCol = j;
                        perfectInfo.score = maxScore;
                    }
                }
            }

            return perfectInfo;
        }
        
        private static bool CheckCapturedBoardPosition(ShapeTemplate shapeInfo, int row, int col)
        {
            if (row == -1 && col == -1)
            {
                Log.Error("Skip CheckCapturedBoardPosition");
                return true;
            }
            var index = 0;
            for (int i = row; i < row + shapeInfo.rowCount; i++)
            {
                for (int j = col; j < col + shapeInfo.columnCount; j++)
                {
                    try
                    {
                        var isFilled = _capturedBoardStatus[i, j];
                        if (index >= shapeInfo.boolValues.Length)
                        {
                            Log.Error("index out of range");
                        }
                        if (shapeInfo.boolValues[index] && isFilled)
                        {
                            return false;
                        }
                        index++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            return true;
        }

        private static int GetAround(ShapeTemplate item, int row, int col)
        {
            var score = 0;
            var left = GetLeft(item, row, col);
            var right = GetRight(item, row, col);
            var up = GetUp(item, row, col);
            var down = GetDown(item, row, col);
            Log.Info($"PerfectInfo, id: {item.id}, GetAroundScore, {left}, {right}, {up}, {down}");
            score += left + right + up + down;
            // if (left > 0 && right > 0)
            // {
            //     score += item.rowCount;
            //     if (up > 0 || down > 0)
            //     {
            //         score += item.columnCount;
            //     }
            // }
            //
            // if (up > 0 && down > 0)
            // {
            //     score += item.columnCount;
            //     if (left > 0 || right > 0)
            //     {
            //         score += item.rowCount;
            //     }
            // }
            Log.Info($"PerfectInfo, id: {item.id}, GetAroundScore, return, {score}");
            return score;
        }

        private static int GetLeft(ShapeTemplate item, int row, int col)
        {
            if (col == 0)
            {
                // return 0;
                return item.rowCount / 2;
            }
            
            var lastState = false;
            for (int i = row; i < row + item.rowCount; i++)
            {
                // var currentState = Board.Instance.BoardInfo[i, col - 1].State != Enums.CellState.Default;
                var currentState = IsFilled(i, col - 1);
                if (i == row)
                {
                    lastState = currentState;
                    continue;
                }

                if (currentState != lastState)
                {
                    return 0;
                }
            }
            
            return lastState ? item.rowCount : 0;
        }

        private static int GetRight(ShapeTemplate item, int row, int col)
        {
            if (col + item.columnCount >= _columnSize)
            {
                // return 0;
                return item.rowCount / 2;
            }

            var lastState = false;
            for (int i = row; i < row + item.rowCount; i++)
            {
                // var currentState = Board.Instance.BoardInfo[i, col + item.cols].State != Enums.CellState.Default;
                var currentState = IsFilled(i, col + item.columnCount);
                if (i == row)
                {
                    lastState = currentState;
                    continue;
                }

                if (currentState != lastState)
                {
                    return 0;
                }
            }
            
            return lastState ? item.rowCount : 0;
        }

        private static int GetUp(ShapeTemplate item, int row, int col)
        {
            if (row <= 0)
            {
                // return 0;
                return item.columnCount / 2;
            }

            var lastState = false;
            for (int i = col; i < col + item.columnCount; i++)
            {
                // var currentState = Board.Instance.BoardInfo[row + 1, i].State != Enums.CellState.Default;
                var currentState = IsFilled(row - 1, i);
                if (i == col)
                {
                    lastState = currentState;
                    continue;
                }

                if (currentState != lastState)
                {
                    return 0;
                }
            }
            
            return lastState ? item.columnCount : 0;
        }

        private static int GetDown(ShapeTemplate item, int row, int col)
        {
            if (row + item.rowCount >= _rowSize)
            {
                // return 0;
                return item.columnCount / 2;
            }
            
            var lastState = false;
            var rowNum = row + item.rowCount;
            for (int i = col; i < col + item.columnCount; i++)
            {
                // var currentState = Board.Instance.BoardInfo[row - item.rows, i].State != Enums.CellState.Default;
                var currentState = IsFilled(rowNum, i);
                if (i == col)
                {
                    lastState = currentState;
                    continue;
                }

                if (currentState != lastState)
                {
                    return 0;
                }
            }
            return lastState ? item.columnCount : 0;
        }
        
        private static void UpdateBoardStatus(PerfectInfo shapeInfo)
        {
            if (shapeInfo.score <= 0)
            {
                return;
            }
            for (int i = 0; i < shapeInfo.shapeTemplate.rowCount; i++)
            {
                for (int j = 0; j < shapeInfo.shapeTemplate.columnCount; j++)
                {
                    if (shapeInfo.shapeTemplate.boolValues[i * shapeInfo.shapeTemplate.columnCount + j])
                    {
                        _capturedBoardStatus[shapeInfo.placeRow + i, shapeInfo.placeCol + j] = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// 检查形状是否可以放在棋盘的指定位置上面
        /// </summary>
        /// <returns></returns>
        private static bool CheckBoardPosition(ShapeTemplate shapeInfo, int row, int col)
        {
            var index = 0;
            for (int i = row; i < row + shapeInfo.rowCount; i++)
            {
                for (int j = col; j < col + shapeInfo.columnCount; j++)
                {
                    // var isFilled = Board.Instance.BoardInfo[i, j].State != Enums.CellState.Default;
                    var filled = IsFilled(i, j);
                    // if (index >= shapeInfo.boolValues.Length)
                    // {
                    //     Log.Error("index out of range");
                    // }
                    if (shapeInfo.boolValues[index] && filled)
                    {
                        // Log.Info($"CheckBoardPosition, {shapeInfo.id}, RowAndCol: {row}, {col}, false,IAndJ: {i}, {j}");
                        return false;
                    }
                    index++;
                }
            }
            // Log.Info($"CheckBoardPosition, {shapeInfo.id}, RowAndCol: {row}, {col}, true");
            return true;
        }
        
        private static bool IsPerfect(bool[] a, bool[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == b[i])
                {
                    return false;
                }
            }

            return true;
        }
        
        private static bool[] GetBoolData(ShapeTemplate item, int row, int col)
        {
            var boolArray = new bool[item.rowCount * item.columnCount];
            var index = 0;
            for (int i = row; i < row + item.rowCount; i++)
            {
                for (int j = col; j < col + item.columnCount; j++)
                {
                    boolArray[index] = IsFilled(i, j);
                    index++;
                }
            }

            // PrintBoolArray(boolArray, row, col);
            return boolArray;
        }

        private static bool IsFilled(int row, int col)
        {
            return !_fieldManager.GetAllCells()[row, col].IsEmpty();
        }
    }
    
    // public class PerfectInfo
    // {
    //     public int shapeId;
    //     public int placeRow;
    //     public int placeCol;
    //     public int score;
    //
    //     public PerfectInfo(int id)
    //     {
    //         shapeId = id;
    //         score = 0;
    //         placeRow = -1;
    //         placeCol = -1;
    //     }
    // }
    
    public class PerfectInfo {
        public int id;
        
        // 形状在棋盘放置的位置和分数
        public int placeRow;
        public int placeCol;
        public int score;
        
        public ShapeTemplate shapeTemplate;
    }
}