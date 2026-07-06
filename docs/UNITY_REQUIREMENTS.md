# NutriMind Unity Requirements — v10 LiteraQuest Mission 1 Planning

## Current temporary Unity goal

Quiz Portal / Assessment Room implementation is temporarily on hold.

The current planning focus is the first playable LiteraQuest Term 1 gameplay vertical slice:

```text
Grade 5 Mission 1: Festival Storybook Rescue
Grade 6 Mission 1: Echoes of the Lantern Village
```

This is a Mission 1 plan only. Use shared mission logic with grade-specific content data. The project owner designs and places the scene environment, objects, and dynamic Canvas UI. The AI coding agent implements reusable logic, UI binding to owner-designed panels, interactions, Knowledge Locks, collectibles, gates, final restoration, and tests.

Do not implement the full mission catalog, Mission 2+, Health/PE missions, Science missions, server mission tracking, rewards shop, spendable coins, EXP economy, economy inventory, pets, cosmetics, equipment, or full world restoration.

## Preserved v8 quiz documentation

The previous Canvas Quiz Portal requirements remain preserved below for later continuation, but they are not the active implementation task while the Mission 1 gameplay plan is active.

---


## Current Unity goal

Finish Unity only through the Quiz Portal / Assessment Room.

Do not start open-world mission gameplay, mission progress tracking, rewards shop, inventory, pets, cosmetics, world restoration, or mission rewards.

## Official visual reference

The quiz UI storyboard is the official Phase 8B visual reference:

```text
design-references/quiz-ui-storyboard-reference.png
```

Use the storyboard as a reference for layout, spacing, visual style, state coverage, and UI hierarchy.

Do not implement the storyboard as a static screenshot. All meaningful text and data must be dynamic UI elements.

## UI system decision

The overall Unity project may remain hybrid Canvas + UI Toolkit where older application scenes already exist.

For Phase 8B Quiz System, use Canvas/uGUI.

Do not use UI Toolkit for Quiz Portal screens, quiz list screens, quiz session screens, quiz item presenters, submit confirmation, quiz result, locked modal, error modal, or unsupported item modal unless the project owner explicitly changes this decision.

## Canvas implementation requirements

Use:

- Canvas root;
- Canvas Scaler for Android landscape scaling;
- Graphic Raycaster;
- EventSystem;
- RectTransform anchors and pivots;
- Image components for sprite assets;
- sliced sprites / 9-slice for scalable frames and buttons;
- TextMeshPro for dynamic labels;
- Button components for actions;
- Toggle or custom Button state groups for filters and answer selection;
- ScrollRect for Available Quiz List;
- prefab variants for reusable rows, buttons, modals, answer options, and state panels.

Generated assets must be imported as Sprites or Sprite Sheets and sliced where needed.

## Correct Quiz Portal flow

The quiz flow is:

```text
Main Interface
→ Quiz Portal Home
→ Available Quiz List
→ Quiz List Detail Modal when needed
→ Quiz Instructions
→ Quiz Session Shell
→ Current Item Presenter
→ Submit Confirmation
→ Quiz Result Screen
```

There is no required Subject Selection scene before the Quiz Portal.

Subject is the primary student-facing filter in Available Quiz List.

Term is still metadata, but it should not be shown as a primary filter chip in the current student UI.

## Available Quiz List primary filters

Use only these primary filter chips/buttons:

- All
- LiteraQuest
- PE/Health
- Science

Do not build Term 1, Term 2, or Term 3 as main filter chips for this milestone.

Each row may still show term metadata, such as:

```text
LiteraQuest • Term 1 • Grade 5 • 20 items
```

## Current Phase 8B screens and states

Implement these for the current milestone:

- Quiz Portal Home
- Available Quiz List
- Empty Quiz State
- Locked Quiz State as an Available Quiz List Canvas modal/panel
- Quiz Instructions
- Quiz Session Shell
- Multiple Choice Single Presenter
- Multiple Choice Multiple Presenter
- True/False Presenter
- Unsupported Item State
- Submit Confirmation
- Quiz Result Screen
- Quiz Error/Retry State

## Important modal decision

Do not create a separate scene for Locked Quiz State.

The Available Quiz List owns the following modal/panel states:

- locked quiz details
- unavailable quiz details
- completed quiz summary if result visibility allows a lightweight summary
- compatibility warning details if a quiz cannot be started because the current Unity client does not support required item types

The modal must preserve:

- selected subject filter
- list scroll position
- selected quiz context
- loaded list data
- current error/loading state where applicable

Closing the modal returns the student to the same Available Quiz List state.

## Current implemented item presenters

Current Unity implementation target:

- `multiple_choice_single`
- `multiple_choice_multiple`
- `true_false`
- `unsupported_item_state`

The following are still part of the broader quiz data model but not active Unity presenter targets until approved:

- `matching`
- `ordering`
- `fill_blank`
- `short_answer`
- `categorization`
- `drag_drop`
- `image_hotspot`
- `labeling`
- `scenario_choice`
- `reading_passage`
- `cloze`
- `numeric`
- `likert_reflection`

If an unsupported item reaches Unity, the client must show the Unsupported Item State and must not crash.

## Quiz Portal Home requirements

The default Quiz Portal Home is global, not English-only and not LiteraQuest-only.

Required actions:

- Available Quizzes
- My Quiz Results
- Back to Main Menu

The screen must not imply adventure gameplay is available.

## Available Quiz List requirements

Show all assigned quizzes by default.

Subject filters are optional inside this screen but should be visible as the main filters.

Each quiz row should show:

- quiz title
- subject
- term
- grade
- item count
- status
- action

Expected row actions:

- `Start` for available playable quizzes
- `View` for locked or unavailable quizzes
- `View Result` for completed quizzes when result viewing is available
- disabled action when no student action is allowed

## View modal requirements

When the student taps `View`, open a Canvas modal/panel inside Available Quiz List.

For a locked quiz, show:

- quiz title
- subject
- term
- grade
- item count
- status: Locked
- locked reason
- available date/time if provided
- Back or Close

For an unavailable quiz, show:

- quiz title
- subject
- term
- grade
- status
- reason
- Back or Close

For a completed quiz, either open Quiz Result Screen or show a result summary modal depending on server result visibility.

Do not expose answer keys, hidden teacher notes, or admin-only details.

## Quiz Session Shell requirements

Quiz Session Shell is a reusable Canvas layout, not a static screenshot.

It should own:

- session panel/frame;
- question number text;
- progress bar shell and fill;
- timer badge;
- question content container;
- answer presenter slot;
- Previous button;
- Next button;
- Submit entry;
- validation state display.

Item presenters own only item-specific answer UI.

## Dynamic UI rule

These must be dynamic runtime UI, not baked into image assets:

- screen titles unless the button/panel is deliberately static;
- quiz titles;
- subject labels;
- term labels;
- grade labels;
- item count;
- status;
- dynamic action text when server-driven;
- instructions;
- question text;
- answer choices;
- score;
- feedback;
- locked reason;
- unavailable reason;
- error messages.

Image assets may be used for:

- panel backgrounds;
- button frames;
- static buttons with fixed labels when the label will never change;
- decorative icons;
- banners without dynamic text;
- characters;
- trophies;
- shields;
- ornaments;
- progress bar shell/fill sprites;
- timer icons;
- radio/checkbox icons.
