using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Project.EditorTools
{
    internal static class NamingConventionsUtil
    {
        public static string ValidateName(string name, string ext, Type mainType, string path)
        {
            if (string.IsNullOrEmpty(name)) return "Empty name";

            // Scripts: PascalCase file name
            if (ext == ".cs")
            {
                if (!IsPascalCase(name))
                    return "C# file should be PascalCase and match class name";
                return null;
            }

            // Scenes
            if (ext == ".unity")
                return name.StartsWith("scn_") ? null : "Scene should start with 'scn_'";

            // Prefabs
            if (ext == ".prefab")
                return name.StartsWith("pf_") ? null : "Prefab should start with 'pf_'";

            // Materials
            if (mainType == typeof(Material))
                return name.StartsWith("mat_") ? null : "Material should start with 'mat_'";

            // Textures & Sprites
            if (IsTextureExt(ext))
            {
                if (path.Contains("/UI/Sprites/"))
                    return name.StartsWith("spr_") ? null : "UI Sprite should start with 'spr_'";
                return name.StartsWith("tex_") ? null : "Texture should start with 'tex_'";
            }

            // Shader and Shader Graph
            if (ext == ".shader")
                return name.StartsWith("sh_") ? null : "Shader should start with 'sh_'";
            if (ext == ".shadergraph")
                return name.StartsWith("sg_") ? null : "Shader Graph should start with 'sg_'";

            // VFX Graph
            if (ext == ".vfx")
                return name.StartsWith("vfx_") ? null : "VFX Graph should start with 'vfx_'";

            // Animation clips
            if (mainType == typeof(AnimationClip))
                return name.StartsWith("anm_") ? null : "Animation clip should start with 'anm_'";

            // Animator assets
            if (mainType == typeof(AnimatorController))
                return name.StartsWith("ac_") ? null : "Animator Controller should start with 'ac_'";
            if (mainType == typeof(AnimatorOverrideController))
                return name.StartsWith("aoc_") ? null : "Animator Override Controller should start with 'aoc_'";

            // Audio
            if (mainType == typeof(AudioClip))
                return (name.StartsWith("sfx_") || name.StartsWith("mus_") || name.StartsWith("vox_"))
                    ? null
                    : "Audio should start with 'sfx_', 'mus_', or 'vox_'";

            // Audio Mixer asset (Editor type)
            if (mainType != null && mainType.FullName == "UnityEditor.Audio.AudioMixerController")
                return name.StartsWith("mix_") ? null : "Audio Mixer should start with 'mix_'";

            // ScriptableObjects (heuristic: .asset under ScriptableObjects folder)
            if (ext == ".asset" && path.Contains("/ScriptableObjects/"))
                return name.StartsWith("so_") ? null : "ScriptableObject asset should start with 'so_'";

            return null; // Unknown or not enforced
        }

        public static string SuggestName(string name, string ext, Type mainType, string path)
        {
            // If valid, no suggestion needed
            if (ValidateName(name, ext, mainType, path) == null)
                return null;

            // Remove existing known prefixes when building suggestion
            string core = name;
            string[] knownPrefixes = {"pf_","mat_","tex_","spr_","sh_","sg_","vfx_","anm_","ac_","aoc_","sfx_","mus_","vox_","mix_","so_","scn_"};
            foreach (var p in knownPrefixes)
            {
                if (core.StartsWith(p))
                {
                    core = core.Substring(p.Length);
                    break;
                }
            }

            // For scripts, prefer PascalCase
            if (ext == ".cs")
            {
                return ToPascalCase(core);
            }

            if (ext == ".unity")
                return "scn_" + ToPascalCase(core);

            if (ext == ".prefab")
                return "pf_" + core;

            if (mainType == typeof(Material))
                return "mat_" + core;

            if (IsTextureExt(ext))
            {
                var prefix = path.Contains("/UI/Sprites/") ? "spr_" : "tex_";
                return prefix + core;
            }

            if (ext == ".shader")
                return "sh_" + core;
            if (ext == ".shadergraph")
                return "sg_" + core;
            if (ext == ".vfx")
                return "vfx_" + core;

            if (mainType == typeof(AnimationClip))
                return "anm_" + core;
            if (mainType == typeof(AnimatorController))
                return "ac_" + core;
            if (mainType == typeof(AnimatorOverrideController))
                return "aoc_" + core;

            if (mainType == typeof(AudioClip))
                return "sfx_" + core; // default to SFX

            if (mainType != null && mainType.FullName == "UnityEditor.Audio.AudioMixerController")
                return "mix_" + core;

            if (ext == ".asset" && path.Contains("/ScriptableObjects/"))
                return "so_" + core;

            return name; // fallback: unchanged
        }

        public static bool IsTextureExt(string ext)
        {
            switch (ext)
            {
                case ".png":
                case ".tga":
                case ".jpg":
                case ".jpeg":
                case ".tif":
                case ".tiff":
                case ".psd":
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsPascalCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            if (!char.IsUpper(s[0])) return false;
            if (s.Any(c => c == ' ' || c == '-' || c == '_')) return false;
            return true;
        }

        public static string ToPascalCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var parts = s
                .Replace('-', ' ')
                .Replace('_', ' ')
                .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1) : string.Empty)));
        }
    }
}

