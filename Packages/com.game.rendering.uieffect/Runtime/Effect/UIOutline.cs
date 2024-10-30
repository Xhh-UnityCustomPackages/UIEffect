using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [UIEffectPath("UIOutline")]
    public class UIOutline : BaseMeshEffect
    {
#if UNITY_EDITOR
        public override bool HasCustomInspectors => true;

        public OutlineStyle style
        {
            get => m_Style;
            set => m_Style = value;
        }

        public Color EffectColor
        {
            get => m_EffectColor;
            set => m_EffectColor = value;
        }

        public Vector2 EffectOffset
        {
            get => m_EffectOffset;
            set => m_EffectOffset = value;
        }

        public Vector2 EffectOffset2
        {
            get => m_EffectOffset2;
            set => m_EffectOffset2 = value;
        }
#endif

        public enum OutlineStyle
        {
            Outline,
            Outline8,
            Outline8Split,
            Shadow,
        }

        [SerializeField] private OutlineStyle m_Style = OutlineStyle.Outline;
        [SerializeField] private Color m_EffectColor = new Color(0f, 0f, 0f, 1f);
        [SerializeField] private Vector2 m_EffectOffset = new Vector2(2f, -2f);
        [SerializeField] private Vector2 m_EffectOffset2 = new Vector2(2f, -2f);
        // [SerializeField] private bool m_UseGraphicAlpha = true;

        public override void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled || vh.currentVertCount <= 0)
            {
                return;
            }

            vh.GetUIVertexStream(s_Verts);

            _graphicVertexCount = s_Verts.Count;

            // 核心逻辑
            //================================
            // Append shadow vertices.
            //================================
            {
                var start = s_Verts.Count - _graphicVertexCount;
                var end = s_Verts.Count;
                var targetColor = m_EffectColor;
                targetColor.a *= graphic.color.a;//乘以原始定点色A通道
                ApplyOutline(s_Verts, targetColor, ref start, ref end, false);
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(s_Verts);

            s_Verts.Clear();
        }


        private void ApplyOutline(List<UIVertex> verts, Color color, ref int start, ref int end, bool alpha)
        {
            if (color.a <= 0)
                return;

            var x = m_EffectOffset.x;
            var y = m_EffectOffset.y;


            switch (m_Style)
            {
                // Append Outline.
                case OutlineStyle.Outline:
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, x, y, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, x, -y, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, -x, y, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, -x, -y, alpha);
                    break;
                // Append Outline8.
                case OutlineStyle.Outline8:
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, x, y, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, x, -y, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, -x, y, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, -x, -y, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, -x, 0, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, 0, -y, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, x, 0, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, 0, y, alpha);
                    break;
                case OutlineStyle.Outline8Split:
                    var left = x;
                    var right = y;
                    var top = m_EffectOffset2.x;
                    var bottom = m_EffectOffset2.y;
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, left, top, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, left, bottom, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, right, top, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, right, bottom, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, left, 0, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, 0, bottom, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, right, 0, alpha);
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, 0, top, alpha);
                    break;
                case OutlineStyle.Shadow:
                    ApplyOutlineZeroAlloc(verts, color, ref start, ref end, x, y, alpha);
                    break;
            }
        }

        private void ApplyOutlineZeroAlloc(List<UIVertex> verts, Color color, ref int start, ref int end, float x, float y, bool alpha)
        {
            // Check list capacity.
            var count = end - start;
            var neededCapacity = verts.Count + count;
            if (verts.Capacity < neededCapacity)
                verts.Capacity *= 2;

            var normalizedIndex = -1;


            // Add
            var vt = default(UIVertex);
            for (var i = 0; i < count; i++)
            {
                verts.Add(vt);
            }

            // Move
            for (var i = verts.Count - 1; count <= i; i--)
            {
                verts[i] = verts[i - count];
            }

            // Append shadow vertices to the front of list.
            // * The original vertex is pushed backward.
            for (var i = 0; i < count; ++i)
            {
                vt = verts[i + start + count];

                var v = vt.position;
                vt.position.Set(v.x + x, v.y + y, v.z);
                var vertColor = m_EffectColor;
                vertColor.a = alpha ? color.a * vt.color.a / 255 : color.a;
                vt.color = vertColor;


                // Set UIEffect parameters
                if (0 <= normalizedIndex)
                {
                    vt.uv0 = new Vector2(
                        vt.uv0.x,
                        normalizedIndex
                    );
                }

                verts[i] = vt;
            }

            // Update next shadow offset.
            start = end;
            end = verts.Count;
        }


#if UNITY_EDITOR
        public override void Reset()
        {
            m_EffectColor = new Color(0f, 0f, 0f, 0.5f);
            m_EffectOffset = new Vector2(1f, 1f);
        }
#endif
    }
}