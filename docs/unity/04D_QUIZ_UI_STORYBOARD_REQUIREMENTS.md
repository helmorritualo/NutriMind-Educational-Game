# Quiz UI Storyboard Requirements — v8 Canvas-Aligned

## Official reference

Use this image as the Phase 8B visual reference:

```text
design-references/quiz-ui-storyboard-reference.png
```

The storyboard defines the visual direction and required state coverage for the current quiz-first milestone.

## Canvas interpretation

The storyboard and generated assets should be implemented using Canvas/uGUI.

Do not implement these screens as UI Toolkit UXML/USS.

Build screens from:

- Canvas prefabs;
- RectTransform layout;
- Image sprites;
- sliced sprites;
- TextMeshPro labels;
- Button components;
- Toggle groups;
- ScrollRect;
- reusable prefab variants.

## Screens represented

1. Quiz Portal Home
2. Available Quiz List
3. Empty Quiz State
4. Locked Quiz State
5. Quiz Instructions
6. Multiple Choice Single
7. Multiple Choice Multiple
8. True/False Presenter
9. Submit Confirmation
10. Quiz Result Screen
11. Quiz Error/Retry and Unsupported Item states

## Important interpretation

The storyboard label `Locked Quiz State` represents an Available Quiz List detail state/modal, not a required separate Unity scene.

The `View` action in the Available Quiz List should open an in-screen Canvas modal/panel.

## Visual style

Preserve:

- warm parchment panels;
- gold ornamental borders;
- navy blue title plaques;
- green positive buttons;
- red destructive/cancel buttons;
- blue primary/navigation buttons;
- friendly fantasy-academic theme;
- child-readable typography;
- large tap targets;
- clean panel separation from scenic backgrounds.

## Implementation rule

Do not use the storyboard as a single flattened UI image.

Create structured Canvas UI with:

- sprite assets for chrome and decoration;
- dynamic TextMeshPro labels for all meaningful text;
- prefabs for repeated UI rows/buttons/modals;
- runtime bindings for provider data.

## Global portal rule

Quiz Portal Home is global by default. It must not be English-only or LiteraQuest-only.

Subject-specific branding may appear only when the player explicitly filters by subject or views a subject-scoped quiz.

## List rule

Available Quiz List shows all assigned quizzes by default.

Primary filters:

- All
- LiteraQuest
- PE/Health
- Science

Term is row/detail metadata, not a primary filter chip.

The list owns detail modals for locked, unavailable, completed, and compatibility-blocked quiz rows.

## Current presenter scope

Build only:

- Multiple Choice Single
- Multiple Choice Multiple
- True/False
- Unsupported Item State

Other presenters are deferred until approved.
