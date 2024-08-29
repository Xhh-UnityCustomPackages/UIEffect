using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UIEffect
{
    public class GraphicGroup : MonoBehaviour
    {
        [SerializeField] Color color;

        private void OnValidate()
        {
            //找到所有的Graphic
            var graphics = GetComponentsInChildren<Graphic>();
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].CrossFadeColor(color, 0.1f, true, true);
            }
        }

    }
}
