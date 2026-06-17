---
name: nutrimind-ui-toolkit-migration
description: Create NutriMind runtime UI with UI Toolkit and migrate approved Canvas or uGUI prefabs into UXML, USS, UIDocument, shared PanelSettings, and C# controllers.
required_editor_version: ">=6000.4.10f1"
---
# UI Toolkit Migration
Use UI Toolkit for all new runtime UI. For a Canvas migration, inspect the original prefab and dependencies, reuse valid sprites/textures/fonts/icons/audio, recreate structure in UXML, styling in USS, and behavior through controllers and UI Toolkit events. Use shared PanelSettings and theme. Preserve safe-area, Android landscape, touch, focus, loading, error, and accessibility states. Validate UXML/USS and preview visually. Keep the original Canvas prefab untouched until approval. Screens use shared stores and providers, not direct JSON or HTTP.
