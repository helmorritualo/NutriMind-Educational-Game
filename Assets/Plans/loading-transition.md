# Project Overview
- **Game Title**: NutriMind Educational Game
- **High-Level Concept**: A child-friendly educational game focusing on nutrition, mindfulness, and cognitive growth, currently executing a quiz-first alignment.
- **Players**: Single player (students).
- **Inspiration / Reference Games**: Modern gamified learning platforms (e.g., Duolingo, Kahoot) with structured assessments and visual rewards.
- **Tone / Art Direction**: Soft, vibrant, child-safe, and clear. Avoids noisy backgrounds and uses friendly character guidance.
- **Target Platform**: StandaloneWindows64 (PC) and Android.
- **Screen Orientation / Resolution**: Landscape 1920x1080.
- **Render Pipeline**: Universal Render Pipeline (URP).

# Game Mechanics
## Core Gameplay Loop
The player logs in, selects their subject and term (or applies filters), selects a quiz, reads instructions, takes the assessment through individual question panels (with safe unsupported item fallback), submits, and receives feedback and display-only results (score, pass/fail, non-economic badges).

## Controls and Input Methods
Standard mouse click and touch inputs. Large, highly responsive UI Toolkit elements with tap-to-select support to ensure robust performance on both Standalone Windows and mobile Android environments.

# UI
The system is built on a hybrid UI architecture (Canvas/uGUI + UI Toolkit). The **Loading/Transition** scene will utilize **UI Toolkit** for its interface. It will reuse the existing, pre-approved `LoadingOverlay.uxml` template to display a beautiful spinning wheel, a centered status label, and support fully responsive layouts (including safe-area padding and Android landscape auto-scaling).

# Key Asset & Context
The following files are the primary focus of this Loading/Transition Scene implementation:
- **`Assets/_Project/Nutrimind/Scenes/App/LoadingTransition.unity`**: The newly created scene representing the Loading/Transition state.
- **`Assets/_Project/Nutrimind/Runtime/App/LoadingTransitionController.cs`**: The main C# view controller for managing the scene. It triggers on start, initializes composition root context, performs safe asynchronous preloading of the quiz catalog via the active data provider, manages a minimum visual display timer (1.5 seconds) to prevent visual flickering, and coordinates the state transition to `AppState.InWorld` (loading `"QuizPortal"`) or fallback scenes.
- **`Assets/_Project/Nutrimind/UI/Documents/Screens/LoadingTransition.uxml`**: The visual container for the loading transition screen, wrapping and nesting the reusable `LoadingOverlay.uxml` component under a beautifully themed background element (`nm-app-background`).
- **`Assets/_Project/Nutrimind/Runtime/App/AppBootstrap.cs`**: Updated to register the new `"Loading"` key and its path in the global `SceneRegistry`.
- **`Assets/_Project/Nutrimind/Runtime/App/TermSelectionController.cs`**: Updated to route to the `"Loading"` scene key when `AppState.LoadingWorld` is triggered, replacing the direct bypass fallback.
- **`Assets/_Project/Nutrimind/Tests/EditMode/App/SharedClient/LoadingTransitionControllerTests.cs`**: A new comprehensive EditMode unit/integration test suite to verify loading state mutations, safe-area application, minimum-display-time enforcement, and correct navigation routing.

# Implementation Steps

