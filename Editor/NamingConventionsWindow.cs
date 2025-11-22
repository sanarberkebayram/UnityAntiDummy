using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Project.EditorTools
{
    public class NamingConventionsWindow : EditorWindow
    {
        private Vector2 _scroll;

        private class ValidationRow
        {
            public bool ok;
            public string message; // empty when ok
            public string assetPath; // may be null for non-asset rows
            public string displayName; // file name
            public Texture icon;
        }

        private readonly List<ValidationRow> _rows = new List<ValidationRow>();

        // UI state
        private bool _showScripts = true;
        private bool _showAssets = true;
        private bool _showResults = true;
        private bool _showOK = false; // default: show only warnings

        // Styles
        private GUIStyle _title;
        private GUIStyle _h2;
        private GUIStyle _wrap;
        private GUIStyle _monoWrap;
        private GUIStyle _statusOk;
        private GUIStyle _statusWarn;

        [MenuItem("Tools/Project/Naming Conventions")] 
        private static void Open()
        {
            var window = GetWindow<NamingConventionsWindow>(true, "Naming Conventions");
            window.minSize = new Vector2(640, 520);
            window.Show();
        }

        private void OnEnable()
        {
            // initialize styles lazily on enable
            BuildStyles();
        }

        private void BuildStyles()
        {
            _title = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14
            };
            _h2 = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12
            };
            _wrap = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };
            _monoWrap = new GUIStyle(EditorStyles.helpBox)
            {
                richText = false,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            _statusOk = new GUIStyle(EditorStyles.label);
            _statusOk.normal.textColor = new Color(0.2f, 0.7f, 0.2f);
            _statusWarn = new GUIStyle(EditorStyles.label);
            _statusWarn.normal.textColor = new Color(0.95f, 0.65f, 0.2f);
        }

        private void OnGUI()
        {
            if (_title == null) BuildStyles();
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Naming Conventions", _title);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Open Docs", GUILayout.Width(120))) OpenDocs();
                if (GUILayout.Button("Validate Selection", GUILayout.Width(160))) ValidateSelection();
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space(6);
            _showScripts = EditorGUILayout.Foldout(_showScripts, "Scripts (C#)", true);
            if (_showScripts)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(
                        "Types/Methods/Properties/Events: PascalCase\n" +
                        "Interfaces: I + PascalCase\n" +
                        "Constants: PascalCase\n" +
                        "Private fields: _camelCase\n" +
                        "Private static fields: s_camelCase\n" +
                        "Locals/Parameters: camelCase\n" +
                        "MonoBehaviour filename matches class (PascalCase)\n" +
                        "Async methods end with 'Async'",
                        _wrap);
                }
            }

            EditorGUILayout.Space(6);
            _showAssets = EditorGUILayout.Foldout(_showAssets, "Assets", true);
            if (_showAssets)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(
                        "Prefixes: pf_, mat_, tex_, spr_, sh_, sg_, vfx_, anm_, ac_, aoc_, sfx_, mus_, mix_, so_, scn_, fnt_\n" +
                        "Texture map suffixes: _bc, _n, _m, _r, _ms, _ao, _e, _h",
                        _wrap);
                }
            }

            EditorGUILayout.Space(6);
            _showResults = EditorGUILayout.Foldout(_showResults, "Validation Results", true);
            if (_showResults)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _showOK = GUILayout.Toggle(_showOK, "Show OK", "Button", GUILayout.Width(90));
                    GUILayout.FlexibleSpace();
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawResultsHeader();
                    _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(160));
                    var any = false;
                    foreach (var r in _rows)
                    {
                        if (!r.ok || _showOK)
                        {
                            DrawResultRow(r);
                            any = true;
                        }
                    }
                    if (!any)
                        GUILayout.Label("No results yet. Click 'Validate Selection'.", _wrap);
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private static void OpenDocs()
        {
            // 1) Try to resolve the md file relative to this package (prefer package-local doc)
            var pkgRoot = GetThisPackageRootPath();
            if (!string.IsNullOrEmpty(pkgRoot))
            {
                var pkgDocPath = pkgRoot.Replace('\\', '/') + "/NamingConventions.md";
                var pkgDoc = AssetDatabase.LoadMainAssetAtPath(pkgDocPath);
                if (pkgDoc != null)
                {
                    EditorGUIUtility.PingObject(pkgDoc);
                    AssetDatabase.OpenAsset(pkgDoc);
                    return;
                }
            }

            // 2) Fallback: search in Assets and Packages
            var searchScopes = new[] { "Assets", "Packages" };
            var candidates = AssetDatabase.FindAssets("NamingConventions t:TextAsset", searchScopes)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith("NamingConventions.md"))
                .ToList();

            var path = candidates.FirstOrDefault();
            if (!string.IsNullOrEmpty(path))
            {
                var obj = AssetDatabase.LoadMainAssetAtPath(path);
                EditorGUIUtility.PingObject(obj);
                AssetDatabase.OpenAsset(obj);
            }
            else
            {
                EditorUtility.DisplayDialog("Naming Conventions", "Could not find NamingConventions.md", "OK");
            }
        }

        private static string GetThisPackageRootPath()
        {
            // Find this EditorWindow script and infer the package root from its path
            var guids = AssetDatabase.FindAssets("t:MonoScript NamingConventionsWindow");
            foreach (var guid in guids)
            {
                var scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                if (scriptPath.EndsWith("NamingConventionsWindow.cs"))
                {
                    var editorDir = Path.GetDirectoryName(scriptPath); // .../Editor
                    if (string.IsNullOrEmpty(editorDir)) return null;
                    var pkgRoot = Path.GetDirectoryName(editorDir); // package root
                    return pkgRoot;
                }
            }
            return null;
        }

        private void ValidateSelection()
        {
            _rows.Clear();
            var objs = Selection.objects;
            if (objs == null || objs.Length == 0)
            {
                _rows.Add(new ValidationRow { ok = true, message = "Select assets in the Project window to validate." });
                Repaint();
                return;
            }

            foreach (var obj in objs)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var name = Path.GetFileNameWithoutExtension(path);
                var ext = Path.GetExtension(path).ToLowerInvariant();
                var mainType = AssetDatabase.GetMainAssetTypeAtPath(path);

                var result = ValidateName(name, ext, mainType, path);
                var row = new ValidationRow
                {
                    ok = string.IsNullOrEmpty(result),
                    message = result ?? string.Empty,
                    assetPath = path,
                    displayName = Path.GetFileName(path),
                    icon = AssetPreview.GetMiniThumbnail(obj)
                };
                _rows.Add(row);
            }

            Repaint();
        }

        private static string ValidateName(string name, string ext, System.Type mainType, string path)
            => NamingConventionsUtil.ValidateName(name, ext, mainType, path);

        private void DrawResultsHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Status", GUILayout.Width(60));
                GUILayout.Label("Asset", GUILayout.Width(320));
                GUILayout.Label("Message", GUILayout.ExpandWidth(true));
            }
            EditorGUILayout.LabelField(GUIContent.none, GUI.skin.horizontalSlider);
        }

        private void DrawResultRow(ValidationRow r)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(r.ok ? new GUIContent("OK") : new GUIContent("WARN"), r.ok ? _statusOk : _statusWarn, GUILayout.Width(60));

                if (!string.IsNullOrEmpty(r.assetPath))
                {
                    var content = new GUIContent(r.displayName, r.icon);
                    if (GUILayout.Button(content, EditorStyles.label, GUILayout.Width(320)))
                    {
                        var obj = AssetDatabase.LoadMainAssetAtPath(r.assetPath);
                        EditorGUIUtility.PingObject(obj);
                        Selection.activeObject = obj;
                    }
                }
                else
                {
                    GUILayout.Label(r.displayName ?? "", GUILayout.Width(320));
                }

                GUILayout.Label(string.IsNullOrEmpty(r.message) ? "" : r.message, _wrap, GUILayout.ExpandWidth(true));
            }
        }
    }
}
