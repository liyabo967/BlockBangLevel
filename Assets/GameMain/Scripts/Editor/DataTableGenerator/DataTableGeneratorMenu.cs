//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using UnityEditor;
using UnityEngine;

namespace Blockbang.Editor.DataTableTools
{
    public sealed class DataTableGeneratorMenu
    {
        private static readonly string[] DataTableNames =
        {
            "Element",
            "ElementCategory",
            "CollectElement",
            
            "ShopProduct",
            "Music",
            "Sound",
            "UISound",
            "Scene",
            "UIForm"
        };
        
        [MenuItem("Game Framework/Generate DataTables")]
        private static void GenerateDataTables()
        {
            foreach (string dataTableName in DataTableNames)
            {
                DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(dataTableName);
                if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                {
                    Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    break;
                }

                DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
            }

            AssetDatabase.Refresh();
        }
    }
}
