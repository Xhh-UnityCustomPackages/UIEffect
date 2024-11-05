using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Configuration;

namespace Game.Core.UIEffect.Editor
{
    [CustomEditor(typeof(GraphicGroup), true)]
    public class GraphicGroupEditor : UnityEditor.Editor
    {
        #region SerializedProperty
        private GraphicGroup m_GraphicGroup;
        private SerializedProperty m_Mode;
        private SerializedProperty m_Color;
        private SerializedProperty m_Color2;
        private SerializedProperty m_Offset1;
        #endregion

        protected const string _undoText = "Modified GraphicGroup";

        protected virtual void OnEnable()
        {
            Initialization();
        }


        void Initialization()
        {
            m_GraphicGroup = target as GraphicGroup;
            m_Mode = serializedObject.FindProperty("m_Mode");
            m_Color = serializedObject.FindProperty("m_Color");
            m_Color2 = serializedObject.FindProperty("m_Color2");
            m_Offset1 = serializedObject.FindProperty("m_Offset1");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(target, _undoText);

            EditorGUILayout.PropertyField(m_Mode);
            EditorGUILayout.PropertyField(m_Color);

            if (m_Mode.enumValueIndex != (int)GraphicGroup.Mode.Single)
            {
                EditorGUILayout.PropertyField(m_Color2);
                EditorGUILayout.PropertyField(m_Offset1);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
