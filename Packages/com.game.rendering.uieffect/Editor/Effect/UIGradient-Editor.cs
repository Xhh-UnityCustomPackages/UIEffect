using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Core.UIEffect.Editor
{
    public class UIGradient_Editor
    {
        public static void DrawInspector(UIGradient gradient)
        {
            EditorGUI.BeginChangeCheck();
            gradient.direction = (UIGradient.Direction)EditorGUILayout.EnumPopup(gradient.direction);
            if (EditorGUI.EndChangeCheck())
            {
                gradient.graphic.SetVerticesDirty();
            }

            switch (gradient.direction)
            {
                case UIGradient.Direction.Horizontal:
                    gradient.color1 = EditorGUILayout.ColorField("Left", gradient.color1);
                    gradient.color2 = EditorGUILayout.ColorField("Right", gradient.color2);
                    gradient.offset1 = EditorGUILayout.Slider("Offset", gradient.offset1, -1, 1);
                    break;

                case UIGradient.Direction.Vertical:
                    gradient.color1 = EditorGUILayout.ColorField("Top", gradient.color1);
                    gradient.color2 = EditorGUILayout.ColorField("Bottom", gradient.color2);
                    gradient.offset1 = EditorGUILayout.Slider("Offset", gradient.offset1, -1, 1);
                    break;
            }
        }
    }
}