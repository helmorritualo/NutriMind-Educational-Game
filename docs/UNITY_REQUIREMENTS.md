# Unity Requirements Index

## Purpose

This file is the entry point and authority map for the Unity client requirements. Detailed Unity requirements are split into focused documents under `docs/unity/`.

The Unity client is the student-facing game. Unity owns presentation, scene flow, controls, world interactions, local input state, feedback presentation, and safe handling of server responses. The server owns identity, approved content, station availability, scoring, rewards, progress, and reports.

## Documentation Authority

Read the Unity documents in this order:

1. [`unity/01_FOUNDATION_AND_DELIVERY_ORDER.md`](unity/01_FOUNDATION_AND_DELIVERY_ORDER.md)
2. [`unity/02_GAME_FLOW_AND_STATE_MODEL.md`](unity/02_GAME_FLOW_AND_STATE_MODEL.md)
3. [`unity/03_SHARED_CLIENT_SYSTEMS.md`](unity/03_SHARED_CLIENT_SYSTEMS.md)
4. [`unity/04_SERVER_CONNECTION_AND_UNITY_API.md`](unity/04_SERVER_CONNECTION_AND_UNITY_API.md)
5. [`unity/04A_LEARNING_THROUGH_GAMEPLAY_STORY_AND_REWARDS.md`](unity/04A_LEARNING_THROUGH_GAMEPLAY_STORY_AND_REWARDS.md)
6. [`unity/05_SCENES_WORLDS_AND_STATION_FRAMEWORK.md`](unity/05_SCENES_WORLDS_AND_STATION_FRAMEWORK.md)
7. [`unity/06_LITERAQUEST_GAMEPLAY.md`](unity/06_LITERAQUEST_GAMEPLAY.md)
8. [`unity/07_SCIENCE_QUEST_GAMEPLAY.md`](unity/07_SCIENCE_QUEST_GAMEPLAY.md)
9. [`unity/08_HEALTH_QUEST_GAMEPLAY.md`](unity/08_HEALTH_QUEST_GAMEPLAY.md)
10. [`unity/09_UI_CONTROLS_ACCESSIBILITY_AND_PRESENTATION.md`](unity/09_UI_CONTROLS_ACCESSIBILITY_AND_PRESENTATION.md)
11. [`unity/10_TESTING_INTEGRATION_AND_RELEASE.md`](unity/10_TESTING_INTEGRATION_AND_RELEASE.md)
12. [`unity/11_DEMO_DATA_AND_LOCAL_PROVIDER.md`](unity/11_DEMO_DATA_AND_LOCAL_PROVIDER.md)

Canonical local-demo fixture (loadable via `Resources` in editor/development builds; deserializes into `DemoFixtureDto`):

- `Assets/_Project/Nutrimind/Resources/DemoData/full-demo-student-data.json`

The older [`unity/examples/full-demo-student-data.json`](unity/examples/full-demo-student-data.json) used divergent field names and is **superseded**; the canonical fixture follows the snake_case DTO contract. The canonical Unity data contract schemas are defined once in `docs/SERVER_REQUIREMENTS.md` (Canonical Unity Data Contract Schemas) and mirrored field-for-field in `unity/04_SERVER_CONNECTION_AND_UNITY_API.md` and `unity/11_DEMO_DATA_AND_LOCAL_PROVIDER.md`.

When requirements conflict:

1. Server requirements are authoritative for API paths, authentication, payloads, validation, scoring, rewards, progress, and student-safe data.
2. Unity requirements are authoritative for Unity architecture, scenes, controls, gameplay presentation, local assets, and crash-safe integration.
3. Subject gameplay documents are authoritative for the current demo mechanic and scene scope.
4. Server contract changes may update Unity API requirements, but they must not silently redefine Unity-owned scene behavior.

## Agent Task Phase Prompts

Operational prompts are split under `agent-prompts/unity/` so Unity work can be assigned and reviewed in manageable phases. Each prompt maps to one numbered Unity requirement document and requires the agent to stop after its selected checkpoint, screen, world, station, or test unit.

Start with:

- [`../agent-prompts/unity/00_MASTER_ORCHESTRATOR.md`](../agent-prompts/unity/00_MASTER_ORCHESTRATOR.md)
- [`../agent-prompts/unity/README.md`](../agent-prompts/unity/README.md)

