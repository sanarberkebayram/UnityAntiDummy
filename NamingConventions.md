# Naming Conventions

This project uses strict, readable naming for both scripts and assets. Keep names short, consistent, and searchable.

## Scripts (C#)
- Classes/Structs/Enums/Delegates: PascalCase (e.g., PlayerController, DamageType)
- Interfaces: I + PascalCase (e.g., IHealth, IInteractable)
- Methods: PascalCase (e.g., ApplyDamage, LoadSceneAsync)
- Properties/Events: PascalCase (e.g., CurrentHealth, OnDied)
- Public Fields: PascalCase (discouraged — prefer properties or [SerializeField] private)
- Constants: PascalCase (e.g., MaxLives)
- Private Instance Fields: _camelCase (e.g., _currentHealth)
- Private Static Fields: s_camelCase (e.g., s_instance)
- Local Variables/Parameters: camelCase (e.g., damageAmount)
- Namespaces: Company.Product.Feature (PascalCase segments)
- MonoBehaviour Script Filename: Must match main class (PascalCase)
- Async Methods: Suffix Async (e.g., LoadSceneAsync)

Notes
- Keep `[SerializeField]` fields private using `_camelCase`.
- Prefer properties for public data exposure.
- Keep one public type per file, file name matches the type.

## Assets (prefixes + suffixes)
Use lowercase prefixes to quickly identify asset types. Optional descriptive suffixes follow PascalCase.

- Prefabs: pf_<Name> (e.g., pf_Player, pf_UI_Button)
- Materials: mat_<Name> (e.g., mat_PlayerBody)
- Textures: tex_<Name>[_map]
  - Maps: _bc (BaseColor/Albedo), _n (Normal), _m (Metallic), _r (Roughness), _ms (MetallicSmoothness), _ao (AmbientOcclusion), _e (Emission), _h (Height)
  - Examples: tex_Stone_bc, tex_Stone_n, tex_Stone_ms
- Sprites (UI): spr_<Name> (e.g., spr_HealthBar)
- Shaders: sh_<Name> (e.g., sh_CharacterLit)
- Shader Graphs: sg_<Name> (e.g., sg_UI_Unlit)
- VFX Graphs: vfx_<Name> (e.g., vfx_Explosion)
- Anim Clips: anm_<Name> (e.g., anm_Run)
- Animator Controllers: ac_<Name> (e.g., ac_Player)
- Animator Override Controllers: aoc_<Name> (e.g., aoc_PlayerSnow)
- Audio SFX: sfx_<Name> (e.g., sfx_ImpactWood)
- Audio Music: mus_<Name> (e.g., mus_BossPhase)
- Audio Mixer: mix_<Name> (e.g., mix_Master)
- ScriptableObjects: so_<Name> (e.g., so_WeaponConfig)
- Scenes: scn_<Domain>_<Name> (e.g., scn_UI_MainMenu, scn_GP_Level01)
- Fonts/TMP: fnt_<Name> (e.g., fnt_Roboto)

Folder Hints
- Place ScriptableObjects in `Assets/UnityAntiDummy/ScriptableObjects/<Domain>`.
- Place UI sprites in `Assets/UnityAntiDummy/UI/Sprites` with `spr_` prefix.
- Keep per-feature assets grouped; avoid duplicating materials/textures.

Validation
- Use the in-Editor window: Tools > Project > Naming Conventions to view rules and validate the current selection.
