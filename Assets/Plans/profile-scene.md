# Project Overview
- **Game Title**: NutriMind Educational Game
- **High-Level Concept**: A child-friendly educational game focusing on nutrition, mindfulness, and cognitive growth, currently executing a quiz-first alignment milestone.
- **Players**: Single player.
- **Inspiration / Reference Games**: Modern gamified learning platforms (e.g., Duolingo, Kahoot) with structured assessments and visual rewards.
- **Tone / Art Direction**: Soft, vibrant, child-safe, and clear. Avoids noisy backgrounds and uses friendly character guidance.
- **Target Platform**: StandaloneWindows64 (PC) and Android.
- **Screen Orientation / Resolution**: Landscape 1920x1080.
- **Render Pipeline**: Universal Render Pipeline (URP).

# Game Mechanics
## Core Gameplay Loop
The player logs in, accesses their profile, views progress/achievements, and returns to the Main Interface to participate in assessments inside the Quiz Portal.

## Controls and Input Methods
All interactions are mouse clicks and touch taps. Buttons feature comfortable, large touch targets (minimum 44x44 pixels) and distinct highlighted feedback for perfect PC/mobile responsiveness.

# UI
The Profile screen uses **Canvas/uGUI** for high compatibility, responsive anchoring, and flawless native rendering.
- **Profile Info Panel (`comp/box1`)**: Displays masked LRN, Classroom name/section, Grade level, and Coins achievement using crisp TextMeshPro text components.
- **Title and Secondary Labels**: Shows Student Name and grade level details in high contrast.
- **Back Navigation Button (`back`)**: Anchored securely to respect device safe-areas, with a direct click listener returning to the Main Menu.

# UI & Scene Performance Optimization
To ensure maximum performance, smooth transitions, high frame rates, and optimal CPU/GPU usage (especially on mobile and Android landscape targets), the following optimization practices will be implemented:

1. **Canvas Subdivision (Batching)**:
   - Split dynamic elements (such as updating text labels, progress bars, and coin indicators) onto a nested sub-canvas separate from static UI elements (such as backgrounds, decorative icons, and borders). This isolates Canvas rebuilding to only the dirty sub-elements rather than rebuilding the entire layout mesh, significantly reducing CPU spikes.
   
2. **GraphicRaycaster & Raycast Target Pruning**:
   - Turn off `Raycast Target` on all static background images, borders, labels, and text components that do not require click/touch interaction. This drastically reduces the search space for the `GraphicRaycaster` during touch/mouse sweeps, lowering the cost of input processing.
   
3. **Overdraw Minimization**:
   - Avoid nesting multiple transparent/semi-transparent layers. Backgrounds will be set to solid/opaque where possible, and any inactive elements or popups will be completely disabled (`SetActive(false)`) rather than hidden using alpha = 0, saving GPU rasterization cost.
   
4. **Allocation-Free Scripting**:
   - Do not invoke `GetComponent`, `Find`, or string operations inside `Update()` loops. Cache all components, RectTransforms, and UI elements during `Awake()` or serialized bindings.
   
5. **Smooth and Safe Scene Transitions**:
   - Both `MainMenuController` and `ProfileController` will utilize a consistent, lightweight `CanvasGroup` fader to manage transitions smoothly.
   - On entering a scene, fade in from 0 to 1 over 0.3 seconds. On exiting, fade out from 1 to 0 over 0.3 seconds before launching `AppNavigation.LoadScene`.
   - Prevent multi-click/double-click transition queue bugs by checking an `_isTransitioning` guard flag and immediately disabling the `GraphicRaycaster` on transition start.
   - Perform a localized `System.GC.Collect()` sweep on the exact transition frame to release loaded resources and garbage memory before entering the next gameplay context.

# Key Asset & Context
### New Scripts
1. **`Assets/_Project/Nutrimind/Runtime/App/ProfileController.cs`**:
   - Thin MonoBehaviour coordinating the Profile Canvas UI elements.
   - Integrates with `CompositionRoot.Instance.DataProvider` to retrieve the latest student profile, classroom details, and progress summary asynchronously via `GetBootstrapAsync()`.
   - Manages loading states, error fallbacks, and transition fade effects.
   - Restricts double inputs by disabling raycasts during in-flight screen transitions.
   - Dynamic landscape safe-area layout calculation using `NutriMindSafeAreaUtility`.

### Prefab Variants
1. **`Assets/_Project/Nutrimind/Prefabs/Profile/ProfileVariant.prefab`**:
   - Non-destructive **Prefab Variant** created from the vendor's `PREFAB PROFILE` to maintain clean separation of asset ownership.
   - Attaches `ProfileController` and binds required TMP Text fields, the updated back button, and a transition `CanvasGroup`.

# Implementation Steps

### Step 1: Create the `ProfileController.cs` script
- **Description**: Create the controller class under `NutriMind.Runtime.App`. Implement async data fetching, UI bindings, transition lock-outs, and error recovery using standard DTOs. Ensure it has transition fade in/out effects using `CanvasGroup` and disables input raycasting during transitions.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Establish the Profile Prefab Variant & Scene Reference Replacement
- **Description**: Open the `Profile.unity` scene. Create a new Prefab Variant from `PREFAB PROFILE` named `ProfileVariant.prefab` inside `Assets/_Project/Nutrimind/Prefabs/Profile/`. Replace the existing instance in the scene with this project-owned variant.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Add Button & CanvasGroup Components and Wire the Variant
- **Description**: Open `ProfileVariant.prefab` in prefab edit mode. 
  - Add a standard `Button` component to the `back` GameObject.
  - Add a `CanvasGroup` component to the `Canvas` root for fading transitions.
  - Attach the `ProfileController` script to the prefab root.
  - Wire up all required TextMeshPro references: Name, LRN Number, Classroom, Grade, Level Number, Coins, and Badges.
  - Wire up the Back Button and CanvasGroup to the controller.
  - Uncheck `Raycast Target` on all static background, panel border, and text elements inside the prefab for GraphicRaycaster optimization.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

### Step 4: Verify Scene EventSystem and Run Playmode Checks
- **Description**: Ensure `Profile.unity` contains an active `EventSystem` component (so Canvas clicks are registered). Run in Playmode to verify successful asynchronous loading, correct mock bindings, and seamless back navigation to `MainMenu`.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

### Step 5: Implement UI and Navigation Integration Tests
- **Description**: Create a small EditMode test file `ProfileControllerTests.cs` to assert class existence, serializable fields, and state behavior constraints.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

# Verification & Testing
1. **Compilation Verification**: Clean Console with zero compilation warnings or errors.
2. **Dynamic Binding Verification**: Log in as a mock user and open Profile scene. Confirm that Name, LRN, Classroom Section, Level, and Coins are bound to exact values fetched from the provider.
3. **No Overlap / Input Guard Check**: Rapidly clicking the back button multiple times must not trigger multiple navigation tasks or print transition exceptions.
4. **Landscape Safe Area & UI Scaling**: Verify visually in Editor Game View using 16:9, 18:9, and Android landscape aspects to ensure the "back" button and profile elements never clip behind native notches.