### Step 1: Create the Loading Screen UXML Visual Tree
- **Description**: Create the UXML file `Assets/_Project/Nutrimind/UI/Documents/Screens/LoadingTransition.uxml` using UI Toolkit. It should define a full-screen container with class `nm-app-background` and include/nest the existing `Assets/_Project/Nutrimind/UI/Documents/Overlays/LoadingOverlay.uxml` template in its center. It should also have an outer padding element to which safe-area styles (`.nm-safe-area-left`, etc.) can be bound.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Implement `LoadingTransitionController`
- **Description**: Create the C# script `Assets/_Project/Nutrimind/Runtime/App/LoadingTransitionController.cs` under the main assembly namespace `NutriMind.Runtime.App`. The controller should:
  - Reference a `UIDocument` to retrieve the visual tree.
  - Dynamically query `CompositionRoot.Instance`.
  - Inside `Start()`, initiate an asynchronous data preloading routine (using safe cancelable Task/Coroutine patterns and a standard `CancellationToken`).
  - Request the quiz catalog via `CompositionRoot.Instance.DataProvider.GetQuizzesAsync(ct)`.
  - Maintain a minimum visible display time of `1.5` seconds to prevent ugly screen flicker for ultra-fast connections.
  - Apply the standard UI Toolkit safe-area padding using `NutriMindSafeAreaUtility`.
  - Transition the `AppStateMachine` to `AppState.InWorld` on success.
  - Transition to `"QuizPortal"` via `AppNavigation.LoadScene("QuizPortal")`. If `"QuizPortal"` is not registered yet (Phase 8B), fall back to `"MainMenu"`.
  - Handle connection failures or exceptions by displaying a child-friendly error message within the loading screen, supporting manual retry or return to the main menu.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Register the Loading Scene Key in AppBootstrap
- **Description**: Modify `AppBootstrap.cs` to register the `"Loading"` key pointing to `Assets/_Project/Nutrimind/Scenes/App/LoadingTransition.unity`.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

### Step 4: Update Existing Scenes and Controller Transitions
- **Description**: Modify `TermSelectionController.cs` (lines 280-292) to navigate to the `"Loading"` scene key when transitioning to `AppState.LoadingWorld` instead of executing a direct scene load bypass.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

### Step 5: Update AppBootstrap Tests
- **Description**: Update `AppBootstrapTests.cs` to expect exactly 10 registered scene keys, and assert that the `"Loading"` key is successfully registered with its correct asset path.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

### Step 6: Create the Loading Scene file in Unity
- **Description**: Create the scene file `Assets/_Project/Nutrimind/Scenes/App/LoadingTransition.unity`. Add a UIDocument component, set its PanelSettings to `RuntimePanelSettings.asset`, bind its Source Asset to the new `LoadingTransition.uxml` file, and attach the `LoadingTransitionController` component. Register the scene in Unity's Build Settings at the appropriate build index.
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 2, Step 3
- **Parallelizable**: No

### Step 7: Create and Run EditMode Unit Tests
- **Description**: Write `LoadingTransitionControllerTests.cs` to verify:
  - Success path: preloading data successfully triggers state transition to `AppState.InWorld` and navigates to the destination scene.
  - Failure path: connection failure correctly handles exceptions, presents a child-friendly visual error, and lets users return to the main menu.
  - Minimum display time validation.
  - Safe-area calculation bounds.
  Run all project EditMode unit tests using the Test Runner API.
- **Assigned role**: developer
- **Dependencies**: Step 2, Step 5
- **Parallelizable**: No

# Verification & Testing
1. **Compilation Check**: Verify zero compilation errors across runtime and test assemblies.
2. **Automated Unit Tests**: Execute `AppBootstrapTests` and the new `LoadingTransitionControllerTests` to verify correct registry state and routing logic.
3. **Interactive Playmode Check**: Run Playmode from the `Bootstrap` scene:
   - Perform a fake login.
   - Select a Subject (e.g. Literacy) -> transitions to Term Selection.
   - Click "Term 1" -> Canvas fades out smoothly, StateMachine transitions, the `LoadingTransition` scene is loaded.
   - Verify that the spinning loader is visible for at least 1.5 seconds.
   - Verify that it fetches the mock quizzes from the active provider.
   - Verify that it transitions back to `MainMenu` seamlessly if the Quiz Portal is not registered, or `QuizPortal` if registered.
   - Verify safe area and Android landscape auto-scaling in Game View.
