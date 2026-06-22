# AGENTS.md — NutriMind Unity Repository

## Purpose

This file is for AI coding agents working inside the Unity project repository.

The current Unity milestone is quiz-first. The Unity project should be completed only up to the Quiz Portal / Assessment Room for now. Do not start open-world gameplay missions, rewards shop, inventory, cosmetics, pets, world restoration, or mission progress tracking until the project owner gives a future go signal.

## Required Reading

Before making changes, read these files if they exist in this repository:

`README.md`
`docs/UNITY_REQUIREMENTS.md`
`docs/unity/01_FOUNDATION_AND_DELIVERY_ORDER.md`
`docs/unity/02_GAME_FLOW_AND_STATE_MODEL.md`
`docs/unity/03_SHARED_CLIENT_SYSTEMS.md`
`docs/unity/04_SERVER_CONNECTION_AND_UNITY_API.md`
`docs/unity/04B_QUIZ_PORTAL_AND_ASSESSMENT_SYSTEM.md`
`docs/unity/04C_REWARDS_SHOP_DEFERRED.md`
`docs/unity/05_APPLICATION_SCENES_AND_HYBRID_UI.md`
`docs/unity/10_TESTING_INTEGRATION_AND_RELEASE.md`
`docs/unity/11_DEMO_DATA_AND_LOCAL_PROVIDER.md`
`agent-prompts/unity/00_MASTER_ORCHESTRATOR.md`
`agent-prompts/unity/UNITY_QUIZ_FIRST_PHASE_PROMPTS.md`

If a fresh AI session starts, ask for or locate the latest phase completion report before continuing.

## Current Project State

Unity Phases 1 to 6 were already completed under older documentation. Do not restart the Unity project from scratch.

Preserve valid completed work, including project structure, assemblies, asset folders, imported third-party assets, existing application scene UI designs, existing Canvas UI, existing UI Toolkit UI, app flow, session logic, provider abstraction, profile/settings/logout flow, LocalDemoJson infrastructure, fake login, resettable demo state, and useful tests.

The required next alignment step is Phase 6C if it has not already been completed.

Phase 6C means aligning the completed Unity work to the current quiz-first Laravel REST contract version `quiz_first_laravel_1`.

## Current Unity Scope

Implement and validate only:

application bootstrap
splash screen
login
main interface
profile
settings
subject selection
term selection or quiz filter
loading/transition screen
Quiz Portal / Assessment Room
available quiz list
quiz instructions
quiz session shell
quiz item presenters
submit confirmation
quiz result screen
safe unsupported item state
LocalDemoJson quiz-first provider behavior
HTTP-compatible DTOs for the Laravel server

## Deferred Unity Scope

Do not implement these yet:

LiteraQuest mission gameplay
PE/Health mission gameplay
Science mission gameplay
mission objective tracking
mission completion
mission rewards
world restoration
spendable coins
EXP economy
item shop
inventory
pets
cosmetics
titles
equipment
server-driven mission progress
90-mission Grade 5 and Grade 6 catalog

If these systems already exist from older work, do not expand them. Disable, isolate, or mark them deferred unless needed only as harmless placeholders.

## Server Contract

The backend is now Laravel + React/Inertia + PostgreSQL.

Unity communicates with the server through HTTPS REST JSON APIs. No WebSocket is required for the current milestone.

Use contract version `quiz_first_laravel_1`.

Active Unity API behavior covers config, login, logout, refresh, bootstrap, profile, settings, subjects, terms, quizzes, quiz detail, quiz attempts, quiz results, and sync status.

Mission, reward shop, inventory, and world restoration APIs are deferred.

## UI System

The Unity UI system is hybrid Canvas + UI Toolkit.

Existing application-scene UI designs must be inspected and reused before creating new UI. Do not rebuild valid completed screens from scratch only to change the UI technology.

Canvas/uGUI is allowed for existing application scenes and already-made designs.

UI Toolkit is allowed for new structured screens, quiz lists, quiz panels, menus, and forms where it fits.

