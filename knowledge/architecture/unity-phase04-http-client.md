# Unity Phase 04 HTTP/API Client

Status: approved after Phase 04 validation on 2026-06-14.

## Scope

Unity now has a shared server-facing client layer for the student API under `Assets/_Project/Nutrimind/Runtime/App/`:

- `IGameDataProvider` exposes async, cancellation-aware methods for config, auth, bootstrap, profile, settings, catalog, station content, attempts, completion, progress, rewards, and sync status.
- `HttpProvider` implements the HTTPS REST provider using an injectable `IHttpRequestTransport`; production transport is `UnityWebRequestTransport`.
- `LocalDemoJsonProvider` remains a placeholder and must not be used as a production fallback.
- DTOs live in `Runtime/App/Dto/` and use explicit `Newtonsoft.Json` snake_case mapping plus safe enum/unknown-field handling.
- `HttpProviderConfig` holds base URL, API prefix, timeout, retry, and polling settings. It rejects non-HTTPS base URLs.
- `AuthSessionState` stores bearer token metadata and student identity returned by login.
- `SyncPollingService` polls sync revisions and dispatches events via `MainThreadDispatcher`; `CompositionRoot` installs a hidden dispatcher pump in HTTP mode.
- Optional realtime support is metadata-only/no-op unless a real WSS transport is added later.

## Contract and safety rules

- Server remains authoritative for identity, content, availability, scoring, rewards, progress, reports, and sync revisions.
- `HttpProvider` does not silently fall back to local/demo data.
- Release builds must not use `DataProviderMode.LocalDemoJson`; `CompositionRoot` defaults release builds to `Http` and guards explicit LocalDemoJson selection outside editor/development builds.
- Attempt retries send the same `client_attempt_uuid`.
- Automatic retries are limited to safe operations: GET and explicitly idempotent POSTs. Settings PATCH is not automatically retried.
- Unknown JSON fields are ignored, and unknown enum strings map to safe defaults rather than crashing.
- Unknown server error codes preserve `code` and `request_id` but use a generic student-safe message.
- Error `details` and `field_errors` are recursively sanitized; sensitive keys such as `answer_key` and `correct_answer` are redacted.
- Logs must not include PINs, bearer tokens, Authorization headers, raw response bodies, answer keys, SQL/provider internals, or stack traces.

## Validation evidence

- Unity Editor currently reports `6000.5.0f1`.
- Package manifest requests `com.unity.nuget.newtonsoft-json` version `3.2.2`; `packages-lock.json` also has a transitive `com.unity.ai.assistant` dependency entry on `3.2.1`, so package-resolution drift should be watched when Unity re-resolves packages.
- Unity compilation check on 2026-06-17: `unity_get_compilation_errors` returned `count: 0`.
- Missing-reference checks on 2026-06-17: scene scope `0`, asset scope `0`.
- EditMode App test job `fc0f392ab756`: 514 total, 513 passed, 1 failed, 0 skipped, duration 8.96s. The remaining failure is `CompositionRootModeConfigTests.CreateForMode_Http_CreatesDispatcherPump` on dispatcher-pump `HideAndDontSave` behavior and is unrelated to the reviewed API contract changes.
- Focused regression coverage now includes disposed-root replacement in `CompositionRoot.CreateForMode` and parameterized documented safe server error-code message preservation/redaction in `HttpProviderTests`.

## Known limitations

- No live dev-server integration test was run in this phase.
- No PlayMode or Android build smoke was run in this phase.
- Realtime remains optional metadata-only/no-op.
- `HttpProviderConfig.BaseUrl` must still be assigned by the app/bootstrap configuration before HTTP mode can contact a real server.
