# Quiz Portal and Assessment Room

## Current Active Unity Feature

Build the Quiz Portal before gameplay missions.

## Required Screens / Panels

- Quiz Portal Home
- Available Quiz List
- Empty Quiz State
- Locked/Unavailable Quiz State
- Quiz Instructions
- Quiz Session Shell
- Item Renderer Area
- Item Navigation
- Submit Confirmation
- Quiz Result
- Unsupported Item State
- Error/Retry State

## Hybrid UI Recommendation

Use existing Canvas visual style if the app scenes are already Canvas-based.

Use UI Toolkit where it is faster for structured lists/forms.

Do not mix Canvas and UI Toolkit inside one screen without a clear reason. If mixed, document which layer owns input, navigation, and focus.

## Quiz Renderer Priority

1. multiple choice single
2. multiple choice multiple
3. true/false
4. matching
5. ordering
6. fill blank
7. short answer
8. unsupported item fallback

## Android Rule

If drag/drop is unreliable, use tap-to-select alternatives.
