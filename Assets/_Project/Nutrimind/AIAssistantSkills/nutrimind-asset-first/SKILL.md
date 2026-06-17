---
name: nutrimind-asset-first
description: Re-scan and reuse NutriMind owner-provided and third-party assets before creating or generating UI, prefabs, models, textures, materials, audio, animations, or world props.
required_editor_version: ">=6000.4.10f1"
---
# Asset-First Workflow
Before every visual task, search `Assets/_Project/Nutrimind/ThirdParty/`, project-owned provided assets, existing prefab variants, UI/world/station references, and recently imported assets. Never destructively edit vendor assets. Use owner-provided assets first, then third-party assets, existing variants, new variants or adapters, generated assets, and finally placeholders. Report source and final asset paths.
