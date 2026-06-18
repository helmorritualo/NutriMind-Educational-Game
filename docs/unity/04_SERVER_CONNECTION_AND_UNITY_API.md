# Unity Server Connection and Student API

## Authority

`docs/SERVER_REQUIREMENTS.md` is authoritative for the implemented API. This document defines what Unity must consume and how it must remain safe when the server evolves.

## Base Contract

```txt
API prefix: /api/v1
Authoritative transport: HTTPS JSON
Student authentication: LRN + PIN, then Bearer token
Realtime: optional metadata-only WSS/WebSocket
```

Unity does not call dashboard server functions directly.

## Provider Modes

Unity consumes this contract through one provider abstraction, `IGameDataProvider`. The active provider is selected by `DataProviderMode`:

```txt
DataProviderMode.LocalDemoJson -> development-only fixture bundle while the server is unavailable
DataProviderMode.Http          -> real HTTPS API for integration and production
```

Default mode selection (in `CompositionRoot`):

```txt
editor / development build  -> LocalDemoJson
production / release build   -> Http
```

`IGameDataProvider` is the single contract every consumer depends on; consumers never branch on `DataProviderMode`. It declares exactly **19** methods, each returning `Task<DataResult<T>>`:

```txt
PingAsync                 -> GET    /student/ping
GetConfigAsync            -> GET    /student/config
LoginAsync                -> POST   /student/auth/login
LogoutAsync               -> POST   /student/auth/logout
GetBootstrapAsync         -> GET    /student/bootstrap
GetProfileAsync           -> GET    /student/profile
GetSettingsAsync          -> GET    /student/settings
PatchSettingsAsync        -> PATCH  /student/settings
GetSubjectsAsync          -> GET    /student/subjects
GetTermsAsync             -> GET    /student/subjects/{subject_slug}/terms
GetStationsAsync          -> GET    /student/subjects/{subject_slug}/terms/{term_number}/stations
GetStationContentAsync    -> GET    /student/stations/{station_id}/content
StartStationAsync         -> POST   /student/stations/{station_id}/start
SubmitAttemptAsync        -> POST   /student/challenges/{challenge_id}/attempts
CompleteStationAsync      -> POST   /student/stations/{station_id}/complete
GetProgressSummaryAsync   -> GET    /student/progress/summary
GetRewardsAsync           -> GET    /student/rewards
UseRewardAsync            -> POST   /student/rewards/{reward_code}/use
GetSyncStatusAsync        -> GET    /student/sync/status
```

`GetStationsAsync` returns a `StationListDto` object (not a bare array) so Science exploration-preview terms can return an empty `stations` array with `preview_mode = "exploration_only"`. See **Station List Scope** below.

The local provider does not define an alternate game contract. Each nested local payload must deserialize into the same DTO used by the corresponding HTTP endpoint. The outer demo bundle is only a fixture container and is not itself an API response.

Switching providers must require configuration/composition changes only. It must not require scene, station, form, or gameplay rewrites.

Production builds must reject `LocalDemoJson`: in a release build `CompositionRoot.CreateForMode(LocalDemoJson)` throws `InvalidOperationException`, and there is **no** silent fallback from `Http` to local demo data on failure. Release packaging must exclude the full demo fixture where practical.

## Station List Scope

`GET /api/v1/student/subjects/{subject_slug}/terms/{term_number}/stations` returns a `StationListDto`:

```json
{
  "subject_slug": "sciencequest",
  "grade_level": 5,
  "term_number": 1,
  "stations": [],
  "preview_mode": "exploration_only",
  "message": "This world is an exploration preview."
}
```

LiteraQuest and PE/Health terms return populated `stations` arrays. Science exploration-preview terms return an empty `stations` array with `preview_mode = "exploration_only"`. An empty list is a valid no-station preview state, not an error.

## World Metadata

Term and station responses include `world_metadata` for Unity scene loading. Canonical fields (agree with `docs/SERVER_REQUIREMENTS.md`):

```txt
world_theme_key
world_title
unity_scene_key
unity_scene_name
scene_address_key
environment_tags[]
mechanic_family
```

The server returns scene identity; Unity owns the local scene asset. A missing local scene key is handled as a world-unavailable state, never by loading a wrong fallback world.

## Startup Endpoints

| Endpoint | Purpose |
|---|---|
| `GET /api/v1/student/ping` | Basic connectivity check |
| `GET /api/v1/student/config` | Compatibility, maintenance, polling, and optional realtime configuration |

Expected config areas:

```txt
api_version
contract_version
server_time
maintenance_mode
minimum_unity_client_version
supported_languages
polling.enabled
polling.default_interval_seconds
polling.minimum_interval_seconds
realtime.enabled
realtime.transport
realtime.url
realtime.events_are_metadata_only
```

