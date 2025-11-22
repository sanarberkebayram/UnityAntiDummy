using System.Collections.Generic;
using UnityEditor;

namespace Project.EditorTools
{
    public static class ProjectMaintenanceTools
    {
        [MenuItem("Tools/Project/Maintenance/Force Refresh", priority = 0)]
        public static void ForceRefresh()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            EditorUtility.DisplayDialog("Force Refresh", "AssetDatabase refresh completed.", "OK");
        }

        [MenuItem("Tools/Project/Maintenance/Reimport Naming Tools", priority = 1)]
        public static void ReimportNamingTools()
        {
            var paths = new List<string>
            {
                "Assets/_Project/Scripts/Editor/NamingConventionsWindow.cs",
                "Assets/_Project/Scripts/Editor/NamingConventionsUtil.cs",
                "Assets/_Project/Scripts/Editor/NamingConventionsRenamerWindow.cs",
                "Assets/_Project/Scripts/Editor/NamingConventionsImportPostprocessor.cs"
            };

            foreach (var p in paths)
            {
                AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Reimport", "Naming tools reimported.", "OK");
        }
    }
}

