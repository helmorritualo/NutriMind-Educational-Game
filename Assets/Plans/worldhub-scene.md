# Project Overview
- **Game Title**: NutriMind Educational Game
- **High-Level Concept**: A child-safe, interactive educational game that teaches nutrition, mindfulness, and cognitive growth, currently running on a quiz-first milestone where the main interface launches the Quiz Portal / Assessment Room.
- **Players**: Single player.
- **Inspiration / Reference Games**: Mobile Legends / Honor of Kings layout style (for the Main Menu), and structured multi-subject selection dashboards (for the Worldhub / Subject Selection).
- **Tone / Art Direction**: Vibrant, clean, child-friendly, and highly readable. Uses a soft visual language with distinct, un-cluttered buttons and panels.
- **Target Platform**: StandaloneWindows64 (PC) and Android.
- **Screen Orientation / Resolution**: Landscape 1920x1080.
- **Render Pipeline**: Universal Render Pipeline (URP).

# Game Mechanics
## Core Gameplay Loop
The player logs in, selects their primary activity from the Main Menu, navigates to the Subject Selection screen (Worldhub), chooses an active subject (LiteraQuest or PE & Health Quest), filters by terms, and takes structured quizzes.

## Controls and Input Methods
Standard mouse click or touch tap gestures. UI layouts feature comfortable touch targets (minimum 44x44 pixels) and clear highlighted states to accommodate Android landscape and PC gameplay seamlessly.

# UI
The **Subject Selection / Worldhub** scene is built using a clean Canvas/uGUI structure.
1. **Removing Unnecessary Settings Button**: As requested, the duplicate `settings` button in this scene is completely unnecessary and will be removed to declutter the layout and ensure settings are accessed solely via the Main Interface.
2. **Back Button**: The `back` image is converted into an interactable button that fades the Canvas and returns the player to the Main Menu.
3. **Subject Cards (`lit`, `he`, `sci`)**:
   - `lit` (LiteraQuest) and `he` (HealthQuest) are converted into interactable buttons that save the selected subject in both `SubjectTermStore` and `SessionScope`, transition the state machine to `SelectingTerm`, and navigate to the Term Selection scene.
   - `sci` (ScienceQuest) represents a deferred exploration/adventure subject. In this milestone, it is disabled (interactability turned off) and visually dimmed (or padlocked) to clearly mark it as unavailable, conforming to the milestone's quiz-first scope.
4. **Performance & Batching Optimization**:
   - GraphicRaycaster optimization: `Raycast Target` will be disabled on static images and labels (such as `bg`).
   - Symmetrical fade transitions using `CanvasGroup`.
   - Automatic Safe Area adaptation via a runtime wrapper panel.

# Key Asset & Context
### New Scripts
1. **`Assets/_Project/Nutrimind/Runtime/App/WorldhubController.cs`**:
   - Thin MonoBehaviour coordinating the Subject Selection Canvas elements.
   - Binds the click handlers of `lit`, `he`, `sci`, and `back` buttons.
   - Restricts/dimms the deferred `sci` button in this milestone.
   - Transitions the state machine cleanly and manages CancellationTokenSource for async processes.
   - Automates Safe Area adjustments at runtime via a dynamically generated panel.

### Modified Assets
1. **`Assets/_Project/Nutrimind/Runtime/App/AppStateMachine.cs`**:
   - Add back-navigation transitions in `s_allowed` HashSet:
     - `(AppState.SelectingSubject, AppState.MainMenu)` to support returning to Main Menu from Worldhub.
     - `(AppState.SelectingTerm, AppState.SelectingSubject)` to support returning to Worldhub from Term Selection.
2. **`Assets/_Project/Nutrimind/Runtime/App/AppBootstrap.cs`**:
   - Register a placeholder key `"TermSelection"` in `RegisterScenes()` pointing to `Assets/_Project/Nutrimind/Scenes/App/Literaquest Term/Terms.unity` so navigation can resolve without errors during transitions.
3. **`Assets/_Project/Nutrimind/Scenes/App/Worldhub.unity`**:
   - Remove/destroy the unnecessary `settings` GameObject.
   - Add a `CanvasGroup` component to the root `Canvas` if not present.
   - Restructure Canvas elements: Add `Button` components to `back`, `lit`, `he`, and `sci`.
   - Attach `WorldhubController` to a new or existing root controller GameObject, and bind all serialized fields in the Inspector.

# Implementation Steps

### Step 1: Update AppStateMachine allowed transitions
- **Description**: Open `AppStateMachine.cs` and insert back-transition mappings in the `s_allowed` HashSet to support returning to Main Menu and returning to Subject Selection.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Register the TermSelection scene key in AppBootstrap
- **Description**: Open `AppBootstrap.cs` and add a registration for `"TermSelection"` in `RegisterScenes()` pointing to `"Assets/_Project/Nutrimind/Scenes/App/Literaquest Term/Terms.unity"`.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 3: Implement WorldhubController script
- **Description**: Create the `WorldhubController.cs` script. Define fields for `_backButton`, `_litButton`, `_heButton`, `_sciButton`, `_mainCanvasGroup`, and `_graphicRaycaster`. Implement safe-area handling, state transitions, caching, and clean asynchronous screen fades.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 4: Scene Setup and Component Wiring
- **Description**: Open the `Worldhub.unity` scene. Remove the redundant `settings` GameObject. Add `Button` components to `back`, `lit`, `he`, and `sci` GameObjects. Attach `WorldhubController` to a root controller object, bind all serialized fields, and save the scene.
- **Assigned role**: developer
- **Dependencies**: Step 2, Step 3
- **Parallelizable**: No

### Step 5: Verification and Testing
- **Description**: Play the scene in the Editor. Verify the fade transition, back navigation to Main Menu, and subject selection mechanics. Check safe-area scaling across aspect ratios. Run the EditMode unit tests using the Test Runner.
- **Assigned role**: developer
- **Dependencies**: Step 4
- **Parallelizable**: No

# Verification & Testing
1. **Layout & Cleanliness**: Verify the redundant settings button is completely removed. Ensure other buttons are anchored correctly and readable on multiple landscape aspects.
2. **Back Navigation**: Click the Back button -> Canvas fades out smoothly, StateMachine transitions to `AppState.MainMenu`, and loads the `MainMenu` scene.
3. **Subject Selection Mechanics**:
   - Click `lit` -> selected subject saved in session scope and store, state transitions to `SelectingTerm`, and transitions to the `TermSelection` scene.
   - Click `he` -> selected subject saved, state transitions to `SelectingTerm`, and transitions to the `TermSelection` scene.
   - Click `sci` -> nothing happens (the button is disabled/locked to preserve milestone scope).
4. **Console Integrity**: Confirm zero exceptions or broken references.
