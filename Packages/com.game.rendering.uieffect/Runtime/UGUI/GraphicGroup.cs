using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    public class GraphicGroup : MonoBehaviour
    {
        [SerializeField] private Color m_Color = Color.white;


        public Color Color
        {
            get => m_Color;
            set 
            {
                m_Color = value;
                UpdateColor(m_Color);
            }
        }

        public void UpdateColor(Color color)
        {
            //找到所有的Graphic
            var graphics = GetComponentsInChildren<Graphic>();
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].canvasRenderer.SetColor(color);
            }
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            UpdateColor(Color.white);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateColor(m_Color);
        }
#endif

    }
}
