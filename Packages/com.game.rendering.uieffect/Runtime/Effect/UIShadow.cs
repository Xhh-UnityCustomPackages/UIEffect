using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [UIEffectPath("UIShadow")]
    public class UIShadow : BaseMeshEffect
    {
        [SerializeField] private Color m_EffectColor = new Color(0f, 0f, 0f, 0.5f);
        [SerializeField] private Vector2 m_EffectDistance = new Vector2(10f, -10f);
        [SerializeField] private bool m_UseGraphicAlpha = true;


        public Color effectColor
        {
            get { return m_EffectColor; }
            set
            {
                if (m_EffectColor == value) return;
                m_EffectColor = value;
                SetVerticesDirty();
            }
        }

        private const float kMaxEffectDistance = 600f;
        public Vector2 effectDistance
        {
            get { return m_EffectDistance; }
            set
            {
                if (value.x > kMaxEffectDistance)
                    value.x = kMaxEffectDistance;
                if (value.x < -kMaxEffectDistance)
                    value.x = -kMaxEffectDistance;

                if (value.y > kMaxEffectDistance)
                    value.y = kMaxEffectDistance;
                if (value.y < -kMaxEffectDistance)
                    value.y = -kMaxEffectDistance;

                if (m_EffectDistance == value) return;
                m_EffectDistance = value;
                SetEffectParamsDirty();
            }
        }

        public bool useGraphicAlpha
        {
            get { return m_UseGraphicAlpha; }
            set
            {
                if (m_UseGraphicAlpha == value) return;
                m_UseGraphicAlpha = value;
                SetEffectParamsDirty();
            }
        }


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

                ApplyShadow(s_Verts, effectColor, ref start, ref end, effectDistance, useGraphicAlpha);
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(s_Verts);

            s_Verts.Clear();

        }

        private void ApplyShadow(List<UIVertex> verts, Color color, ref int start, ref int end, Vector2 distance, bool alpha)
        {
            if (color.a <= 0)
                return;

            var x = distance.x;
            var y = distance.y;

            // Append Shadow.
            ApplyShadowZeroAlloc(verts, color, ref start, ref end, x, y, alpha);
        }

        private void ApplyShadowZeroAlloc(List<UIVertex> verts, Color color, ref int start, ref int end, float x, float y, bool alpha)
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
                var vertColor = effectColor;
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
            m_EffectDistance = new Vector2(10f, -10f);
        }
#endif
    }
}
