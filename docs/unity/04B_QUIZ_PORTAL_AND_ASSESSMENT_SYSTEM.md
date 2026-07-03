# Quiz Portal and Assessment System — v8 Canvas-Aligned

## Purpose

The Quiz Portal / Assessment Room is the current active student learning milestone.

It is separate from future open-world mission gameplay.

## UI implementation

Use Canvas/uGUI for the Quiz Portal and all Phase 8B quiz screens.

Use generated assets as sprites, sliced sprites, sprite sheets, Image components, Buttons, TextMeshPro labels, ScrollRects, Toggle groups, RectTransforms, and prefabs.

Do not use UI Toolkit for the quiz system unless the project owner explicitly changes the requirement.

## Correct flow

```text
Main Interface
→ Quiz Portal Home
→ Available Quiz List
→ Quiz List Detail Modal when needed
→ Quiz Instructions
→ Quiz Session Shell
→ Item Presenter
→ Submit Confirmation
→ Quiz Result Screen
```

Subject Selection and Term Selection are not mandatory entry screens for the Quiz Portal.

## Global Quiz Portal

The default Quiz Portal is global.

It must not be English-only, LiteraQuest-only, or mission-world-specific.

Subject is the primary student-facing filter.

Term is metadata in rows/details and not a main filter chip for this milestone.

## Required Phase 8B units

Current approved implementation units:

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

Do not add other quiz presenters unless explicitly approved.

## Available Quiz List ownership

Available Quiz List owns these internal states:

- loading list
- populated list
- empty list
- subject filter changes
- locked row
- unavailable row
- completed row
- locked/unavailable detail modal
- optional completed result summary modal
- error/retry state

Do not create a separate scene for locked quiz details.

## Row action behavior

Use this behavior:

```text
Available playable quiz → Start
Locked quiz → View
Unavailable quiz → View
Completed quiz → View Result or View Summary
Unsupported required item type → View compatibility message
```

## View modal behavior

The `View` action opens an in-screen Canvas modal/panel.

The modal must not destroy or reload the list unless explicitly refreshing.

The modal must preserve:

- subject filter
- list scroll position
- selected quiz row
- current list data
- previous navigation target

Modal content for locked/unavailable quizzes must include safe metadata only:

- quiz title
- subject
- term
- grade
- item count
- status
- locked/unavailable reason
- available date/time if any
- compatibility warning if any
- Close/Back

Do not expose:

- answer keys
- hidden item content
- teacher private notes
- admin data
- other students' results

## Current supported Unity item types

Supported in current Unity milestone:

- `multiple_choice_single`
- `multiple_choice_multiple`
- `true_false`

Safe fallback required:

- unsupported item state

Deferred Unity presenters:

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

## Result visibility

The server/provider controls whether Review Answers appears.

Review Answers must not be shown when result visibility forbids review.

Completed quiz rows may navigate to Quiz Result Screen or open a summary modal depending on visibility and implementation scope.

## Dynamic data

All quiz content is provider/server-driven.

Do not hardcode quiz content into Canvas prefabs or MonoBehaviours except fixed static button labels that will never change.

## Rewards boundary

Result celebration is display-only. Do not grant coins, EXP, inventory, cosmetics, pets, titles, equipment, purchases, or shop currency.
