using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [UIEffectPath("UIShiny")]
    public class UIShiny : BaseMaterialEffect
    {
        public override string MaterialKeyWord => "UISHINY";

        [SerializeField, Range(0, 1)] private float m_EffectFactor = 0.5f;

        [Tooltip("Width for shiny effect.")]
        [SerializeField, Range(0, 1)] private float m_Width = 0.25f;

        [Tooltip("Rotation for shiny effect.")]
        [SerializeField, Range(-180, 180)] private float m_Rotation = 0;

        [Tooltip("Softness for shiny effect.")]
        [SerializeField, Range(0.01f, 1)] private float m_Softness = 1f;

        [Tooltip("Brightness for shiny effect.")]
        [SerializeField, Range(0, 1)] private float m_Brightness = 1f;

        [Tooltip("Gloss factor for shiny effect.")]
        [SerializeField, Range(0, 1)] private float m_Gloss = 1;

        [SerializeField] private EffectPlayer m_Player;


        public float EffectFactor
        {
            get => m_EffectFactor;
            set
            {
                m_EffectFactor = value;
                SetMaterialParamsDirty();
            }
        }

        public float Width
        {
            get { return m_Width; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_Width, value)) return;
                m_Width = value;
                SetMaterialParamsDirty();
            }
        }

        public float Rotation
        {
            get { return m_Rotation; }
            set
            {
                if (Mathf.Approximately(m_Rotation, value)) return;
                m_Rotation = value;
                SetMaterialParamsDirty();
            }
        }

        public float Softness
        {
            get { return m_Softness; }
            set
            {
                value = Mathf.Clamp(value, 0.01f, 1);
                if (Mathf.Approximately(m_Softness, value)) return;
                m_Softness = value;
                SetMaterialParamsDirty();
            }
        }

        public float Brightness
        {
            get { return m_Brightness; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_Brightness, value)) return;
                m_Brightness = value;
                SetMaterialParamsDirty();
            }
        }

        public float Gloss
        {
            get { return m_Gloss; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_Gloss, value)) return;
                m_Gloss = value;
                SetMaterialParamsDirty();
            }
        }

        static class ShaderConstants
        {
            internal static readonly int _ShinyParams1 = Shader.PropertyToID("_ShinyParams1");
            internal static readonly int _ShinyParams2 = Shader.PropertyToID("_ShinyParams2");
        }

        public override void OnInit()
        {
            OnEnable();
        }

        public override void OnEnable()
        {
            if (m_Player == null) m_Player = new EffectPlayer();
            m_Player.OnEnable(f => EffectFactor = f);
        }

        public override void OnDisable()
        {
            if (m_Player != null)
                m_Player.OnDisable();
        }

        public override void UpdateMaterialParams(Material newMaterial)
        {
            newMaterial.SetVector(ShaderConstants._ShinyParams1, new Vector4(m_EffectFactor, m_Width, m_Softness, m_Brightness));
            newMaterial.SetVector(ShaderConstants._ShinyParams2, new Vector4(m_Gloss, m_Rotation, 0, 0));
        }

#if UNITY_EDITOR
        public override void Reset()
        {
            m_Rotation = 0;
            m_EffectFactor = 0.5f;
            m_Softness = 1f;
            m_Brightness = 1f;
            m_Gloss = 1;
        }
#endif
    }
}
