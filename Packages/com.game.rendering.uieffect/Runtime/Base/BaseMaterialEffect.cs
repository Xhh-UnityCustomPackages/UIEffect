using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    public abstract class BaseMaterialEffect : BaseUIEffect
    {
        public abstract string MaterialKeyWord { get; }
        public virtual bool InstantiateMaterial => false;
        private bool m_MaterialParamDirty = false;
        private Material m_ModifierMaterial;

        public Material ModifierMaterial
        {
            get => m_ModifierMaterial;
            set => m_ModifierMaterial = value;
        }

        public void SetMaterialDirty()
        {
        }

        public virtual void ModifyMaterial(Material newMaterial)
        {
            UpdateMaterialParams(newMaterial);
        }

        public virtual void SetMaterialParamsDirty()
        {
            m_MaterialParamDirty = true;
            if (m_MaterialParamDirty && ModifierMaterial != null)
            {
                UpdateMaterialParams(ModifierMaterial);
                m_MaterialParamDirty = false;
            }
        }

        public virtual void UpdateMaterialParams(Material material)
        {
        }
    }
}