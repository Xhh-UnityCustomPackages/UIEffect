using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [UIEffectPath("UIDissolve")]
    public class UIDissolve : BaseMaterialEffect
    {
        public override string MaterialKeyWord => "DISSOLVE";

        [SerializeField] Texture2D m_DissolveTex = null;
        [SerializeField] float m_DissolveTexTilling = 1;

        [SerializeField, Range(0, 1)]
        float m_EffectFactor = 0.5f;

        [Tooltip("Edge width.")]
        [SerializeField, Range(0, 1)]
        float m_Width = 0.5f;

        [Tooltip("Edge softness.")]
        [SerializeField, Range(0, 1)]
        float m_Softness = 0.5f;

        [Tooltip("Edge color.")]
        [SerializeField, ColorUsage(false)]
        Color m_Color = new Color(0.0f, 0.25f, 1.0f);

        private static Texture _defaultTransitionTexture;
        private static Texture defaultTransitionTexture
        {
            get
            {
                return _defaultTransitionTexture
                    ? _defaultTransitionTexture
                    : (_defaultTransitionTexture = Resources.Load<Texture>("Default-Transition"));
            }
        }

        public float effectFactor
        {
            get { return m_EffectFactor; }
            set { m_EffectFactor = value; }
        }

        static class ShaderConstants
        {
            internal static readonly int DissolveParams = Shader.PropertyToID("_DissolveParams");
            internal static readonly int DissolveColor = Shader.PropertyToID("_DissolveColor");
            internal static readonly int DissolveTex = Shader.PropertyToID("_DissolveTex");
        }

        public override void ModifyMaterial(Material newMaterial)
        {
            newMaterial.SetVector(ShaderConstants.DissolveParams, new Vector4(m_EffectFactor, m_Width, m_Softness, m_DissolveTexTilling));
            newMaterial.SetColor(ShaderConstants.DissolveColor, m_Color);

            if (m_DissolveTex != null)
                newMaterial.SetTexture(ShaderConstants.DissolveTex, m_DissolveTex);
            else
                newMaterial.SetTexture(ShaderConstants.DissolveTex, defaultTransitionTexture);
        }
    }
}
