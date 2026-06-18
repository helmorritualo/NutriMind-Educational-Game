# Server Requirements

## Purpose

This document defines the server-side requirements for the educational quest game platform. It is independent from the Unity requirements document, but it contains the shared contract details needed for the server and Unity client to connect safely.

The server is the source of truth for identity, classroom data, content, station unlocks, attempts, scoring, rewards, progress, reports, audit records, AI draft generation, and Unity-facing API responses. Unity is the presentation and interaction client.

## Documentation Authority and Cross-Side Synchronization

The project has two top-level requirement authorities:

- `docs/SERVER_REQUIREMENTS.md` for server, dashboard, data, security, testing, and Unity-facing contract requirements
- `docs/UNITY_REQUIREMENTS.md` as the Unity authority index, with detailed Unity requirements under `docs/unity/`

The server requirements are authoritative for API paths, authentication, payload schemas, validation, error codes, scoring, rewards, progress, compatibility, polling, optional realtime metadata, and student-safe data boundaries.

The server-side agent may update Unity documentation when a verified server contract change affects the client. It must update `docs/UNITY_REQUIREMENTS.md` and the relevant detailed Unity document—normally `docs/unity/04_SERVER_CONNECTION_AND_UNITY_API.md`, and any affected gameplay or demo-data document. Server changes must not silently redefine Unity-owned scene design, controls, presentation, or asset behavior.

The server agent must read the Unity index and relevant split Unity documents before changing a Unity-facing contract.

## Shared Product Snapshot

The product is an online educational game platform for Grade 5 and Grade 6 students. Teachers manage classroom content and students complete learning stations in Unity.

Supported quest lines:

| Quest line | Learning area | Scope expectation |
|---|---|---|
| LiteraQuest | English literacy | Server supports 5 station slots per term; current Unity demo implements 2 playable stations per term, 6 total |
| Science Quest | Science inquiry and concepts | Server supports all slots; current Unity demo exposes the 3 term worlds as exploration previews with no station gameplay |
| PE/Health Quest | PE, health, wellness, safety, and decision making | Stable server slug may remain `health_quest`; current Unity demo implements 2 playable stations per term, 6 total |

Each subject has 3 terms and 5 station slots per term for Grade 5 and Grade 6. The server continues to support the full 45-slot subject structure. The current Unity demo consumes 12 playable stations—6 LiteraQuest and 6 PE/Health—and three Science term-world records with intentionally empty station lists. Deferred server content must not be treated as required Unity implementation until the project owner expands the client scope.

## Actors

### Super Admin

The Super Admin manages school-level configuration: school years, grade sections, teacher accounts, teacher assignments, subject availability, account lifecycle, system settings, system AI configuration, platform reports, audit logs, and security visibility.

### Teacher

The Teacher manages classroom-level learning: classroom server, students, imports, content packs, challenge templates, AI-generated draft questions, review/approval decisions, world task metadata, station unlocks, reports, and teacher account settings.

### Student

The Student uses only the Unity client. The student logs in with LRN and PIN, selects subject and term, explores the term world, completes station tasks, receives server feedback, earns rewards, and views progress/profile/settings. Students never access dashboard pages, AI tools, draft questions, answer keys, teacher notes, or admin data.

## Server Technology Baseline

The active server stack is:

```txt
TanStack Start
React
TypeScript
Bun
PostgreSQL
Drizzle ORM
Better Auth
Redis
Resend
shadcn/ui
Tailwind
TanStack Query/Form/Table/Virtual/Store/Pacer
Zod
@t3-oss/env-core
Biome
```

TanStack Start owns the web app, dashboards, server routes, and Unity HTTP API. PostgreSQL is the persistent source of truth. Redis supports cache, revision metadata, rate limits, and realtime coordination. Resend handles account emails. Better Auth handles admin and teacher dashboard identity.

## Product Delivery Requirements

The server delivery is production-scope for the capstone system. It includes all admin, teacher, content, student API, security, report, audit, email, settings, and data-management requirements needed to operate the game.