Unity must not crash when optional config fields are added.

## Student Endpoints

| Endpoint | Purpose |
|---|---|
| `POST /api/v1/student/auth/login` | Student login |
| `POST /api/v1/student/auth/logout` | Student logout |
| `GET /api/v1/student/bootstrap` | Student, classroom, subjects, progress, and wallet bootstrap |
| `GET /api/v1/student/profile` | Student-safe profile |
| `GET /api/v1/student/settings` | Load settings |
| `PATCH /api/v1/student/settings` | Save settings |
| `GET /api/v1/student/subjects` | Available subject platforms |
| `GET /api/v1/student/subjects/{subject_slug}/terms` | Terms and world metadata |
| `GET /api/v1/student/subjects/{subject_slug}/terms/{term_number}/stations` | Station state list |
| `GET /api/v1/student/stations/{station_id}/content` | Approved station content and world tasks |
| `POST /api/v1/student/stations/{station_id}/start` | Start or resume station session |
| `POST /api/v1/student/challenges/{challenge_id}/attempts` | Submit attempt |
| `POST /api/v1/student/stations/{station_id}/complete` | Complete station where separate completion is required |
| `GET /api/v1/student/progress/summary` | Progress overview |
| `GET /api/v1/student/rewards` | Reward wallet |
| `POST /api/v1/student/rewards/{reward_code}/use` | Use a reward |
| `GET /api/v1/student/sync/status` | Revision-based refresh metadata |

If the server implements session refresh, mastery, or leaderboard endpoints, the server requirements, local fixture coverage, and this file must be updated together.

## Full Demo Bundle Mapping

The canonical, up-to-date fixture is the project file:

```txt
Assets/_Project/Nutrimind/Resources/DemoData/full-demo-student-data.json
```

It is loadable at runtime via `Resources` in editor/development builds and deserializes into `DemoFixtureDto`. The older `examples/full-demo-student-data.json` used divergent field names (e.g. `access_token`/`student_id`/`display_name`) and is **superseded**: the canonical fixture follows the DTO contract exactly (`token`, `student.id`, `student.name`, `student.lrn_masked`). All fixture field names follow the canonical snake_case DTO names defined in `docs/SERVER_REQUIREMENTS.md` (Canonical Unity Data Contract Schemas).

The fixture contains one fabricated Grade 5 student session and these logical payload groups:

| Fixture key | HTTP contract represented |
|---|---|
| `responses.ping` | `GET /student/ping` |
| `responses.config` | `GET /student/config` |
| `responses.login` | `POST /student/auth/login` |
| `responses.bootstrap` | `GET /student/bootstrap` |
| `responses.profile` | `GET /student/profile` |
| `responses.settings` | settings GET/PATCH snapshot |
| `responses.subjects` | `GET /student/subjects` |
| `terms_by_subject` | subject terms endpoint |
| `stations_by_scope` | subject/term station list endpoint |
| `station_content_by_id` | station content endpoint |
| `station_start_by_id` | station start/resume endpoint |
| `attempt_result_by_challenge_id` | simulated attempt response templates |
| `completion_result_by_station_id` | simulated completion responses |
| `responses.progress_summary` | progress summary endpoint |
| `responses.rewards` | reward wallet endpoint |
| `responses.sync_status` | sync status endpoint |
| `error_fixtures` | safe representative error envelopes |

The focused demo fixture must cover the 12 current playable stations: 6 LiteraQuest and 6 PE/Health. It must also provide three Science term-world records with intentionally empty station lists so Science exploration scenes can be demonstrated without fabricating tasks, scoring, progress, or rewards.

A local demo evaluator may use fabricated expected answers stored in a clearly separated `demo_only_evaluation` section. These values are fake development content, must not be exposed through student DTOs, and must never contain copied production answer keys.

## Optional Learning-Gameplay Content Fields

Station-content responses may add optional, student-safe fields. These use the canonical snake_case names defined in `docs/SERVER_REQUIREMENTS.md` (Canonical Unity Data Contract Schemas) and agree with it field-for-field.

Station mission/content fields:

```txt
story_context
mission_title
mission_summary
learning_skill
student_learning_goal
instructions
completion_rule { type, required_count }
world_tasks[]   { task_id, task_key, task_type, portal_key, interactable_key, prefab_key, world_position_hint, challenge_id, required }
```

NPC guides:

```txt
npc_guides[] { npc_key, display_name, role, avatar_key, intro_dialogue, completion_dialogue }
```

Learning cycle is a canonical **object** with one short guidance string per phase — **not** an array of strings:

