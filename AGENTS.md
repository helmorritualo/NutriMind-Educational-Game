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