The phase prompts do not replace these requirements. When a prompt and a requirement conflict, the requirement document is authoritative.

### Prompt Loading Rule

For every fresh Unity-agent session, load both:

1. `agent-prompts/unity/00_MASTER_ORCHESTRATOR.md`
2. exactly one selected phase prompt from `agent-prompts/unity/`

The master orchestrator supplies the permanent scope, sequence, asset policy, stop rules, and report format. The selected phase prompt supplies the current unit of work. A phase prompt is not intended to run without the master orchestrator.

In a continuing conversation where the master orchestrator has already been loaded and remains in context, the user may send only the next phase prompt after explicitly approving the previous checkpoint. For a new or uncertain session, always reload both files.

Use `agent-prompts/unity/SESSION_START_TEMPLATE.md` to identify the selected phase, selected unit, and approved prerequisites.

## Current Demo Scope

The current Unity milestone is intentionally focused. It does not require every future station supported by the server.

| Platform | Current Unity delivery |
|---|---|
| LiteraQuest | Three term exploration worlds and two complete playable station scenes per term: six stations total |
| PE/Health Quest | Three term exploration worlds and two complete playable station scenes per term: six stations total; the server slug may remain `health_quest` |
| Science Quest | Three term exploration worlds only; no functional station gameplay, task submission, scoring, rewards, or station portals in this milestone |

Current playable demo delivery:

```txt
6 LiteraQuest station scenes
+ 6 PE/Health station scenes
= 12 complete playable station scenes

3 LiteraQuest term worlds
+ 3 PE/Health term worlds
+ 3 Science term worlds
= 9 exploration world scenes
```

The first full demo may use one fabricated Grade 5 student. The architecture and DTOs must remain grade-aware so additional grades can be added without rewriting shared systems.

Additional server-supported station slots are deferred. They are not required for the current Unity demo unless the project owner explicitly expands the scope.

## Development Model

Unity development must follow two layers:

1. **Foundation dependencies/bootstrap** — project folders, provided-asset inventory, asset/prefab catalog, packages, UI Toolkit design assets, shared services, data providers, navigation, scene registry, shared world/station framework, and tests.
2. **One-scene-at-a-time development** — complete, verify, report, and stop after each application screen, world scene, or station gameplay scene.

Do not build several scenes in parallel. Do not begin the next scene until the current scene has been reviewed and explicitly approved.

## Non-Negotiable Unity Rules

- All new runtime UI uses UI Toolkit with UXML, USS, `UIDocument`, and shared `PanelSettings`.
- Do not create new Canvas/uGUI game screens, HUDs, dialogs, or station panels.
- The project root and `Assets/_Project/Nutrimind/` structure must exist before UI or scene creation.
- User-provided project assets and prefabs are the first choice for UI, worlds, stations, props, characters, effects, icons, materials, and interactions.
- User-provided visual references guide layout, hierarchy, components, spacing, typography, world composition, prop direction, and station presentation. Background images are supplied separately by the user under the UI background folder.
- The agent creates or generates a new asset only when no suitable provided asset or prefab exists after an explicit inventory.
- A missing final background uses a neutral placeholder and must not block development.
- HTTPS REST is sufficient for the complete required game loop.
- WSS/WebSocket remains optional and metadata-only.
- Unity never decides official correctness, scoring, progress, unlocks, rewards, or completion when using the real server.
- Catalog and availability data come from the active data provider. Unity must not hardcode canonical subject, term, station, curriculum, reward, or availability data.
- Local scene, prefab, icon, audio, and presentation mappings may use stable server keys.
- Unknown optional fields and enum values must not crash the client.
- Network callbacks must not mutate Unity objects from unsafe threads.
- Missing scene/content mappings must show a safe unavailable state, not load an unrelated scene.
- Attempt retries reuse the same `client_attempt_uuid`.
- Manual and AI-assisted teacher content are equivalent to Unity after server approval and publication.
- `LocalDemoJson` and `Http` must use the same DTOs, stores, UI, scenes, and gameplay systems.
- Production builds must reject or exclude local demo data.

## Safe Mistakes, Story, NPCs, Exploration, and Rewards