The Unity client has a smaller playable delivery scope, but the server data model, dashboards, content flow, API, and reports still support all subjects, terms, station slots, station states, content lifecycle states, and historical data.

## Data Initialization and No-Hardcoded-Catalog Policy

Production product data must come from database records and governed management workflows, not permanent source-code arrays, hidden seed scripts, or environment variables.

The following are database-managed, editable, auditable, and portable:

- subject platforms and stable slugs
- subject availability by school year, grade, section, or classroom
- terms and station catalog records
- world and station metadata
- curriculum references and competency mappings
- content packs, challenge templates, and official questions
- reward catalog entries and grant/use rules
- classrooms, assignments, memberships, and student records
- content lifecycle, review, publication, unlock, and archive state

Approved ways to create or update product data are:

- admin dashboard forms
- teacher authoring and review workflows
- validated CSV/JSON/XLSX imports with preview, errors, and confirmation
- idempotent setup/import commands
- optional AI draft generation followed by teacher review
- explicit development/demo fixtures that are marked non-production

Seed or bootstrap code is limited to minimum technical initialization, such as first-super-admin creation, required system settings, reference lookup bootstrapping that cannot be administered yet, and explicit demo/test fixtures. Seed scripts are not the normal production content-management path.

Environment variables store deployment configuration and secrets. They must not store editable subject, station, curriculum, challenge, reward, or classroom catalog data.

The server agent must not solve missing management features by adding permanent hardcoded data. It must create an appropriate database model and admin, teacher, import, or controlled-bootstrap path.

## Data Hierarchy

```txt
School Year
  -> Grade Section
      -> Teacher Assignment
      -> Classroom Server
          -> Student Memberships
          -> Classroom Subject
              -> Subject Term
                  -> Subject Station
                      -> Content Pack
                          -> Challenge / Official Question
                              -> Optional AI Draft Source
                              -> Teacher Review State
                              -> World Task / Interactable
                              -> Server-side Answer Key / Scoring Rule
```

Unity receives only approved, active, published, unlocked, student-safe data. Answer keys, scoring rules, AI settings, AI draft questions, rejected questions, review notes, admin security data, hidden scoring data, raw tokens, PINs, and private audit data remain server-only.

## Core Server Domains

### Identity and Access

Admin and teacher dashboard accounts use Better Auth browser sessions. Optional MFA is available for admin and teacher accounts. Teacher access is scoped to assigned grade sections and owned classrooms.

Student Unity login uses LRN + PIN and returns a server-issued bearer token. Student tokens are scoped to the student, classroom, grade, and active account status. Token revocation is required when a student is deactivated or the PIN is reset.

### School and Classroom Setup

The server supports school years, Grade 5 and Grade 6 sections, teacher assignments, classroom servers, student memberships, student imports, and subject availability. Classroom data is scoped so teachers cannot view or change other teachers’ students, content, or reports.

### Content Lifecycle

Learning content follows this lifecycle:

```txt
Curriculum + AI Reference Guide
  -> Manual draft or AI-generated draft
  -> Teacher review
  -> Approved official question
  -> Published content pack
  -> Unlocked station
  -> Playable in Unity
  -> Archived when retired or when the school year closes
```

Only approved, active, published, and unlocked content is returned to Unity. Draft, rejected, archived, teacher-only, and answer-key data never appears in the Unity API.

### Curriculum Metadata

Each planned station stores curriculum metadata so learning goals stay stable even when content examples or Unity interactions change:

```txt
curriculum_source_title
curriculum_source_url
curriculum_source_type
grade_level
subject_platform
term_number
station_number
learning_skill
student_learning_goal
competency_tags
teacher_editable
verification_note
```

Station mechanics can change, but the learning skill, student learning goal, and competency tags stay aligned to the selected grade, subject, term, and station.

### Challenge Types

The server supports these Unity-facing challenge types:

```txt
multiple_choice
true_false
matching
sorting
ordering
fill_blank
short_response
scenario_choice
```

