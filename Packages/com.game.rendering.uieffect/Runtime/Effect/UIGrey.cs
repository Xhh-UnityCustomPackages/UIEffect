using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [UIEffectPath("UIGrey")]
    public class UIGrey : BaseMaterialEffect
    {
        public override string MaterialKeyWord => "GREY";
        [SerializeField, Range(0, 1)] private float m_GreyIntensity = 1;


        static class ShaderConstants
        {
            internal static readonly int GreyFactor = Shader.PropertyToID("_EffectFactor");
        }

        public override void ModifyMaterial(Material newMaterial)
        {
            newMaterial.SetFloat(ShaderConstants.GreyFactor, m_GreyIntensity);
        }
    }
}
