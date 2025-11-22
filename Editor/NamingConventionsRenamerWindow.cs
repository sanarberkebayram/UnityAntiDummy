using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools
{
    [Serializable]
    internal class NamingIssue
    {
        public string assetPath;
        public string currentName;
        public string ext;
        public string typeName;
        public string suggestion;
        public bool selected = true;
    }

    [FilePath("ProjectSettings/NamingIssuesStore.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class NamingIssuesStore : ScriptableSingleton<NamingIssuesStore>
    {
        public List<NamingIssue> issues = new List<NamingIssue>();

        public void AddOrUpdate(NamingIssue issue)
        {
            var existing = issues.FirstOrDefault(i => i.assetPath == issue.assetPath);
            if (existing != null)
                issues[issues.IndexOf(existing)] = issue;
            else
                issues.Add(issue);
            Save(true);
        }

        public void Clear()
        {
            issues.Clear();
            Save(true);
        }

        public static void SaveStore() => instance.Save(true);
    }

    public class NamingConventionsRenamerWindow : EditorWindow
    {
        private Vector2 _scroll;

        public static void ShowWithIssues()
        {
            var window = GetWindow<NamingConventionsRenamerWindow>(true, "Naming Convention Issues");
            window.minSize = new Vector2(760, 420);
            window.Show();
            window.Repaint();
        }

        private void OnGUI()
        {
            var store = NamingIssuesStore.instance;
            EditorGUILayout.LabelField("Assets not matching naming conventions", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply All", GUILayout.Width(120))) ApplyAll();
                if (GUILayout.Button("Skip All", GUILayout.Width(120))) { store.Clear(); Close(); }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", GUILayout.Width(100))) Repaint();
            }

            EditorGUILayout.Space();
            DrawHeader();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (store.issues.Count == 0)
            {
                GUILayout.Label("No issues.");
            }
            else
            {
                for (int i = 0; i < store.issues.Count; i++)
                {
                    DrawIssueRow(store.issues[i], i);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(" ", GUILayout.Width(20));
                GUILayout.Label("Asset", GUILayout.Width(280));
                GUILayout.Label("Type", GUILayout.Width(120));
                GUILayout.Label("Current Name", GUILayout.Width(160));
                GUILayout.Label("Suggestion", GUILayout.ExpandWidth(true));
                GUILayout.Label("Actions", GUILayout.Width(150));
            }
        }

        private void DrawIssueRow(NamingIssue issue, int index)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                issue.selected = GUILayout.Toggle(issue.selected, GUIContent.none, GUILayout.Width(20));

                var obj = AssetDatabase.LoadMainAssetAtPath(issue.assetPath);
                var icon = obj != null ? AssetPreview.GetMiniThumbnail(obj) : EditorGUIUtility.IconContent("console.warnicon").image;
                if (GUILayout.Button(new GUIContent(Path.GetFileName(issue.assetPath), icon), EditorStyles.label, GUILayout.Width(280)))
                {
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeObject = obj;
                }

                GUILayout.Label(issue.typeName, GUILayout.Width(120));
                GUILayout.Label(issue.currentName, GUILayout.Width(160));
                issue.suggestion = EditorGUILayout.TextField(issue.suggestion);

                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(150)))
                {
                    if (GUILayout.Button("Apply", GUILayout.Width(70))) ApplyOne(issue);
                    if (GUILayout.Button("Skip", GUILayout.Width(70))) RemoveIssue(issue);
                }
            }
        }

        private void ApplyOne(NamingIssue issue)
        {
            if (string.IsNullOrWhiteSpace(issue.suggestion))
            {
                EditorUtility.DisplayDialog("Rename", "Suggestion cannot be empty.", "OK");
                return;
            }

            var error = AssetDatabase.RenameAsset(issue.assetPath, issue.suggestion);
            if (!string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("Rename Failed", error, "OK");
                return;
            }
            RemoveIssue(issue);
        }

        private void RemoveIssue(NamingIssue issue)
        {
            var store = NamingIssuesStore.instance;
            store.issues.Remove(issue);
            NamingIssuesStore.SaveStore();
            Repaint();
        }

        private void ApplyAll()
        {
            var store = NamingIssuesStore.instance;
            // Copy to avoid modification during iteration
            var targets = store.issues.Where(i => i.selected).ToList();
            foreach (var issue in targets)
            {
                var error = AssetDatabase.RenameAsset(issue.assetPath, issue.suggestion);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning($"Rename failed for {issue.assetPath}: {error}");
                }
            }
            // Remove all (both applied and skipped selections); keep non-selected
            store.issues = store.issues.Where(i => !targets.Contains(i)).ToList();
            NamingIssuesStore.SaveStore();
            Repaint();
        }
    }
}