The server validates answer data, scores objective challenge types, stores short responses for teacher review when needed, and returns safe feedback.

### Manual Game Content Creation

Manual authoring is required and remains the complete fallback when AI is disabled, unconfigured, unavailable, over quota, unaffordable, or not preferred by the teacher.

Teachers can manually create, edit, preview, review, approve, publish, unpublish, and archive game content within their assigned scope. The manual editor supports the canonical data required by Unity and server scoring, including:

- classroom/ownership context
- subject platform, grade, term, and station
- content-pack title, description, and learning objective
- curriculum/reference-guide source and competency tags
- challenge type and mechanic family
- prompt/instructions and student-visible content
- options, matching pairs, ordering items, sort groups, blanks, scenario choices, or short-response guidance
- correct answer or review rubric stored server-side only
- explanation and feedback
- difficulty and scoring profile
- world-task metadata and stable keys when needed
- optional media references
- story context, mission title/summary, and NPC guide dialogue
- Discover, Practice, Apply, and Review activity guidance
- hint tiers, retry policy, reflection, and misconception-aware feedback
- optional discoveries/fun facts and reward previews
- world restoration state keys and presentation metadata
- lifecycle and publication state

Manual and AI-assisted content use the same validation, ownership, lifecycle, review, publication, unlock, scoring, reward, audit, and Unity-safe serialization rules.

### Prompt-Based AI Assistance

AI assistance is an optional teacher-authoring enhancement, not a student feature and not a replacement for the manual editor.

The teacher experience uses a natural-language prompt for intent, classroom context, lesson nuance, constraints, tone, examples, and requested improvements. Separate structured inputs define canonical data that teachers should not need to repeat in the prompt, including:

- subject platform
- grade level
- term and station
- curriculum/reference context
- challenge type
- question/activity count
- difficulty
- answer format
- scoring profile
- competency tags

Structured fields override conflicting prompt text for canonical metadata. The server validates structured inputs and provider limits before generation. Generated output returns to the same manual content editor as an editable `ai_draft`; it never becomes playable automatically. A teacher must review, correct, approve, and publish it.

Provider resolution supports an admin default, an allowed teacher override where configured, and a clear not-configured state. The server enforces provider allowlists, prompt and output limits, teacher/school quotas, concurrency limits, maximum generated item counts, and usage/cost metadata. Provider secrets remain server-only and never appear in browser payloads, Unity, exports, logs, or reports.

The AI review flow records safe source-prompt metadata, structured generation parameters, provider resolution, usage, edits, approvals, rejections, and configuration changes for audit visibility without exposing secrets.

### Learning-Through-Gameplay Content and Narrative

The server content model supports learning missions in which the educational concept is performed through gameplay rather than presented as a detached quiz. Teacher authoring and imports may define optional student-safe fields for:

- story context and term mission
- mission title and summary
- NPC guide keys, short dialogue, and progress responses
- Discover, Practice, Apply, and Review instructions
- hint tiers and retry policy
- optional discoveries, fun facts, and bonus content
- reflection prompt and misconception-aware feedback
- reward previews
- world restoration state keys and completion presentation metadata

These fields remain editable, auditable, publishable content records rather than hardcoded Unity data. They use the same lifecycle and approval requirements as questions and world tasks. Health-related story, NPC, hint, and feedback content must remain teacher-approved, age-appropriate, and free from personalized diagnosis, dosage, or treatment instructions.

Student API responses may expose only published, student-safe narrative, hint, discovery, reflection, reward-preview, and restoration metadata. Hidden answers, scoring logic, internal review notes, and unsafe guidance remain server-only.

### Safe Mistakes and Hint Policy

The server supports retry and hint policies that encourage learning without harsh punishment. A wrong attempt may return an approved misconception-aware message, next hint tier, remaining-attempt metadata, and safe retry action. Ordinary mistakes should not remove earned rewards, permanently block content, or create duplicate progress. The server remains authoritative for attempt limits, hint usage, scoring effects, and completion.

