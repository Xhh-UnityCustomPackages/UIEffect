using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class GraphicMaterial : MonoBehaviour
    {
        public static Vector3 UIRootPosition = Vector3.one * 1000;
        [SerializeField] private RectTransform m_RectTransform;
        [SerializeField] private Material m_Material;

        [Header("MaterialProp")]
        [SerializeField] private float _Dissolve_progress;
        [SerializeField] private float _EdgeIntensity;
        [SerializeField] private float _Gradient_Intensity;

        private Material m_OverrideMaterial;


        private void OnEnable()
        {
            UpdateMaterial();
        }

        private void OnDisable()
        {
            RestoreMaterial();
            if (!Application.isPlaying)
                DestroyImmediate(m_OverrideMaterial);
            else
                Destroy(m_OverrideMaterial);
        }

        private void Update()
        {
            SetMaterialParam();
        }

        void SetMaterialParam()
        {
            if (m_OverrideMaterial == null || m_RectTransform == null)
                return;

            var position = m_RectTransform.position - UIRootPosition;
            var offset = new Vector3(position.x / m_RectTransform.lossyScale.x, position.y / m_RectTransform.lossyScale.y, position.z / m_RectTransform.lossyScale.z);
            var anchoredPosition = offset;

            float halfWidth = m_RectTransform.rect.width * 0.5f;
            float halfHeight = m_RectTransform.rect.height * 0.5f;
            m_OverrideMaterial.SetVector("_EffectRect", new Vector4(position.x + anchoredPosition.x - halfWidth, position.y + anchoredPosition.y - halfHeight, halfWidth + anchoredPosition.x, halfHeight + anchoredPosition.y));

            m_OverrideMaterial.SetFloat("_Dissolve_progress", _Dissolve_progress);
            m_OverrideMaterial.SetFloat("_EdgeIntensity", _EdgeIntensity);
            m_OverrideMaterial.SetFloat("_Gradient_Intensity", _Gradient_Intensity);
        }

        void RestoreMaterial()
        {
            if (m_RectTransform == null)
                return;
            var graphics = GetComponentsInChildren<Graphic>();
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i].canvasRenderer.materialCount > 0)
                    graphics[i].canvasRenderer.SetMaterial(graphics[i].defaultMaterial, 0);
            }
        }

        void UpdateMaterial()
        {
            if (m_RectTransform == null)
                return;
            var graphics = GetComponentsInChildren<Graphic>();

            if (m_Material == null)
            {
                RestoreMaterial();
                return;
            }

            SetMaterialParam();

            if (m_OverrideMaterial == null)
                m_OverrideMaterial = Material.Instantiate(m_Material);

            //找到所有的Graphic
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].canvasRenderer.materialCount = 1;
                graphics[i].canvasRenderer.SetMaterial(m_OverrideMaterial, 0);
            }
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_RectTransform == null)
                m_RectTransform = GetComponent<RectTransform>();
            UpdateMaterial();
        }
#endif
    }
}
