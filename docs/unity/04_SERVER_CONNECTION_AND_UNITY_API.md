# Unity Server Connection and API Contract — v6

## Backend

The backend is Laravel + React/Inertia + PostgreSQL.

Unity uses HTTPS REST JSON APIs.

No WebSocket is required for the current milestone.

## Contract version

```text
quiz_first_laravel_1
```

## Active endpoints

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

## Quiz list query behavior

`GET /api/v1/student/quizzes` should support optional filters:

- subject
- term
- status
- completed
- locked

The default response should return all quizzes assigned to the student.

## Quiz summary fields

Each quiz summary should include:

- quiz id
- title
- subject
- term
- grade level
- item count
- status
- locked reason if any
- action availability
- time limit if any
- attempt status if any
- result summary if completed and visible

## Compatibility field

The server should provide item/presenter compatibility metadata so Unity can avoid starting quizzes that require unsupported item types unless safe fallback is intentionally allowed.

## Active Unity-supported item types

- `multiple_choice_single`
- `multiple_choice_multiple`
- `true_false`

Unsupported types must render safe fallback and must not crash.

## Deferred endpoints

Mission, reward shop, inventory, and world restoration endpoints are deferred.
