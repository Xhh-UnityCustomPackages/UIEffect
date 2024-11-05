using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [RequireComponent(typeof(RectTransform))]
    public class GraphicGroup : MonoBehaviour
    {
        public enum Mode
        {
            Single,
            Horizontal,
            Vertical
        }

        [SerializeField] private Mode m_Mode = Mode.Single;
        [SerializeField] private Color m_Color = Color.white;
        [SerializeField] private Color m_Color2 = Color.white;
        [SerializeField] private RectTransform m_RectTransform;

        [SerializeField, Range(-1, 1)] private float m_Offset1;
        
        public Color Color
        {
            get => m_Color;
            set
            {
                m_Color = value;
                UpdateColor(m_Color);
            }
        }

        float rotation
        {
            get
            {
                return m_Mode == Mode.Horizontal ? -90
                : m_Mode == Mode.Vertical ? 0
                : 0;
            }
        }

        public float offset
        {
            get => m_Offset1;
            set
            {
                if (Mathf.Approximately(m_Offset1, value)) return;
                m_Offset1 = value;
            }
        }

        [System.NonSerialized] private static readonly VertexHelper s_VertexHelper = new VertexHelper();

        protected virtual void OnPopulateMesh(RectTransform rect, Color color, VertexHelper vh)
        {
            var r = rect.rect;
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

            Color32 color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(1f, 0f));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        public void UpdateColor(Color color)
        {
            //找到所有的Graphic
            var graphics = GetComponentsInChildren<Graphic>();
            if (m_Mode == Mode.Single)
            {
                for (int i = 0; i < graphics.Length; i++)
                {
                    graphics[i].canvasRenderer.SetColor(color);
                }
            }
            else
            {
                // Gradient rotation.
                var rad = rotation * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                var rect = m_RectTransform.rect;
                var localMatrix = new Matrix2x3(rect, dir.x, dir.y); // Get local matrix.
                for (int i = 0; i < graphics.Length; i++)
                {
                    var originMesh = graphics[i].canvasRenderer.GetMesh();
                    if (originMesh == null) continue;
                    var mesh = Instantiate(originMesh);

                    using (var vh = new VertexHelper(mesh))
                    {
                        var vertex = default(UIVertex);

                        for (var v = 0; v < vh.currentVertCount; v++)
                        {
                            vh.PopulateUIVertex(ref vertex, v);

                            Vector2 normalizedPos = localMatrix * vertex.position + new Vector2(offset, offset);
                            color = Color.LerpUnclamped(m_Color2, m_Color, normalizedPos.y);

                            vertex.color = color;

                            vh.SetUIVertex(vertex, v);
                        }


                        vh.FillMesh(mesh);
                    }
                    graphics[i].canvasRenderer.SetMesh(mesh);
                }
            }
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            UpdateColor(Color.white);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateColor(m_Color);
        }

        private void Reset()
        {
            if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
        }
#endif

    }
}
