# Phase 04 Unity API Client State

Status: Phase 04 implementation re-verified on 2026-06-17 after interrupted-session reconstruction; API contract changes pass with one unrelated EditMode failure remaining in dispatcher-pump hideFlags behavior.

## Implemented

- Shared async `IGameDataProvider` contract for the student API surface.
- HTTPS `HttpProvider` with config validation, bearer auth, safe error mapping, retry/idempotency policy, and all documented Phase 04 REST endpoints.
- DTO coverage for config, auth, bootstrap/profile/settings, subjects/terms/world/stations/content, station start/resume, attempts, completion, progress, rewards, sync status, and optional student-safe narrative/reward/restoration metadata.
- `AuthSessionState` stores token type and student identity from login and clears all state on logout/reset.
- `SyncPollingService` supports opaque sync revision polling with main-thread event dispatch.
- `NoOpRealtimeService` and realtime event DTOs represent optional metadata-only WSS behavior without making WSS authoritative.
- Release-build guard prevents silent `LocalDemoJson` fallback outside editor/development builds.

## Not implemented in this phase

- Final demo fixtures or LocalDemoJson data bundles.
- UI screens, worlds, stations, or gameplay mechanics.
- Live server integration test.
- Real WSS transport.

## Validation

- Unity Editor: `6000.5.0f1`.
- Package manifest: `com.unity.nuget.newtonsoft-json` requested as `3.2.2`.
- Unity compilation: 0 errors on 2026-06-17.
- Missing references: 0 scene references and 0 asset references reported by Unity MCP on 2026-06-17.
- EditMode App test run `fc0f392ab756`: 514 total, 513 passed, 1 failed. The failure is `CompositionRootModeConfigTests.CreateForMode_Http_CreatesDispatcherPump` on the dispatcher-pump `HideAndDontSave` assertion and is tracked as unrelated to the API contract changes.
- Added regression coverage for disposed `CompositionRoot` replacement and parameterized safe server error-code redaction/preserved-message behavior.

## Server contract gaps to confirm

- Subject path parameter naming: `subject_slug` vs `subject_id`.
- Station list endpoint authority: server docs prefer `/student/subjects/{subject_slug}/terms/{term_number}/stations`.
- Attempt endpoint authority: server docs prefer `/student/challenges/{challenge_id}/attempts`.
- Station completion path/parameter: clarify whether `{station_id}` path plus optional `station_session_id` body is canonical.
