using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Game.Core.UIEffect.Editor
{
    public class UIEffectInspectors
    {
        public bool DrawerInitialized;
        public List<SerializedProperty> PropertiesList = new List<SerializedProperty>();
        protected SerializedProperty _currentProperty;

        public virtual void OnEnable()
        {
            DrawerInitialized = false;
            PropertiesList.Clear();
        }

        public virtual void OnDisable()
        {
            PropertiesList.Clear();
        }

        public virtual void Initialization(SerializedProperty currentProperty, BaseUIEffect uiEffect)
        {
            if (DrawerInitialized)
            {
                return;
            }
            _currentProperty = currentProperty;

            // List<FieldInfo> fieldInfoList;
            // int fieldInfoLength = UIEffect_FieldInfo.GetFieldInfo(uiEffect, out fieldInfoList);

            // Debug.LogError($"fieldInfoLength:{fieldInfoLength}");
            // for (int i = 0; i < fieldInfoLength; i++)
            // {
            //     Debug.LogError($"fieldInfoList:{fieldInfoList[i].Name}");
            // }
            // Debug.LogError($"currentProperty:{currentProperty.propertyPath}");

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    FillPropertiesList(currentProperty);
                } while (currentProperty.NextVisible(false) && !currentProperty.propertyPath.EndsWith("]"));//可能会访问到下个ArrayElement元素 所以加个限制
            }
            DrawerInitialized = true;
        }

        public void FillPropertiesList(SerializedProperty serializedProperty)
        {
            // Debug.LogError($"serializedProperty:{serializedProperty.propertyPath}");
            SerializedProperty property = serializedProperty.Copy();
            PropertiesList.Add(property);
        }

        public void DrawInspector(SerializedProperty currentProperty, BaseUIEffect uiEffect)
        {
            Initialization(currentProperty, uiEffect);
            // if (!DrawBase(currentProperty, feedback))
            // {
            //     DrawContainer(feedback);
            DrawContents(uiEffect);
            // }
        }

        protected virtual void DrawContents(BaseUIEffect feedback)
        {
            if (PropertiesList.Count == 0)
            {
                return;
            }

            // EditorGUILayout.Space();
            //从1开始是为了排除Active 属性
            for (int i = 1; i < PropertiesList.Count; i++)
            {
                if (PropertiesList[i] == null)
                    continue;
                EditorGUILayout.PropertyField(PropertiesList[i], true);
            }
        }
    }
}
