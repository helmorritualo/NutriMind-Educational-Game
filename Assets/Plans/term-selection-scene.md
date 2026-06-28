# Project Overview
- Game Title: NutriMind Educational Game
- High-Level Concept: An educational game focusing on nutritional science, health, and literacy.
- Players: Single player (students)
- Inspiration / Reference Games: Educational RPGs, quiz games
- Tone / Art Direction: Child-friendly, colorful, clear and structured UI
- Target Platform: PC (StandaloneWindows64), Android
- Screen Orientation / Resolution: Landscape (1920x1080 reference)
- Render Pipeline: URP (PC_RPAsset)

# Game Mechanics
## Core Gameplay Loop
The player selects subjects and terms, which serves as a quiz filter leading to the Quiz Portal/Assessment Room. Once there, they complete educational quizzes aligned with the Grade 5/6 curriculum. Completed quizzes update their student profile and progress.

## Controls and Input Methods
Standard mouse click and touch inputs (large, highly responsive buttons for easy child-friendly tap actions, scale feedback on hover and tap via `Hover` components).

# UI
The Term Selection and Quiz Filter scenes (LiteraQuestTerms and HealthQuestTerms) use Canvas-based UI with interactive card elements for Term 1, Term 2, and Term 3, with a "Back" button for navigation. Backgrounds are simple and non-confusing to ensure visual clarity and high readability.

# Key Asset & Context
- `TermSelectionController.cs`: A new thin MonoBehavior script responsible for:
  - Dynamic terms state binding. It queries the data provider (`CompositionRoot.Instance.DataProvider.GetTermsAsync(subjectSlug)`) asynchronously.
  - Visually updating the three Term cards (checking availability, dimming locked terms).
  - Event listeners for buttons (`Term 1`, `Term 2`, `Term 3`, `back`).
  - Dynamic safe-area padding calculation and screen adjustment using `NutriMindSafeAreaUtility`.
  - Thread-safe, cancelable async operations using CancellationToken.
  - Standard smooth fader transitions (via CanvasGroup alpha).
- `LiteraQuestTerms.unity` & `HealthQuestTerms.unity`: Existing Canvas scenes. We will attach `TermSelectionController` and wire up the respective serializable fields in the inspector.

# Implementation Steps

## Step 1: Implement TermSelectionController C# script
- **Description**: Create a unified `TermSelectionController.cs` script under `Assets/_Project/Nutrimind/Runtime/App/`. Inside `Start()`, retrieve the selected subject from `CompositionRoot.Instance.Session.SelectedSubject` (or `SubjectTermStore`). Call `GetTermsAsync` on the provider asynchronously to retrieve the term status (is_available, progress, titles). Map and bind the three term buttons to Term 1, Term 2, Term 3. If a term is locked, dim its visual layout and disable interactivity. Setup back button listener to load "Worldhub" and transition back to `SelectingSubject`. Include safe area adjustment and smooth fading.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

## Step 2: Attach and Wire Controller in Scenes
- **Description**: Open the existing scenes `LiteraQuestTerms.unity` and `HealthQuestTerms.unity` additively. Add the `TermSelectionController` component to the `TERM PREFABS` canvas root. Wire the Inspector fields (Term 1, Term 2, Term 3 button references, back button reference, and main CanvasGroup reference). Ensure each button has a `GraphicRaycaster` reachable and the EventSystem is present.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Verification & Integration Tests
- **Description**: Add unit/integration tests in `Assets/_Project/Nutrimind/Tests/EditMode/App/SharedClient/TermSelectionTests.cs` to test the state transitions and model binding logic, verifying that the selected term ID is stored correctly in `SubjectTermStore` and that the correct transition occurs in `AppStateMachine`.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

# Verification & Testing
- **Automated Tests**: Execute edit-mode tests (`TermSelectionTests`) using the test runner or custom script.
- **Manual Checks**: Play the bootstrap scene, log in, select subject, and then transition to term selection. Verify that:
  - All interactive cards reflect provider-driven availability.
  - Interactive cards trigger appropriate state transition (`SelectingTerm` -> `LoadingWorld`) and save selections.
  - Safe-area panel adjusts perfectly.
  - No errors or warnings in the Unity console.
