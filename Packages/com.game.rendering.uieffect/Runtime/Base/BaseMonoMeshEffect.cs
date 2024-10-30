using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [RequireComponent(typeof(Graphic))]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class BaseMonoMeshEffect : UIBehaviour, IMeshModifier
    {
        RectTransform _rectTransform;
        Graphic _graphic;

        /// <summary>
        /// The Graphic attached to this GameObject.
        /// </summary>
        public Graphic graphic
        {
            get { return _graphic ? _graphic : _graphic = GetComponent<Graphic>(); }
        }

        /// <summary>
        /// The RectTransform attached to this GameObject.
        /// </summary>
        protected RectTransform rectTransform
        {
            get { return _rectTransform ? _rectTransform : _rectTransform = GetComponent<RectTransform>(); }
        }

        /// <summary>
        /// Call used to modify mesh. (legacy)
        /// </summary>
        /// <param name="mesh">Mesh.</param>
        public virtual void ModifyMesh(Mesh mesh)
        {
        }

        /// <summary>
        /// Call used to modify mesh.
        /// </summary>
        /// <param name="vh">VertexHelper.</param>
        public virtual void ModifyMesh(VertexHelper vh)
        {
            ModifyMesh(vh, graphic);
        }

        public virtual void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled)
                return;

            var rect = rectTransform.rect;

            // Calculate vertex position.
            var vertex = default(UIVertex);
            var count = vh.currentVertCount;
            for (var i = 0; i < count; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);

                float x = Mathf.Clamp01(vertex.position.x / rect.width + 0.5f);
                float y = Mathf.Clamp01(vertex.position.y / rect.height + 0.5f);

                //等于记录图集UV 和原始UV
                vertex.uv0 = new Vector4(vertex.uv0.x, vertex.uv0.y, x, y);

                vh.SetUIVertex(vertex, i);
            }
        }
    }
}