### Story Rewards and Exploration

The reward catalog may include coins, stars, subject crystals, badges, titles, cosmetic skins, cosmetic pets/companions, and cosmetic-area unlocks. The current Unity demo requires only coins/stars, subject-themed term crystals or badges, and visible provider-confirmed world restoration. Additional cosmetics remain optional.

Optional discoveries and fun facts are approved content records. They are not required for station completion and must not be implemented as unlimited grind or a way to bypass educational tasks.

### Scoring, Rewards, and Progress

The server owns attempt validation, idempotency, scoring, reward grants, station completion, mastery labels, progress revisions, and reward wallet state. Unity displays server feedback and never invents scores, unlocks, rewards, or progress locally.

Attempt submissions include `client_attempt_uuid` for safe retry behavior. Duplicate retries with the same UUID resolve idempotently instead of creating double scores or double rewards.

### Reports and Audit

Admin reports cover school-wide progress and usage. Teacher reports cover owned classrooms only. Audit logs cover account lifecycle, content lifecycle, AI generation/review, station unlocks, attempt submission, security events, exports, and Unity API errors through request IDs.

## Dashboard Requirements

### Admin Dashboard

| Route | Purpose |
|---|---|
| `/admin` | System overview, activity, setup progress |
| `/admin/school-years` | School year lifecycle |
| `/admin/grade-sections` | Grade 5 and Grade 6 section setup |
| `/admin/teachers` | Teacher accounts, setup emails, reset/deactivation |
| `/admin/teacher-assignments` | Teacher-to-section assignments |
| `/admin/subject-platforms` | Database-managed platform catalog, stable slugs, labels, grade availability, display state, and Unity-facing keys |
| `/admin/terms-stations` | Database-managed term, station-slot, world metadata, and stable-key catalog |
| `/admin/imports` | Validated catalog/curriculum/demo imports with preview, errors, confirmation, and history |
| `/admin/rewards` | Reward catalog, active state, grant/use rules, and Unity display metadata |
| `/admin/ai` | Provider configuration, allowlists, quotas, limits, usage visibility, and health |
| `/admin/classrooms` | Read-only classroom server overview |
| `/admin/reports` | School-wide progress and usage reports |
| `/admin/audit-logs` | Detailed lifecycle and security audit logs |
| `/admin/security` | Security center, auth events, session/token visibility, rate limits, provider health, backup/export status |
| `/admin/settings` | Profile, email, password, MFA, theme, sessions, notifications, system email settings |

### Teacher Dashboard

| Route | Purpose |
|---|---|
| `/teacher` | Classroom overview, recent attempts, unlocked stations |
| `/teacher/classrooms` | Classroom server management from assigned sections |
| `/teacher/students` | Student list, create/edit/deactivate, PIN reset |
| `/teacher/students/import` | CSV/XLSX import preview, validation, confirmation |
| `/teacher/content` | Content packs by classroom, subject, term, station |
| `/teacher/content/editor` | Full manual content editor plus optional prompt-based AI assistance with structured canonical fields and editable draft review |
| `/teacher/station-unlocks` | Station lock/unlock/schedule control |
| `/teacher/reviews` | Short-response and teacher-reviewed answers |
| `/teacher/reports` | Classroom progress, attempts, rewards, export summaries |
| `/teacher/settings` | Profile, email, password, MFA, theme, sessions, notifications |

Dashboard fields use human-readable labels. Stable IDs and slugs can exist internally, but visible choices explain the grade, section, subject, term, station, status, and ownership context.

## Unity API Contract

### Base Contract

```txt
API version: /api/v1
Unity transport: HTTPS JSON for authoritative game state
Student auth: LRN + PIN login, then Bearer token
Realtime transport: optional WebSocket for metadata events only
```

Unity does not call dashboard server functions directly. Unity uses documented JSON endpoints only.

### Student Endpoints

