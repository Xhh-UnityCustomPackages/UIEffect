using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Game.Core.UIEffect.Editor
{
    [CustomEditor(typeof(UIEffect), true)]
    public class UIEffectEditor : UnityEditor.Editor
    {
        #region SerializedProperty
        private UIEffect m_UIEffect;
        protected SerializedProperty m_UIEffects;
        #endregion SerializedProperty

        #region Consts
        protected Color _draggedColor = new Color(0, 1, 1, 0.2f);
        protected GUIContent _UIEffectResetGUIContent;
        protected GUIContent _UIEffectRemoveGUIContent;
        protected const string _undoText = "Modified UIEffect";
        protected const string _UIEffectsSectionTitle = "UIEffects";


        protected Dictionary<string, UIEffectInspectors> m_UIEffectInspectorMap = new();
        #endregion Consts

        protected Event _currentEvent;
        protected SerializedProperty m_UIEffectListProperty;
        protected BaseUIEffect m_PickedUIEffect;
        protected bool m_PickedUIEffectIsExpanded;
        protected bool m_CachedGUI = false;

        protected virtual void OnEnable()
        {
            Initialization();
            foreach (var inspector in m_UIEffectInspectorMap)
            {
                inspector.Value.OnEnable();
            }
            // EditorApplication.playModeStateChanged += ModeChanged;
        }

        protected virtual void OnDisable()
        {
            foreach (var inspector in m_UIEffectInspectorMap)
            {
                inspector.Value.OnDisable();
            }
            // EditorApplication.playModeStateChanged -= ModeChanged;
        }


        void Initialization()
        {
            m_UIEffect = target as UIEffect;
            m_UIEffects = serializedObject.FindProperty("m_UIEffects");
            PrepareUIEffectTypeList();

            m_CachedGUI = false;

        }


        #region Types
        protected List<string> _typeNames = new List<string>();
        public static string[] _typeDisplays = null;
        public static List<UIEffectTypePair> _typesAndNames = new List<UIEffectTypePair>();

        public struct UIEffectTypePair
        {
            public System.Type UIEffectType;
            public string UIEffectName;
        }


        void PrepareUIEffectTypeList()
        {
            if ((_typeDisplays != null) && (_typeDisplays.Length > 0))
            {
                return;
            }

            // Retrieve available feedbacks
            List<System.Type> types = (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
                                       from assemblyType in domainAssembly.GetTypes()
                                       where assemblyType.IsSubclassOf(typeof(BaseUIEffect))
                                       select assemblyType).ToList();

            // Create display list from types
            _typeNames.Clear();
            for (int i = 0; i < types.Count; i++)
            {
                UIEffectTypePair _newType = new UIEffectTypePair();
                _newType.UIEffectType = types[i];
                _newType.UIEffectName = UIEffectPathAttribute.GetUIEffectDefaultPath(types[i]);
                if ((_newType.UIEffectName == "BaseUIEffect") || (_newType.UIEffectName == null))
                {
                    continue;
                }
                _typesAndNames.Add(_newType);
            }


            _typesAndNames = _typesAndNames.OrderBy(t => t.UIEffectName).ToList();

            _typeNames.Add("Add new UIEffect...");
            for (int i = 0; i < _typesAndNames.Count; i++)
            {
                _typeNames.Add(_typesAndNames[i].UIEffectName);
            }

            _typeDisplays = _typeNames.ToArray();
        }

        #endregion Types


        public override void OnInspectorGUI()
        {
            _currentEvent = Event.current;
            serializedObject.Update();
            Undo.RecordObject(target, _undoText);

            InspectorCaching();
            DrawUIEffectsList();
            DrawBottomBar();


            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void InspectorCaching()
        {
            if (m_CachedGUI)
            {
                return;
            }

            UIEffectStyling.InitStyling();

            _UIEffectResetGUIContent = new GUIContent("Reset");
            _UIEffectRemoveGUIContent = new GUIContent("Remove");
        }

        protected UIEffectInspectors _UIEffectInspector;

        protected virtual void DrawUIEffectsList()
        {
            // if (m_UIEffects == null)
            //     return;
            UIEffectStyling.DrawSection(_UIEffectsSectionTitle);

            for (int i = 0; i < m_UIEffects.arraySize; i++)
            {
                DrawUIEffectHeader(i);
                if (m_PickedUIEffect == null)
                    continue;

                m_PickedUIEffect.IsExpanded = m_PickedUIEffectIsExpanded;
                if (m_PickedUIEffectIsExpanded)
                {
                    UIEffectStyling.DrawSplitter();
                    EditorGUI.BeginDisabledGroup(!m_PickedUIEffect.Active);
                    {
                        // EditorGUILayout.Space();

                        SerializedProperty currentProperty = m_UIEffectListProperty;
                        if (m_PickedUIEffect.IsExpanded)
                        {
                            if (m_UIEffectInspectorMap.TryGetValue(m_PickedUIEffect.GetType().Name, out _UIEffectInspector))
                            {
                                _UIEffectInspector.DrawInspector(currentProperty, m_PickedUIEffect);
                            }
                            else
                            {
                                UIEffectInspectors newInspector = new UIEffectInspectors();
                                m_UIEffectInspectorMap.Add(m_PickedUIEffect.GetType().Name, newInspector);
                                newInspector.Initialization(currentProperty, m_PickedUIEffect);
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
        }

        protected virtual void DrawUIEffectHeader(int i)
        {
            UIEffectStyling.DrawSplitter();
            m_UIEffectListProperty = m_UIEffects.GetArrayElementAtIndex(i);
            m_PickedUIEffect = m_UIEffect.UIEffects[i];

            if (m_PickedUIEffect == null)
            {
                return;
            }

            m_PickedUIEffectIsExpanded = m_PickedUIEffect.IsExpanded;

            Rect headerRect = UIEffectStyling.DrawHeader(
                ref m_PickedUIEffectIsExpanded,
                ref m_PickedUIEffect.Active,
                m_PickedUIEffect.GetType().Name,
                (GenericMenu menu) =>
                {
                    menu.AddItem(_UIEffectResetGUIContent, false, () => { m_PickedUIEffect.Reset(); });
                    menu.AddItem(_UIEffectRemoveGUIContent, false, () => { RemoveUIEffect(i); });
                },
                m_UIEffect);

            EditorGUI.DrawRect(headerRect, _draggedColor);
        }

        private void RemoveUIEffect(int index)
        {
            Undo.RecordObject(target, "Remove UIEffect");
            m_UIEffectInspectorMap.Remove(m_UIEffect.UIEffects[index].GetType().Name);
            (target as UIEffect).RemoveUIEffect(index);
            serializedObject.ApplyModifiedProperties();
            ForceRepaint();
            PrefabUtility.RecordPrefabInstancePropertyModifications(m_UIEffect);
        }

        protected virtual void DrawBottomBar()
        {
            if (m_UIEffects != null && m_UIEffects.arraySize > 0)
            {
                UIEffectStyling.DrawSplitter();
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                int newItem = EditorGUILayout.Popup(0, _typeDisplays) - 1;

                if (newItem >= 0)
                {
                    serializedObject.Update();
                    Undo.RecordObject(target, "Add new UIEffect");
                    var effect = AddUIEffect(_typesAndNames[newItem].UIEffectType);
                    if (effect != null)
                    {
                        serializedObject.ApplyModifiedProperties();
                        PrefabUtility.RecordPrefabInstancePropertyModifications(m_UIEffect);
                        ForceRepaint();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        protected virtual BaseUIEffect AddUIEffect(System.Type type)
        {
            return (target as UIEffect).AddUIEffect(type);
        }

        public virtual void ForceRepaint()
        {
            m_UIEffectInspectorMap.Clear();
            Initialization();
            // (target as UIEffect).RefreshCache();
            Repaint();
        }

        protected virtual void Reset()
        {
            ForceRepaint();
        }
    }
}
