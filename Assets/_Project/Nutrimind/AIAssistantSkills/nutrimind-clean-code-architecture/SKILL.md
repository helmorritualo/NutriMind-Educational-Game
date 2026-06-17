---
name: nutrimind-clean-code-architecture
description: Apply NutriMind Unity clean-code, assembly, lifecycle, UI Toolkit, async, testability, and pragmatic design-pattern rules when creating or reviewing C# scripts.
required_editor_version: ">=6000.4.10f1"
---
# NutriMind Clean Code and Architecture

Before changing scripts, inspect the existing architecture and assembly definitions.

Use simple, explicit designs. Apply SOLID pragmatically and prefer composition over inheritance.

Use patterns only when they solve a real project need:

- State for app, station, and challenge flow.
- Strategy for challenge presenters, hint policies, and provider implementations.
- Adapter for third-party assets, legacy Canvas UI, and external services.
- Factory or registry for approved presenter, prefab, provider, and stable-key resolution.
- Typed events for one-to-many notifications.
- Presenter-based separation for UI Toolkit.
- ScriptableObjects for editor-authored configuration and asset mappings, not mutable student authority.
- Object pooling only for confirmed high-frequency creation and destruction.

Keep MonoBehaviours thin and put deterministic rules into testable plain C# classes.

Construct dependencies in CompositionRoot. Do not add global singletons or service locators.

Use private SerializeField fields for Inspector references, validate required references, and preserve serialized-field migrations.

Subscribe in OnEnable and unsubscribe in OnDisable. Cancel async work on screen close, scene unload, session end, or replacement. Mutate Unity objects only on the main thread.

Respect assembly boundaries: runtime never references editor code, test assemblies use explicit references, and circular dependencies are forbidden.

For UI Toolkit, keep UXML as structure, USS as presentation, and C# as behavior. Cache VisualElements and clean up callbacks.

Measure before optimizing. Avoid hot-loop allocations, repeated scene searches, repeated UI queries, and unnecessary Instantiate/Destroy operations.

Add focused Edit Mode or Play Mode tests for changed behavior. Run compilation and relevant tests before reporting completion.
