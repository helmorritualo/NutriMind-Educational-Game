# Testing, Integration, and Release — v9 LiteraQuest Mission 1

## Active Mission 1 validation

For the current gameplay task, validate only LiteraQuest Term 1 Mission 1 for Grade 5 and Grade 6.

Required checks:

- Unity compilation;
- Console review;
- scene missing reference checks;
- player spawn checks;
- player movement smoke test;
- camera follow/bounds smoke test;
- interact trigger checks;
- interaction prompt checks;
- dialogue panel checks;
- Knowledge Lock question flow checks;
- wrong-answer hint and retry checks;
- correct-answer progression checks;
- collectible locked/unlocked state checks;
- pickup checks;
- mission inventory counter checks;
- gate/path unlock checks;
- objective tracker checks;
- final restoration checks;
- mission complete panel checks;
- mission reset/local demo checks;
- Grade 5 full route smoke test;
- Grade 6 full route smoke test;
- Android landscape/touch smoke test if available.

Do not claim a test passed unless it actually ran.

## Preserved v8 quiz validation

The earlier Canvas Quiz validation checklist is preserved below for later continuation. It is not the active validation target during Mission 1 gameplay work.

---

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
