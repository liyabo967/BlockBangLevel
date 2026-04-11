using UnityEngine;

namespace Quester
{
    public static class TransformExtensions
    {
        /// <summary>
        /// 删除所有子物体（运行时）
        /// </summary>
        public static void ClearChildren(this Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(parent.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 删除所有子物体（Editor模式）
        /// </summary>
        public static void ClearChildrenImmediate(this Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 重置 Transform
        /// </summary>
        public static void ResetLocal(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        /// 设置 X
        /// </summary>
        public static void SetX(this Transform t, float x)
        {
            Vector3 pos = t.position;
            pos.x = x;
            t.position = pos;
        }

        /// <summary>
        /// 设置 Y
        /// </summary>
        public static void SetY(this Transform t, float y)
        {
            Vector3 pos = t.position;
            pos.y = y;
            t.position = pos;
        }

        /// <summary>
        /// 设置 Z
        /// </summary>
        public static void SetZ(this Transform t, float z)
        {
            Vector3 pos = t.position;
            pos.z = z;
            t.position = pos;
        }

        /// <summary>
        /// 设置 Local X
        /// </summary>
        public static void SetLocalX(this Transform t, float x)
        {
            Vector3 pos = t.localPosition;
            pos.x = x;
            t.localPosition = pos;
        }

        /// <summary>
        /// 设置 Local Y
        /// </summary>
        public static void SetLocalY(this Transform t, float y)
        {
            Vector3 pos = t.localPosition;
            pos.y = y;
            t.localPosition = pos;
        }

        /// <summary>
        /// 设置 Local Z
        /// </summary>
        public static void SetLocalZ(this Transform t, float z)
        {
            Vector3 pos = t.localPosition;
            pos.z = z;
            t.localPosition = pos;
        }

        /// <summary>
        /// 获取第一个子物体
        /// </summary>
        public static Transform FirstChild(this Transform t)
        {
            return t.childCount > 0 ? t.GetChild(0) : null;
        }

        /// <summary>
        /// 获取最后一个子物体
        /// </summary>
        public static Transform LastChild(this Transform t)
        {
            return t.childCount > 0 ? t.GetChild(t.childCount - 1) : null;
        }
    }
}