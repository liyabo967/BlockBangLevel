using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BlockPuzzleGameToolkit.Scripts
{
    [CreateAssetMenu(fileName = "SeasonShape", menuName = "GameMain/SeasonShapes", order = 1)]
    public class SeasonShape : ScriptableObject
    {
        public readonly int Rows = 15;
        public readonly int Columns = 11;
        public readonly int MaxLevel = 88;
        
        [SerializeField]
        private SeasonShapeMatrix _matrix;
        
        [Serializable]
        public class SeasonShapeMatrix
        {
            public SeasonShapeRow[] rows;
        }

        [Serializable]
        public class SeasonShapeRow
        {
            public bool[] columns;
        }

        public SeasonShapeMatrix Matrix => _matrix;

        private void OnEnable()
        {
            InitializeIfNeeded();
        }

        public void InitializeIfNeeded()
        {
            if (_matrix == null)
            {
                Debug.LogError("Initialize Matrix");
                _matrix = new SeasonShapeMatrix();
                _matrix.rows = new SeasonShapeRow[Rows];
                for (int i = 0; i < Rows; i++)
                {
                    var row = new SeasonShapeRow()
                    {
                        columns = new bool[Columns]
                    };
                    _matrix.rows[i] = row;
                }
            }
            
        }

        public void UpdateMatrix(int row, int column, bool value)
        {
            _matrix.rows[row].columns[column] = value;
        }

        public void RegenerateMatrix()
        {
            var arr = Generate(Rows, Columns, MaxLevel);
            ApplyByArr(arr);
        }

        public void ApplyByArr(bool[,] arr)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    _matrix.rows[i].columns[j] = arr[i, j];
                }
            }
        }

        public void MoveToLeft()
        {
            _matrix = MoveBoolArray(_matrix, 0, -1);
        }

        public void MoveToRight()
        {
            _matrix = MoveBoolArray(_matrix, 0, 1);
        }

        public void MoveToUp()
        {
            _matrix = MoveBoolArray(_matrix, -1, 0);
        }

        public void MoveToDown()
        {
            _matrix = MoveBoolArray(_matrix, 1, 0);
        }
        
        public SeasonShapeMatrix MoveBoolArray(SeasonShapeMatrix matrix, int offsetX, int offsetY)
        {
            int width = matrix.rows.GetLength(0);
            int height = matrix.rows[0].columns.GetLength(0);

            bool[,] result = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!matrix.rows[x].columns[y])
                        continue;

                    int newX = x + offsetX;
                    int newY = y + offsetY;

                    // 超出边界的部分直接丢弃
                    if (newX < 0 || newX >= width ||
                        newY < 0 || newY >= height)
                    {
                        continue;
                    }

                    result[newX, newY] = true;
                }
            }

            ApplyByArr(result);
            return _matrix;
        }
        

        /// <summary>
        /// 生成一个二维 bool 数组，保证：
        /// 1. true 的数量恰好为 count
        /// 2. 所有 true 四方向连通
        /// </summary>
        private static bool[,] Generate(int rows, int cols, int count)
        {
            bool[,] map = new bool[rows, cols];

            // 四个方向：上、下、左、右
            Vector2Int[] directions =
            {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(1, 0)
            };

            // 随机选择一个起点
            Vector2Int start = new Vector2Int(
                Random.Range(0, rows),
                Random.Range(0, cols)
            );

            map[start.x, start.y] = true;

            // 当前已经生成的 true
            List<Vector2Int> cells = new List<Vector2Int>
            {
                start
            };

            while (cells.Count < count)
            {
                // 从已有的 true 中随机选择一个
                Vector2Int current =
                    cells[Random.Range(0, cells.Count)];

                // 收集这个点周围还没有变成 true 的格子
                List<Vector2Int> candidates = new List<Vector2Int>();

                foreach (Vector2Int dir in directions)
                {
                    Vector2Int next = current + dir;

                    if (next.x < 0 || next.x >= rows ||
                        next.y < 0 || next.y >= cols)
                        continue;

                    if (!map[next.x, next.y])
                    {
                        candidates.Add(next);
                    }
                }

                // 如果这个点周围已经没有空位置
                if (candidates.Count == 0)
                {
                    // 找其他仍然可以扩张的 true
                    bool found = false;

                    foreach (Vector2Int cell in cells)
                    {
                        foreach (Vector2Int dir in directions)
                        {
                            Vector2Int next = cell + dir;

                            if (next.x >= 0 && next.x < rows &&
                                next.y >= 0 && next.y < cols &&
                                !map[next.x, next.y])
                            {
                                current = cell;
                                found = true;
                                break;
                            }
                        }

                        if (found)
                            break;
                    }

                    if (!found)
                        break;

                    candidates.Clear();

                    foreach (Vector2Int dir in directions)
                    {
                        Vector2Int next = current + dir;

                        if (next.x >= 0 && next.x < rows &&
                            next.y >= 0 && next.y < cols &&
                            !map[next.x, next.y])
                        {
                            candidates.Add(next);
                        }
                    }
                }

                // 随机选择一个相邻格子
                Vector2Int selected =
                    candidates[Random.Range(0, candidates.Count)];

                map[selected.x, selected.y] = true;
                cells.Add(selected);
            }

            return map;
        }
    }
}