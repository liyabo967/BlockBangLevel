using System;
using System.Linq;
using UnityEngine;

namespace Quester
{
    public static class GameObjectExtension
    {
        public static void DestroyAllChildren(this GameObject go, params GameObject[] excludeObj)
        {
            for (int i = go.transform.childCount - 1; i >= 0; i--)
            {
                if (excludeObj.Contains(go.transform.GetChild(i).gameObject))
                {
                    continue;
                }
                GameObject.Destroy(go.transform.GetChild(i).gameObject);
            }
        }
        
        // 删除当前 GameObject 的所有子对象
        public static void DestroyAllChildren(this Transform transform)
        {
            // 反向循环避免修改集合时的索引问题
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                // 在编辑模式下使用 DestroyImmediate，运行时使用 Destroy
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEngine.Object.DestroyImmediate(transform.GetChild(i).gameObject);
                else
                    UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
#else
             UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
#endif
            }
        }
        
        public static string AppendString(this string str, params object[] stringarr)
        {
            System.Text.StringBuilder strb = new System.Text.StringBuilder();
            strb.Append(str);
            for (int i = 0; i < stringarr.Length; i++)
            {
                strb.Append(stringarr[i]);
            }

            str = strb.ToString();
            return str;
        }
        
        public static T ToEnum<T>(this int enumInt)
        {
            return (T)Enum.Parse(typeof(T), enumInt.ToString());
        }
        
        public static T ToEnum<T>(this string enumStr)
        {
            return (T)Enum.Parse(typeof(T), enumStr);
        }
    }
}