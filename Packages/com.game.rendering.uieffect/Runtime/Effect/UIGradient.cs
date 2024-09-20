using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [UIEffectPath("UIGradient")]
    public class UIGradient : BaseMeshEffect
    {
#if UNITY_EDITOR
        public override bool HasCustomInspectors => true;

        public Direction direction
        {
            get => m_Direction;
            set => m_Direction = value;
        }

        public Color color1
        {
            get => m_Color1;
            set => m_Color1 = value;
        }

        public Color color2
        {
            get => m_Color2;
            set => m_Color2 = value;
        }

        public Color color3
        {
            get => m_Color3;
            set => m_Color3 = value;
        }

        public Color color4
        {
            get => m_Color4;
            set => m_Color4 = value;
        }

        public float offset1
        {
            get => m_Offset1;
            set => m_Offset1 = value;
        }

        public float offset2
        {
            get => m_Offset2;
            set => m_Offset2 = value;
        }
#endif

        public enum Direction
        {
            Horizontal,
            Vertical,

            // Angle,
            Split, //四个角分别设置
        }

        [SerializeField] private Direction m_Direction;

        [Tooltip("Color1: Top or Left.")] [SerializeField]
        private Color m_Color1 = Color.white;

        [Tooltip("Color2: Bottom or Right.")] [SerializeField]
        private Color m_Color2 = Color.white;

        [Tooltip("Color3: Left.")] [SerializeField]
        private Color m_Color3 = Color.white;

        [Tooltip("Color4: Right.")] [SerializeField]
        private Color m_Color4 = Color.white;

        [Tooltip("Gradient rotation.")] [SerializeField] [Range(-180, 180)]
        float m_Rotation;

        [Tooltip("Gradient offset for Horizontal, Vertical or Angle.")] [SerializeField] [Range(-1, 1)]
        float m_Offset1;

        [Tooltip("Gradient offset for Diagonal.")] [SerializeField] [Range(-1, 1)]
        float m_Offset2;

        public Vector2 offset
        {
            get => new Vector2(m_Offset1, m_Offset2);
            set
            {
                if (Mathf.Approximately(m_Offset1, value.x) && Mathf.Approximately(m_Offset2, value.y)) return;
                m_Offset1 = value.x;
                m_Offset2 = value.y;
                SetVerticesDirty();
            }
        }

        float rotation
        {
            get
            {
                return m_Direction == Direction.Horizontal ? -90
                    : m_Direction == Direction.Vertical ? 0
                    : m_Rotation;
            }
            set
            {
                if (Mathf.Approximately(m_Rotation, value)) return;
                m_Rotation = value;
                SetVerticesDirty();
            }
        }

        public override void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            var rect = default(Rect);
            rect = graphic.rectTransform.rect;
            var vertex = default(UIVertex);

            // Gradient rotation.
            var rad = rotation * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // Calculate vertex color.
            var localMatrix = new Matrix2x3(rect, dir.x, dir.y); // Get local matrix.
            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);

                // Normalize vertex position by local matrix.
                Vector2 normalizedPos;
                // if (m_GradientStyle == GradientStyle.Split)
                // {
                //     // Each characters.
                //     normalizedPos = localMatrix * s_SplitedCharacterPosition[i % 4] + offset2;
                // }
                // else
                // {
                normalizedPos = localMatrix * vertex.position + offset;
                // }

                // Interpolate vertex color.
                Color color = Color.white;
                if (direction == Direction.Split)
                {
                    color = Color.LerpUnclamped(
                        Color.LerpUnclamped(m_Color1, m_Color2, normalizedPos.x),
                        Color.LerpUnclamped(m_Color3, m_Color4, normalizedPos.x),
                        normalizedPos.y);
                }
                else
                {
                    color = Color.LerpUnclamped(m_Color2, m_Color1, normalizedPos.y);
                }

                // Correct color.
                // vertex.color *= (m_ColorSpace == ColorSpace.Gamma) ? color.gamma
                //     : (m_ColorSpace == ColorSpace.Linear) ? color.linear
                //     : color;

                vertex.color *= color;

                vh.SetUIVertex(vertex, i);
            }
        }
    }
}