using UnityEditor;

namespace GameFramework.FairyGUI.Editor
{
    public static class MenuItems
    {
        [MenuItem("Game Framework/FairyGUI/Generate All")]
        public static void GenerateAll()
        {
            foreach (var settings in FairyGUIEditorSettings.Instance.settings)
                FairyGUICodeGenerator.Generate(settings);
            
            AssetDatabase.Refresh();
        }
    }
}