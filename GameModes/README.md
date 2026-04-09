# 🎮 Game Modes Directory

This directory contains isolated game modes (playstyles), each with its own code, resources, and configuration.

## Structure

```
GameModes/
├── base_game/          # Base game (Slay the Spire clone)
│   ├── Code/           # Gameplay-specific C# scripts
│   ├── Config/         # JSON configuration files
│   ├── Scenes/         # Godot scene files (.tscn)
│   └── Resources/      # Art & audio assets
│       ├── Images/     # PNG images
│       ├── Audio/      # OGG/WAV audio
│       └── Icons/      # UI icons
│
├── frost_expansion/    # Future: Ice theme expansion
└── shadow_realm/       # Future: Shadow mod
```

## Adding a New Game Mode

1. Create a new folder under `GameModes/<mode_name>/`
2. Follow the same structure as `base_game/`
3. Add your mode-specific files
4. Update `PackageManager.cs` to recognize the new mode

## Integration with Package System

Each folder corresponds to a downloadable package. When a package is installed:

1. Download ZIP from CDN
2. Extract to `user://packages/<mode_id>/`
3. Load configuration from `package_config.json`
4. Register gameplay extensions via `IPackageExtension`

## Notes

- **Framework code** stays in `Scripts/` (shared across all modes)
- **Mode-specific code** goes in `GameModes/<mode>/Code/`
- **Resources** are isolated per mode to prevent conflicts
- **Configuration** is self-contained in each mode's folder

Last updated: 2026-04-10 01:41
