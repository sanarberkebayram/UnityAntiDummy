using System.Collections.Generic;
using UnityEditor;

namespace UnityAntiDummy.EditorTools
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
            // Reimport all MonoScripts in the same Editor folder as this tool,
            // which works both when used from Assets or from a UPM package.
            var editorFolder = GetThisEditorFolder();
            if (!string.IsNullOrEmpty(editorFolder))
            {
                var guids = AssetDatabase.FindAssets("t:MonoScript", new[] { editorFolder });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Reimport", "Naming tools reimported.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Reimport", "Could not locate the package's Editor folder.", "OK");
            }
        }

        private static string GetThisEditorFolder()
        {
            var guids = AssetDatabase.FindAssets("t:MonoScript ProjectMaintenanceTools");
            foreach (var guid in guids)
            {
                var scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                if (scriptPath.EndsWith("ProjectMaintenanceTools.cs"))
                {
                    return System.IO.Path.GetDirectoryName(scriptPath)?.Replace('\\', '/');
                }
            }
            return null;
        }
    }
}
