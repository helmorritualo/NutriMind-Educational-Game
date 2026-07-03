# Unity Quiz-First Phase Prompts — v8 Canvas-Aligned

## Phase 8B — Single Quiz Portal Unit

Use one current unit per session.

Approved current units:

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

Use `design-references/quiz-ui-storyboard-reference.png` as the visual reference.

Use Canvas/uGUI for Phase 8B quiz UI. Use existing generated sprite assets first. Use Canvas, Canvas Scaler, RectTransform, Image, sliced sprites, Button, Toggle/custom selection, ScrollRect, TextMeshPro, and prefabs. Do not use UI Toolkit for quiz screens unless the project owner explicitly changes this decision.

The Quiz Portal is global by default. Do not require Subject Selection or Term Selection before the Quiz Portal. Subject is the visible filter in Available Quiz List. Term is row/detail metadata, not a primary filter chip.

The Available Quiz List `View` action must open an in-screen Canvas modal/panel, not a new scene. The modal must preserve list filters, scroll position, selected row, and list data. Use this modal for locked, unavailable, completed summary, and compatibility-blocked quiz details.

Do not implement unapproved presenters, gameplay missions, rewards shop, inventory, or world restoration.

## Phase 12A — Quiz-First Milestone Validation

Validate the storyboard-aligned quiz-first milestone only.

Confirm that implemented Unity quiz support includes:

- global Quiz Portal
- Canvas/uGUI quiz screens
- Available Quiz List with subject filters
- Available Quiz List View modal/panel
- Empty Quiz State
- Locked Quiz State as modal/panel
- Quiz Instructions
- Quiz Session Shell
- Multiple Choice Single
- Multiple Choice Multiple
- True/False
- Unsupported Item State
- Submit Confirmation
- Quiz Result Screen
- Quiz Error/Retry State

Confirm mission gameplay, mission tracking, rewards shop, inventory, and world restoration are still deferred.