- NPC mentors introduce short missions, offer approved hints, and celebrate completion; they do not replace gameplay with lectures.
- Wrong attempts preserve progress where safe and provide encouraging, misconception-aware feedback and tiered hints.
- Optional fun facts, bonus coins, story notes, and secret cosmetic areas may be discovered in term worlds but are never required to finish stations.
- Official coins, crystals, badges, titles, cosmetics, unlocks, and other rewards remain provider/server-authoritative.
- Completing stations may visually restore parts of the world only after the accepted result.
- Science remains exploration-only and must not award station progress or station-completion rewards in this milestone.

## Asset and Prefab Priority Policy

Unity development must follow this order for every UI screen, world, station, and shared system:

1. **Inventory first.** Inspect the assets, prefabs, materials, models, textures, sprites, icons, audio, animations, UXML, USS, and reference images already present in the Unity project.
2. **Use user-provided assets first.** Reuse a suitable supplied asset or prefab when it satisfies the current requirement.
3. **Adapt non-destructively.** Prefer prefab variants, project-owned materials, import-setting adjustments, colliders, LODs, bindings, wrappers, or UI Toolkit styling over editing vendor/source assets directly.
4. **Create only when missing.** Create or generate a new project-owned asset only after the agent verifies that no suitable supplied asset or prefab exists.
5. **Use project references for direction.** New assets may use image references stored in the project as guidance for shape language, composition, proportions, mood, material direction, interface structure, and subject identity. They must be original project assets rather than blind copies of the reference.
6. **Keep backgrounds user-owned.** Do not create or generate final screen background images unless the user explicitly requests it. Use the user-supplied file from the UI background folder or a neutral placeholder.
7. **Report provenance.** For each completed scene, report which assets were reused, adapted, newly created/generated, or still represented by placeholders.

The agent must not create duplicate replacement assets merely because creating a new asset is easier than locating an existing one. It must not overwrite imported package or user source assets destructively. If an asset's license, source, intended use, or suitability is unclear, stop and report it instead of silently shipping it.

## Learning-Through-Gameplay Design

The current game must feel like an exploration and mission game in which learning is required to change the world, help characters, restore locations, and progress. It must not become a decorative walk followed by a detached quiz.

All playable LiteraQuest and PE/Health stations follow **Discover -> Practice -> Apply -> Review**, use safe mistakes and provider-approved hints, and provide a visible world consequence after accepted completion. Optional NPC guidance, discoveries, coins, subject crystals, badges, titles, cosmetics, and secret areas must remain data-driven and must not replace the learning objective.

The detailed cross-cutting requirements are in [`unity/04A_LEARNING_THROUGH_GAMEPLAY_STORY_AND_REWARDS.md`](unity/04A_LEARNING_THROUGH_GAMEPLAY_STORY_AND_REWARDS.md). Because Phases 1–4 may already be complete, agents perform an additive compatibility review rather than rebuilding approved foundation work.

## Data Source Modes

```txt
LocalDemoJson -> editor/development demo while the server is unavailable
Http          -> integration, staging, and production
```

The local fixture covers the 12 playable demo stations, all nine term worlds, fake profile/settings/progress/rewards, and empty Science station lists. Science exploration worlds must not fabricate station completion or rewards.

## Required Student Loop

```txt
Splash
  -> compatibility/demo-mode check
  -> LRN + PIN or explicit demo login
  -> bootstrap
  -> main interface
  -> subject selection
  -> term selection
  -> registered term world
  -> station portal for LiteraQuest or PE/Health
  -> dedicated station gameplay scene
  -> attempt submission
  -> feedback, progress, and reward result
  -> return to the originating term world
```

For Science Quest in this milestone:

```txt
subject selection
  -> term selection
  -> Science exploration world
  -> movement, camera, ambient presentation, HUD, and return action only
```

## Completion Definition

The current demo milestone is complete when:

- foundation/bootstrap dependencies are shared and reusable
- provided assets and prefabs have been inventoried and reused before new asset creation
- UI Toolkit design assets and common components are established
- core application scenes/screens work one at a time
- nine term exploration worlds load safely
- six LiteraQuest and six PE/Health station scenes complete their full local-demo loop
- Science term worlds are clearly marked as exploration previews and have no unfinished functional station controls
- the same scene code can switch from `LocalDemoJson` to `Http` through provider configuration
- tests, Android landscape checks, accessibility, failure handling, and documentation are consistent

A scene opening successfully is not enough. Each required scene must satisfy its own acceptance checks before the next scene begins.
