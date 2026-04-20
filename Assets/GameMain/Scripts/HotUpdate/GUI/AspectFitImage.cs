using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.GUI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class AspectFitImage : Image
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (sprite == null)
            {
                base.OnPopulateMesh(vh);
                return;
            }

            vh.Clear();

            Rect rect = GetPixelAdjustedRect();

            float rectW = rect.width;
            float rectH = rect.height;

            float texW = sprite.rect.width;
            float texH = sprite.rect.height;

            if (texW <= 0 || texH <= 0) return;

            // 居中偏移
            float offsetX = rectW * 0.5f;
            float offsetY = rectH * 0.5f;

            // 四个顶点
            Vector2 v0 = new Vector2(-offsetX, -offsetY);
            Vector2 v1 = new Vector2(-offsetX, offsetY);
            Vector2 v2 = new Vector2(offsetX, offsetY);
            Vector2 v3 = new Vector2(offsetX, -offsetY);

            // UV 
            Vector2 uv0;
            Vector2 uv1;
            Vector2 uv2;
            Vector2 uv3;
            
            bool fitWidth = rectW / texW > rectH / texH;
            if (fitWidth)
            {
                var scale = rectW / texW;
                var scaleHeight = texH * scale;
                var uvY = (scaleHeight - rectH) * 0.5f / scaleHeight;
                uv0 = new Vector2(0, uvY);
                uv1 = new Vector2(0, 1 - uvY);
                uv2 = new Vector2(1, 1 - uvY);
                uv3 = new Vector2(1, uvY);
            }
            else
            {
                var scale = rectH / texH;
                var scaleWidth = texW * scale;
                var uvX = (scaleWidth - rectW) * 0.5f / scaleWidth;
                uv0 = new Vector2(uvX, 0);
                uv1 = new Vector2(uvX, 1);
                uv2 = new Vector2(1 - uvX, 1);
                uv3 = new Vector2(1 - uvX, 0);
            }

            vh.AddVert(v0, color, uv0);
            vh.AddVert(v1, color, uv1);
            vh.AddVert(v2, color, uv2);
            vh.AddVert(v3, color, uv3);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}