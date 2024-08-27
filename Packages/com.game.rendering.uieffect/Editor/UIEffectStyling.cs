using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Core.UIEffect.Editor
{
    public class UIEffectStyling
    {
        public static readonly GUIStyle SmallTickbox = new GUIStyle("ShurikenToggle");

        private static Rect _splitterRect;

        static readonly Color _splitterdark = new Color(0.12f, 0.12f, 0.12f, 1.333f);
        static readonly Color _splitterlight = new Color(0.6f, 0.6f, 0.6f, 1.333f);
        public static Color Splitter { get { return EditorGUIUtility.isProSkin ? _splitterdark : _splitterlight; } }

        static readonly Color _headerbackgrounddark = new Color(0.1f, 0.1f, 0.1f, 0.2f);
        static readonly Color _headerbackgroundlight = new Color(1f, 1f, 1f, 0.4f);
        public static Color HeaderBackground { get { return EditorGUIUtility.isProSkin ? _headerbackgrounddark : _headerbackgroundlight; } }

        private static Rect _backgroundRect;
        private static Rect _reorderRect;
        private static Rect _labelRect;
        private static Rect _foldoutRect;
        private static Rect _toggleRect;
        private static Texture2D _menuIcon;
        static readonly Texture2D _paneoptionsicondark;
        static readonly Texture2D _paneoptionsiconlight;
        public static Texture2D PaneOptionsIcon { get { return EditorGUIUtility.isProSkin ? _paneoptionsicondark : _paneoptionsiconlight; } }

        private static Rect _menuRect;
        private static Rect _genericMenuRect;
        private static GenericMenu _genericMenu;
        private static Color _headerBackgroundColor;

        static UIEffectStyling()
        {
            _paneoptionsicondark = (Texture2D)EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
            _paneoptionsiconlight = (Texture2D)EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");
        }

        public static void InitStyling()
        {
            _menuIcon = PaneOptionsIcon;
            _genericMenu = new GenericMenu();
        }

        static public void DrawSplitter()
        {
            // Helper to draw a separator line

            _splitterRect = GUILayoutUtility.GetRect(1f, 1f);

            _splitterRect.xMin = 0f;
            _splitterRect.width += 4f;

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            EditorGUI.DrawRect(_splitterRect, Splitter);
        }


        static public void DrawSection(string title)
        {
            EditorGUILayout.Space();
            DrawSplitter();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        static public Rect DrawHeader(ref bool expanded, ref bool activeField, string title, System.Action<GenericMenu> fillGenericMenu, UIEffect host)
        {
            var e = Event.current;

            // Initialize Rects
            _backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var offset = 4f;

            _reorderRect = _backgroundRect;
            _reorderRect.xMin -= 8f;
            _reorderRect.y += 5f;
            _reorderRect.width = 9f;
            _reorderRect.height = 9f;

            _labelRect = _backgroundRect;
            _labelRect.xMin += 32f + offset;
            _labelRect.xMax -= 20f;

            _foldoutRect = _backgroundRect;
            _foldoutRect.y += 1f;
            _foldoutRect.xMin += offset;
            _foldoutRect.width = 13f;
            _foldoutRect.height = 13f;

            _toggleRect = _backgroundRect;
            _toggleRect.x += 16f;
            _toggleRect.xMin += offset;
            _toggleRect.y += 2f;
            _toggleRect.width = 13f;
            _toggleRect.height = 13f;

            _headerBackgroundColor = HeaderBackground;

            EditorGUI.DrawRect(_backgroundRect, _headerBackgroundColor);

            // Foldout
            expanded = GUI.Toggle(_foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);

            // Title
            EditorGUI.LabelField(_labelRect, title, EditorStyles.boldLabel);

            // Active checkbox
            EditorGUI.BeginChangeCheck();
            activeField = GUI.Toggle(_toggleRect, activeField, GUIContent.none, SmallTickbox);
            if (EditorGUI.EndChangeCheck())
            {
                host.SetVerticesDirty();
                host.SetMaterialDirty();
            }

            // Handle events
            _menuRect.x = _labelRect.xMax + 4f;
            _menuRect.y = _labelRect.y;
            _menuRect.width = _menuIcon.width;
            _menuRect.height = _menuIcon.height;

            // Dropdown menu icon
            GUI.DrawTexture(_menuRect, _menuIcon);
            if (e.type == EventType.MouseDown)
            {
                if (_menuRect.Contains(e.mousePosition))
                {
                    fillGenericMenu(_genericMenu);

                    _genericMenuRect.x = _menuRect.x;
                    _genericMenuRect.y = _menuRect.yMax;
                    _genericMenuRect.width = 0f;
                    _genericMenuRect.height = 0f;
                    _genericMenu.DropDown(_genericMenuRect);
                    e.Use();
                }
            }

            return _backgroundRect;
        }
    }
}
