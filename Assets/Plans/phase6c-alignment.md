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
The player logs in, selects their subject and term (or applies filters), selects a quiz, reads instructions, takes the assessment through individual question panels (with safe unsupported item fallback), submits, and receives feedback and display-only results (score, pass/fail, non-economic badges).

## Controls and Input Methods
Menu interactions via standard mouse/pointer click. Question interactions utilize tap-to-select, matching connectors, ordering, and fill-in-the-blank text inputs. Drag and drop is supported on PC but has click-to-select fallback for Android reliability.

# UI
The system is built on a hybrid UI architecture (Canvas/uGUI + UI Toolkit). Menus, Profile, and Settings screens utilize Canvas UI. The Quiz Portal, list of quizzes, instructions, quiz sessions, and quiz results utilize UI Toolkit for structured layouts, menus, and forms. Shared states and DTOs drive both UI layers uniformly without duplicated business logic.

# Key Asset & Context
The following files are the primary focus of this Phase 6C Alignment milestone:
- **`Assets/_Project/Nutrimind/Runtime/App/IGameDataProvider.cs`**: Declarations for REST API endpoints. Will be modified to replace station-centric methods with quiz-centric ones (`GetQuizzesAsync`, `GetQuizDetailAsync`, `SubmitQuizAttemptAsync`, `GetQuizResultsAsync`, `GetQuizResultAsync`).
- **`Assets/_Project/Nutrimind/Runtime/App/LocalDemoJsonProvider.cs`**: Live simulation provider. Will be modified to parse and mutate the new quiz-first student data, managing quiz states (locked, unlocked, draft, completed) instead of stations, and supporting idempotent attempt replies and conflict results.
- **`Assets/_Project/Nutrimind/Runtime/App/HttpProvider.cs`**: Core production HTTP client transport. Will be updated to hit the new Laravel quiz endpoints and handle the updated serialization patterns.
- **`Assets/_Project/Nutrimind/Runtime/App/Dto/ApiDto.cs`**: JSON contract classes. Will be updated to represent `quiz_first_laravel_1` specifications.
- **`Assets/_Project/Nutrimind/Resources/DemoData/full-demo-student-data.json`**: Static local mock database. Will be updated to contain full quizzes, questions, pre-baked safe errors, and settings templates.
- **`Assets/_Project/Nutrimind/Editor/DemoFixtureGenerator.cs`**: Editor tooling. Will be updated to serialize the updated quiz-first structures instead of station-centric ones.
- **`Assets/_Project/Nutrimind/Tests/EditMode/App/SharedClient/LocalDemoJsonProviderTests.cs`**: Unit tests checking JSON validation, DTO deserialization, reset states, idempotency, sync states, and local-versus-remote parity.
- **`Assets/_Project/Nutrimind/Tests/EditMode/App/SharedClient/GameDataProviderContractTests.cs`**: Reflection tests validating method signatures, Task-wrapped DataResult envelopes, and snake_case DTO bindings.

# Implementation Steps

### Step 1: Update API contract DTOs in `ApiDto.cs`
- **Description**: Implement updated DTO classes for the `quiz_first_laravel_1` Laravel contract. Specifically, remove old station, challenge, and reward-shop DTOs. Create new DTOs:
  - `QuizDto`, `QuizListDto` (`GET /api/v1/student/quizzes`)
  - `QuizDetailDto`, `QuizItemDto`, `QuizItemOptionDto` (`GET /api/v1/student/quizzes/{quiz_id}`)
  - `QuizAttemptRequestDto`, `QuizAttemptResponseDto` (`POST /api/v1/student/quizzes/{quiz_id}/attempts`)
  - `QuizResultDto`, `QuizResultListDto` (`GET /api/v1/student/quiz-results` and `GET /api/v1/student/quiz-results/{attempt_id}`)
  - Update `BootstrapDto` and `SyncStatusDto` to map to `quiz_revision` instead of station-unlock revision.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

