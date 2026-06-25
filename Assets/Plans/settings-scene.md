# Project Overview
- **Game Title**: NutriMind Educational Game
- **High-Level Concept**: A child-friendly educational game focusing on nutrition, mindfulness, and cognitive growth, currently executing a quiz-first alignment milestone.
- **Players**: Single player.
- **Inspiration / Reference Games**: Modern gamified learning platforms (e.g., Duolingo, Kahoot) with structured assessments and visual rewards.
- **Tone / Art Direction**: Soft, vibrant, child-safe, and clear. Avoids noisy backgrounds and uses friendly character guidance.
- **Target Platform**: PC (StandaloneWindows64) and Android.
- **Screen Orientation / Resolution**: Landscape 1920x1080.
- **Render Pipeline**: Universal Render Pipeline (URP) with PC_RPAsset.

# Game Mechanics
## Core Gameplay Loop
The player accesses the Settings scene from the Main Menu, modifies their game preferences (music, volume, language, text size, and accessibility), saves their changes, and returns to the Main Menu. Alternatively, they can choose to log out of their session.

## Controls and Input Methods
All interactions are mouse clicks and touch taps. Sliders and dropdowns feature comfortable, large touch targets (minimum 44x44 pixels) and distinct visual focus/highlighted states for perfect PC/mobile responsiveness.

# UI
The Settings screen uses **Canvas/uGUI** for high compatibility, responsive anchoring, and flawless native rendering.
- **Background (`Settings bg`)**: Handles high-performance video or static background display.
- **Dialogue Panel Background (`Image` with sprite `SETT`)**: Contains all controls.
- **Volume Slider (`Volume`)**: Standard Slider component controlling general volume / SFX.
- **Music Slider (`Music`)**: Standard Slider component controlling music volume.
- **Language Dropdown (`Language`)**: TMP Dropdown component for language selection (e.g., "English", "Filipino").
- **Text Size Dropdown (`Text Size`)**: TMP Dropdown component for font size scale selection (e.g., "Small", "Medium", "Large").
- **Accessibility Dropdown (`Accessibility`)**: TMP Dropdown component for accessibility preferences (e.g., "Off", "On").
- **Logout Button (`Logout`)**: Triggers best-effort async server logout and session reset.
- **Save & Back Button (`Image` with sprite `btn`)**: This will be renamed to `SaveAndBackButton`, upgraded with a `Button` component, and populated with a legible text label "SAVE & CLOSE" to act as a responsive back/save trigger.

# UI & Scene Performance Optimization
To ensure maximum performance, low power draw, and optimal CPU/GPU usage (especially on Android and low-end mobile devices), the following best practices will be implemented:

1. **Canvas Subdivision & Isolation**:
   - Split dynamic elements (such as sliders, dropdowns, and buttons) from static elements (such as backgrounds and frames) using nested sub-canvases where appropriate. This localizes canvas rebuilding to only modified controls.
   
2. **GraphicRaycaster & Raycast Target Pruning**:
   - Turn off `Raycast Target` on all static images, backgrounds, panel frames, and static TextMeshPro text labels. Only interactive controls (sliders, dropdowns, buttons) will have raycast targets enabled. This significantly reduces the CPU sweep cost during touch/mouse input detection.
   
3. **GPU Overdraw Minimization**:
   - Avoid overlapping multiple semi-transparent layers. Inactive dropdown templates or popup elements will be completely disabled (`SetActive(false)`) instead of using zero alpha, preventing wasteful rasterization.
   - Stop or suspend any running `VideoPlayer` immediately when transitioning away from the scene to release GPU resources.
   
4. **Allocation-Free Scripting**:
   - No `GetComponent`, `Find`, or string concatenations inside update loops.
   - Use event-driven subscriptions (`onValueChanged.AddListener` and `onClick.AddListener`) instead of polling values frame-by-frame.
   
5. **Input Lock-Out & Transition Fading**:
   - Use a lightweight `CanvasGroup` fader to fade the screen in (0.3s) and out (0.3s) smoothly.
   - Immediately disable the scene's `GraphicRaycaster` and check an `_isTransitioning` flag on click to prevent multi-click queue issues or double transition exceptions.
   - Perform a localized `System.GC.Collect()` on the transition frame to release unused assets before loading the next scene.

