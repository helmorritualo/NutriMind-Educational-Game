# Unity Phase 8B Prompt — Single Quiz Portal Unit for Cursor, Canvas/uGUI Version

You are working in Cursor on the NutriMind Unity project. This is a new AI coding session. Before changing files, read `AGENTS.md`, all files in `.cursor/rules/`, `UNITY_AI_ASSISTANT_CUSTOM_INSTRUCTIONS.txt` if present, `README.md`, `docs/UNITY_REQUIREMENTS.md`, `docs/unity/03_SHARED_CLIENT_SYSTEMS.md`, `docs/unity/04_SERVER_CONNECTION_AND_UNITY_API.md`, `docs/unity/04B_QUIZ_PORTAL_AND_ASSESSMENT_SYSTEM.md`, `docs/unity/04D_QUIZ_UI_STORYBOARD_REQUIREMENTS.md`, `docs/unity/04E_AVAILABLE_QUIZ_LIST_VIEW_OVERLAY.md`, `docs/unity/04C_REWARDS_SHOP_DEFERRED.md`, `docs/unity/05_APPLICATION_SCENES_AND_HYBRID_UI.md`, `docs/unity/09_UI_CONTROLS_ACCESSIBILITY_AND_PRESENTATION.md`, `docs/unity/10_TESTING_INTEGRATION_AND_RELEASE.md`, and the latest Phase 6C, Phase 7, and Phase 8 completion reports.

The current quiz unit for this session is `<CURRENT_QUIZ_UNIT>`.

Implement only the current quiz unit named above. Do not work on any other quiz unit in this session. Do not continue to the next quiz unit after finishing this one. Stop after reporting.

The current approved Phase 8B unit order is only this: Quiz Portal Home, Available Quiz List, Empty Quiz State, Locked Quiz State as Available Quiz List Canvas modal/panel, Quiz Instructions, Quiz Session Shell, Multiple Choice Single Presenter, Multiple Choice Multiple Presenter, True/False Presenter, Unsupported Item State, Submit Confirmation, Quiz Result Screen, Quiz Error/Retry State. This list is only for orientation. This session must implement only `<CURRENT_QUIZ_UNIT>`.

Use Canvas/uGUI for this quiz unit. Do not use UI Toolkit for this quiz unit. Use existing generated sprites and sprite sheets, Canvas root, Canvas Scaler, Graphic Raycaster, EventSystem, RectTransform anchors/pivots, Image components, sliced sprites, TextMeshPro labels, Button components, Toggle or custom selection components, ScrollRect where needed, and prefabs/prefab variants.

Use the generated quiz UI assets as reusable sprites, not as one flat screenshot. Important text and quiz data must be dynamic TextMeshPro labels unless the asset is a deliberately static button with a label that will never change.

If the current unit is Available Quiz List, implement subject filters only as the visible filter row: All, LiteraQuest, PE/Health, and Science. Do not implement Term 1, Term 2, or Term 3 as primary filter chips. Term remains row metadata. Implement or prepare row action behavior: Start opens playable quizzes, View opens locked/unavailable detail modal, and View Result opens result screen or summary according to result visibility. The modal must preserve filters, scroll position, selected quiz, list data, and navigation context.

If the current unit is Locked Quiz State, implement it as the Available Quiz List in-screen Canvas modal/panel, not as a separate scene. The modal must show quiz title, subject, term, grade, item count, status, locked reason, available date/time if provided, compatibility warning if any, and Back/Close. It must not expose answer keys, hidden quiz items, teacher-only notes, admin data, or other students' results.

Use provider-driven quiz data. Do not hardcode quiz content in UI scripts. Use shared quiz stores, quiz session service, answer draft service, quiz validation service, quiz submission coordinator, quiz result store, safe error service, navigation service, and quiz item presenter registry. Keep UI scripts thin. Do not place provider calls directly in low-level UI components. Do not add unnecessary singletons or service locators.

The backend contract is Laravel REST with contract version `quiz_first_laravel_1`. No WebSocket is required. Quiz submission uses `client_attempt_uuid`.

Do not implement Matching, Ordering, Fill Blank, Short Answer, Categorization, Drag/Drop, Image Hotspot, Labeling, Reading Passage, Cloze, Numeric, or Reflection presenters in this session. Do not implement gameplay missions, mission tracking, rewards shop, inventory, pets, cosmetics, world restoration, spendable coins, EXP, or the future 90-mission catalog.

Validate serialized references, Canvas references, Canvas Scaler, Graphic Raycaster, EventSystem, RectTransform anchors/pivots, Image sprite references, TextMeshPro labels, Button callbacks, Toggle/custom selection state, ScrollRect behavior where used, modal blocking behavior, focus/navigation behavior, touch targets, safe area, Android landscape layout, and Console output. Register and unregister callbacks safely. Avoid unnecessary Update loops.

Run Unity compilation, Console review, Canvas root checks, Canvas Scaler checks, Graphic Raycaster checks, EventSystem checks, scene reference checks, prefab reference checks, sprite reference checks, safe-area checks, Android landscape layout checks, touch target checks, and the checks relevant to `<CURRENT_QUIZ_UNIT>`. Run Edit Mode or Play Mode tests where available or where new testable logic is added.

Report the completed quiz unit, files changed, assets changed, existing UI assets reused, new UI assets created, tests and checks run, exact results, Console warnings or errors, deferred items, remaining gaps, and the next recommended quiz unit. Stop after reporting.
