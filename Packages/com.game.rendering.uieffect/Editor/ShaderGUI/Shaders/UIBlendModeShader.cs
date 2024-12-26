using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using System;

namespace Game.Core.UIEffect.Editor
{
    public class UIBlendModeShader : ShaderGUI
    {
        #region Style

        public enum BlendMode
        {
            Normal,
            SoftAdditive,
            Multiply,
            Multiply2X,
            Darken,
            Lighten,
            Screen,
            LinearDodge
        }

        public static readonly string[] blendModeNames = Enum.GetNames(typeof(BlendMode));

        public static GUIContent SrcBlendText = EditorGUIUtility.TrTextContent("Specular Highlights",
            "When enabled, the Material reflects the shine from direct lighting.");

        #endregion


        protected MaterialEditor materialEditor { get; set; }

        public bool m_FirstTimeApply = true;

        private MaterialProperty _BlendMode;
        private MaterialProperty _SrcBlend;
        private MaterialProperty _DstBlend;

        // These have to be stored due to how MaterialHeaderScopeList callbacks work (they don't provide this data in the callbacks)
        MaterialEditor m_MaterialEditor;

        override public void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            materialEditor = materialEditorIn;
            Material targetMat = materialEditor.target as Material;

            FindProperties(properties);

            if (m_FirstTimeApply)
            {
                OnOpenGUI(targetMat, materialEditorIn, properties);
                m_FirstTimeApply = false;
            }

            // ShaderPropertiesGUI(materialEditor, targetMat, properties);
            materialEditor.PopupShaderProperty(_BlendMode, SrcBlendText, blendModeNames);

            switch (_BlendMode.floatValue)
            {
                case (float)BlendMode.Normal:
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    targetMat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    break;
                case (float)BlendMode.SoftAdditive:
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    break;
                case (float)BlendMode.Multiply:
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    targetMat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    break;
                case (float)BlendMode.Multiply2X:
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    targetMat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    break;
                case (float)BlendMode.Darken:
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Min);
                    break;
                case (float)BlendMode.Lighten:
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Max);
                    break;
                case (float)BlendMode.Screen:
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    break;
                case (float)BlendMode.LinearDodge:
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    break;
                default:
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    targetMat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    break;
            }

            materialEditor.ShaderProperty(_BlendMode, SrcBlendText);
            // materialEditor.ShaderProperty(_SrcBlend, SrcBlendText);
            // materialEditor.ShaderProperty(_DstBlend, SrcBlendText);
        }

        public virtual void OnOpenGUI(Material material, MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            // Generate the foldouts
            // m_MaterialScopeList.RegisterHeaderScope(Styles.SurfaceOptions, (uint)Expandable.SurfaceOptions, DrawSurfaceOptions);
            // m_MaterialScopeList.RegisterHeaderScope(Styles.SurfaceInputs, (uint)Expandable.SurfaceInputs, DrawSurfaceInputs);
            // m_MaterialScopeList.RegisterHeaderScope(Styles.AdvancedLabel, (uint)Expandable.Advanced, DrawAdvancedOptions);
        }

        public virtual void FindProperties(MaterialProperty[] properties)
        {
            var material = materialEditor?.target as Material;
            if (material == null)
                return;

            _BlendMode = FindProperty("_BlendMode", properties);
            _SrcBlend = FindProperty("_SrcBlend", properties);
            _DstBlend = FindProperty("_DstBlend", properties);
        }
    }
}