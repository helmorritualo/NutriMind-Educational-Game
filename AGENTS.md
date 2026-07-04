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
- For hand-alignment of scene-template rows, strip Layout Groups and LayoutElements from the row and its children (and disable `Content`'s child control) so RectTransform Pos/Size fields are editable in the Inspector. Re-enable layout on the prefab only if runtime auto-layout is desired.

## Learned Workspace Facts

- `QuizPortalSceneSetupEditor` must persist controller references with `SerializedObject.ApplyModifiedPropertiesWithoutUndo()`; setter-only wiring does not survive scene save.
- Available Quiz List subject filter chips need per-subject unselected sprites from the `AvailableQuizzes` sheet (`AvailableQuizzes_7`, `_8`, `_9`); do not reuse the All unselected sprite (`AvailableQuizzes_6`) for LiteraQuest, PE/Health, or Science.
- Phase 8B visual specs also live under `docs/unity/` (e.g. `04D_QUIZ_UI_STORYBOARD_REQUIREMENTS.md`, `04E_AVAILABLE_QUIZ_LIST_VIEW_OVERLAY.md`) and `docs/design-reference-quiz-system/`, in addition to `design-references/quiz-ui-storyboard-reference.png`.
- `QuizListRow` prefab (`Assets/_Project/Nutrimind/Prefabs/UI/QuizListRow.prefab`, guid `0092d0593f7bee1418c83d525c985c90`) is wired to `AvailableQuizListController._rowPrefab`; `Content` starts empty and rows spawn dynamically from demo/server data via `RenderList`/`SpawnRow`.
- Status badges are sprite-driven from data: `QuizStates_2` for `unlocked`/`completed`, `QuizStates_3` for `locked`. Action buttons: `AvailableQuizzes_16` (Start) for `unlocked`, `AvailableQuizzes_17` (View) for `locked`. TMP status/action labels are intentionally null because the sprites bake in the text.
- Demo data (`Resources/DemoData/full-demo-student-data.json`) contains mixed `unlocked` and `locked` quiz states, so the dynamic list shows both Available and Locked rows.
