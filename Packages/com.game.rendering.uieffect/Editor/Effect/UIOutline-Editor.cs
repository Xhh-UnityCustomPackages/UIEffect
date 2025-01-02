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
                    effect.EffectColor = EditorGUILayout.ColorField("EffectColor:", effect.EffectColor);
                    effect.EffectOffset = EditorGUILayout.Vector2Field("EffectOffset:", effect.EffectOffset);
                    break;
                case UIOutline.OutlineStyle.Outline8:
                    effect.EffectColor = EditorGUILayout.ColorField("EffectColor:", effect.EffectColor);
                    effect.EffectOffset = EditorGUILayout.Vector2Field("EffectOffset:", effect.EffectOffset);
                    break;
                case UIOutline.OutlineStyle.Outline8Split:
                    effect.EffectColor = EditorGUILayout.ColorField("EffectColor:", effect.EffectColor);
                    effect.EffectOffset = EditorGUILayout.Vector2Field("EffectOffset:", effect.EffectOffset);
                    effect.EffectOffset2 = EditorGUILayout.Vector2Field("EffectOffset2:", effect.EffectOffset2);
                    break;
                case UIOutline.OutlineStyle.Shadow:
                    effect.EffectColor = EditorGUILayout.ColorField("EffectColor:", effect.EffectColor);
                    effect.EffectOffset = EditorGUILayout.Vector2Field("EffectOffset:", effect.EffectOffset);
                    break;
                case UIOutline.OutlineStyle.Circle:
                    effect.EffectColor = EditorGUILayout.ColorField("EffectColor:", effect.EffectColor);
                    effect.EffectOffset = EditorGUILayout.Vector2Field("EffectOffset:", effect.EffectOffset);

                    effect.CircleCount = EditorGUILayout.IntField("CircleCount:", effect.CircleCount);
                    effect.FirstSample = EditorGUILayout.IntField("FirstSample:", effect.FirstSample);
                    effect.SampleIncrement = EditorGUILayout.IntField("SampleIncrement:", effect.SampleIncrement);
                    break;
                case UIOutline.OutlineStyle.Box:
                    effect.EffectColor = EditorGUILayout.ColorField("EffectColor:", effect.EffectColor);
                    effect.EffectOffset = EditorGUILayout.Vector2Field("EffectOffset:", effect.EffectOffset);
                    effect.HalfSampleCount = EditorGUILayout.Vector2IntField("HalfSampleCount:", effect.HalfSampleCount);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                effect.graphic.SetVerticesDirty();
            }
        }
    }
}