# Unity Anti Dummy (Editor Tools)

Editor-only utilities to enforce naming conventions and simplify project maintenance. Designed as a Unity Package (UPM) for installation via Git.

## Install (Git URL)

- Open `Window > Package Manager`.
- Click the `+` button → `Add package from Git URL…`.
- Enter your repo URL, for example:
  `https://github.com/sanarberkebayram/UnityAntiDummy.git#main`

> Requires Unity 6000.2.9f1 or newer (Unity 6).

## What’s Inside

- Naming validator on import: checks newly imported assets and opens a bulk renamer window with suggestions.
- On-demand validator window: validate selected assets from a dedicated UI.
- Maintenance shortcuts: force refresh and quick reimport for these tools.

### Editor Menus

- `Tools/Project/Naming Conventions`: Opens the validator UI.
- `Tools/Project/Maintenance/Force Refresh`: Saves and forces a synchronous asset refresh.
- `Tools/Project/Maintenance/Reimport Naming Tools`: Reimports this toolset’s editor scripts.

### Naming Rules (summary)

- Scripts (`.cs`): PascalCase file name, matching class name.
- Scenes (`.unity`): prefix `scn_`.
- Prefabs (`.prefab`): prefix `pf_`.
- Materials: prefix `mat_`.
- Textures: prefix `tex_` (or `spr_` under `/UI/Sprites/`).
- Shaders: `sh_`; Shader Graph: `sg_`; VFX Graph: `vfx_`.
- Animation clip: `anm_`; Animator Controller: `ac_`; Animator Override Controller: `aoc_`.
- Audio clip: `sfx_` / `mus_` / `vox_`.
- Audio Mixer: `mix_`.
- ScriptableObjects (`.asset` under `/ScriptableObjects/`): `so_`.

Suggestions are auto-generated from the current name (e.g., `MyButton` → `spr_MyButton`).

For the complete and up-to-date rules, see [NamingConventions.md](./NamingConventions.md).

## Files of Interest

- `Editor/NamingConventionsImportPostprocessor.cs`: Validates imported assets and queues issues for review.
- `Editor/NamingConventionsWindow.cs`: UI for validating selected assets and viewing guidance.
- `Editor/NamingConventionsRenamerWindow.cs`: Bulk rename window for applying suggestions.
- `Editor/NamingConventionsUtil.cs`: Core validation and suggestion logic.
- `Editor/ProjectMaintenanceTools.cs`: Force refresh and reimport helpers.

## Assembly Definitions

This package includes an Editor-only assembly definition so scripts compile inside UPM:

- `Editor/UnityAntiDummy.Editor.asmdef`

## Notes

- This package is Editor-only; it contains no Runtime scripts.
- If you change naming rules, update `Editor/NamingConventionsUtil.cs`.
- `.editorconfig` is included at the package root to keep code style consistent across editors (Rider/VS Code/Visual Studio). Ensure your IDE has EditorConfig support enabled.

## License

See `LICENSE` in the repository root.
