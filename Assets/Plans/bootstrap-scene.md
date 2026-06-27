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

### Step 1: Verify Existing Scripts
- **Description**: Verify that `AppBootstrap.cs` and `AppNavigation.cs` are present, complete, and free of compile errors.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Recreate and Set Up the Bootstrap Scene
- **Description**: Create the missing scene `Assets/_Project/Nutrimind/Scenes/App/Bootstrap.unity`. Add:
  - A `Main Camera` configured with a solid black background.
  - A GameObject `NutriMind-AppRoot` with `AppBootstrap` attached.
  - A `UIDocument` component with Panel Settings set to `RuntimePanelSettings.asset` and Source Asset set to `AppShell.uxml`.
  - Assign the `UIDocument` to the `_uiDocument` reference on the `AppBootstrap` component.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Enable Scene in Build Settings
- **Description**: Configure the Editor Build Settings to ensure `Assets/_Project/Nutrimind/Scenes/App/Bootstrap.unity` is included and enabled at Build Index 0.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

### Step 4: Execute Unit and Integration Verification
- **Description**: Run the `AppBootstrapTests` suite to verify correct scene registration and navigation transitions, followed by an interactive Playmode check to ensure smooth startup.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

# Verification & Testing
1. **Compilation Check**: Verify that there are zero compiler warnings or errors.
2. **Registry Tests**: Run the new `AppBootstrapTests` suite to ensure correct registration of all scene keys.
3. **Flow State Tests**: Ensure all existing state-flow unit tests continue to pass.
4. **Interactive Playmode Check**: Enter Playmode from the `Bootstrap` scene in the Unity Editor and observe:
   - Initial "Initializing Composition Root..." message on status-label.
   - Successful transition to the `SplashScreen` scene within 1 second.
   - No exceptions, missing references, or errors in the Console.
