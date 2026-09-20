using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlockPuzzleGameToolkit.Scripts.Editor.SeasonShapeEditor
{
    [CustomEditor(typeof(SeasonShape))]
    public class SeasonShapeEditor : UnityEditor.Editor
    {
        private VisualElement _root;
        private VisualElement _matrixContainer;
        private SeasonShape _seasonShape;
        private Label _countlabel;
        private Label _msglabel;
        private int _activeCount;
        
        private readonly Color _activeColor = new(0.6f, 0.6f, 0.6f);
        private readonly Color _defaultColor = new(0.3f, 0.3f, 0.3f);
        
        private void OnEnable()
        {
            _seasonShape = (SeasonShape)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();

            // Load and apply USS
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/GameMain/SeasonShapes/SeasonShape.uss");
            _root.styleSheets.Add(styleSheet);
            _countlabel = new Label("0/88") { name = "title" };
            _msglabel = new Label("") { name = "msg" };
            _root.Add(_countlabel);
            _root.Add(_msglabel);
            
            
            var buttons = new VisualElement();
            // Left
            var leftButton = CreateButton("Left", () =>
            {
                _seasonShape.MoveToLeft();
            });
            var rightButton = CreateButton("Right", () =>
            {
                _seasonShape.MoveToRight();
            });
            var upButton = CreateButton("Up", () =>
            {
                _seasonShape.MoveToUp();
            });
            var downButton = CreateButton("Down", () =>
            {
                _seasonShape.MoveToDown();
            });
            
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.Add(leftButton);
            buttons.Add(rightButton);
            buttons.Add(upButton);
            buttons.Add(downButton);
            _root.Add(buttons);

            var regenerateMatrixButton = new Button(() =>
            {
                _msglabel.text = "";
                _seasonShape.RegenerateMatrix();
                UpdateMatrixUI();
                Save();
            });
            regenerateMatrixButton.Add(new Label("Regenerate"));
            _root.Add(regenerateMatrixButton);
            
            _matrixContainer = new VisualElement { name = "grid-container" };
            _root.Add(_matrixContainer);
            UpdateMatrixUI();
            return _root;
        }

        private Button CreateButton(string text, Action action)
        {
            var button = new Button(() =>
            {
                action.Invoke();
                Save();
                UpdateMatrixUI();
            });
            button.text = text;
            button.style.width = Length.Percent(25);
            button.style.flexGrow = 1;
            return button;
        }

        private void UpdateMatrixUI()
        {
            _matrixContainer.Clear();

            _activeCount = 0;
            for (var i = 0; i < _seasonShape.rows; i++)
            {
                var row = new VisualElement();
                row.AddToClassList("grid-row");
                _matrixContainer.Add(row);

                for (var j = 0; j < _seasonShape.columns; j++)
                {
                    int rowIndex = i;
                    int columnIndex = j;
                    var cell = new Button();
                    cell.AddToClassList("grid-cell");
                    var active = _seasonShape.Matrix[i, j];
                    cell.style.backgroundColor = active ? _activeColor : _defaultColor;
                    _activeCount += active ? 1 : 0;
                    cell.clicked += () =>
                    {
                        if (!active && _activeCount >= _seasonShape.seasonLength)
                        {
                            _msglabel.text = $"最多 {_seasonShape.seasonLength} 个";
                            return;
                        }

                        _msglabel.text = "";
                        active = !active;
                        _activeCount += active ? 1 : -1;
                        _seasonShape.UpdateMatrix(rowIndex, columnIndex, active);
                        cell.style.backgroundColor = active ? _activeColor : _defaultColor;
                        UpdateCountText();
                        Save();
                    };
                    row.Add(cell);
                }
            }

            UpdateCountText();
        }

        private void UpdateCountText()
        {
            _countlabel.text = $"{_activeCount}/{_seasonShape.seasonLength}";
        }
        
        private void Save()
        {
            EditorUtility.SetDirty(target);
        }
    }
}