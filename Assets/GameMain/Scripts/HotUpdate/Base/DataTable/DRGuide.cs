//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-03-16 15:57:33.169
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
    /// 引导。
    /// </summary>
    public class DRGuide : DataRowBase
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
        /// 获取关卡。
        /// </summary>
        public int Level
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取引导类型。
        /// </summary>
        public int Type
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取参数。
        /// </summary>
        public string Param
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取引导多语言。
        /// </summary>
        public string TipKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取提示框位置。
        /// </summary>
        public string TipPos
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取延迟。
        /// </summary>
        public int Delay
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
            Level = int.Parse(columnStrings[index++]);
            Type = int.Parse(columnStrings[index++]);
            Param = columnStrings[index++];
            TipKey = columnStrings[index++];
            TipPos = columnStrings[index++];
            Delay = int.Parse(columnStrings[index++]);

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
                    Level = binaryReader.Read7BitEncodedInt32();
                    Type = binaryReader.Read7BitEncodedInt32();
                    Param = binaryReader.ReadString();
                    TipKey = binaryReader.ReadString();
                    TipPos = binaryReader.ReadString();
                    Delay = binaryReader.Read7BitEncodedInt32();
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
