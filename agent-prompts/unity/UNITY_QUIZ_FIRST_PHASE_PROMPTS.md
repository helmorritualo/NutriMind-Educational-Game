# Unity Quiz-First Phase Prompts — Hybrid Canvas + UI Toolkit

## Phase 6C — Provider and Contract Update

Read the updated docs. Update LocalDemoJson/HTTP DTOs to `quiz_first_laravel_1`. Simulate only account, profile, settings, subjects, terms, quizzes, attempts, results, sync. Do not simulate mission progress or rewards shop. Run tests and stop.

## Phase 7 — Shared App and Quiz Framework

Implement UI-agnostic stores/services/presenters for quiz flow. Support Canvas and UI Toolkit adapters. Do not create mission systems. Run tests and stop.

## Phase 8 — Existing and Missing Application Scenes

Inspect existing application-scene UI designs first. Reuse and complete existing scenes. Create only missing application scenes. Use Canvas if the existing design is Canvas-based; use UI Toolkit where it fits. Do not rebuild finished UI from scratch. Stop after each scene.

Use `<CURRENT_APP_SCENE>`:
- Bootstrap/Application Root
- Splash
- Login
- Main Interface
- Profile
- Settings
- Subject Selection
- Term Selection or Quiz Filter
- Loading/Transition

## Phase 8B — Quiz Portal / Assessment Room

Design and implement one `<CURRENT_QUIZ_UNIT>` at a time. Choose Canvas, UI Toolkit, or hybrid based on existing project UI. Use shared quiz presenters/services. Do not implement gameplay missions.

Use `<CURRENT_QUIZ_UNIT>`:
- Quiz Portal Home
- Available Quiz List
- Empty Quiz State
- Locked Quiz State
- Quiz Instructions
- Quiz Session Shell
- Multiple Choice Single Presenter
- Multiple Choice Multiple Presenter
- True/False Presenter
- Matching Presenter
- Ordering Presenter
- Fill Blank Presenter
- Short Answer Presenter
- Unsupported Item State
- Submit Confirmation
- Quiz Result Screen
- Quiz Error/Retry State

## Phase 12A — Quiz-First Milestone Validation

Validate app scenes, hybrid UI, quiz flow, LocalDemoJson, HTTP readiness, Android landscape, input, safe area, and no accidental mission/shop implementation.
