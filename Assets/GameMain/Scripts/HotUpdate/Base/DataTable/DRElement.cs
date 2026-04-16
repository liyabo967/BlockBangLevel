//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-04-16 15:42:41.965
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Quester
{
    /// <summary>
    /// 元素表。
    /// </summary>
    public class DRElement : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取元素编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取元素名称。
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取元素类别ID。
        /// </summary>
        public int CategoryId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否旁消。
        /// </summary>
        public bool IsRoundMatch
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取不可消除。
        /// </summary>
        public bool IsInvincible
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否可匹配。
        /// </summary>
        public bool IsMatchable
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否基础块。
        /// </summary>
        public bool IsHidingBase
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取元素层级。
        /// </summary>
        public int Layer
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取等级。
        /// </summary>
        public int Level
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string Prefab
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取元素图片。
        /// </summary>
        public string Sprite
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            index++;
            Name = columnStrings[index++];
            CategoryId = int.Parse(columnStrings[index++]);
            IsRoundMatch = bool.Parse(columnStrings[index++]);
            IsInvincible = bool.Parse(columnStrings[index++]);
            IsMatchable = bool.Parse(columnStrings[index++]);
            IsHidingBase = bool.Parse(columnStrings[index++]);
            Layer = int.Parse(columnStrings[index++]);
            Level = int.Parse(columnStrings[index++]);
            Prefab = columnStrings[index++];
            Sprite = columnStrings[index++];

            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    Name = binaryReader.ReadString();
                    CategoryId = binaryReader.Read7BitEncodedInt32();
                    IsRoundMatch = binaryReader.ReadBoolean();
                    IsInvincible = binaryReader.ReadBoolean();
                    IsMatchable = binaryReader.ReadBoolean();
                    IsHidingBase = binaryReader.ReadBoolean();
                    Layer = binaryReader.Read7BitEncodedInt32();
                    Level = binaryReader.Read7BitEncodedInt32();
                    Prefab = binaryReader.ReadString();
                    Sprite = binaryReader.ReadString();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private void GeneratePropertyArray()
        {

        }
    }
}
