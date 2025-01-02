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


        public int CircleCount
        {
            get => m_CircleCount;
            set => m_CircleCount = value;
        }

        public int FirstSample
        {
            get => m_FirstSample;
            set => m_FirstSample = value;
        }

        public int SampleIncrement
        {
            get => m_SampleIncrement;
            set => m_SampleIncrement = value;
        }

        public Vector2Int HalfSampleCount
        {
            get => m_HalfSampleCount;
            set => m_HalfSampleCount = value;
        }
#endif

        public enum OutlineStyle
        {
            Outline,
            Outline8,
            Outline8Split,
            Shadow,

            //下面两个来源自https://github.com/n-yoda/unity-vertex-effects
            Circle,
            Box,
        }

        [SerializeField] private OutlineStyle m_Style = OutlineStyle.Outline8Split;
        [SerializeField] private Color m_EffectColor = new Color(0f, 0f, 0f, 1f);
        [SerializeField] private Vector2 m_EffectOffset = new Vector2(3f, -3f);
        [SerializeField] private Vector2 m_EffectOffset2 = new Vector2(3f, -6f);

        [Header("Circle")] [SerializeField] private int m_CircleCount = 2;
        [SerializeField] private int m_FirstSample = 4;
        [SerializeField] private int m_SampleIncrement = 2;

        [Header("Box")] [SerializeField] private Vector2Int m_HalfSampleCount = new Vector2Int(1, 1);

        [SerializeField] private bool m_UseGraphicAlpha = true;

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
                targetColor.a *= graphic.color.a; //乘以原始定点色A通道
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
                case OutlineStyle.Circle:
                {
                    var count = 0;
                    var original = verts.Count;
                    var sampleCount = m_FirstSample;

                    var dx = m_EffectOffset.x / m_CircleCount;
                    var dy = m_EffectOffset.y / m_CircleCount;
                    for (int i = 1; i <= m_CircleCount; i++)
                    {
                        var rx = dx * i;
                        var ry = dy * i;
                        var radStep = 2 * Mathf.PI / sampleCount;
                        var rad = (i % 2) * radStep * 0.5f;
                        for (int j = 0; j < sampleCount; j++)
                        {
                            var next = count + original;
                            ApplyShadow(verts, color, count, next, rx * Mathf.Cos(rad), ry * Mathf.Sin(rad));
                            count = next;
                            rad += radStep;
                        }

                        sampleCount += m_SampleIncrement;
                    }
                }
                    break;
                case OutlineStyle.Box:
                {
                    var original = verts.Count;
                    var count = 0;
                    var dx = m_EffectOffset.x / m_HalfSampleCount.x;
                    var dy = m_EffectOffset.y / m_HalfSampleCount.y;
                    for (int ix = -m_HalfSampleCount.x; ix <= m_HalfSampleCount.x; ix++)
                    {
                        for (int iy = -m_HalfSampleCount.y; iy <= m_HalfSampleCount.y; iy++)
                        {
                            if (!(ix == 0 && iy == 0))
                            {
                                var next = count + original;
                                ApplyShadow(verts, color, count, next, dx * ix, dy * iy);
                                count = next;
                            }
                        }
                    }
                }
                    break;
            }
        }


        protected new void ApplyShadow(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
        {
            UIVertex vt;

            // The capacity calculation of the original version seems wrong.
            var neededCpacity = verts.Count + (end - start);
            if (verts.Capacity < neededCpacity)
                verts.Capacity = neededCpacity;

            for (int i = start; i < end; ++i)
            {
                vt = verts[i];
                verts.Add(vt);

                Vector3 v = vt.position;
                v.x += x;
                v.y += y;
                vt.position = v;
                var newColor = color;
                if (m_UseGraphicAlpha)
                    newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
                vt.color = newColor;
                verts[i] = vt;
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
            m_Style = OutlineStyle.Outline8Split;
            m_EffectColor = new Color(0f, 0f, 0f, 1f);
            m_EffectOffset = new Vector2(3f, -3f);
            m_EffectOffset2 = new Vector2(3f, -6f);
        }
#endif
    }
}