```json
{
  "learning_cycle": {
    "discover": "...",
    "practice": "...",
    "apply": "...",
    "review": "..."
  }
}
```

Hint policy:

```json
{
  "hint_policy": {
    "max_hint_tier": 3,
    "preserve_world_progress": true,
    "penalize_ordinary_mistake": false,
    "tiers": [ { "tier": 1, "text": "..." } ]
  }
}
```

Discoveries (each always optional, never required for completion):

```txt
discoveries[] { discovery_key, type, title, description, optional, reward_preview }
```

Reflection prompt (single string): `reflection_prompt`.

Reward previews (presentation/motivation only — not earned rewards):

```txt
reward_preview[] { code, reward_key, reward_type, display_name, icon_key, quantity, grant_scope }
```

Success feedback:

```txt
success_feedback { message, encouraging_phrases[] }
```

World restoration state (applied only after an accepted completion):

```txt
world_restoration_state { state_key, apply_after_accepted_completion, state_data }
```

These fields are additive. Unity must tolerate their absence and unknown optional values. They must never expose answer keys, hidden scoring rules, teacher notes, AI prompts, private health guidance, or unapproved content.

Unity uses these fields to configure shared mission, NPC, hint, discovery, reflection, reward, and restoration components. Official feedback, hints, reward grants, and restoration completion remain provider-authoritative.

## Login Request

```json
{
  "lrn": "123456789012",
  "pin": "123456",
  "device_name": "Unity Android",
  "client_version": "0.1.0"
}
```

Unity must not log or persist a real PIN. The full demo fixture may define an obviously fabricated development-only login so the login UI can be demonstrated; it must be rejected outside editor/development demo mode. The bearer token is stored only according to the selected active-session security design.

## Attempt Request

```json
{
  "station_session_id": "ssn_01HXAMPLE123456789",
  "station_id": "station_501",
  "client_attempt_uuid": "8d7a1f64-7b46-4f8d-8a33-8e0dbf7393a1",
  "answer": "B",
  "time_spent_seconds": 34,
  "used_rewards": [
    { "code": "hint_scroll", "quantity": 1 }
  ]
}
```

Retrying the same submitted answer uses the same UUID. Editing the answer before submission creates a new attempt identity only when the original answer was not accepted as the same request.

## Supported Answer Shapes

Unity must serialize the answer shape documented by the server for the challenge type. Expected families include:

```txt
multiple_choice -> option key or ID
true_false -> boolean
matching -> list/map of left-right stable keys
sorting -> stable item keys grouped by target/bin
ordering -> ordered list of stable item keys
fill_blank -> text or keyed blank values
short_response -> text plus any allowed metadata
scenario_choice -> selected stable path/choice key
```

Unity must not infer correctness locally from hidden or guessed answer data.

## Attempt Result

Authoritative response from `POST /api/v1/student/challenges/{challenge_id}/attempts`:

```jsonc
{
  "attempt_id": "...",
  "client_attempt_uuid": "...",
  "challenge_id": "...",
  "status": "...",
  "accepted": true,
  "correct": true,
  "is_replay": false,
  "review_status": "...",
  "feedback": {
    "is_correct": true,
    "message": "...",
    "explanation": "...",
    "misconception_message": "...",
    "encouraging_message": "...",
    "retry_action": "...",
    "retry_allowed": true,
    "remaining_attempts": 2,
    "current_hint_tier": 0,
    "next_hint_tier": 1,
    "hint_text": "..."
  },
  "score_awarded": 10,
  "progress": { "completed_challenges": 1, "required_challenges": 1, "station_progress_percent": 100 },
  "rewards_granted": [ { "reward_code": "...", "reward_type": "...", "display_name": "...", "quantity": 1 } ],
  "progress_updated": true,
  "progress_revision": "...",
  "reward_wallet_revision": "..."
}
```

Correctness (`correct`) and acceptance (`accepted`) are authoritative — Unity never infers them locally.

## Station Completion Result

Authoritative response from `POST /api/v1/student/stations/{station_id}/complete`:

```jsonc
{
  "station_id": "...",
  "status": "...",
  "completed": true,
  "is_replay": false,
  "score_total": 20,
  "portal_state": "completed",
  "unlocks": [ { "station_id": "...", "station_key": "...", "state": "unlocked" } ],
  "term_completion": {
    "subject_slug": "literaquest",
    "term_number": 1,
    "completed": true,
    "crystal": { "reward_code": "...", "reward_type": "...", "display_name": "...", "quantity": 1 },
    "badge":   { "reward_code": "...", "reward_type": "...", "display_name": "...", "quantity": 1 }
  },
  "rewards_granted": [ { "reward_code": "...", "reward_type": "...", "display_name": "...", "quantity": 1 } ],
  "world_restoration_result": { "state_key": "...", "restored": true },
  "progress_summary": { /* ProgressSummary */ },
  "progress_revision": "...",
  "reward_wallet_revision": "..."
}
```

