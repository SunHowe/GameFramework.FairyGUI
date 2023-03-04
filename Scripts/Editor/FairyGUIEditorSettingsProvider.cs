using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace GameFramework.FairyGUI.Editor
{
    public class FairyGUIEditorSettingsProvider : SettingsProvider
    {
        private SerializedObject m_SerializedObject;
        private SerializedProperty m_PropertySettings;

        public FairyGUIEditorSettingsProvider() : base("Project/FairyGUI Settings", SettingsScope.Project)
        {
        }

        private void InitGUI()
        {
            var setting = FairyGUIEditorSettings.Instance;
            m_SerializedObject?.Dispose();
            m_SerializedObject = new SerializedObject(setting);

            m_PropertySettings = m_SerializedObject.FindProperty(nameof(FairyGUIEditorSettings.settings));
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            EditorStatusWatcher.OnEditorFocused += OnEditorFocused;
            InitGUI();
        }

        private void OnEditorFocused()
        {
            InitGUI();
            Repaint();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            EditorStatusWatcher.OnEditorFocused -= OnEditorFocused;
            FairyGUIEditorSettings.Instance.Save();
        }

        public override void OnGUI(string searchContext)
        {
            if (m_SerializedObject == null || !m_SerializedObject.targetObject)
                InitGUI();

            // ReSharper disable once PossibleNullReferenceException
            m_SerializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(m_PropertySettings);

            if (!EditorGUI.EndChangeCheck())
                return;
            
            m_SerializedObject.ApplyModifiedProperties();
            FairyGUIEditorSettings.Instance.Save();
        }

        private static FairyGUIEditorSettingsProvider s_Provider;

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            if (s_Provider != null)
                return s_Provider;

            s_Provider = new FairyGUIEditorSettingsProvider();
            using var so = new SerializedObject(FairyGUIEditorSettings.Instance);
            s_Provider.keywords = GetSearchKeywordsFromSerializedObject(so);
            return s_Provider;
        }
    }
}