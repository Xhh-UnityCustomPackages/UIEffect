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
        private static readonly StringBuilder s_StringBuilder = new StringBuilder();
        private static List<string> s_KeywordList = new List<string>();

        RectTransform _rectTransform;
        Graphic _graphic;

        public Graphic graphic => _graphic ? _graphic : _graphic = GetComponent<Graphic>();
        public RectTransform rectTransform => _rectTransform ? _rectTransform : _rectTransform = GetComponent<RectTransform>();

        [SerializeReference]
        public List<BaseUIEffect> m_UIEffects = new();


        public List<BaseUIEffect> UIEffects => m_UIEffects;


        public void AddUIEffect(BaseUIEffect uiEffect)
        {
            m_UIEffects.Add(uiEffect);
            // Debug.LogError($"m_UIEffects.Length:{m_UIEffects.Count} {uiEffect.GetType().Name}");
            if (uiEffect is BaseMeshEffect meshEffect)
            {
                SetVerticesDirty();
            }
            else if (uiEffect is BaseMaterialEffect materialEffect)
            {
                SetMaterialDirty();
            }
        }

        public void RemoveUIEffect(int index)
        {
            if (m_UIEffects.Count < index)
            {
                return;
            }
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
        public void ModifyMesh(Mesh mesh) { }

        public void ModifyMesh(VertexHelper vh)
        {
            ModifyMesh(vh, graphic);
        }

        public virtual void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled) return;

            foreach (var effect in m_UIEffects)
            {
                if (!effect.Active)
                    continue;

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
        public virtual Material GetModifiedMaterial(Material baseMaterial, Graphic graphic)
        {
            if (!isActiveAndEnabled) return baseMaterial;

            baseMaterial.shaderKeywords = null;
            s_KeywordList.Clear();
            bool hasMaterialEffect = false;
            foreach (var effect in m_UIEffects)
            {
                if (!effect.Active)
                    continue;

                if (effect is BaseMaterialEffect materialEffect)
                {
                    if (!string.IsNullOrEmpty(materialEffect.MaterialKeyWord))
                        s_KeywordList.Add(materialEffect.MaterialKeyWord);
                    hasMaterialEffect = true;
                }
            }

            if (!hasMaterialEffect)
                return baseMaterial;

            var modifiedMaterial = MaterialCache.GetMaterial(this.GetInstanceID(), baseMaterial, graphic);
            SetShaderVariants(modifiedMaterial, s_KeywordList);
            ModifyMaterial(modifiedMaterial);
            return modifiedMaterial;
        }

        protected void ModifyMaterial(Material newMaterial)
        {
            foreach (var effect in m_UIEffects)
            {
                if (!effect.Active)
                    continue;

                if (effect is BaseMaterialEffect materialEffect)
                {
                    materialEffect.ModifyMaterial(newMaterial);
                }
            }
        }

        protected void SetShaderVariants(Material newMaterial, List<string> variants)
        {
            var keywords = variants
                .Select(x => x.ToString().ToUpper())
                // .Concat(newMaterial.shaderKeywords)//加上原来的
                .Distinct()
                .ToArray();

            newMaterial.shaderKeywords = keywords;

            // Add variant name
            s_StringBuilder.Clear();
            s_StringBuilder.Length = 0;
            s_StringBuilder.Append(newMaterial.shader.name);
            foreach (var keyword in keywords)
            {
                s_StringBuilder.Append("|");
                s_StringBuilder.Append(keyword);
            }

            newMaterial.name = s_StringBuilder.ToString();
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
            // connector.OnEnable(graphic);
            // SetVerticesDirty();

            if (m_UIEffects == null)
                return;

            foreach (var effect in m_UIEffects)
            {
                effect.Init(this);
            }
            SetVerticesDirty();
            SetMaterialDirty();
        }

        protected override void OnDisable()
        {
            SetVerticesDirty();
        }

        public void SetVerticesDirty()
        {
            graphic.SetVerticesDirty();
        }

        public void SetMaterialDirty()
        {
            graphic.SetMaterialDirty();
        }


    }
}