Completing both required term stations sets `term_completion.completed = true` and may grant a subject-themed `crystal` and/or `badge`. `world_restoration_result` is provider-confirmed; Unity owns only the restoration animation.

## Idempotent Replay

Attempt retries reuse the same `client_attempt_uuid`; completions are keyed by station. A duplicate `client_attempt_uuid` (or re-completion of an already-completed station) returns the **same** result with `is_replay = true` and never double-scores, double-grants rewards, or re-awards a term crystal/badge. Unity must treat `is_replay = true` as an already-applied result and must not re-animate reward grants as newly earned.

## Error Envelope

```json
{
  "message": "Station is locked.",
  "code": "STATION_LOCKED",
  "request_id": "req_01HYEXAMPLE123456789",
  "retryable": false,
  "details": {},
  "field_errors": {},
  "retry_after_seconds": null,
  "action": "refresh_sync_status"
}
```

Canonical error codes (full known/safe set; agrees with `docs/SERVER_REQUIREMENTS.md`). Server-issued envelope codes:

```txt
UNAUTHENTICATED
TOKEN_EXPIRED
STUDENT_INACTIVE
VALIDATION_ERROR
RATE_LIMITED
SYNC_RATE_LIMITED
SERVER_UNAVAILABLE
SERVER_TIMEOUT
MAINTENANCE_MODE
STATION_LOCKED
STATION_ALREADY_COMPLETED
CONTENT_NOT_PUBLISHED
SESSION_NOT_FOUND
SESSION_FORBIDDEN
WORLD_SCENE_UNAVAILABLE
STALE_CONTENT
CLIENT_VERSION_UNSUPPORTED
CONFIG_VERSION_UNSUPPORTED
REALTIME_UNAVAILABLE
AI_NOT_CONFIGURED
NOT_FOUND
```

Client-side-only codes (transport/local failures; never emitted by the server):

```txt
NETWORK_ERROR
CONFIGURATION_ERROR
PROVIDER_DISPOSED
INVALID_RESPONSE
UNKNOWN_ERROR
```

Supported `action` behavior:

```txt
login_again
retry
wait_then_retry
refresh_sync_status
return_to_menu
show_offline_prompt
contact_teacher
```

Any unrecognized `action` value maps to a safe `Unknown` action. Unknown error codes use a safe generic message and retain request ID support details.

## Polling

Unity stores opaque revisions such as:

```txt
student_progress_revision
student_settings_revision
station_unlock_revision
published_content_revision
reward_wallet_revision
```

A changed revision triggers a targeted refresh. Unity respects server polling intervals and stops polling during logout, app pause, destroyed session, or unavailable state.

## Optional Realtime

WSS/WebSocket is optional. When enabled, events only indicate that data changed. Unity refreshes authoritative data over HTTP.

Expected event families:

```txt
sync_revision_changed
station_unlock_changed
published_content_changed
student_progress_changed
reward_wallet_changed
settings_changed
server_maintenance_notice
session_revoked
```

Unknown, duplicate, malformed, or disconnected realtime events are non-fatal. Polling remains the fallback.

## Switching to the Real Server

Moving from the local full-demo dataset to the server must follow this sequence:

1. Select the `Http` provider in environment/build configuration.
2. Keep the same DTOs, stores, screens, scenes, answer builders, and attempt coordinator.
3. Replace simulated local auth, attempts, completion, progress, rewards, and revisions with endpoint calls through the HTTP provider.
4. Run contract fixtures and integration tests against representative server responses.
5. Resolve contract differences in the provider/DTO layer rather than creating per-scene compatibility code.
6. Confirm that release builds fail closed if configured for local demo data.

## Crash-Safe Rules

- Parse asynchronously without blocking the main thread.
- Apply scene/UI changes on the main thread.
- Cancel requests whose scene/session owner no longer exists.
- Treat missing required fields as safe contract errors.
- Treat unknown optional fields as ignorable.
- Treat unknown enums as unsupported states.
- Preserve local answer state on retryable network failure.
- Require server result before official success/reward presentation.
- Never display raw stack traces, SQL errors, provider errors, or tokens.

## Student-Safe Boundary

Unity must never receive, store, display, or depend on:

```txt
answer keys before submission
hidden scoring formulas
AI provider configuration or secrets
AI drafts or rejected content
teacher review notes
admin security data
private audit logs
real PINs
raw exception messages
unrelated student records
real student records inside demo fixtures
production answer keys inside demo fixtures
```
