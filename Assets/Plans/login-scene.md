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
The player boots the application, views the Splash video clip, is presented with the Login screen where they authenticate using their unique Learner Reference Number (LRN) and PIN (or bypass using a development-only Demo Login button), bootstraps their profile and game configuration, is directed to the Main Interface, and participates in gamified nutrition and mindfulness assessments in the Quiz Portal.

## Controls and Input Methods
Menu interactions via standard mouse/pointer click and touch screen taps. Text input fields (LRN and PIN) support physical keyboard entry and native Android virtual/on-screen keyboards. Both New Input System and Legacy Input Manager are supported.

# UI
The Login screen uses **Canvas/uGUI** for high compatibility, precise pixel positioning of input fields, and native platform soft-keyboard integration.
- **Board Panel**: A child-safe, friendly schoolboard container centering the form fields and aligning visual focus.
- **LRN and PIN Input Fields**: Responsive, stylized TextMeshPro Input Fields with clear, high-contrast text and a minimum touch target size (at least 44x44 dp equivalent).
- **Interactive Buttons**: Visually appealing, chunky click targets for regular "Login" and development-only "Demo Login" (hidden in non-development production builds).
- **Error Label**: A high-visibility, friendly error label rendered using TextMeshPro centered above the login button, styled with high contrast, informing users of invalid credentials or server/connectivity failures.
- **Loading State Indicator**: Prevents double submissions by disabling inputs and showing a visual text cue during active asynchronous web operations.

## UI Performance Optimization
To maximize UI performance, minimize Draw Calls (SetPass calls), and prevent battery drain/thermal throttling on Android landscape devices:
- **Canvas Subdivision**: Isolate the dynamic elements (Input Fields, Loading Indicator, and Error Label) onto a separate nested Canvas. This ensures that when the user types or a loading spinner activates, only the sub-canvas gets its mesh rebuilt, while the heavy static elements (Background image, Schoolboard graphic) remain cached.
- **Graphic Raycaster Pruning**: Disable `GraphicRaycaster` on all static canvases and disable `Raycast Target` on all text labels and background images that do not require touch interactions. This reduces graphic raycast overhead per frame.
- **Font Atlas Pre-Warming**: Pre-load and pre-warm TextMeshPro font atlases to avoid garbage collection spikes and render stutter when displaying warnings or status updates.
- **Pixel Perfect Tuning**: Toggle "Pixel Perfect" option off on the Main Canvas scaling if it's unnecessary, as it can trigger expensive CPU-side calculation loops on high-DPI screens.

## Smooth Transition Optimization (Splash to Login)
To ensure a seamless, high-fidelity experience when transitioning from the Splash scene to the Login scene:
- **Asynchronous Load & Pre-activation**: Modify `SplashController.cs` or trigger scene transition using non-blocking asynchronous load `SceneManager.LoadSceneAsync("Login")`. Keep the Splash video asset playing/paused on a fading Canvas overlay, and only fade out the Splash Canvas overlay after the Login scene is fully loaded and initialized.
- **Canvas Alpha Fader**: Implement a lightweight canvas alpha fade-in/fade-out mechanism to gracefully blend between the Splash screen background and the Login scene background, preventing any sudden frame-drops or black-screen flashes.
- **Garbage Collection Deferral**: Force garbage collection or let it run during the video playback *prior* to scene activation, preventing hitching during active scene transitions.

# Key Asset & Context
The primary assets and scripts for this unit include:
- **`Assets/_Project/Nutrimind/Scenes/App/Login.unity`**: The existing Canvas-based login scene.
- **`Assets/_Project/Nutrimind/Runtime/App/LoginController.cs`**: A new controller MonoBehaviour that orchestrates state transitions, binds to Canvas input/button components, triggers async data operations, handles errors, and executes navigation.
- **`Assets/_Project/Nutrimind/Tests/EditMode/App/SharedClient/LoginControllerTests.cs`**: A new unit test suite validating Login state transitions, client-side validations, credential checks, and error UI presentations.

# Implementation Steps

### Step 1: Design and Implement `LoginController.cs`
- **Description**: Create the controller MonoBehaviour `LoginController.cs` under `Assets/_Project/Nutrimind/Runtime/App/`. It will:
  - Hold references to `TMP_InputField` for LRN and PIN, a regular `Button` for Login, a development-only `Button` for Demo Login, and a `TextMeshProUGUI` for error messages.
  - On `Start`, initialize its state: hide error text, subscribe to buttons, and ensure state machine is in `AppState.LoggedOut`.
  - Provide input validation (checks for non-empty input before firing request).
  - Handle "Demo Login" clicks by auto-filling credentials `000000000001` and `1234` and invoking login immediately.
  - Implement a production guard: If running in a production/release build (neither `UNITY_EDITOR` nor `DEVELOPMENT_BUILD`), the demo button is programmatically destroyed or hidden.
  - Execute the async `LoginAsync` call on the provider during `AppState.Authenticating`, showing a loading state (disabling inputs).
  - Handle authentication failure: display friendly server error returned, re-enable inputs, and transition state back to `AppState.LoggedOut`.
  - Handle authentication success: apply login response, transition to `AppState.Bootstrapping`, call `GetBootstrapAsync()`, then transition to `AppState.MainMenu` and navigate to the MainMenu scene.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Configure the Login Scene UI and Canvas (including Performance Optimizations)
