using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [UIEffectPath("UIRotate")]
    public class UIRotate : BaseMaterialEffect
    {
        public override string MaterialKeyWord => "_ROTATE_ON";

        [SerializeField, Min(0f)] private float m_RotateSpeed = 1;
        // [SerializeField] private Vector2 m_RotateCenter = new Vector2(0.5f, 0.5f);

        static class ShaderConstants
        {
            internal static readonly int RotateSpeed = Shader.PropertyToID("_RotateSpeed");
            internal static readonly int RotateCenter = Shader.PropertyToID("_RotateCenter");
        }

        public override void UpdateMaterialParams(Material newMaterial)
        {
            newMaterial.SetFloat(ShaderConstants.RotateSpeed, m_RotateSpeed);
            // newMaterial.SetVector(ShaderConstants.RotateCenter, m_RotateCenter);
        }


#if UNITY_EDITOR
        public override void Reset()
        {
            m_RotateSpeed = 1;
            // m_RotateCenter = new Vector2(0.5f, 0.5f);
        }
#endif
    }
}
