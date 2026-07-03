# UI Controls, Accessibility, and Presentation — v8 Canvas Quiz UI

## Storyboard reference

Use:

```text
design-references/quiz-ui-storyboard-reference.png
```

as the quiz UI visual reference.

## Canvas phase direction

Phase 8B quiz UI must use Canvas/uGUI.

Use:

- Canvas root;
- Canvas Scaler;
- Graphic Raycaster;
- EventSystem;
- RectTransform anchors and pivots;
- Image components;
- sliced sprites;
- TextMeshPro labels;
- Button components;
- Toggle groups or custom selectable buttons;
- ScrollRect;
- layout groups only where they do not fight fixed visual composition;
- prefab variants for repeated rows and buttons.

## Dynamic text

Meaningful text must be runtime TextMeshPro text, not baked into sprites unless the asset is a deliberately static button or static decorative sign.

This includes titles, labels, quiz content, answers, scores, feedback, and errors.

## Readability

Maintain:

- large child-readable text;
- safe contrast;
- large tap targets;
- strong selected state;
- clear disabled/locked state;
- non-color-only feedback;
- Android landscape support;
- safe-area support.

## Visual consistency

Use the storyboard's parchment/gold/navy/green/red style.

Use the scenic fantasy background only behind strong panels or with dim/blur/overlay so question text remains readable.

## Quiz-specific states

Each quiz UI screen must support relevant loading, populated, empty, locked, disabled, selected, validation, submitting, result, and error states.
