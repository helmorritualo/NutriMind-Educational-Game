# Testing, Integration, and Release — v8 Canvas Quiz-First

## Validate storyboard-aligned screens

Verify:

- Quiz Portal Home
- Available Quiz List
- Available Quiz List subject filters
- Available Quiz List `View` modal
- Empty Quiz State
- Locked Quiz State as list modal/panel
- Unavailable Quiz Detail modal if present
- Completed Quiz Summary or Result flow if present
- Quiz Instructions
- Quiz Session Shell
- Multiple Choice Single Presenter
- Multiple Choice Multiple Presenter
- True/False Presenter
- Unsupported Item State
- Submit Confirmation
- Quiz Result Screen
- Quiz Error/Retry State

## Required Canvas checks

- Unity compilation
- Console review
- scene reference checks
- Canvas root checks
- Canvas Scaler checks
- Graphic Raycaster checks
- EventSystem checks
- RectTransform anchors/pivots checks
- Image sprite reference checks
- TextMeshPro reference checks
- Button callback checks
- Toggle/selection state checks
- ScrollRect checks for Available Quiz List
- modal blocking input checks
- safe-area checks
- Android landscape checks
- touch target checks
- provider DTO checks
- LocalDemoJson checks
- quiz list filter checks
- list scroll preservation after modal close
- selected filter preservation after modal close
- locked modal checks
- unavailable modal checks
- empty state checks
- submit confirmation checks
- result visibility checks
- unsupported item fallback checks
- no mission progress side effects
- no reward/shop/inventory side effects

## Completion boundary

Do not claim full gameplay completion.

The milestone is complete only through Quiz Portal / Assessment Room.
