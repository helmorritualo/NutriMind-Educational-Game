# Unity Full Demo Data and Local Provider Requirements

## Purpose

This document defines a complete development/demo student session while the server is unavailable.

The local dataset must imitate the real student API contract so changing from JSON to HTTP does not require rewriting UI, scenes, station mechanics, stores, or navigation.

Canonical fixture (loadable at runtime via `Resources` in editor/development builds; deserializes into `DemoFixtureDto`):

```txt
Assets/_Project/Nutrimind/Resources/DemoData/full-demo-student-data.json
```

The older `docs/unity/examples/full-demo-student-data.json` used divergent field names (e.g. `access_token`/`student_id`/`display_name`) and is **superseded**. The canonical fixture above follows the snake_case DTO contract exactly (`token`, `student.id`, `student.name`, `student.lrn_masked`) as defined in `docs/SERVER_REQUIREMENTS.md` (Canonical Unity Data Contract Schemas).

## Provider Boundary

```txt
IGameDataProvider or equivalent
  |- LocalJsonGameDataProvider
  `- HttpGameDataProvider
```

Required behavior:

- same DTOs
- same result/error abstractions
- same stores
- same UI Toolkit screens
- same world/station scenes
- centralized JSON access
- centralized HTTP access
- no scene reads JSON directly
- no station sends raw HTTP directly
- no scattered gameplay-changing `if (demoMode)` branches

## Current Local Demo Coverage

The full fake student bundle covers:

- fake config and development-only login
- bootstrap, profile, classroom, and settings
- LiteraQuest, PE/Health Quest, and Science Quest subject cards
- all three terms for each subject
- all nine term world scene keys
- six LiteraQuest playable stations
- six PE/Health playable stations
- empty station lists for all three Science terms
- station start/resume responses for the 12 playable stations
- fabricated approved content for the 12 playable stations
- simulated attempts and completion for the 12 playable stations
- progress, rewards, and sync revisions
- safe error fixtures

Science worlds are exploration-only. The fixture must not create Science station content, attempts, completion, scores, or rewards in this milestone.

## Demo Story and Learning-Gameplay Coverage

The full demo fixture should include fabricated, student-safe examples for `story_context`, `mission_summary`, `npc_guides[]`, the four-step `learning_cycle`, `hint_policy` tiers, optional `discoveries[]`, `reflection_prompt`, `reward_preview[]`, and `world_restoration_state` for all 12 playable stations. These fields use the same DTO structure expected from HTTP and remain optional for compatibility. The `learning_cycle` is a canonical **object** — `{ discover, practice, apply, review }`, one guidance string per phase — not an array of strings. All field names follow the canonical snake_case schema in `docs/SERVER_REQUIREMENTS.md`.

Demo state may simulate discoveries, hint use, coins/stars, Language/Wellness Crystals, badges, and world restoration. It must preserve idempotency and reset to the immutable source fixture. Science fixture data must not simulate station missions, station rewards, or station completion.

## Required Fixture Shape

The outer object is the `DemoFixtureDto` container — it is **not** an API response. Its fields:

```txt
fixture_format_version
fixture_id
mode
notice
demo_auth                        { lrn, pin, allow_demo_login_button, development_build_only }
responses                        (DemoResponsesDto — see below)
terms_by_subject                 subject_slug -> TermDto[]
stations_by_scope                "subject:grade:term" -> StationListDto
station_content_by_id            station_id -> StationContentDto
station_start_by_id              station_id -> StationStartResponseDto
attempt_result_by_challenge_id   challenge_id -> { response_template: AttemptResponseDto, safe_mistake: AttemptFeedbackDto }
completion_result_by_station_id  station_id -> StationCompleteResponseDto
demo_only_evaluation             fixture-only fabricated expected answers (never exposed via student DTOs)
error_fixtures                   name -> error envelope (same shape as HTTP errors)
demo_scope                       (optional)
gameplay_design                  (optional)
```

`responses` (DemoResponsesDto) holds the fixed, non-scoped endpoint payloads, each the exact DTO returned by its HTTP endpoint:

```txt
responses.ping             PingResponseDto
responses.config           ApiConfigDto
responses.login            LoginResponseDto
responses.bootstrap        BootstrapDto
responses.profile          StudentProfileDto
responses.settings         SettingsDto
responses.subjects         SubjectDto[]
responses.progress_summary ProgressSummaryDto
responses.rewards          RewardWalletDto
responses.sync_status      SyncStatusDto
```

The outer fixture is a development container. Every nested payload deserializes into the **same** DTO the `HttpProvider` uses, guaranteeing provider parity. `stations_by_scope` values are `StationListDto` objects, so Science exploration-preview terms supply an empty `stations` array with `preview_mode = "exploration_only"`. Fabricated expected answers live only in `demo_only_evaluation`; they are fake development values, never exposed through student DTOs, and must never contain production answer keys.

## Fake Student Rules

The fixture represents one fabricated student and may include:

- fake student ID
- clearly fake 12-digit demo LRN
- fake display name
- grade level
- fake section/classroom and teacher display name
- language/accessibility settings
- subject/term availability
- progress summary
- reward wallet
- revision values

Never include real student information, real credentials, real access tokens, provider secrets, teacher/admin private data, or production answer keys.

## Required Station Coverage

```txt
LiteraQuest: 6
PE/Health:   6
Science:     0
Total:      12 playable stations
```

Each playable station requires:

- station ID and stable station key
- subject slug, grade, and term
- station scene key
- title and description
- state and progress
- portal/interactable/prefab keys
- completion rule
- at least one world task
- at least one challenge
- valid answer shape
- simulated attempt result
- simulated completion result

Science `stations_by_scope` entries must exist for each term as `StationListDto` objects with an empty `stations` array and `preview_mode = "exploration_only"`. This proves the UI handles intentional no-station scope safely as a valid preview state, not an error.

## Demo Fixture Asset-Key Compatibility

The local demo fixture must use the same stable local presentation keys expected by the HTTP contract, including scene, portal, interactable, prefab, icon, and optional environment keys.

Before adding a new demo key:

1. resolve it to a suitable provided asset/prefab when available
2. otherwise resolve it to an approved project-owned variant or newly created/generated asset
3. otherwise use a documented placeholder or unsupported state

Do not change fixture keys merely to match arbitrary asset filenames. Maintain a catalog mapping between stable contract keys and local asset references.

## Local Attempts and Mutable State

The local provider accepts the same attempt DTO used by HTTP, including `client_attempt_uuid`.

It must:

- produce one result per unique attempt
- return the same result for an identical retry
- reject/safely handle the same UUID with a different payload
- update progress once
- grant a simulated reward once
- use the HTTP-compatible safe error shape

The source fixture remains immutable. Runtime uses a resettable session copy for:

- settings
- station start state
- accepted attempt UUIDs
- station completion
- progress
- rewards
- revisions

## Storage and Configuration

Canonical project path (loadable via `Resources` at runtime in editor/development builds):

```txt
Assets/_Project/Nutrimind/Resources/DemoData/full-demo-student-data.json
```

Configuration is explicit:

```txt
data_source: LocalDemoJson | Http
api_base_url: required for Http
full_demo_fixture_key: required for LocalDemoJson
show_demo_indicator: true for LocalDemoJson
```

Never infer local demo mode only because HTTP failed.

## Production Protection

- production selects `Http` (default mode is `Http` in release builds, `LocalDemoJson` in editor/development builds)
- release builds reject `LocalDemoJson`: `CompositionRoot.CreateForMode(LocalDemoJson)` throws `InvalidOperationException`
- CI/build validation detects demo mode in production settings
- demo fixtures are excluded from release packaging where practical
- no automatic fallback from real HTTP failure to fake/local data

## Switch to HTTP

Switching is complete when:

1. composition selects `HttpGameDataProvider`
2. the same DTOs/stores continue working
3. the same UI/world/station scenes continue working
4. local answer evaluator is not used
5. login, attempts, progress, rewards, and completion use real endpoints
6. fixture contract tests remain as regression tests
7. integration tests pass against the server

Fix differences in contract mapping/provider layers, not inside individual scenes.

## Demo Acceptance

```txt
start app
-> demo login
-> load fake profile/settings
-> view three subjects and nine terms
-> enter nine registered worlds
-> complete 6 LiteraQuest stations
-> complete 6 PE/Health stations
-> enter 3 Science exploration-only worlds with no stations
-> submit idempotent simulated attempts
-> see progress/rewards update once
-> reset fake student state
```

The local demo proves scene and contract-shaped behavior. It is not proof of final server integration.
