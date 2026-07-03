# Unity Phase 12A Prompt — Quiz-First Milestone Validation for Cursor, Canvas/uGUI Version

You are working in Cursor on the NutriMind Unity project. This is a new AI coding session. Before changing files, read `AGENTS.md`, all files in `.cursor/rules/`, `UNITY_AI_ASSISTANT_CUSTOM_INSTRUCTIONS.txt` if present, `README.md`, all Unity requirement docs, and all Phase 6C, Phase 7, Phase 8, and Phase 8B completion reports.

Perform final validation only for the quiz-first Unity milestone. Do not start or claim completion of open-world mission gameplay. Do not implement new gameplay missions, mission tracking, rewards shop, inventory, pets, cosmetics, world restoration, spendable coins, EXP, or the future 90-mission catalog.

Verify startup, splash, login, logout, profile, settings, main interface, Quiz Portal entry, Quiz Portal Home, Available Quiz List, subject-only visible filters, no Term 1–3 primary filter chips, Empty Quiz State, Locked Quiz State as Canvas modal/panel, Quiz Instructions, Quiz Session Shell, Multiple Choice Single Presenter, Multiple Choice Multiple Presenter, True/False Presenter, Unsupported Item State, Submit Confirmation, Quiz Result Screen, Quiz Error/Retry State, answer drafting, submit confirmation behavior, quiz attempt submission, result visibility behavior, safe errors, LocalDemoJson reset, HTTP DTO readiness, production rejection of LocalDemoJson, Canvas/uGUI input safety, existing hybrid app compatibility, Android landscape, safe area, touch input, missing references, Canvas Scaler, Graphic Raycaster, EventSystem, RectTransform layout, Image sprite references, TextMeshPro labels, Button callbacks, modal blocking behavior, and Console errors.

Confirm that the old station-as-quiz flow is not active. Confirm that quiz completion does not update mission progress. Confirm that quiz result display does not grant spendable coins, EXP, inventory items, cosmetics, pets, titles, equipment, purchases, or shop currency. Confirm that mission gameplay, reward shop, inventory, and world restoration are still deferred.

Run all available Edit Mode tests, Play Mode tests, JSON validation, provider parity tests, LocalDemoJson tests, quiz attempt idempotency checks where available, UI checks, scene reference checks, Canvas checks, Android build or device smoke checks if available, and full Console review. Do not claim any test or check passed unless it actually ran.

Fix only clear defects that are inside the quiz-first milestone. Do not expand scope. Do not add unapproved quiz item presenters. Do not start future gameplay systems. If a defect requires a large design decision, document it as a remaining gap instead of inventing a new scope.

Report exact checks run, exact results, files changed, assets changed, tests passing or failing, Console warnings or errors, defects fixed, remaining gaps, deferred features confirmed, and whether the Unity side is complete through Quiz Portal only. Stop after reporting.
