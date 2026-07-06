# AGENTS.md — NutriMind Unity Repository v8

## Current milestone

Unity is quiz-first and Canvas-aligned for the quiz system.

Use `design-references/quiz-ui-storyboard-reference.png` as the Phase 8B visual reference.

## Quiz UI technology

Use Canvas/uGUI for the Quiz Portal and Phase 8B screens.

Do not use UI Toolkit for quiz screens unless the project owner explicitly changes the requirement.

Use generated assets as sprites/sprite sheets, sliced sprites, Image components, Buttons, TextMeshPro labels, RectTransforms, ScrollRects, Toggles/custom selection groups, Canvas Scaler, and prefabs.

## Current Phase 8B units

Build only:

- Quiz Portal Home
- Available Quiz List
- Empty Quiz State
- Locked Quiz State as Available Quiz List Canvas modal/panel
- Quiz Instructions
- Quiz Session Shell
- Multiple Choice Single Presenter
- Multiple Choice Multiple Presenter
- True/False Presenter
- Unsupported Item State
- Submit Confirmation
- Quiz Result Screen
- Quiz Error/Retry State

## Important rules

Quiz Portal is global by default.

Subject is the primary visible filter in Available Quiz List.

Term is metadata, not a primary student filter chip.

Use dynamic TextMeshPro labels and provider data. Do not bake meaningful dynamic text into images.

Do not implement mission gameplay, rewards shop, inventory, world restoration, or unapproved presenters.

Use contract version `quiz_first_laravel_1`.

## Learned User Preferences

- Match Phase 8B Quiz Portal Canvas layout to the docs design reference and storyboard as closely as practical.
- Prefer authoring and aligning quiz list rows in-scene under `QuizListScrollArea/Viewport/Content` before recreating or assigning a `QuizListRow` prefab.
- For hand-alignment of scene-template rows, strip Layout Groups and LayoutElements from the row and its children (and disable `Content`'s child control) so RectTransform Pos/Size fields are editable in the Inspector; use **NutriMind → Quiz Portal → Unlock Quiz List Row For Manual Edit** to remove layout drivers. Re-enable layout on the prefab only if runtime auto-layout is desired.
- Keep Empty Quiz State copy baked in `QuizStates_0`; wire show/hide and Refresh/Back only—no dynamic TMP message labels.
- Prefer Unity's native MCP server in Cursor (`user-unity-mcp`) for Editor wiring, console checks, and scene saves when Unity is open and connected.
- For LiteraQuest Mission 1 gameplay HUD/modals, build Canvas at 1920×1080 with QuizAssets sliced sprites and LiberationSans TMP (same typography as Quiz Portal); use **NutriMind → LiteraQuest Mission 1 → Setup Mission Canvas UI** for one-shot scene setup.
- When mission gameplay UI looks broken, use Unity MCP scene/game capture (`Unity_Camera_Capture`) to verify layout before iterating.
- Current gameplay focus is Grade 5 LiteraQuest Mission 1 Area 1 only; Phase 8B quiz is on hold until Area 1 MVP lands.
- Do not redesign owner-placed LiteraQuest Mission 1 scenes; implement logic, data binding, and Inspector wiring only.
- Use serialized scene references in mission gameplay controllers; avoid runtime `Find` except a one-time fallback with clear warnings.
- Use `CharacterController` (not Rigidbody) for LiteraQuest player movement; attach `PlayerMovementController` on scene `Player`.
- Prefer a low third-person camera behind the player (not ground-level/top-down) for LiteraQuest mission areas.

## Learned Workspace Facts

- `QuizPortalSceneSetupEditor` must persist controller references with `SerializedObject.ApplyModifiedPropertiesWithoutUndo()`; setter-only wiring does not survive scene save.
- Quiz list UI: subject chips use `AvailableQuizzes_7`/`_8`/`_9`; rows from `QuizListRow` prefab on `AvailableQuizListController`; status/action sprites `QuizStates_2`/`_3` and `AvailableQuizzes_16`/`_17`; `EmptyQuizStatePanel` uses `QuizStates_0`/`_2`/`_3`.
- Phase 8B specs/sprites live under `docs/unity/`, `docs/design-reference-quiz-system/`, and `Assets/_Project/Nutrimind/Art/Sprite/QuizAssets/`.
- Demo data `Resources/DemoData/full-demo-student-data.json` has mixed `unlocked` and `locked` quiz states.
- Unity Editor MCP is `user-unity-mcp`; legacy `com.anklebreaker.unity-mcp` will be removed from `Packages/manifest.json`.
- LiteraQuest Mission 1 scene: `Assets/_Project/Nutrimind/Scenes/App/Literaquest Term/LiteraQuest_Term1_Mission1.unity`; canvas setup via `LiteraQuestMission1CanvasSetupEditor.cs` (`NutriMind/LiteraQuest Mission 1/Setup Mission Canvas UI`).
- `PlayerMovementController` on `Player`; disable `PlayerModel` `CapsuleCollider`; snaps to `PlayerSpawnPoint`, grounds feet via raycast; Joystick Pack `Fixed Joystick` + `JumpButton `; Input System + legacy keyboard fallback.
- `SimpleCameraFollow` on `Main Camera` → `Player`; third-person offset `(0, 2.5, -5)`, `lookAtOffset` `(0, 1.3, 0)`.
- `G5Area01MissionController` on `Systems/G5Area01MissionController `; mission scripts under `Runtime/Gameplay/Missions/`; `GateBlocker` is a `BoxCollider` child under `GateToNextArea ` (controller toggles it, not `ClosedGate` MeshCollider).
- Mission 1 object names often have trailing spaces (`G5Area01MissionController`, `FarmerLira_NPC`, `GateToNextArea`, `ClosedGate`, `StoryMapFragment`, `JumpButton`).
- Mission 1 specs: `docs/unity/06B_LITERAQUEST_MISSION1_SCENE_OBJECT_AND_ASSET_CHECKLIST.md` and `06C_LITERAQUEST_MISSION1_AI_AGENT_IMPLEMENTATION_PLAN.md`.
- Imported Built-in pipeline third-party materials render pink in URP—convert to `Universal Render Pipeline/Lit` (e.g. Skyden Games `Colors_Mat.mat`, `Colors Water_Mat.mat`).
