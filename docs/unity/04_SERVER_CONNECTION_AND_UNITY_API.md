# Unity API Contract — Laravel Quiz-First Server

## Backend

The backend is Laravel + PostgreSQL and exposes REST JSON endpoints.

No WebSocket is required.

## Active Endpoints

```http
GET /api/v1/student/config
POST /api/v1/student/auth/login
POST /api/v1/student/auth/logout
POST /api/v1/student/auth/refresh
GET /api/v1/student/bootstrap
GET /api/v1/student/profile
PATCH /api/v1/student/settings
GET /api/v1/student/subjects
GET /api/v1/student/subjects/{subject_slug}/terms
GET /api/v1/student/quizzes
GET /api/v1/student/quizzes/{quiz_id}
POST /api/v1/student/quizzes/{quiz_id}/attempts
GET /api/v1/student/quiz-results
GET /api/v1/student/quiz-results/{attempt_id}
GET /api/v1/student/sync-status
```

## Deferred Endpoints

- mission endpoints
- rewards/shop endpoints
- inventory endpoints

## Contract Version

Use:

```text
quiz_first_laravel_1
```

## Quiz Attempt Idempotency

Use `client_attempt_uuid`.

Same UUID + same payload = replay result.

Same UUID + different payload = conflict.
