# AGENTS.md — NutriMind Unity Repository

## Purpose

This file is for Cursor, OpenCode, Unity AI Assistant, and other AI coding agents working inside the NutriMind Unity project repository.

NutriMind is a student learning game. The current milestone is quiz-first. Unity must finish the application flow and Quiz Portal / Assessment Room before any open-world gameplay missions are implemented.

## Required Reading

Before making changes, read these files when they exist:

`README.md`
`docs/UNITY_REQUIREMENTS.md`
`docs/unity/01_FOUNDATION_AND_DELIVERY_ORDER.md`
`docs/unity/02_GAME_FLOW_AND_STATE_MODEL.md`
`docs/unity/03_SHARED_CLIENT_SYSTEMS.md`
`docs/unity/04_SERVER_CONNECTION_AND_UNITY_API.md`
`docs/unity/04B_QUIZ_PORTAL_AND_ASSESSMENT_SYSTEM.md`
`docs/unity/04C_REWARDS_SHOP_DEFERRED.md`
`docs/unity/05_APPLICATION_SCENES_AND_HYBRID_UI.md`
`docs/unity/09_UI_CONTROLS_ACCESSIBILITY_AND_PRESENTATION.md`
`docs/unity/10_TESTING_INTEGRATION_AND_RELEASE.md`
`docs/unity/11_DEMO_DATA_AND_LOCAL_PROVIDER.md`
`agent-prompts/unity/UNITY_PHASE_8B_QUIZ_PORTAL_SCOPE_PROMPT.md`

If a fresh session starts, locate or ask for the latest Phase 6C, Phase 7, and Phase 8 completion reports before modifying Phase 8B.

## Current Project State

Unity Phases 1 to 6 were already completed under older documentation. Do not restart the Unity project from scratch.

Phase 6C aligns completed provider/data work to the quiz-first Laravel contract.

Phase 7 builds shared app and quiz framework.

Phase 8 completes existing and missing application scenes.

Phase 8B builds the Quiz Portal / Assessment Room.

## Current Phase 8B Scope

For the current Phase 8B implementation, build only these quiz units:

Quiz Portal Home
Available Quiz List
Empty Quiz State
Locked Quiz State
Quiz Instructions
Quiz Session Shell
Multiple Choice Single Presenter
Multiple Choice Multiple Presenter
True/False Presenter
Unsupported Item State
Submit Confirmation
Quiz Result Screen
Quiz Error/Retry State

Do not implement Matching, Ordering, Fill Blank, Short Answer, Categorization, Drag/Drop, Image Hotspot, Labeling, Reading Passage, Cloze, Numeric, or Reflection presenters unless the project owner explicitly adds them to the current phase.

## UI Direction

Use UI Toolkit for the Phase 8B quiz UI.

Use existing UI asset components, references, sprites, icons, panels, fonts, USS variables, UXML templates, and project-owned variants before creating new assets.

Use UXML for structure, USS for styling, and C# presenters/controllers for behavior.

The rest of the project can remain hybrid Canvas + UI Toolkit. Do not rebuild valid application scenes just because Phase 8B uses UI Toolkit.

## Backend Contract

The server is Laravel + React/Inertia + PostgreSQL.

Unity communicates with the server through HTTPS REST JSON APIs.

No WebSocket is required for the current milestone.

Use contract version `quiz_first_laravel_1`.

## Deferred Features

Do not implement:

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
90-mission Grade 5 and Grade 6 catalog

If older code exists for deferred systems, do not expand it. Disable, isolate, or mark it deferred.

## Architecture Rules

Keep MonoBehaviours thin.

Do not put quiz business logic inside UI components.

Use shared stores, services, presenters, DTOs, and provider interfaces.

Do not hardcode quiz content in UI scripts.

Do not hardcode mission content.

Do not add unnecessary global singletons or service locators.

Preserve assembly boundaries. Runtime assemblies must not reference Editor assemblies.

## Validation Rule

Run relevant Unity checks before reporting completion. Do not claim tests passed unless they actually ran.

Every report must include files changed, assets changed, UI assets reused, tests/checks run, Console errors/warnings, remaining gaps, and the next recommended step.
