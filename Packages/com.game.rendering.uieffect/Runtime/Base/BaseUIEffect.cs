using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [RequireComponent(typeof(Graphic))]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    [Serializable]
    public abstract class BaseUIEffect
    {
        public Graphic graphic => m_UIEffect.graphic;
        protected RectTransform rectTransform => m_UIEffect.rectTransform;
        protected bool isActiveAndEnabled => m_UIEffect != null ? m_UIEffect.isActiveAndEnabled : false;
        [SerializeField] protected bool m_Active = true;
        public bool Active
        {
            get => m_Active;
            set
            {
                var oldValue = m_Active;
                m_Active = value;
                if (oldValue == true && m_Active == false)
                {
                    OnDisable();
                }else if(oldValue == false && m_Active==true)
                {
                    OnEnable();
                }
            }
        }
        
        [NonSerialized] protected UIEffect m_UIEffect;

        public void Init(UIEffect uiEffect)
        {
            m_UIEffect = uiEffect;
            OnInit();
        }

        public virtual void OnInit() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }


#if UNITY_EDITOR
        public virtual bool IsExpanded { get; set; }
        public virtual bool HasCustomInspectors => false;
        public virtual void Reset() { }
#endif
    }
}