| Endpoint | Auth | Purpose |
|---|---:|---|
| `GET /api/v1/student/ping` | No | Lightweight connectivity check |
| `GET /api/v1/student/config` | No | Public Unity connection/config contract |
| `POST /api/v1/student/auth/login` | No | LRN + PIN login |
| `POST /api/v1/student/auth/logout` | Yes | End student session |
| `GET /api/v1/student/bootstrap` | Yes | Initial student/classroom/subject/wallet snapshot |
| `GET /api/v1/student/profile` | Yes | Student-safe profile and progress-summary data |
| `GET /api/v1/student/settings` | Yes | Game settings snapshot |
| `PATCH /api/v1/student/settings` | Yes | Persist game settings |
| `GET /api/v1/student/subjects` | Yes | Available subject cards |
| `GET /api/v1/student/subjects/{subject_slug}/terms` | Yes | Term list and world metadata |
| `GET /api/v1/student/subjects/{subject_slug}/terms/{term_number}/stations` | Yes | Station list and unlock/completion state |
| `GET /api/v1/student/stations/{station_id}/content` | Yes | Student-safe station content and world task metadata |
| `POST /api/v1/student/stations/{station_id}/start` | Yes | Start or resume station session |
| `POST /api/v1/student/challenges/{challenge_id}/attempts` | Yes | Submit answer attempt |
| `POST /api/v1/student/stations/{station_id}/complete` | Yes | Finalize station completion |
| `GET /api/v1/student/progress/summary` | Yes | Progress overview |
| `GET /api/v1/student/rewards` | Yes | Reward wallet and usable rewards |
| `POST /api/v1/student/rewards/{reward_code}/use` | Yes | Use server-approved reward |
| `GET /api/v1/student/sync/status` | Yes | Metadata-only polling revisions |

For the current Unity milestone, station-list responses provide six playable LiteraQuest stations and six playable PE/Health stations across their three terms. Science term-world metadata remains available, but Science station-list responses may intentionally return an empty `stations` array. An empty Science station list is a valid preview state, not a server error.

### Public Unity Config Endpoint

`GET /api/v1/student/config` returns a small public contract so Unity can connect without hardcoding fragile assumptions.

```json
{
  "api_version": "v1",
  "contract_version": "1.0",
  "server_time": "2026-06-05T10:30:00+08:00",
  "maintenance_mode": false,
  "minimum_unity_client_version": "0.1.0",
  "supported_languages": ["en", "tl"],
  "polling": {
    "enabled": true,
    "default_interval_seconds": 45,
    "minimum_interval_seconds": 30
  },
  "realtime": {
    "enabled": false,
    "transport": "none",
    "url": null,
    "events_are_metadata_only": true
  }
}
```

This config endpoint is public but contains no secrets. Private feature flags, AI settings, provider details, internal service health, and admin settings stay server-only.

### Auth Payloads

Login request:

```json
{
  "lrn": "123456789012",
  "pin": "123456",
  "device_name": "Unity Android",
  "client_version": "0.1.0"
}
```

Login response:

```json
{
  "token": "plain-text-token-returned-once",
  "token_type": "Bearer",
  "student": {
    "id": "stu_101",
    "name": "Juan Dela Cruz",
    "lrn_masked": "1234••••9012",
    "grade_level": 5,
    "language_preference": "en"
  }
}
```

The token is returned only at login. Stored token hashes or token metadata are not exposed in dashboards or APIs.

### World Metadata

Every term and station response includes world identity for Unity scene loading:

```txt
world_theme_key
world_title
unity_scene_key
unity_scene_name
scene_address_key
environment_tags
mechanic_family
```

The server returns scene identity. Unity owns the local scene asset. Missing local scene keys are handled by Unity as a world-unavailable state, not by loading a wrong fallback world.

### Sync Status

`GET /api/v1/student/sync/status` returns metadata only:

