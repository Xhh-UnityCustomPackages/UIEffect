using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [UIEffectPath("UIFadeLoop")]

    public class UIFadeLoop : BaseMaterialEffect
    {
        public override string MaterialKeyWord => "_FADELOOP_ON";
        [SerializeField, Min(0f)] private float m_FadeSpeed = 1;

        static class ShaderConstants
        {
            internal static readonly int FadeSpeed = Shader.PropertyToID("_FadeSpeed");
        }

        public override void ModifyMaterial(Material newMaterial)
        {
            newMaterial.SetFloat(ShaderConstants.FadeSpeed, m_FadeSpeed);
        }

    }
}
