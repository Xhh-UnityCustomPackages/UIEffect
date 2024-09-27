using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Core.UIEffect.Editor
{
    public class UIOutline_Editor
    {
        public static void DrawInspector(UIOutline effect)
        {
            EditorGUI.BeginChangeCheck();
            effect.style = (UIOutline.OutlineStyle)EditorGUILayout.EnumPopup("Style:", effect.style);

            switch (effect.style)
            {
                case UIOutline.OutlineStyle.Outline:
                    effect.EffectColor = EditorGUILayout.ColorField("EffectOffset:", effect.EffectColor);
                    effect.EffectOffset = EditorGUILayout.Vector2Field("EffectOffset:", effect.EffectOffset);
                    break;
                case UIOutline.OutlineStyle.Outline8:
                    effect.EffectColor = EditorGUILayout.ColorField("EffectOffset:", effect.EffectColor);
                    effect.EffectOffset = EditorGUILayout.Vector2Field("EffectOffset:", effect.EffectOffset);
                    break;
                case UIOutline.OutlineStyle.Outline8Split:
                    effect.EffectColor = EditorGUILayout.ColorField("EffectOffset:", effect.EffectColor);
                    effect.EffectOffset = EditorGUILayout.Vector2Field("EffectOffset:", effect.EffectOffset);
                    effect.EffectOffset2 = EditorGUILayout.Vector2Field("EffectOffset2:", effect.EffectOffset2);
                    break;
                case UIOutline.OutlineStyle.Shadow:
                    effect.EffectColor = EditorGUILayout.ColorField("EffectOffset:", effect.EffectColor);
                    effect.EffectOffset = EditorGUILayout.Vector2Field("EffectOffset:", effect.EffectOffset);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                effect.graphic.SetVerticesDirty();
            }
        }
    }
}