using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    public abstract class BaseMeshEffect : BaseUIEffect, IMeshModifier
    {
        protected static readonly List<UIVertex> s_Verts = new List<UIVertex>(4096);
        protected int _graphicVertexCount;

        protected virtual void SetVerticesDirty()
        {
            graphic.SetVerticesDirty();
        }

        protected virtual void SetEffectParamsDirty()
        {
            if (!isActiveAndEnabled) return;
            SetVerticesDirty();
        }


        public void ModifyMesh(Mesh mesh)
        {
        }

        public void ModifyMesh(VertexHelper verts)
        {
            ModifyMesh(verts, graphic);
        }

        public abstract void ModifyMesh(VertexHelper vh, Graphic graphic);
    }
}