```json
{
  "student_progress_revision": "prog_20260605_000045",
  "student_settings_revision": "settings_20260605_000010",
  "station_unlock_revision": "unlock_20260605_000012",
  "published_content_revision": "content_20260605_000021",
  "reward_wallet_revision": "wallet_20260605_000030",
  "server_time": "2026-06-05T10:22:00+08:00",
  "next_poll_after_seconds": 45
}
```

Revision values are opaque strings. Unity compares equality and fetches only the changed data area.

### Error Format

Every non-success Unity API response uses the same safe shape:

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

Canonical error codes (the complete set the Unity HTTP provider treats as known/safe; any unknown code is rendered with a safe generic message). Server-issued envelope codes:

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

The Unity client additionally recognizes these client-side codes for transport/local failures so its safe-error handling stays uniform (the server never emits them):

```txt
NETWORK_ERROR
CONFIGURATION_ERROR
PROVIDER_DISPOSED
INVALID_RESPONSE
UNKNOWN_ERROR
```

The optional `action` field suggests the client behavior. The canonical action set is:

```txt
login_again
retry
wait_then_retry
refresh_sync_status
return_to_menu
show_offline_prompt
contact_teacher
```

Any unrecognized `action` value maps to a safe `Unknown` action on the client and never crashes it.

The `request_id` is present on all errors and can be shown by Unity in support details. Student-facing messages remain friendly and do not expose stack traces, SQL errors, provider errors, secrets, token values, answer keys, or hidden scoring rules.

## Canonical Unity Data Contract Schemas

These are the canonical, snake_case JSON field names for the student-safe payloads shared by the server (`/api/v1`) and the Unity client. They mirror the Unity DTO layer field-for-field and are the single source of truth used by both `docs/unity/04_SERVER_CONNECTION_AND_UNITY_API.md` and `docs/unity/11_DEMO_DATA_AND_LOCAL_PROVIDER.md`. All listed fields are optional/additive unless noted; unknown fields and unknown enum values must be ignored safely. None of these fields may carry answer keys, hidden scoring, secrets, tokens, PINs, or private notes.

### World Metadata (term and station scope)

```txt
world_theme_key
world_title
unity_scene_key
unity_scene_name
scene_address_key
environment_tags[]
mechanic_family
```

### Station Content and Mission

Returned by `GET /api/v1/student/stations/{station_id}/content`. Core identity/content fields plus optional learning-gameplay and narrative fields:

```jsonc
{
  "station_id": "...",
  "station_key": "...",
  "subject_slug": "...",
  "grade_level": 5,
  "term_number": 1,
  "title": "...",
  "description": "...",
  "learning_skill": "...",
  "student_learning_goal": "...",
  "instructions": "...",
  "completion_rule": { "type": "complete_required_challenges", "required_count": 1 },
  "world_tasks": [ /* WorldTask */ ],
  "challenges": [ /* Challenge */ ],
  "content_revision": "...",
  "world_metadata": { /* World Metadata */ },
  "story_context": "...",
  "mission_title": "...",
  "mission_summary": "...",
  "npc_guides": [ /* NpcGuide */ ],
  "learning_cycle": { /* LearningCycle object */ },
  "hint_policy": { /* HintPolicy */ },
  "discoveries": [ /* Discovery */ ],
  "reflection_prompt": "...",
  "reward_preview": [ /* RewardPreview */ ],
  "world_restoration_state": { /* WorldRestorationState */ },
  "success_feedback": { /* SuccessFeedback */ }
}
```

World task:

```txt
task_id
task_key
task_type
portal_key
interactable_key
prefab_key
world_position_hint
challenge_id
required
```

NPC guide:

```txt
npc_key
display_name
role
avatar_key
intro_dialogue
completion_dialogue
```

