using UnityEditor;
using UnityEngine;

namespace GameFramework.FairyGUI.Editor
{
    public static class MenuItems
    {
        [MenuItem("Game Framework/FairyGUI/Generate All")]
        public static void GenerateAll()
        {
            foreach (var settings in FairyGUIEditorSettings.Instance.settings)
            {
                var components = FairyGUIObjectCollector.Collect(settings);
                foreach (var component in components)
                    Debug.Log(component.ToString());
            }
        }
    }
}