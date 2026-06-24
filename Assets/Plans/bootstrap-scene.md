# Project Overview
- **Game Title**: NutriMind Educational Game
- **High-Level Concept**: A child-friendly educational game focusing on nutrition, mindfulness, and cognitive growth, currently executing a quiz-first alignment.
- **Players**: Single player.
- **Inspiration / Reference Games**: Modern gamified learning platforms (e.g., Duolingo, Kahoot) with structured assessments and visual rewards.
- **Tone / Art Direction**: Soft, vibrant, child-safe, and clear. Avoids noisy backgrounds and uses friendly character guidance.
- **Target Platform**: StandaloneWindows64 (PC) and Android.
- **Screen Orientation / Resolution**: Landscape 1920x1080.
- **Render Pipeline**: Universal Render Pipeline (URP).

# Game Mechanics
## Core Gameplay Loop
The player boots the application, experiences a smooth transition to the Splash screen, logs in (under authenticating state), views the main menu interface, takes nutritionist/mindfulness quizzes in the Quiz Portal, receives structured results, and modifies profile/settings as desired.

## Controls and Input Methods
Standard mouse/touch screen click inputs. The Bootstrap scene requires no interactive inputs; it initializes services, registers the application scenes, and transitions immediately.

# UI
The Bootstrap scene uses **UI Toolkit** for its thin loading status overlay, embedded within the shared `AppShell.uxml` layer. The background layer matches the soft theme color, and a simplecentered status label keeps the user informed of the initialization steps.

# Key Asset & Context
The following files are the primary focus of this unit:
- **`Assets/_Project/Nutrimind/Scenes/App/Bootstrap.unity`**: The newly created entry point scene (build index 0) for the application.
- **`Assets/_Project/Nutrimind/Runtime/App/AppBootstrap.cs`**: The controller MonoBehaviour that triggers CompositionRoot warmup, registers all active application scenes into the `SceneRegistry`, and performs the initial scene transition.
- **`Assets/_Project/Nutrimind/Runtime/App/AppNavigation.cs`**: A thin, static navigation helper that leverages the `NavigationService` to perform asynchronous scene transitions uniformly across the project.
- **`Assets/_Project/Nutrimind/Tests/EditMode/App/SharedClient/AppBootstrapTests.cs`**: Unit tests verifying scene registry configuration, initial state transitions, and safe navigation behavior.

# Implementation Steps

### Step 1: Create the `AppBootstrap.cs` Script
- **Description**: Implement `AppBootstrap.cs` inside `Assets/_Project/Nutrimind/Runtime/App/` namespace. This script should initialize the `CompositionRoot`, populate the `SceneRegistry` with active application scenes, transition the `AppStateMachine` from `Starting` to `CheckingServer`, and asynchronously load the `SplashScreen` scene.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Create the `AppNavigation.cs` Script
- **Description**: Implement a static navigation utility class `AppNavigation.cs` inside `Assets/_Project/Nutrimind/Runtime/App/` or `Assets/_Project/Nutrimind/Runtime/UI/` to allow unified scene transitions across the project by resolving keys from `NavigationService`.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 3: Create the Bootstrap Scene
- **Description**: Create a new scene `Assets/_Project/Nutrimind/Scenes/App/Bootstrap.unity`. In the scene, add:
  - A standard `Main Camera` with black background.
  - A GameObject `NutriMind-AppRoot` carrying the `AppBootstrap` script.
  - A `UIDocument` component using `RuntimePanelSettings.asset` and a simple local UXML containing a loading label (`status-label`) centered on a theme-colored background.
- **Assigned role**: developer
- **Dependencies**: Step 1

### Step 4: Configure Editor Build Settings
- **Description**: Add the Application Scenes to the Unity Build Settings, ensuring `Bootstrap.unity` has index 0 (first scene), followed by `SplashScreen.unity`, `Login.unity`, `MainMenu.unity`, `Profile.unity`, `Settings.unity`, and `Worldhub.unity`.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

### Step 5: Implement unit tests in `AppBootstrapTests.cs`
- **Description**: Create a unit test suite under `Assets/_Project/Nutrimind/Tests/EditMode/App/SharedClient/` to verify that `AppBootstrap` successfully registers all application scene paths and transitions the state machine correctly.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 6: Compile and execute unit tests
- **Description**: Recompile the project and run EditMode tests using Unity Test Runner API to ensure zero compiler warnings/errors and that the new tests pass.
- **Assigned role**: developer
- **Dependencies**: Step 5
- **Parallelizable**: No

# Verification & Testing
1. **Compilation Check**: Verify that there are zero compiler warnings or errors.
2. **Registry Tests**: Run the new `AppBootstrapTests` suite to ensure correct registration of all scene keys.
3. **Flow State Tests**: Ensure all existing state-flow unit tests continue to pass.
4. **Interactive Playmode Check**: Enter Playmode from the `Bootstrap` scene in the Unity Editor and observe:
   - Initial "Initializing Composition Root..." message on status-label.
   - Successful transition to the `SplashScreen` scene within 1 second.
   - No exceptions, missing references, or errors in the Console.
