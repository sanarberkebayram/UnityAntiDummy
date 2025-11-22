using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityAntiDummy.EditorTools
{
    public class NamingConventionsImportPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var issues = new List<NamingIssue>();

            foreach (var path in importedAssets)
            {
                if (string.IsNullOrEmpty(path)) continue;
                if (Directory.Exists(path)) continue; // skip folders
                var ext = Path.GetExtension(path).ToLowerInvariant();
                var name = Path.GetFileNameWithoutExtension(path);

                // Skip meta or irrelevant files
                if (ext == ".meta") continue;

                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                var reason = NamingConventionsUtil.ValidateName(name, ext, type, path);
                if (reason == null) continue; // ok

                var suggestion = NamingConventionsUtil.SuggestName(name, ext, type, path) ?? name;

                issues.Add(new NamingIssue
                {
                    assetPath = path,
                    currentName = name,
                    ext = ext,
                    typeName = type != null ? type.Name : "Unknown",
                    suggestion = suggestion,
                    selected = true
                });
            }

            if (issues.Count > 0)
            {
                // Merge into store and show window after delay to avoid reentrancy
                foreach (var issue in issues)
                    NamingIssuesStore.instance.AddOrUpdate(issue);

                EditorApplication.delayCall += () =>
                {
                    // Ensure we still have issues (another callback may have cleared them)
                    if (NamingIssuesStore.instance.issues.Count > 0)
                        NamingConventionsRenamerWindow.ShowWithIssues();
                };
            }
        }
    }
}
