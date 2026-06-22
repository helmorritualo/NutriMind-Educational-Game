using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NutriMind.Runtime.App.Dto;
using NutriMind.Runtime.App.Http;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// HTTPS JSON implementation of <see cref="IGameDataProvider"/> for the
    /// NutriMind student API.  All 19 contract methods are supported.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The provider owns no Unity objects and can be unit-tested with a fake
    /// <see cref="IHttpRequestTransport"/>.
    /// </para>
    /// <para>
    /// Authentication state lives in the injected <see cref="AuthSessionState"/>.
    /// Login stores the bearer token; logout clears it.  Authenticated calls
    /// fail early with <c>UNAUTHENTICATED</c> when no token is present.
    /// </para>
    /// <para>
    /// Retry is limited to safe/idempotent operations: GET and POST calls
    /// that carry <c>client_attempt_uuid</c>.  Server-declared retryable errors
    /// and <c>retry_after_seconds</c> are respected.  PATCH is not automatically
    /// retried (requires explicit idempotent opt-in).
    /// </para>
    /// </remarks>
    public class HttpProvider : IGameDataProvider, IDisposable
    {
        private readonly HttpProviderConfig _config;
        private readonly AuthSessionState _session;
        private readonly IHttpRequestTransport _transport;
        private bool _disposed;

        /// <summary>
        /// Known safe error codes for which we keep the server-provided message
        /// (after redaction).  Unknown codes get a generic student-safe message.
        /// </summary>
        /// <summary>
        /// Keys in <see cref="DataProviderError.Details"/> or
        /// <see cref="DataProviderError.FieldErrors"/> that may leak
        /// student-unsafe content (answer keys, correct answers, etc.)
        /// and must be replaced with a redacted placeholder.
        /// Comparison is case-insensitive.
        /// </summary>
        private static readonly HashSet<string> SensitiveDetailKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "answer_key",
            "correct_answer"
        };

        /// <summary>
        /// Known safe error codes for which we keep the server-provided message
        /// (after redaction).  Includes the documented server envelope codes
        /// plus client-side codes.  Unknown codes get a generic student-safe message.
        /// </summary>
        private static readonly HashSet<string> KnownSafeErrorCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            // Documented server envelope codes (see docs/SERVER_REQUIREMENTS.md)
            "UNAUTHENTICATED",
            "TOKEN_EXPIRED",
            "STUDENT_INACTIVE",
            "VALIDATION_ERROR",
            "RATE_LIMITED",
            "SYNC_RATE_LIMITED",
            "SERVER_UNAVAILABLE",
            "SERVER_TIMEOUT",
            "MAINTENANCE_MODE",
            "QUIZ_LOCKED",
            "STATION_LOCKED",
            "STATION_ALREADY_COMPLETED",
            "CONTENT_NOT_PUBLISHED",
            "SESSION_NOT_FOUND",
            "SESSION_FORBIDDEN",
            "WORLD_SCENE_UNAVAILABLE",
            "STALE_CONTENT",
            "CLIENT_VERSION_UNSUPPORTED",
            "CONFIG_VERSION_UNSUPPORTED",
            "REALTIME_UNAVAILABLE",
            "AI_NOT_CONFIGURED",
            "NOT_FOUND",
            // Client-side codes
            "NETWORK_ERROR",
            "CONFIGURATION_ERROR",
            "PROVIDER_DISPOSED",
            "INVALID_RESPONSE",
            "UNKNOWN_ERROR",
        };

        /// <summary>
        /// Creates a new HTTP provider with the supplied configuration and
        /// session state.  Uses <see cref="UnityWebRequestTransport"/> when no
        /// transport is provided.
        /// </summary>
        public HttpProvider(HttpProviderConfig config, AuthSessionState session, IHttpRequestTransport transport = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _transport = transport ?? new UnityWebRequestTransport();
        }

        /// <summary>Sanitizes and logs a request without exposing secrets.</summary>
        private static void LogRequest(string method, string url)
        {
            SafeDiagnostics.LogDiagnostic($"HTTP {method} {RedactUrl(url)}");
        }

        /// <summary>Sanitizes and logs a response summary without exposing bodies.</summary>
        private static void LogResponse(string method, string url, long statusCode)
        {
            SafeDiagnostics.LogDiagnostic($"HTTP {method} {RedactUrl(url)} -> {statusCode}");
        }

        /// <summary>Removes query strings from URLs before logging.</summary>
        private static string RedactUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;
            int idx = url.IndexOf('?');
            return idx >= 0 ? url.Substring(0, idx) + "?***" : url;
        }

        /// <summary>
        /// Ensures the configuration is valid before issuing a request.
        /// </summary>
        private DataResult<T>? ValidateConfig<T>()
        {
            var error = _config.Validate();
            if (error != null)
                return DataResult<T>.Fail(error);
            return null;
        }

        /// <summary>
        /// Ensures a bearer token exists for authenticated requests.
        /// </summary>
        private DataResult<T>? ValidateAuth<T>()
        {
            if (string.IsNullOrEmpty(_session.Token))
            {
                return DataResult<T>.Fail(new DataProviderError(
                    "UNAUTHENTICATED",
                    "You need to sign in again.")
                {
                    Action = "login_again"
                });
            }
            return null;
        }

        // ──────────────────────────────────────────────────────────────
        //  Connectivity & Config
        // ──────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public Task<DataResult<PingResponseDto>> PingAsync(CancellationToken ct = default)
            => GetAsync<PingResponseDto>("/student/ping", auth: false, ct: ct);

        /// <inheritdoc />
        public Task<DataResult<ApiConfigDto>> GetConfigAsync(CancellationToken ct = default)
            => GetAsync<ApiConfigDto>("/student/config", auth: false, ct: ct);

        // ──────────────────────────────────────────────────────────────
        //  Auth
        // ──────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<DataResult<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
        {
            if (request == null)
                return DataResult<LoginResponseDto>.Fail(new DataProviderError("VALIDATION_ERROR", "Login request is required."));

            var result = await PostAsync<LoginResponseDto>("/student/auth/login", request, auth: false, idempotent: false, ct: ct)
                .ConfigureAwait(false);

            if (result.Success && result.Data != null && !string.IsNullOrEmpty(result.Data.Token))
            {
                _session.ApplyLoginResponse(result.Data);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<DataResult<object>> LogoutAsync(CancellationToken ct = default)
        {
            if (!string.IsNullOrEmpty(_session.Token))
            {
                // Best-effort server logout; local state is cleared regardless.
                _ = await PostAsync<object>("/student/auth/logout", new LogoutRequestDto(), auth: true, idempotent: true, ct: ct)
                    .ConfigureAwait(false);
            }

            _session.Reset();
            return DataResult<object>.Ok(new object());
        }

        // ──────────────────────────────────────────────────────────────
        //  Bootstrap & Profile
        // ──────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public Task<DataResult<BootstrapDto>> GetBootstrapAsync(CancellationToken ct = default)
            => GetAsync<BootstrapDto>("/student/bootstrap", auth: true, ct: ct);

        /// <inheritdoc />
        public Task<DataResult<StudentProfileDto>> GetProfileAsync(CancellationToken ct = default)
            => GetAsync<StudentProfileDto>("/student/profile", auth: true, ct: ct);

        // ──────────────────────────────────────────────────────────────
        //  Settings
        // ──────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public Task<DataResult<SettingsDto>> GetSettingsAsync(CancellationToken ct = default)
            => GetAsync<SettingsDto>("/student/settings", auth: true, ct: ct);

        /// <inheritdoc />
        public Task<DataResult<SettingsDto>> PatchSettingsAsync(SettingsDto settings, CancellationToken ct = default)
            => PatchAsync<SettingsDto>("/student/settings", settings, auth: true, ct: ct);

        // ──────────────────────────────────────────────────────────────
        //  Subjects, Terms, Stations
        // ──────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public Task<DataResult<List<SubjectDto>>> GetSubjectsAsync(CancellationToken ct = default)
            => GetAsync<List<SubjectDto>>("/student/subjects", auth: true, ct: ct);

        /// <inheritdoc />
        public Task<DataResult<List<TermDto>>> GetTermsAsync(string subjectSlug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(subjectSlug))
                return Task.FromResult(DataResult<List<TermDto>>.Fail(new DataProviderError("VALIDATION_ERROR", "Subject slug is required.")));

            return GetAsync<List<TermDto>>($"/student/subjects/{Uri.EscapeDataString(subjectSlug)}/terms", auth: true, ct: ct);
        }

        /// <inheritdoc />
        public Task<DataResult<QuizListDto>> GetQuizzesAsync(string subjectSlug, int termNumber, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(subjectSlug))
                return Task.FromResult(DataResult<QuizListDto>.Fail(new DataProviderError("VALIDATION_ERROR", "Subject slug is required.")));

            return GetAsync<QuizListDto>(
                $"/student/quizzes?subject_slug={Uri.EscapeDataString(subjectSlug)}&term_number={termNumber}",
                auth: true, ct: ct);
        }

        // ──────────────────────────────────────────────────────────────
        //  Quiz & Assessment System (Laravel quiz_first_laravel_1 REST)
        // ──────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public Task<DataResult<QuizDetailDto>> GetQuizDetailAsync(string quizId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(quizId))
                return Task.FromResult(DataResult<QuizDetailDto>.Fail(new DataProviderError("VALIDATION_ERROR", "Quiz ID is required.")));

            return GetAsync<QuizDetailDto>($"/student/quizzes/{Uri.EscapeDataString(quizId)}", auth: true, ct: ct);
        }

        /// <inheritdoc />
        public Task<DataResult<QuizAttemptResponseDto>> SubmitQuizAttemptAsync(string quizId, QuizAttemptRequestDto request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(quizId))
                return Task.FromResult(DataResult<QuizAttemptResponseDto>.Fail(new DataProviderError("VALIDATION_ERROR", "Quiz ID is required.")));

            if (request == null)
                return Task.FromResult(DataResult<QuizAttemptResponseDto>.Fail(new DataProviderError("VALIDATION_ERROR", "Attempt request is required.")));

            // Attempts are idempotent via client_attempt_uuid, so retries are safe.
            return PostAsync<QuizAttemptResponseDto>(
                $"/student/quizzes/{Uri.EscapeDataString(quizId)}/attempts",
                request,
                auth: true,
                idempotent: true,
                ct: ct);
        }

        /// <inheritdoc />
        public Task<DataResult<QuizResultListDto>> GetQuizResultsAsync(CancellationToken ct = default)
            => GetAsync<QuizResultListDto>("/student/quiz-results", auth: true, ct: ct);

        /// <inheritdoc />
        public Task<DataResult<QuizResultDto>> GetQuizResultAsync(string attemptId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(attemptId))
                return Task.FromResult(DataResult<QuizResultDto>.Fail(new DataProviderError("VALIDATION_ERROR", "Attempt ID is required.")));

            return GetAsync<QuizResultDto>($"/student/quiz-results/{Uri.EscapeDataString(attemptId)}", auth: true, ct: ct);
        }

        // ──────────────────────────────────────────────────────────────
        //  Progress & Rewards
        // ──────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public Task<DataResult<ProgressSummaryDto>> GetProgressSummaryAsync(CancellationToken ct = default)
            => GetAsync<ProgressSummaryDto>("/student/progress/summary", auth: true, ct: ct);

        /// <inheritdoc />
        public Task<DataResult<RewardWalletDto>> GetRewardsAsync(CancellationToken ct = default)
            => GetAsync<RewardWalletDto>("/student/rewards", auth: true, ct: ct);

        /// <inheritdoc />
        public Task<DataResult<UseRewardResponseDto>> UseRewardAsync(string rewardCode, UseRewardRequestDto request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(rewardCode))
                return Task.FromResult(DataResult<UseRewardResponseDto>.Fail(new DataProviderError("VALIDATION_ERROR", "Reward code is required.")));

            return PostAsync<UseRewardResponseDto>(
                $"/student/rewards/{Uri.EscapeDataString(rewardCode)}/use",
                request,
                auth: true,
                idempotent: false,
                ct: ct);
        }

        // ──────────────────────────────────────────────────────────────
        //  Sync
        // ──────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public Task<DataResult<SyncStatusDto>> GetSyncStatusAsync(CancellationToken ct = default)
            => GetAsync<SyncStatusDto>("/student/sync/status", auth: true, ct: ct);

        // ──────────────────────────────────────────────────────────────
        //  Core request helpers
        // ──────────────────────────────────────────────────────────────

        private Task<DataResult<T>> GetAsync<T>(string relativePath, bool auth, CancellationToken ct)
            => SendWithRetryAsync<T>("GET", relativePath, body: null, auth, idempotent: true, ct);

        private Task<DataResult<T>> PostAsync<T>(string relativePath, object body, bool auth, bool idempotent, CancellationToken ct)
            => SendWithRetryAsync<T>("POST", relativePath, body, auth, idempotent, ct);

        private Task<DataResult<T>> PatchAsync<T>(string relativePath, object body, bool auth, CancellationToken ct)
            => SendWithRetryAsync<T>("PATCH", relativePath, body, auth, idempotent: false, ct);

        private async Task<DataResult<T>> SendWithRetryAsync<T>(
            string method,
            string relativePath,
            object body,
            bool auth,
            bool idempotent,
            CancellationToken ct)
        {
            if (_disposed)
                return DataResult<T>.Fail(new DataProviderError("PROVIDER_DISPOSED", "HTTP provider has been disposed."));

            if (ValidateConfig<T>() is { } configError)
                return configError;

            if (auth && ValidateAuth<T>() is { } authError)
                return authError;

            string url = _config.BuildUrl(relativePath);
            string? bodyJson = body == null ? null : JsonConvert.SerializeObject(body, JsonSettings.SafeDefaults);

            // PATCH is NOT automatically retried (Phase 04 requirement).
            // Only GET and explicitly idempotent operations retry.
            bool canRetry = method == "GET" || idempotent;
            int maxAttempts = 1 + (canRetry ? Math.Max(0, _config.MaxRetries) : 0);

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                var request = BuildRequest(method, url, bodyJson, auth);
                LogRequest(method, url);

                HttpTransportResponse response;
                try
                {
                    response = await _transport.SendAsync(request, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    SafeDiagnostics.LogDiagnostic(
                        $"Transport exception for {method} {RedactUrl(url)}: {SafeDiagnostics.SanitizeDiagnostics(ex.Message)}");
                    return DataResult<T>.Fail(MapTransportError(ex));
                }

                LogResponse(method, url, response.StatusCode);

                if (!response.IsNetworkError && !response.IsHttpError)
                {
                    return DeserializeSuccess<T>(response.Body);
                }

                DataProviderError error = MapHttpError(response);

                bool isLastAttempt = attempt == maxAttempts - 1;
                if (isLastAttempt)
                    return DataResult<T>.Fail(error);

                bool shouldRetry = canRetry && IsRetryable(response, error);
                if (!shouldRetry)
                    return DataResult<T>.Fail(error);

                int delayMs = ComputeRetryDelayMs(error, attempt);
                SafeDiagnostics.LogDiagnostic(
                    $"Retrying {method} {RedactUrl(url)} after {delayMs}ms (attempt {attempt + 1}/{maxAttempts - 1})");
                await Task.Delay(delayMs, ct).ConfigureAwait(false);
            }

            return DataResult<T>.Fail(new DataProviderError("UNKNOWN_ERROR", "An unexpected error occurred."));
        }

        private HttpTransportRequest BuildRequest(string method, string url, string? bodyJson, bool auth)
        {
            var request = new HttpTransportRequest
            {
                Method = method,
                Url = url,
                BodyJson = bodyJson,
                TimeoutSeconds = _config.TimeoutSeconds
            };

            request.Headers["Accept"] = "application/json";
            if (!string.IsNullOrEmpty(bodyJson))
                request.Headers["Content-Type"] = "application/json";

            if (auth && !string.IsNullOrEmpty(_session.Token))
                request.Headers["Authorization"] = $"Bearer {_session.Token}";

            return request;
        }

        private DataResult<T> DeserializeSuccess<T>(string body)
        {
            try
            {
                T data = JsonConvert.DeserializeObject<T>(body, JsonSettings.SafeDefaults);
                return DataResult<T>.Ok(data);
            }
            catch (Exception ex)
            {
                SafeDiagnostics.LogDiagnostic($"JSON deserialization failed: {SafeDiagnostics.SanitizeDiagnostics(ex.Message)}");
                return DataResult<T>.Fail(new DataProviderError(
                    "INVALID_RESPONSE",
                    "The server returned data we could not understand."));
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Error mapping
        // ──────────────────────────────────────────────────────────────

        private DataProviderError MapHttpError(HttpTransportResponse response)
        {
            if (TryParseErrorEnvelope(response.Body, out var envelope))
                return envelope;

            string code;
            string message;

            switch (response.StatusCode)
            {
                case 400:
                    code = "VALIDATION_ERROR";
                    message = "Some information was not accepted. Please try again.";
                    break;
                case 401:
                    code = "UNAUTHENTICATED";
                    message = "Your session has ended. Please sign in again.";
                    break;
                case 403:
                    code = "SESSION_FORBIDDEN";
                    message = "You do not have permission to do that.";
                    break;
                case 404:
                    code = "NOT_FOUND";
                    message = "The requested item could not be found.";
                    break;
                case 408:
                    code = "SERVER_TIMEOUT";
                    message = "The server took too long to respond. Please try again.";
                    break;
                case 422:
                    code = "VALIDATION_ERROR";
                    message = "Some information was not accepted. Please try again.";
                    break;
                case 429:
                    code = "RATE_LIMITED";
                    message = "Too many requests. Please wait a moment.";
                    break;
                case >= 500 and <= 599:
                    code = "SERVER_UNAVAILABLE";
                    message = "The server is temporarily unavailable. Please try again.";
                    break;
                default:
                    code = "NETWORK_ERROR";
                    message = "Could not connect to the server. Please check your internet.";
                    break;
            }

            return new DataProviderError(code, message);
        }

        private DataProviderError MapTransportError(Exception ex)
        {
            string code = ex switch
            {
                TaskCanceledException or OperationCanceledException => "SERVER_TIMEOUT",
                TimeoutException => "SERVER_TIMEOUT",
                System.Net.WebException webEx when webEx.Status == WebExceptionStatus.Timeout => "SERVER_TIMEOUT",
                _ => "NETWORK_ERROR"
            };

            string message = code == "SERVER_TIMEOUT"
                ? "The server took too long to respond. Please try again."
                : "Could not connect to the server. Please check your internet.";

            return new DataProviderError(code, message);
        }

        /// <summary>
        /// Set of error codes the client explicitly recognises.
        /// Messages for unrecognised codes are replaced with a generic
        /// student-safe message.  Also redacts tokens, PINs, answer keys,
        /// stack traces, and SQL from all envelope fields.
        /// </summary>
        private bool TryParseErrorEnvelope(string body, out DataProviderError error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(body))
                return false;

            try
            {
                error = JsonConvert.DeserializeObject<DataProviderError>(body, JsonSettings.SafeDefaults);
                if (error == null)
                    return false;

                if (string.IsNullOrEmpty(error.Code))
                    error.Code = "unknown";

                // Sanitise structured fields (Details, FieldErrors) first.
                SanitizeStructuredErrorContent(error);

                // Apply comprehensive redaction to the message.
                if (!string.IsNullOrEmpty(error.Message))
                {
                    error.Message = SafeDiagnostics.RedactAll(error.Message);
                }

                // For unknown error codes, replace with generic safe message.
                if (!KnownSafeErrorCodes.Contains(error.Code))
                {
                    error.Message = "An unexpected error occurred.";
                }

                // Ensure message is never empty.
                if (string.IsNullOrEmpty(error.Message))
                {
                    error.Message = "An unexpected error occurred.";
                }

                return true;
            }
            catch (Exception ex)
            {
                SafeDiagnostics.LogDiagnostic(
                    $"Failed to parse error envelope: {SafeDiagnostics.SanitizeDiagnostics(ex.Message)}");
                return false;
            }
        }

        /// <summary>
        /// Sanitises <see cref="DataProviderError.Details"/> and
        /// <see cref="DataProviderError.FieldErrors"/> by applying
        /// <see cref="SafeDiagnostics.RedactAll"/> to all string values.
        /// Complex/nested values are serialised to string, redacted, and
        /// stored back; primitive values are left as-is.
        /// </summary>
        private static void SanitizeStructuredErrorContent(DataProviderError error)
        {
            if (error.Details != null)
            {
                error.Details = SanitizeObject(error.Details) as Dictionary<string, object>
                    ?? error.Details;
            }

            if (error.FieldErrors != null)
            {
                error.FieldErrors = SanitizeObject(error.FieldErrors) as Dictionary<string, object>
                    ?? error.FieldErrors;
            }
        }

        /// <summary>
        /// Recursively sanitises an object tree, redacting strings.
        /// </summary>
        private static object SanitizeObject(object value)
        {
            if (value is string str)
                return SafeDiagnostics.RedactAll(str);

            if (value is Dictionary<string, object> dict)
            {
                var sanitized = new Dictionary<string, object>(dict.Count);
                foreach (var kvp in dict)
                {
                    string key = SensitiveDetailKeys.Contains(kvp.Key)
                        ? "***REDACTED***"
                        : kvp.Key;
                    sanitized[key] = kvp.Value != null
                        ? SanitizeObject(kvp.Value)
                        : null!;
                }
                return sanitized;
            }

            if (value is List<object> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                        list[i] = SanitizeObject(list[i]);
                }
                return list;
            }

            // JToken types from Newtonsoft.Json deserialisation
            if (value is JObject jobj)
            {
                foreach (var prop in jobj.Properties().ToList())
                {
                    if (prop.Value != null)
                    {
                        if (SensitiveDetailKeys.Contains(prop.Name))
                        {
                            JToken sanitizedValue = JToken.FromObject(SanitizeObject(prop.Value));
                            jobj.Remove(prop.Name);
                            jobj["***REDACTED***"] = sanitizedValue;
                        }
                        else
                        {
                            prop.Value = JToken.FromObject(SanitizeObject(prop.Value));
                        }
                    }
                }
                return jobj;
            }

            if (value is JArray jarr)
            {
                for (int i = 0; i < jarr.Count; i++)
                {
                    if (jarr[i] != null)
                        jarr[i] = JToken.FromObject(SanitizeObject(jarr[i]));
                }
                return jarr;
            }

            // Primitives (int, bool, etc.) are safe to leave as-is
            return value;
        }

        // ──────────────────────────────────────────────────────────────
        //  Retry policy
        // ──────────────────────────────────────────────────────────────

        private bool IsRetryable(HttpTransportResponse response, DataProviderError error)
        {
            if (error.IsRetryable)
                return true;

            if (response.IsRetryableNetworkError || response.IsServerError || response.IsRateOrTimeoutError)
                return true;

            return false;
        }

        private int ComputeRetryDelayMs(DataProviderError error, int attemptIndex)
        {
            int serverDelaySeconds = error.RetryAfterSeconds ?? 0;
            if (serverDelaySeconds > 0)
            {
                serverDelaySeconds = Math.Min(serverDelaySeconds, _config.MaxRetryAfterSeconds);
                return serverDelaySeconds * 1000;
            }

            // Exponential backoff with the configured default base.
            int multiplier = 1 << attemptIndex;
            return _config.DefaultRetryDelayMs * multiplier;
        }

        // ──────────────────────────────────────────────────────────────
        //  Disposal
        // ──────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _transport?.Dispose();
        }
    }
}
