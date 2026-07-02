# Unity Phase 8B Prompt — Quiz Portal Scope

You are working in Cursor on the NutriMind Unity project. This is a new AI coding session.

First read `AGENTS.md`, all files in `.cursor/rules/`, `README.md`, `UNITY_AI_ASSISTANT_CUSTOM_INSTRUCTIONS.txt` if present, `docs/UNITY_REQUIREMENTS.md`, `docs/unity/03_SHARED_CLIENT_SYSTEMS.md`, `docs/unity/04_SERVER_CONNECTION_AND_UNITY_API.md`, `docs/unity/04B_QUIZ_PORTAL_AND_ASSESSMENT_SYSTEM.md`, `docs/unity/04C_REWARDS_SHOP_DEFERRED.md`, `docs/unity/05_APPLICATION_SCENES_AND_HYBRID_UI.md`, `docs/unity/09_UI_CONTROLS_ACCESSIBILITY_AND_PRESENTATION.md`, `docs/unity/10_TESTING_INTEGRATION_AND_RELEASE.md`, and the latest Phase 6C, Phase 7, and Phase 8 completion reports.

Unity Phases 1 to 6 already exist. Do not restart the project. Preserve valid completed foundation work, assets, app scene UI, session logic, provider abstraction, LocalDemoJson infrastructure, fake login, profile/settings/logout, resettable demo state, and useful tests.

The current milestone is quiz-first. Implement only the Phase 8B Quiz Portal scope listed below. Do not implement gameplay missions, mission tracking, rewards shop, inventory, pets, cosmetics, world restoration, spendable coins, EXP, or the future 90-mission catalog.

The quiz UI for this phase must use UI Toolkit. Use existing UI asset components, references, sprites, icons, panels, USS variables, UXML templates, project-owned style assets, and project-owned variants before creating new assets. Use UXML for structure, USS for styling, and C# presenters/controllers for behavior.

Implement only these Phase 8B units in this scope: Quiz Portal Home, Available Quiz List, Empty Quiz State, Locked Quiz State, Quiz Instructions, Quiz Session Shell, Multiple Choice Single Presenter, Multiple Choice Multiple Presenter, True/False Presenter, Unsupported Item State, Submit Confirmation, Quiz Result Screen, and Quiz Error/Retry State.

Do not implement Matching, Ordering, Fill Blank, Short Answer, Categorization, Drag/Drop, Image Hotspot, Labeling, Reading Passage, Cloze, Numeric, or Reflection presenters in this session unless the project owner explicitly adds them.

Use provider-driven quiz data. Do not hardcode quiz content in UI scripts. Use shared quiz stores, quiz session service, quiz answer draft service, quiz validation service, quiz submission coordinator, quiz result store, safe error service, navigation service, and quiz item presenter registry.

The backend contract is Laravel REST with contract version `quiz_first_laravel_1`. No WebSocket is required. Quiz submission uses `client_attempt_uuid`.

Quiz Portal Home must clearly present the Quiz Portal or Assessment Room as the active feature and must not imply adventure gameplay is available.

Available Quiz List must show assigned quizzes from provider data and must support loading, populated, empty, locked/unavailable, and error states. Use ListView or an equivalent UI Toolkit list approach if the list can grow.

Empty Quiz State must explain that no quizzes are currently available and provide safe navigation back to the main interface.

Locked Quiz State must explain why a quiz is unavailable when the provider supplies a reason and must avoid exposing hidden answer or teacher-only data.

Quiz Instructions must show title, subject, term, grade, item count, attempt limit, time limit if any, teacher instructions if visible, and start action.

Quiz Session Shell must manage quiz progress, current item, navigation, answer draft state, validation, and submit entry. It must not contain item-specific logic that belongs in item presenters.

Multiple Choice Single Presenter must allow exactly one selected option and update the answer draft service.

Multiple Choice Multiple Presenter must allow multiple selected options and update the answer draft service.

True/False Presenter must show two clear choices and update the answer draft service.

Unsupported Item State must safely show that the item type is not supported yet and must prevent invalid submission when required.

Submit Confirmation must summarize unanswered or invalid items and confirm before submission without showing answer keys.

Quiz Result Screen must show the server/provider result according to result visibility, including score, pass/fail if available, feedback if allowed, and safe return navigation.

Quiz Error/Retry State must support safe retry, return to Quiz Portal, return to Main Interface, and session-expired handling where applicable.

Keep UI scripts thin. Use presenters/services for behavior. Do not place provider calls directly in low-level UI components. Do not add unnecessary singletons or service locators.

Validate all serialized references, UIDocument references, UXML references, USS references, and event callbacks. Register and unregister callbacks safely. Avoid repeated full-tree queries and unnecessary Update loops.

Run Unity compilation, Console review, UI Toolkit UIDocument checks, PanelSettings checks, USS/UXML reference checks, safe-area checks, Android landscape layout checks, touch target checks, quiz list checks, answer draft checks, submit confirmation checks, result screen checks, unsupported item fallback checks, error/retry checks, LocalDemoJson checks, and provider DTO checks. Run Edit Mode or Play Mode tests where available or where new testable logic is added.

Report the files changed, assets changed, existing UI assets reused, new UI assets created, tests/checks run, exact results, Console warnings or errors, deferred items, remaining gaps, and next recommended unit. Stop after this Phase 8B scope.