A screen may be Canvas, UI Toolkit, or hybrid. Choose the simplest reliable approach that matches existing project style.

Do not duplicate business logic between Canvas and UI Toolkit. Both UI systems must call shared stores, presenters, services, and DTOs.

Keep backgrounds clear behind static UI. Avoid confusing decorative backgrounds that make buttons or labels hard to read.

## Asset-First Rule

Before creating scripts, prefabs, UI, sprites, icons, panels, models, materials, or scenes, inspect:

existing application scenes
existing Canvas prefabs
existing UI Toolkit assets
owner-provided image references
`Assets/_Project/Nutrimind/ThirdParty/`
existing project-owned variants
existing scripts and assemblies

Use this priority:

owner-provided asset
third-party asset
existing project variant
new project-owned variant or adapter
new generated asset
placeholder

Never destructively edit vendor or third-party source assets. Create project-owned variants, adapters, prefabs, materials, or wrappers.

## Code Architecture

Keep MonoBehaviours thin. Put deterministic logic in plain C# classes when practical.

Use shared services and stores for session, profile, settings, subjects, terms, quiz availability, quiz details, quiz session, quiz answers, validation, submission, results, safe errors, and navigation.

Do not put official scoring, answer-key logic, server authority, or persistent economy logic inside scene MonoBehaviours.

Do not hardcode quiz content in UI scripts. Quiz content must come from LocalDemoJson or HTTP provider DTOs.

Do not hardcode final mission content. Mission content is deferred.

Use focused interfaces only when they improve testing, provider swapping, or UI separation. Do not add unnecessary design patterns.

Avoid new global singletons or service locators.

Preserve assembly boundaries. Runtime assemblies must not reference Editor assemblies.

## Quiz System Rules

The quiz system must support the contract for these item types:

multiple_choice_single
multiple_choice_multiple
true_false
matching
ordering
fill_blank
short_answer
categorization
drag_drop
image_hotspot
labeling
scenario_choice
reading_passage
cloze
numeric
likert_reflection

The first Unity implementation should prioritize:

multiple choice single
multiple choice multiple
true/false
matching
ordering
fill blank
short answer
safe unsupported item fallback

Unsupported item types must not crash. They must show a safe unsupported state and prevent invalid submission when required.

Quiz submission must use `client_attempt_uuid`.

Same UUID and same payload should replay the previous result when provider behavior is simulated.

Same UUID and different payload should produce a conflict error when provider behavior is simulated.

## Rewards and Shop

Rewards and item shop are deferred.

Do not implement spendable coins, EXP economy, item shop, inventory, pets, cosmetics, titles, equipment, purchases, or equipped items.

Quiz result UI may show score, feedback, pass/fail, and optional display-only stars or badges. These must not create a spendable economy.

## Unity Lifecycle and Async Rules

Validate serialized references.

Subscribe and unsubscribe callbacks symmetrically.

Cancel async work when a screen closes, a scene unloads, a session ends, or a newer request replaces it.

Do not mutate Unity objects from background threads.

Avoid `Find`, scene-wide searches, repeated VisualElement queries, heavy allocations, or unnecessary work in Update loops.

Use guarded error handling. Do not swallow exceptions silently.

## Testing and Validation

Run the most relevant checks available in the project. Use Unity Test Runner, Edit Mode tests, Play Mode tests, JSON validation, scene validation, and Console review as applicable.

For every phase report:

what existing work was preserved
what changed
what was removed or deferred
files and assets changed
tests run
Console errors or warnings
remaining gaps
whether the current phase is complete

## Phase Order

Use this order for the current milestone:

Phase 6C — align completed provider/data work to quiz-first Laravel contract
Phase 7 — shared app and quiz framework
Phase 8 — existing and missing application scenes, one scene at a time
Phase 8B — Quiz Portal / Assessment Room, one quiz unit at a time
Phase 12A — quiz-first milestone validation

Do not start Phase 9, Phase 10, or Phase 11 gameplay mission work.
