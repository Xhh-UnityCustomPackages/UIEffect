using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Text;
using System.Linq;

namespace Game.Core.UIEffect
{
    [RequireComponent(typeof(Graphic))]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UIEffect : UIBehaviour, IMeshModifier, IMaterialModifier
    {
        private static List<string> s_KeywordList = new List<string>();

        RectTransform _rectTransform;
        Graphic _graphic;

        public Graphic graphic => _graphic != null ? _graphic : _graphic = GetComponent<Graphic>();
        public RectTransform rectTransform => _rectTransform ? _rectTransform : _rectTransform = GetComponent<RectTransform>();

        [SerializeReference] public List<BaseUIEffect> m_UIEffects = new();


        public List<BaseUIEffect> UIEffects => m_UIEffects;
        private Material m_ModifMaterial;
        private Hash128 m_MaterialHashCode;

        public T GetUIEffect<T>() where T : BaseUIEffect
        {
            foreach (var effect in m_UIEffects)
            {
                if (effect is T t)
                {
                    return t;
                }
            }

            return null;
        }


        public void AddUIEffect(BaseUIEffect uiEffect)
        {
            m_UIEffects.Add(uiEffect);

            if (uiEffect is BaseMeshEffect meshEffect) SetVerticesDirty();
            else if (uiEffect is BaseMaterialEffect materialEffect) SetMaterialDirty();
        }

        public void RemoveUIEffect(int index)
        {
            if (m_UIEffects.Count < index)
            {
                return;
            }

            m_UIEffects[index].OnDisable();
            m_UIEffects.RemoveAt(index);
        }

        public BaseUIEffect AddUIEffect(System.Type uiEffectType)
        {
            foreach (var effect in m_UIEffects)
            {
                if (effect.GetType() == uiEffectType)
                    return null;
            }

            BaseUIEffect newUIEffect = (BaseUIEffect)Activator.CreateInstance(uiEffectType);
            newUIEffect.Init(this);
            AddUIEffect(newUIEffect);
            return newUIEffect;
        }

        #region IMeshModifier

        public void ModifyMesh(Mesh mesh)
        {
        }

        public void ModifyMesh(VertexHelper vh)
        {
            ModifyMesh(vh, graphic);
        }

        public virtual void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled) return;

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

            foreach (var effect in m_UIEffects)
            {
                if (effect == null) continue;
                if (!effect.Active) continue;

                if (effect is BaseMeshEffect meshEffect)
                    meshEffect.ModifyMesh(vh, graphic);
            }
        }

        #endregion IMeshModifier

        #region IMaterialModifier

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            return GetModifiedMaterial(baseMaterial, graphic);
        }

        private const uint k_ShaderId = 2 << 3;

        Hash128 CalcMaterialHash(Material baseMaterial)
        {
            s_KeywordList.Sort();
            uint hashCode = 0;
            foreach (var effect in s_KeywordList)
            {
                hashCode = (hashCode * 397) ^ (uint)effect.GetHashCode();
            }

            return new Hash128(
                (uint)baseMaterial.GetInstanceID(),
                k_ShaderId + hashCode,
                0,
                0
            );
        }

        public virtual Material GetModifiedMaterial(Material baseMaterial, Graphic graphic)
        {
            if (!isActiveAndEnabled) return baseMaterial;

            baseMaterial.shaderKeywords = null;
            s_KeywordList.Clear();
            bool hasMaterialEffect = false;
            foreach (var effect in m_UIEffects)
            {
                if (effect == null) continue;
                if (!effect.Active) continue;

                if (effect is BaseMaterialEffect materialEffect)
                {
                    if (!string.IsNullOrEmpty(materialEffect.MaterialKeyWord))
                        s_KeywordList.Add(materialEffect.MaterialKeyWord);
                    hasMaterialEffect = true;
                }
            }

            if (!hasMaterialEffect)
            {
                if (m_ModifMaterial != null)
                {
                    if (Application.isPlaying)
                        Destroy(m_ModifMaterial);
                    else
                        DestroyImmediate(m_ModifMaterial);
                }

                return baseMaterial;
            }

            var hashCode = CalcMaterialHash(baseMaterial);
            if (m_ModifMaterial == null || m_MaterialHashCode != hashCode)
            {
                m_MaterialHashCode = hashCode;
                var modifiedMaterial = MaterialCache.GetMaterial(hashCode, baseMaterial, graphic, s_KeywordList);
                m_ModifMaterial = (modifiedMaterial);
            }

            ModifyMaterial(m_ModifMaterial);
            return m_ModifMaterial;
        }

        protected void ModifyMaterial(Material newMaterial)
        {
            foreach (var effect in m_UIEffects)
            {
                if (!effect.Active)
                    continue;

                if (effect is BaseMaterialEffect materialEffect)
                {
                    materialEffect.ModifierMaterial = newMaterial;
                    materialEffect.ModifyMaterial(newMaterial);
                }
            }
        }

        #endregion IMaterialModifier


#if UNITY_EDITOR
        protected override void Reset()
        {
            if (!isActiveAndEnabled) return;
            m_UIEffects.Clear();

            SetVerticesDirty();
            SetMaterialDirty();
        }

        /// <summary>
        /// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
        /// </summary>
        protected override void OnValidate()
        {
            if (!isActiveAndEnabled) return;
            SetVerticesDirty();
            SetMaterialDirty();
        }
#endif


        protected override void OnEnable()
        {
            if (m_UIEffects == null)
                return;

            foreach (var effect in m_UIEffects)
            {
                if (effect == null) continue;
                effect.Init(this);
            }

            SetVerticesDirty();
            SetMaterialDirty();
        }

        protected override void OnDisable()
        {
            SetVerticesDirty();
            SetMaterialDirty();
        }

        public void SetVerticesDirty()
        {
            graphic.SetVerticesDirty();
        }

        public void SetMaterialDirty()
        {
            graphic.SetMaterialDirty();
        }

        //当响应动画时
        protected override void OnDidApplyAnimationProperties()
        {
            if (!isActiveAndEnabled) return;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }
}