Learning cycle is a canonical **object** (one short, student-safe guidance string per phase), not an array of strings:

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
    "tiers": [ { "tier": 1, "text": "..." }, { "tier": 2, "text": "..." } ]
  }
}
```

Discovery (always optional; never required for completion):

```txt
discovery_key
type
title
description
optional
reward_preview   (single RewardPreview)
```

Reflection prompt is a single string field: `reflection_prompt`.

Reward preview (presentation/motivation only — not an earned reward):

```txt
code
reward_key
reward_type
display_name
icon_key
quantity
grant_scope        (e.g. "term_after_both_stations" for a subject crystal)
```

Success feedback:

```json
{
  "success_feedback": {
    "message": "...",
    "encouraging_phrases": ["...", "..."]
  }
}
```

World restoration state (authored within station content; applied only after an accepted completion):

```txt
state_key
apply_after_accepted_completion
state_data
```

### Attempt Result

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
  "progress": {
    "completed_challenges": 1,
    "required_challenges": 1,
    "station_progress_percent": 100
  },
  "rewards_granted": [ /* RewardGrant */ ],
  "progress_updated": true,
  "progress_revision": "...",
  "reward_wallet_revision": "..."
}
```

Reward grant:

```txt
reward_code
reward_type
display_name
quantity
```