# Key Asset & Context
### New Scripts
1. **`Assets/_Project/Nutrimind/Runtime/App/SettingsController.cs`**:
   - Thin MonoBehaviour coordinating the Settings Canvas UI.
   - Integrates with `CompositionRoot.Instance.DataProvider` to fetch the latest settings asynchronously via `GetSettingsAsync()`.
   - On "Save & Close" click, construct a `SettingsDto` and send it to `PatchSettingsAsync(settings)`.
   - On "Logout" click, trigger `LogoutAsync()`, call `Session.Clear()`, `AuthSession.Reset()`, and return to `Login`.
   - Manages safe-area layouts, smooth fade-in/out transitions, and input raycast guards.

### Prefab Variants
1. **`Assets/_Project/Nutrimind/Prefabs/Settings/SettingsVariant.prefab`**:
   - Non-destructive **Prefab Variant** created from the existing `PREFABS SETTINGS 1` in `Settings.unity` to separate asset ownership.
   - Attaches `SettingsController` and binds required Sliders, Dropdowns, Buttons, and transition CanvasGroup.

# Implementation Steps

### Step 1: Create the `SettingsController.cs` script
- **Description**: Implement `SettingsController` inside the `NutriMind.Runtime.App` namespace. Wire up properties for `VolumeSlider`, `MusicSlider`, `LanguageDropdown`, `TextSizeDropdown`, `AccessibilityDropdown`, `LogoutButton`, `SaveButton`, `MainCanvasGroup`, and `SafeAreaPanel`. Implement loading, patching, logging out, and smooth fading transitions.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Establish the Settings Prefab Variant & Scene Reference Replacement
- **Description**: Open the `Settings.unity` scene. Create a new Prefab Variant from `PREFABS SETTINGS 1` named `SettingsVariant.prefab` inside `Assets/_Project/Nutrimind/Prefabs/Settings/`. Replace the existing instance in the scene with this project-owned variant.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Upgrade the Back Button and Add Text child
- **Description**: Open `SettingsVariant.prefab` in prefab edit mode:
  - Find the GameObject named `Image` (child at index 8 with the `btn` sprite). Rename it to `SaveAndBackButton`.
  - Add a standard `Button` component to it.
  - Create a child TextMeshProUGUI element under it, set its text to "SAVE & CLOSE", choose a highly readable child-friendly font asset, center it, and set font styling to high contrast.
  - Sizing must be verified to have a comfortable tap target of at least 44x44 (the existing 332x55 size is excellent).
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

### Step 4: Wire up UI Components and Apply Performance Optimizations
- **Description**: Inside the `SettingsVariant.prefab`:
  - Add a `CanvasGroup` component to the `Canvas` root for fading transitions.
  - Attach the `SettingsController` to the root and wire all required references (Sliders, Dropdowns, Buttons, CanvasGroup, and safe-area RectTransform).
  - Uncheck `Raycast Target` on all static graphic elements (including the panel background `SETT` sprite, dropdown labels, and static TextMeshPro header labels) for raycast sweep optimization.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

### Step 5: Implement Dynamic Safe Area and Sizing
- **Description**: Wire a serialized safe-area panel RectTransform inside `SettingsVariant` to calculate and apply physical safe-area scaling during `Start()`, ensuring the UI never overlaps screen notches on diverse phone sizes or in Android landscape.
- **Assigned role**: developer
- **Dependencies**: Step 4
- **Parallelizable**: No

### Step 6: Verify and Run Playmode / EditMode Tests
- **Description**: Run Playmode to verify successful asynchronous loading, correct bindings of default settings values from `LocalDemoJsonProvider`, successful saving of altered preferences, and correct navigation behaviors. Ensure the Unity console is clean. Run any existing edit mode style/accessibility style tests to verify compatibility.
- **Assigned role**: developer
- **Dependencies**: Step 5
- **Parallelizable**: No

# Verification & Testing
1. **Compilation Verification**: Clean Console with zero compilation warnings or errors.
2. **Dynamic Preference Loading**: Confirm Sliders and Dropdowns accurately reflect current Settings state on load.
3. **Persistence Test**: Change language/volumes, click "Save & Close", return to Main Menu, open Settings again, and verify that the customized preferences were correctly stored and reloaded.
4. **Input Protection Guard**: Rapid clicking of buttons must not throw double transition exceptions.
5. **Landscape Safe Area**: Verify in Editor Game View using diverse aspect ratios (e.g. 16:9, 18:9, 19.5:9) to ensure no controls clip under cutouts.