### Step 2: Refactor `IGameDataProvider` interface
- **Description**: Update the contract interface `IGameDataProvider` to include the new quiz-centric methods:
  - `Task<DataResult<QuizListDto>> GetQuizzesAsync(CancellationToken ct = default);`
  - `Task<DataResult<QuizDetailDto>> GetQuizDetailAsync(string quizId, CancellationToken ct = default);`
  - `Task<DataResult<QuizAttemptResponseDto>> SubmitQuizAttemptAsync(string quizId, QuizAttemptRequestDto request, CancellationToken ct = default);`
  - `Task<DataResult<QuizResultListDto>> GetQuizResultsAsync(CancellationToken ct = default);`
  - `Task<DataResult<QuizResultDto>> GetQuizResultAsync(string attemptId, CancellationToken ct = default);`
  - Remove/deprecate station-centric methods such as `GetStationsAsync`, `GetStationContentAsync`, `StartStationAsync`, `SubmitAttemptAsync`, `CompleteStationAsync`, and `UseRewardAsync`.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Update `HttpProvider` and `LocalDemoJsonProvider`
- **Description**: 
  - Update `HttpProvider.cs` to implement the updated `IGameDataProvider` methods, pointing to the new REST endpoints.
  - Update `LocalDemoJsonProvider.cs` to parse, deep-clone, and simulate state mutations for quizzes, quiz items, attempts, and results. Preserve settings/auth/bootstrap/profile simulation. Ensure production guard triggers and rejects construction in release builds. Ensure same `client_attempt_uuid` with matching answers returns a replay result (idempotent), while different answers under the same UUID produce a conflict error.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

### Step 4: Redesign demo fixture JSON structure and update `DemoFixtureGenerator`
- **Description**: Update `DemoFixtureGenerator.cs` to write a fully compliant, rich quiz-first data structure containing Grade 5/Grade 6 subject/term metadata, standard quizzes, and item types (multiple choice, true/false, matching, ordering, short answer, fill blank, scenario, plus unsupported fallback). Generate the updated file `full-demo-student-data.json`.
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 3
- **Parallelizable**: No

### Step 5: Update contract and provider tests
- **Description**: 
  - Update `GameDataProviderContractTests.cs` to validate the new quiz methods, return types, and snake_case properties.
  - Update `LocalDemoJsonProviderTests.cs` to test the new quiz-first workflows (login, logout, bootstrap, settings patch, sync status, retrieving quiz list, retrieving quiz detail, submitting idempotent attempt, duplicate client UUID conflicts, unsupported quiz item fallbacks, and reset behavior).
- **Assigned role**: developer
- **Dependencies**: Step 4
- **Parallelizable**: No

### Step 6: Compile and execute tests
- **Description**: Compile the Unity project, resolve any obsolete compiler warnings/errors, and run the EditMode unit tests using the Test Runner API.
- **Assigned role**: developer
- **Dependencies**: Step 5
- **Parallelizable**: No

# Verification & Testing
1. **Compilation Check**: Verify that there are zero compiler warnings or errors in runtime and test assemblies.
2. **Contract Reflection Tests**: Execute `GameDataProviderContractTests` to ensure exact snake_case JSON property mapping and Task-wrapped generic signatures.
3. **Local Demo Simulation Tests**: Execute `LocalDemoJsonProviderTests` to verify:
   - Valid JSON deserialization into DTO graphs.
   - Idempotent quiz submission (re-submitting identical UUID and answers returns a replay).
   - Duplicate attempt conflicts (submitting same UUID with different answer inputs returns a distinct conflict status/error code).
   - Settings PATCH volume increments, language preference preservation, and revision bumps.
   - Sync-status tracking with matching revisions.
   - Clear state restoration upon resetting demo state.
   - Safe rejection of `LocalDemoJsonProvider` construction in release builds.
