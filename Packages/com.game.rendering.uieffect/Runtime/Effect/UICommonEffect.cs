using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [UIEffectPath("UICommonEffect")]
    public class UICommonEffect : BaseMaterialEffect
    {
        public enum ColorMode
        {
            Multiply = 0,
            Fill = 1,
        }
        public enum EffectMode
        {
            None = 0,
            Grayscale = 1,
        }

        public override string MaterialKeyWord => "";

        [SerializeField] private ColorMode m_ColorMode = ColorMode.Multiply;
        [SerializeField] private EffectMode m_EffectMode = EffectMode.None;
        [SerializeField, Range(0, 1)] private float m_EffectFactor = 1;

        static class ShaderConstants
        {
            internal static readonly int EffectFactor = Shader.PropertyToID("_EffectFactor");
        }


        public override void ModifyMaterial(Material newMaterial)
        {
            if (m_ColorMode == ColorMode.Fill)
                newMaterial.EnableKeyword("FILL");

            switch (m_EffectMode)
            {
                case EffectMode.Grayscale: newMaterial.EnableKeyword("GREY"); break;
            }
            newMaterial.SetFloat(ShaderConstants.EffectFactor, m_EffectFactor);
        }

    }
}
