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
The player boots the application, views the introductory Splash video clip, is automatically authenticated or enters credentials on the Login screen, selects their subject and term, takes nutrition/mindfulness quizzes in the Quiz Portal, receives structured results, and manages settings.

## Controls and Input Methods
Standard mouse/touch screen click inputs. The Splash scene supports tap/click anywhere to skip the video playback and instantly load the next screen.

# UI
The Splash scene uses **Canvas/uGUI** for the video display and a clean, responsive layout.
- **RawImage Component**: Configured with a dynamic aspect-ratio fitting strategy to scale gracefully across high-aspect-ratio mobile devices without visual distortion.
- **Error Overlay Panel**: A child-safe, friendly notification dialog rendered using TextMeshPro if the server connection fails, containing a clearly labeled "Retry" button.
- **Skip Instruction Text**: A subtle "Tap to skip" prompt that fades in or displays clearly to guide young learners.

# Key Asset & Context
The following files are the primary focus of this unit:
- **`Assets/_Project/Nutrimind/Scenes/App/SplashScreen.unity`**: The existing application splash scene.
- **`Assets/_Project/Nutrimind/Runtime/App/SplashController.cs`**: A new controller MonoBehaviour that manages the `VideoPlayer` lifecycle, handles click/tap skip events, initiates the asynchronous configuration check (`IGameDataProvider.GetConfigAsync`), and handles transition states (`CheckingServer`, `LoggedOut`, `MaintenanceBlocked`, `UpdateRequired`).
- **`Assets/_Project/Nutrimind/Tests/EditMode/App/SharedClient/SplashControllerTests.cs`**: A new unit test suite validating Splash state transitions, skip logic, and error presentation.

# Implementation Steps

### Step 1: Design and Implement `SplashController.cs`
- **Description**: Create a thin controller MonoBehaviour `SplashController.cs` under `Assets/_Project/Nutrimind/Runtime/App/`. It should:
  - Reference the `VideoPlayer` and the rendering Canvas UI elements.
  - On `Start`, trigger the video playback and concurrently start a non-blocking `GetConfigAsync()` server configuration task on the active data provider.
  - Monitor video playback progress and bind to `VideoPlayer.loopPointReached` to trigger the exit transition.
  - Detect mouse clicks or touch taps on the screen (using Legacy or New Input System) and treat them as skip triggers.
  - Coordinate the results of the server check:
    - On success (`ApiConfigDto` is healthy and not in maintenance mode): Transition the state machine to `AppState.LoggedOut`.
    - On Maintenance Mode: Transition state machine to `AppState.MaintenanceBlocked` and display a clean "Maintenance Mode" error message on the Canvas.
    - On Unsupported Client Version: Transition to `AppState.UpdateRequired` and display an "Update Required" notification.
    - On Network/Connection Failure: Display a friendly "Connection Failed" prompt with a "Retry" button that re-fires the config check.
  - If the config check is successful and the video finishes (or is skipped), navigate to the `Login` scene using `AppNavigation.LoadScene("Login")`.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Set Up and Align the Splash Scene UI
- **Description**: Open `SplashScreen.unity`. Align and configure the UI:
  - Add the `SplashController` script to the `splash` GameObject.
  - Bind references to the `VideoPlayer`, `RawImage`, and add a `CanvasScaler` configured for `Scale With Screen Size` (Reference Resolution: 1920x1080, Match: 0.5) to ensure responsive layout on diverse Android landscape ratios.
  - Add an AspectRatioFitter to the `RawImage` or implement simple scale adjustments in `SplashController` to prevent video stretching.
  - Design a simple Canvas overlay containing an error text label (using TextMeshProUGUI) and a child-safe "Retry" button. Ensure both elements have high contrast, clear labeling, and large touch targets (minimum 44x44 dp equivalent).
  - Hide the error overlay by default.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Implement Unit Tests in `SplashControllerTests.cs`
- **Description**: Create a robust test suite `Assets/_Project/Nutrimind/Tests/EditMode/App/SharedClient/SplashControllerTests.cs` to verify:
  - Correct state transitions in the state machine during the Splash lifecycle.
  - Mock provider behavior for successful configs, maintenance mode blocks, and version mismatches.
  - That skipping or completing the video with a healthy config successfully triggers transition to `LoggedOut` and scene loading.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 4: Run Tests, Build, and Verification
- **Description**: Compile the code, resolve any warnings, execute the unit tests inside Unity Test Runner, and run scene diagnostics (EventSystem checks, UI scaling checks, Android landscape scaling verification).
- **Assigned role**: developer
- **Dependencies**: Step 2, Step 3
- **Parallelizable**: No

# Verification & Testing
1. **Compilation Check**: Verify zero compiler warnings or errors.
2. **State Transition Tests**: Verify via `SplashControllerTests` that the state machine behaves exactly as expected in all API conditions.
3. **Responsive Android Landscape Review**: Check the UI scaling in the Unity Game View across multiple landscape resolutions (e.g. 1920x1080, 2160x1080, 2400x1080) and confirm the video area scales correctly without cropping critical content or stretching.
4. **Interactive Skip Check**: Play the scene in Editor Playmode. Tap/click anywhere while the video is playing and verify immediate transition to the `Login` scene (or a log stating transition is called if the Login scene is empty).
5. **Simulated Server Error Validation**: Force the fake local provider (or use a mock) to return maintenance mode or network failure. Play the scene and verify that the video pauses or completes and the friendly error overlay is displayed with a functional "Retry" button.
