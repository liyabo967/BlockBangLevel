//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-04-16 21:00:51.666
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
    /// 商店的商品。
    /// </summary>
    public class DRShopProduct : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取商品。
        /// </summary>
        public string ProductId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取商品类型。
        /// </summary>
        public string ProductType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取内容。
        /// </summary>
        public string Content
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取价格。
        /// </summary>
        public float Price
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取货币。
        /// </summary>
        public string Currency
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取图标。
        /// </summary>
        public string Icon
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
            ProductId = columnStrings[index++];
            ProductType = columnStrings[index++];
            Content = columnStrings[index++];
            Price = float.Parse(columnStrings[index++]);
            Currency = columnStrings[index++];
            Icon = columnStrings[index++];

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
                    ProductId = binaryReader.ReadString();
                    ProductType = binaryReader.ReadString();
                    Content = binaryReader.ReadString();
                    Price = binaryReader.ReadSingle();
                    Currency = binaryReader.ReadString();
                    Icon = binaryReader.ReadString();
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