- **Description**: Open the `Login` scene in the Unity Editor and perform the following layout and performance optimization modifications:
  - Add the `LoginController` component to the `login` GameObject.
  - Create a new TextMeshProUGUI GameObject named `error_text` under the board Canvas hierarchy. Position it centered between the PIN input field (Y = -112) and the Login button (Y = -266) with safe spacing.
  - Set its text style to a warm, readable, child-safe warning color (e.g. high-contrast deep orange/red).
  - Hide it by default or clear its text on Start.
  - Bind all component references (LRN/PIN inputs, Login button, Error text) to the `LoginController` script.
  - Optionally, add or designate a visual button for "Demo Login" at the top-right corner or underneath the main card, style it using existing button sprites, and bind it to the controller.
  - **Canvas Subdivision Optimization**: Group dynamic UI components (Inputs, error texts, buttons) onto a nested sub-canvas. This prevents dirtying the root canvas whenever an input changes or the cursor blinks, dramatically reducing layout rebuilding CPU time.
  - **Graphic Raycaster & Target Pruning**: Remove the `GraphicRaycaster` from any static background canvases. Disable "Raycast Target" on static background images, schoolboards, and decorative TextMeshPro text objects that do not need to process click events.
  - **Pixel Perfect and Pixel Scaler Tuning**: Verify that the main Canvas is configured under `CanvasScaler` for `Scale With Screen Size` (1800x850, Match = 0.5) to maintain perfect consistency across landscapes, and turn off CPU-heavy dynamic pixel alignments if not required.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Implement Smooth Transition from Splash to Login
- **Description**: Update the transition logic (and if necessary, `SplashController.cs`) to ensure a smooth, high-fidelity experience without frame-drops:
  - Utilize `SceneManager.LoadSceneAsync` for non-blocking scene loading.
  - Implement a lightweight alpha fade in the Splash UI to transition cleanly.
  - Pre-warm critical font assets and defer major Garbage Collection calls until the exact transition frame is complete.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 4: Implement Unit Tests in `LoginControllerTests.cs`
- **Description**: Create a comprehensive edit-mode test suite under `Assets/_Project/Nutrimind/Tests/EditMode/App/SharedClient/LoginControllerTests.cs` using NUnit and mocks to verify:
  - Proper local validations (empty LRN or PIN does not initiate network calls and shows local validation error).
  - Proper state machine transition sequence (`LoggedOut` -> `Authenticating` -> `Bootstrapping` -> `MainMenu`) on happy-path authentication.
  - Correct populating of `AuthSessionState` (Token, Student ID, Masked LRN, and Name are correctly applied).
  - Graceful recovery and error display on invalid credentials or connection failure, reverting state back to `LoggedOut`.
  - Safety check verifying that the Demo Login button is inactive or destroyed in release builds.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 5: Run Tests, Build, and Verify Scene Layout
- **Description**: Compile the code, resolve any warnings, execute the unit tests inside Unity Test Runner, and run scene diagnostics (EventSystem checks, UI scaling checks across diverse aspect ratios, and safe areas).
- **Assigned role**: developer
- **Dependencies**: Step 2, Step 3, Step 4
- **Parallelizable**: No

# Verification & Testing
1. **Compilation Check**: Confirm zero compiler warnings or errors in both Runtime and Editor test assemblies.
2. **State Transition Tests**: Run `LoginControllerTests` to verify flawless behavior under success, validation error, and network error scenarios.
3. **Responsive Landscape Review**: Check the UI scaling in the Unity Game View across multiple landscape resolutions (e.g., 16:9, 18:9, 20:9) to confirm inputs and text stay centered, readable, and fit comfortably within safe-area boundaries without stretching or overlapping.
4. **Interactive Credentials Test**: Play the scene in Editor Playmode. Enter incorrect credentials and verify that the friendly error message displays. Enter correct credentials (`000000000001` / `1234`) or click "Demo Login" and verify smooth transition to the MainMenu scene (with console logs confirming successful bootstrapping).
5. **No Duplicate Event Systems**: Confirm the scene does not contain duplicate active EventSystems or missing dependencies.