### Station Completion Result

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
    "crystal": { /* RewardGrant */ },
    "badge":   { /* RewardGrant */ }
  },
  "rewards_granted": [ /* RewardGrant */ ],
  "world_restoration_result": { "state_key": "...", "restored": true },
  "progress_summary": { /* ProgressSummary */ },
  "progress_revision": "...",
  "reward_wallet_revision": "..."
}
```

When both required stations in a term are complete, `term_completion.completed` is true and the provider may grant a subject-themed `crystal` and/or `badge` (each a reward grant). `world_restoration_result` is the provider-confirmed restoration; Unity owns only the animation, not the state authority.

### Idempotent Replay Semantics

Attempts carry `client_attempt_uuid` and completions are keyed by station. A duplicate `client_attempt_uuid` (or a re-completion of an already-completed station) resolves idempotently: the server returns the **same** result with `is_replay = true` and never double-scores, double-grants rewards, or re-awards a term crystal/badge. Ordinary learning mistakes do not remove earned rewards or permanently block content.

### Station List Scope

`GET /api/v1/student/subjects/{subject_slug}/terms/{term_number}/stations` returns an object (not a bare array):

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

Science exploration-preview terms return an empty `stations` array with `preview_mode = "exploration_only"`. An empty list is a valid no-station preview state, not an error.

## Realtime Connection Strategy

HTTP remains authoritative for login, bootstrap, content, attempts, scoring, rewards, settings, progress, and reports.

WebSocket is allowed for realtime metadata events that help Unity refresh safely without aggressive polling. WebSocket events do not carry answer keys, full station content, AI draft data, private notes, scoring rules, tokens, PINs, or private audit data.

### WebSocket Endpoint

```txt
wss://{server-host}/api/v1/student/realtime
```

Authentication uses the student bearer token during the connection handshake or a short-lived realtime ticket issued after login. A realtime ticket is scoped to one authenticated student session and expires quickly.

### WebSocket Event Envelope

```json
{
  "event_id": "evt_01HYEXAMPLE123456789",
  "event_type": "station_unlock_changed",
  "contract_version": "1.0",
  "server_time": "2026-06-05T10:30:00+08:00",
  "student_scope": {
    "student_id": "stu_101",
    "classroom_id": "classroom_7"
  },
  "revisions": {
    "station_unlock_revision": "unlock_20260605_000013"
  },
  "action": "refresh_sync_status"
}
```

Supported event types:

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

Unity treats WebSocket events as hints. After receiving an event, Unity refreshes through HTTP or sync status. WebSocket messages are never treated as scoring authority.

### WebSocket Reliability Rules

- Unknown event types are ignored safely.
- Unknown fields are ignored safely.
- Missing required event envelope fields make the event invalid and non-fatal.
- Reconnect uses backoff and does not block gameplay screens that already have safe data.
- Heartbeat/ping keeps the connection alive when available.
- Session revocation closes Unity session and returns to login through the same safe error UX as `TOKEN_EXPIRED` or `UNAUTHENTICATED`.
- If realtime is unavailable, Unity falls back to polling without changing game rules.

## Server-Agent Configuration Contract

The server agent’s configuration surface is stable and environment-based unless explicitly listed as database-managed settings.

### Environment Configuration

```txt
APP_URL
PUBLIC_SITE_URL
DATABASE_URL
BETTER_AUTH_SECRET
BETTER_AUTH_URL
REDIS_URL or UPSTASH_REDIS_REST_URL / UPSTASH_REDIS_REST_TOKEN
RESEND_API_KEY
RESEND_FROM_EMAIL
UNITY_API_BASE_URL
UNITY_REALTIME_URL
UNITY_REALTIME_ENABLED
UNITY_MIN_CLIENT_VERSION
UNITY_CONTRACT_VERSION
RATE_LIMIT_NAMESPACE
LOG_LEVEL
NODE_ENV
```

AI provider keys are database-managed private settings, not Unity or public environment values. Admin default AI settings and teacher override settings are resolved server-side and never sent to Unity.

### Contract Compatibility

The server treats `/api/v1` as the stable Unity contract. Additive optional fields are compatible. Removed fields, renamed fields, changed enum meanings, changed error codes, changed reward/scoring semantics, and changed world metadata are breaking changes.

The server exposes `contract_version` and `minimum_unity_client_version` through `/api/v1/student/config`. Unity can show a friendly update-required or server-incompatible message instead of crashing on incompatible responses.

## Privacy and Safety Boundaries

The server never returns these to Unity:

```txt
answer keys
correct choice/order/grouping before submission
hidden scoring rules
AI settings or provider keys
AI draft questions
rejected questions
teacher review notes
admin security data
private audit logs
raw tokens
PINs
raw LRN where masked LRN is enough
stack traces
SQL errors
raw exception messages
```

Sensitive data in logs and exports is redacted. Request IDs connect Unity support reports to server logs without exposing internals to students.

## Requirement Gaps Resolved in This Version

The cleaned requirements add or clarify these missing cross-side requirements:

| Gap | Requirement added |
|---|---|
| Unity could hardcode server assumptions | Public `/api/v1/student/config` contract endpoint |
| Polling alone may be slow for unlock/content changes | Optional metadata-only WebSocket event channel |
| WebSocket could destabilize Unity | WebSocket is hint-only; HTTP remains authoritative; fallback to polling |
| Attempts could duplicate on retry | `client_attempt_uuid` idempotency requirement |
| API changes could crash old clients | `contract_version`, `minimum_unity_client_version`, safe additive field policy |
| Missing local Unity scene could load wrong world | Required world metadata and explicit `WORLD_SCENE_UNAVAILABLE` path |
| Error handling could differ by endpoint | Unified Unity API error shape with stable codes and request IDs |
| AI content could reach students too early | Draft-only AI generation and teacher approval gate |
| Teachers could be blocked by AI cost or availability | Complete manual content editor remains required |
| AI form could force teachers to encode intent awkwardly | Prompt-based AI assistance plus structured canonical fields and editable draft review |
| Catalog data could be hidden in seeds/source arrays | Database-managed admin/import/teacher workflows and minimal bootstrap-only seed policy |
| Private server fields could leak to Unity | No-hidden-data boundary repeated in API, profile, settings, content, sync, realtime |

## Current Non-Goals

These are outside the active requirement set unless formally accepted later:

```txt
parent or guardian portal
student-facing AI chat or generation
multiplayer gameplay
full offline progression with later sync
implementing deferred Science station gameplay or additional station slots beyond the approved 12-station Unity demo
WebSocket as scoring/content authority
admin editing teacher classroom content directly
```

## Server Completion Definition

The server side is complete when the platform supports the full school/classroom/content hierarchy, secure admin and teacher dashboards, AI draft generation with teacher approval, Unity `/api/v1` and optional realtime metadata, safe scoring and rewards, reports, audit/security visibility, and all privacy boundaries needed for the Unity client to run without receiving hidden or unsafe data.
