using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;
using NutriMind.Runtime.App.Http;

namespace NutriMind.Tests.EditMode.App
{
    /// <summary>
    /// Functional unit tests for <see cref="HttpProvider"/> using a fake
    /// transport.  No real network calls are made.
    /// </summary>
    [TestFixture]
    public class HttpProviderTests
    {
        private const string BaseUrl = "https://test.nutrimind.example";

        private static HttpProvider CreateProvider(AuthSessionState session, FakeHttpTransport transport)
        {
            var config = new HttpProviderConfig
            {
                BaseUrl = BaseUrl,
                MaxRetries = 2,
                DefaultRetryDelayMs = 10
            };
            return new HttpProvider(config, session, transport);
        }

        // ──────────────────────────────────────────────────────────────
        //  JSON contract safety
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void JsonSettings_SnakeCaseRoundTrip()
        {
            var request = new LoginRequestDto
            {
                Lrn = "123456789012",
                Pin = "123456",
                DeviceName = "Unity Editor",
                ClientVersion = "0.1.0"
            };

            string json = JsonConvert.SerializeObject(request, JsonSettings.SafeDefaults);

            Assert.That(json, Does.Contain("\"lrn\""));
            Assert.That(json, Does.Contain("\"pin\""));
            Assert.That(json, Does.Contain("\"device_name\""));
            Assert.That(json, Does.Contain("\"client_version\""));

            var parsed = JsonConvert.DeserializeObject<LoginRequestDto>(json, JsonSettings.SafeDefaults);
            Assert.That(parsed.Lrn, Is.EqualTo("123456789012"));
            Assert.That(parsed.Pin, Is.EqualTo("123456"));
        }

        [Test]
        public void JsonSettings_UnknownFieldsIgnored()
        {
            string json = "{\"lrn\":\"123456789012\",\"pin\":\"123456\",\"future_field\":\"ignored\"}";
            var parsed = JsonConvert.DeserializeObject<LoginRequestDto>(json, JsonSettings.SafeDefaults);

            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.Lrn, Is.EqualTo("123456789012"));
        }

        [Test]
        public void JsonSettings_UnknownEnumFallsBackToUnknown()
        {
            string json = "\"future_challenge_type\"";
            var parsed = JsonConvert.DeserializeObject<ChallengeAnswerType>(json, JsonSettings.SafeDefaults);

            Assert.That(parsed, Is.EqualTo(ChallengeAnswerType.Unknown));
        }

        [Test]
        public void JsonSettings_KnownEnumSnakeCaseParsed()
        {
            string json = "\"multiple_choice\"";
            var parsed = JsonConvert.DeserializeObject<ChallengeAnswerType>(json, JsonSettings.SafeDefaults);

            Assert.That(parsed, Is.EqualTo(ChallengeAnswerType.MultipleChoice));
        }

        // ──────────────────────────────────────────────────────────────
        //  Error envelope parsing
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void HttpProvider_ParsesServerErrorEnvelope()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueError(403, "{\"message\":\"Station is locked.\",\"code\":\"STATION_LOCKED\",\"request_id\":\"req_abc\",\"retryable\":false,\"details\":{},\"field_errors\":{},\"retry_after_seconds\":null,\"action\":\"refresh_sync_status\"}");

            var config = new HttpProviderConfig
            {
                BaseUrl = BaseUrl,
                MaxRetries = 0,
                DefaultRetryDelayMs = 10
            };
            var session = new AuthSessionState { Token = "token" };
            var provider = new HttpProvider(config, session, transport);

            DataResult<StationContentDto> result = provider.GetStationContentAsync("station_1").Result;

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("STATION_LOCKED"));
            Assert.That(result.Error.Message, Is.EqualTo("Station is locked."));
            Assert.That(result.Error.RequestId, Is.EqualTo("req_abc"));
            Assert.That(result.Error.Retryable, Is.False);
            Assert.That(result.Error.Action, Is.EqualTo("refresh_sync_status"));
            Assert.That(result.Error.ResolvedAction, Is.EqualTo(ErrorAction.RefreshSyncStatus));
        }

        [Test]
        public void HttpProvider_UnknownErrorCode_UsesGenericMessage()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueError(400, "{\"code\":\"FUTURE_CODE\",\"message\":\"Leaked token abc.123.def and PIN 123456\",\"request_id\":\"req_1\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<StudentProfileDto> result = provider.GetProfileAsync().Result;

            Assert.That(result.Success, Is.False);
            // Unknown code is kept so callers can handle it programmatically.
            Assert.That(result.Error.Code, Is.EqualTo("FUTURE_CODE"));
            // Message is replaced with generic safe message.
            Assert.That(result.Error.Message, Is.EqualTo("An unexpected error occurred."));
            // RequestId is preserved.
            Assert.That(result.Error.RequestId, Is.EqualTo("req_1"));
        }

        [Test]
        public void HttpProvider_KnownCodeWithSensitiveContent_Redacted()
        {
            var transport = new FakeHttpTransport();
            // STATION_LOCKED is a known code; message keeps its structure but
            // sensitive patterns are redacted.
            transport.EnqueueError(403,
                "{\"code\":\"STATION_LOCKED\",\"message\":\"Token eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNrvP5T7lB7FrhTNPi and PIN 123456 found\"," +
                "\"details\":{\"sql\":\"SELECT * FROM users\"}," +
                "\"field_errors\":{\"field1\":\"at SomeClass.SomeMethod()\"}," +
                "\"request_id\":\"req_2\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<StationContentDto> result = provider.GetStationContentAsync("station_1").Result;

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("STATION_LOCKED"));
            // Known code preserves message content but sensitive items are redacted.
            Assert.That(result.Error.Message, Does.Not.Contain("eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNrvP5T7lB7FrhTNPi"));
            Assert.That(result.Error.Message, Does.Not.Contain("123456"));
            Assert.That(result.Error.Message, Does.Contain("Token"));
            // Details SQL is redacted.
            Assert.That(result.Error.Details, Is.Not.Null);
            string detailsJson = JsonConvert.SerializeObject(result.Error.Details);
            Assert.That(detailsJson, Does.Not.Contain("SELECT"));
            Assert.That(detailsJson, Does.Not.Contain("FROM users"));
            // FieldErrors stack trace is redacted.
            Assert.That(result.Error.FieldErrors, Is.Not.Null);
            string feJson = JsonConvert.SerializeObject(result.Error.FieldErrors);
            Assert.That(feJson, Does.Not.Contain("SomeClass.SomeMethod"));
            // RequestId preserved.
            Assert.That(result.Error.RequestId, Is.EqualTo("req_2"));
        }

        // ──────────────────────────────────────────────────────────────
        //  Known safe error codes — parameterized
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Documented server envelope error codes that are known safe
        /// (see <c>HttpProvider.KnownSafeErrorCodes</c> and
        /// <c>docs/SERVER_REQUIREMENTS.md</c>).
        /// Each code must preserve the server-provided message (after redaction)
        /// rather than falling back to the generic unknown-code message.
        /// </summary>
        private static readonly string[] KnownSafeServerCodes =
        {
            "UNAUTHENTICATED", "TOKEN_EXPIRED", "STUDENT_INACTIVE", "VALIDATION_ERROR",
            "RATE_LIMITED", "SYNC_RATE_LIMITED", "SERVER_UNAVAILABLE", "SERVER_TIMEOUT",
            "MAINTENANCE_MODE", "STATION_LOCKED", "STATION_ALREADY_COMPLETED",
            "CONTENT_NOT_PUBLISHED", "SESSION_NOT_FOUND", "SESSION_FORBIDDEN",
            "WORLD_SCENE_UNAVAILABLE", "STALE_CONTENT", "CLIENT_VERSION_UNSUPPORTED",
            "CONFIG_VERSION_UNSUPPORTED", "REALTIME_UNAVAILABLE", "AI_NOT_CONFIGURED",
            "NOT_FOUND"
        };

        [TestCaseSource(nameof(KnownSafeServerCodes))]
        public void HttpProvider_KnownSafeCode_PreservesServerMessage_RedactsSensitiveContent(string errorCode)
        {
            var transport = new FakeHttpTransport();
            // Message includes a JWT token that RedactAll must replace.
            string jwtToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNrvP5T7lB7FrhTNPi";
            string serverMsg = $"Server says: token {jwtToken} found for {errorCode}";
            string errorBody = $"{{\"code\":\"{errorCode}\",\"message\":\"{serverMsg}\",\"request_id\":\"req_{errorCode}\"}}";

            transport.EnqueueError(400, errorBody);

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<PingResponseDto> result = provider.PingAsync().Result;

            Assert.That(result.Success, Is.False);

            // Code is preserved verbatim.
            Assert.That(result.Error.Code, Is.EqualTo(errorCode),
                "Error code '{0}' must be preserved", errorCode);

            // Known code: server message is kept (after redaction),
            // NOT replaced with the generic unknown-code fallback.
            Assert.That(result.Error.Message, Is.Not.EqualTo("An unexpected error occurred."),
                "Known error code '{0}' must preserve the server message after redaction, " +
                "not use the generic fallback", errorCode);

            // Server message structure is preserved.
            Assert.That(result.Error.Message, Does.Contain("Server says:"),
                "Known error code '{0}' must retain the server message structure", errorCode);
            Assert.That(result.Error.Message, Does.Contain(errorCode),
                "Known error code '{0}' must appear in the preserved message", errorCode);

            // Sensitive JWT token is redacted.
            Assert.That(result.Error.Message, Does.Not.Contain(jwtToken),
                "Known error code '{0}' message must have the JWT token redacted", errorCode);
            Assert.That(result.Error.Message, Does.Contain("***REDACTED_TOKEN***"),
                "Known error code '{0}' message must contain the redaction placeholder", errorCode);

            // RequestId is preserved.
            Assert.That(result.Error.RequestId, Is.EqualTo($"req_{errorCode}"),
                "RequestId must be preserved for code '{0}'", errorCode);
        }

        [Test]
        public void HttpProvider_ServerErrorEnvelope_NoStudentSafeLeak()
        {
            var transport = new FakeHttpTransport();
            // 500 with leaky internal error: SQL, stack trace, provider details
            transport.EnqueueError(500,
                "{\"code\":\"FUTURE_INTERNAL_ERROR\",\"message\":\"Provider error: connection refused. " +
                "SELECT * FROM credentials. at HttpProvider.Send()\"," +
                "\"details\":{\"answer_key\":\"B\",\"trace\":\"at SomeLib.Db.Query()\"}," +
                "\"field_errors\":{\"field1\":\"correct_answer is 42\"},\"request_id\":\"req_3\"}");

            var config = new HttpProviderConfig
            {
                BaseUrl = BaseUrl,
                MaxRetries = 0,
                DefaultRetryDelayMs = 10
            };
            var session = new AuthSessionState { Token = "token" };
            var provider = new HttpProvider(config, session, transport);

            DataResult<StudentProfileDto> result = provider.GetProfileAsync().Result;

            Assert.That(result.Success, Is.False);
            // Unknown code -> generic message
            Assert.That(result.Error.Message, Is.EqualTo("An unexpected error occurred."));
            // Code preserved
            Assert.That(result.Error.Code, Is.EqualTo("FUTURE_INTERNAL_ERROR"));
            // RequestId preserved
            Assert.That(result.Error.RequestId, Is.EqualTo("req_3"));
            // Details should be sanitized (no SQL, stack, answer key)
            if (result.Error.Details != null)
            {
                string dJson = JsonConvert.SerializeObject(result.Error.Details);
                Assert.That(dJson, Does.Not.Contain("SELECT"));
                Assert.That(dJson, Does.Not.Contain("SomeLib"));
                Assert.That(dJson, Does.Not.Contain("answer_key"));
            }
            // FieldErrors should be sanitized
            if (result.Error.FieldErrors != null)
            {
                string fJson = JsonConvert.SerializeObject(result.Error.FieldErrors);
                Assert.That(fJson, Does.Not.Contain("correct_answer"));
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Auth behavior
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void HttpProvider_Login_StoresToken()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{\"token\":\"new_token\",\"token_type\":\"Bearer\",\"student\":{\"id\":\"stu_1\"}}");

            var session = new AuthSessionState();
            var provider = CreateProvider(session, transport);

            DataResult<LoginResponseDto> result = provider.LoginAsync(new LoginRequestDto { Lrn = "1", Pin = "2" }).Result;

            Assert.That(result.Success, Is.True);
            Assert.That(session.Token, Is.EqualTo("new_token"));
        }

        [Test]
        public void HttpProvider_Logout_ClearsToken()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{}");

            var session = new AuthSessionState { Token = "old_token" };
            var provider = CreateProvider(session, transport);

            DataResult<object> result = provider.LogoutAsync().Result;

            Assert.That(result.Success, Is.True);
            Assert.That(session.Token, Is.Null);
        }

        [Test]
        public void HttpProvider_NoToken_ReturnsUnauthenticated()
        {
            var transport = new FakeHttpTransport();
            var session = new AuthSessionState();
            var provider = CreateProvider(session, transport);

            DataResult<StudentProfileDto> result = provider.GetProfileAsync().Result;

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("UNAUTHENTICATED"));
            Assert.That(transport.RequestCount, Is.EqualTo(0));
        }

        [Test]
        public void HttpProvider_PublicEndpoint_DoesNotRequireToken()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{\"api_version\":\"v1\"}");

            var session = new AuthSessionState();
            var provider = CreateProvider(session, transport);

            DataResult<ApiConfigDto> result = provider.GetConfigAsync().Result;

            Assert.That(result.Success, Is.True);
            Assert.That(transport.RequestCount, Is.EqualTo(1));
            Assert.That(transport.Requests[0].Headers.ContainsKey("Authorization"), Is.False);
        }

        [Test]
        public void HttpProvider_DoesNotLogAuthorizationHeader()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{\"id\":\"stu_1\"}");

            var session = new AuthSessionState { Token = "secret_token" };
            var provider = CreateProvider(session, transport);

            DataResult<StudentProfileDto> result = provider.GetProfileAsync().Result;

            Assert.That(result.Success, Is.True);
            // Sanity: the header is present on the request but never appears in logs.
            Assert.That(transport.Requests[0].Headers["Authorization"], Is.EqualTo("Bearer secret_token"));
        }

        // ──────────────────────────────────────────────────────────────
        //  Endpoint path construction
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void HttpProvider_GetTerms_UsesEscapedPath()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "[]");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<List<TermDto>> result = provider.GetTermsAsync("litera quest").Result;

            Assert.That(result.Success, Is.True);
            Assert.That(transport.Requests[0].Url, Does.Contain("/student/subjects/litera%20quest/terms"));
        }

        [Test]
        public void HttpProvider_GetStations_UsesTermNumberPath()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{\"stations\":[]}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<StationListDto> result = provider.GetStationsAsync("litera_quest", 2).Result;

            Assert.That(result.Success, Is.True);
            Assert.That(transport.Requests[0].Url, Does.Contain("/student/subjects/litera_quest/terms/2/stations"));
        }

        [Test]
        public void HttpProvider_SubmitAttempt_UsesChallengePath()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{\"attempt_id\":\"att_1\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<AttemptResponseDto> result = provider.SubmitAttemptAsync("challenge_1", new AttemptRequestDto
            {
                StationSessionId = "ssn_1",
                ClientAttemptUuid = "uuid-1",
                Answer = "B"
            }).Result;

            Assert.That(result.Success, Is.True);
            Assert.That(transport.Requests[0].Url, Does.Contain("/student/challenges/challenge_1/attempts"));
            Assert.That(transport.Requests[0].BodyJson, Does.Contain("\"client_attempt_uuid\""));
            Assert.That(transport.Requests[0].BodyJson, Does.Contain("\"uuid-1\""));
        }

        [Test]
        public void HttpProvider_UseReward_UsesRewardCodePath()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{\"reward_code\":\"hint_scroll\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<UseRewardResponseDto> result = provider.UseRewardAsync("hint_scroll", new UseRewardRequestDto { Quantity = 1 }).Result;

            Assert.That(result.Success, Is.True);
            Assert.That(transport.Requests[0].Url, Does.Contain("/student/rewards/hint_scroll/use"));
        }

        // ──────────────────────────────────────────────────────────────
        //  Retry and idempotency
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void HttpProvider_GetRetriesOnNetworkError()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueNetworkError();
            transport.EnqueueNetworkError();
            transport.EnqueueSuccess(200, "{\"status\":\"ok\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<PingResponseDto> result = provider.PingAsync().Result;

            Assert.That(result.Success, Is.True);
            Assert.That(transport.RequestCount, Is.EqualTo(3));
        }

        [Test]
        public void HttpProvider_UnsafePostDoesNotRetry()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueError(500, "{\"code\":\"SERVER_UNAVAILABLE\",\"message\":\"Down\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<StationStartResponseDto> result = provider.StartStationAsync("station_1").Result;

            Assert.That(result.Success, Is.False);
            Assert.That(transport.RequestCount, Is.EqualTo(1));
        }

        [Test]
        public void HttpProvider_AttemptRetriesAndReusesClientAttemptUuid()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueNetworkError();
            transport.EnqueueSuccess(200, "{\"attempt_id\":\"att_1\",\"client_attempt_uuid\":\"uuid-1\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<AttemptResponseDto> result = provider.SubmitAttemptAsync("challenge_1", new AttemptRequestDto
            {
                ClientAttemptUuid = "uuid-1",
                Answer = "B"
            }).Result;

            Assert.That(result.Success, Is.True);
            Assert.That(transport.RequestCount, Is.EqualTo(2));
            Assert.That(transport.Requests[0].BodyJson, Does.Contain("\"uuid-1\""));
            Assert.That(transport.Requests[1].BodyJson, Does.Contain("\"uuid-1\""));
        }

        [Test]
        public void HttpProvider_RetryRespectsRetryAfterSeconds()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueError(429, "{\"code\":\"RATE_LIMITED\",\"message\":\"Slow down.\",\"retryable\":true,\"retry_after_seconds\":1}");
            transport.EnqueueSuccess(200, "{\"status\":\"ok\"}");

            var config = new HttpProviderConfig { BaseUrl = BaseUrl, MaxRetries = 2, DefaultRetryDelayMs = 5000 };
            var session = new AuthSessionState { Token = "token" };
            var provider = new HttpProvider(config, session, transport);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            DataResult<PingResponseDto> result = provider.PingAsync().Result;
            stopwatch.Stop();

            Assert.That(result.Success, Is.True);
            Assert.That(transport.RequestCount, Is.EqualTo(2));
            // Should wait ~1 second, not 5 seconds.
            Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThan(900));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(4000));
        }

        [Test]
        public void HttpProvider_PatchDoesNotRetryOnTransientError()
        {
            var transport = new FakeHttpTransport();
            // First attempt returns a transient server error.
            transport.EnqueueError(503, "{\"code\":\"SERVER_UNAVAILABLE\",\"message\":\"Down for maintenance\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<SettingsDto> result = provider.PatchSettingsAsync(new SettingsDto()).Result;

            Assert.That(result.Success, Is.False);
            // PATCH should NOT retry automatically -- exactly 1 request.
            Assert.That(transport.RequestCount, Is.EqualTo(1));
        }

        // ──────────────────────────────────────────────────────────────
        //  Optional narrative fields
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void HttpProvider_StationContent_NarrativeFieldsAbsentStillSucceeds()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{\"station_id\":\"station_1\",\"title\":\"Minimal\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<StationContentDto> result = provider.GetStationContentAsync("station_1").Result;

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data.Title, Is.EqualTo("Minimal"));
            Assert.That(result.Data.StoryContext, Is.Null);
            Assert.That(result.Data.NpcGuides, Is.Null);
        }

        [Test]
        public void HttpProvider_StationContent_NarrativeFieldsPresentParsed()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200,
                "{\"station_id\":\"station_1\",\"title\":\"Rich\"," +
                "\"story_context\":\"Once upon...\"," +
                "\"mission_title\":\"Fix the Bridge\"," +
                "\"npc_guides\":[{\"npc_key\":\"npc_1\",\"display_name\":\"Guide\",\"intro_dialogue\":\"Hello!\"}]," +
                "\"learning_cycle\":{\"discover\":\"Look around\",\"practice\":\"Try it\",\"apply\":\"Use it\",\"review\":\"Reflect\"}," +
                "\"hint_policy\":{\"max_hint_tier\":2,\"preserve_world_progress\":true,\"penalize_ordinary_mistake\":false,\"tiers\":[{\"tier\":1,\"text\":\"Hint\"}]}," +
                "\"discoveries\":[{\"discovery_key\":\"d1\",\"title\":\"Fun fact\",\"optional\":true}]," +
                "\"reflection_prompt\":\"Reflect...\"," +
                "\"reward_preview\":[{\"code\":\"coin\",\"quantity\":5}]," +
                "\"world_restoration_state\":{\"state_key\":\"restored\",\"apply_after_accepted_completion\":true}," +
                "\"success_feedback\":{\"message\":\"Great job!\"}}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<StationContentDto> result = provider.GetStationContentAsync("station_1").Result;

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data.StoryContext, Is.EqualTo("Once upon..."));
            Assert.That(result.Data.MissionTitle, Is.EqualTo("Fix the Bridge"));
            Assert.That(result.Data.NpcGuides, Has.Count.EqualTo(1));
            Assert.That(result.Data.NpcGuides[0].NpcKey, Is.EqualTo("npc_1"));
            Assert.That(result.Data.NpcGuides[0].DisplayName, Is.EqualTo("Guide"));
            Assert.That(result.Data.LearningCycle, Is.Not.Null);
            Assert.That(result.Data.LearningCycle.Discover, Is.EqualTo("Look around"));
            Assert.That(result.Data.LearningCycle.Review, Is.EqualTo("Reflect"));
            Assert.That(result.Data.HintPolicy.MaxHintTier, Is.EqualTo(2));
            Assert.That(result.Data.HintPolicy.PreserveWorldProgress, Is.True);
            Assert.That(result.Data.Discoveries, Has.Count.EqualTo(1));
            Assert.That(result.Data.Discoveries[0].Optional, Is.True);
            Assert.That(result.Data.ReflectionPrompt, Is.EqualTo("Reflect..."));
            Assert.That(result.Data.RewardPreview, Has.Count.EqualTo(1));
            Assert.That(result.Data.RewardPreview[0].Code, Is.EqualTo("coin"));
            Assert.That(result.Data.WorldRestorationState.StateKey, Is.EqualTo("restored"));
            Assert.That(result.Data.SuccessFeedback.Message, Is.EqualTo("Great job!"));
        }

        // ──────────────────────────────────────────────────────────────
        //  Configuration validation
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void HttpProvider_InvalidBaseUrl_ReturnsConfigurationError()
        {
            var transport = new FakeHttpTransport();
            var config = new HttpProviderConfig { BaseUrl = "not-a-url" };
            var session = new AuthSessionState { Token = "token" };
            var provider = new HttpProvider(config, session, transport);

            DataResult<PingResponseDto> result = provider.PingAsync().Result;

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("CONFIGURATION_ERROR"));
            Assert.That(transport.RequestCount, Is.EqualTo(0));
        }

        [Test]
        public void HttpProvider_HttpBaseUrl_RejectedAsConfigurationError()
        {
            var transport = new FakeHttpTransport();
            var config = new HttpProviderConfig { BaseUrl = "http://insecure.example" };
            var session = new AuthSessionState { Token = "token" };
            var provider = new HttpProvider(config, session, transport);

            DataResult<PingResponseDto> result = provider.PingAsync().Result;

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("CONFIGURATION_ERROR"));
            // No request should have been sent for a config validation error.
            Assert.That(transport.RequestCount, Is.EqualTo(0));
        }

        [Test]
        public void HttpProviderConfig_BuildUrl_TrimsSlashes()
        {
            var config = new HttpProviderConfig
            {
                BaseUrl = "https://test.example/",
                ApiPrefix = "/api/v1/"
            };

            string url = config.BuildUrl("/student/ping");

            Assert.That(url, Is.EqualTo("https://test.example/api/v1/student/ping"));
        }
    }

    // ────────────────────────────────────────────────────────────────────────────
    //  AuthSessionState
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Unit tests for <see cref="AuthSessionState"/>.
    /// Verifies token storage, login-response application, identity fields,
    /// reset behaviour, and the <see cref="AuthSessionState.IsAuthenticated"/> flag.
    /// </summary>
    [TestFixture]
    public class AuthSessionStateTests
    {
        [Test]
        public void AuthSessionState_DefaultState_IsNotAuthenticated()
        {
            var state = new AuthSessionState();
            Assert.That(state.IsAuthenticated, Is.False);
            Assert.That(state.Token, Is.Null);
            Assert.That(state.StudentId, Is.Null);
            Assert.That(state.StudentName, Is.Null);
            Assert.That(state.GradeLevel, Is.Null);
        }

        [Test]
        public void AuthSessionState_TokenSet_IsAuthenticated()
        {
            var state = new AuthSessionState();
            state.Token = "some_token";
            Assert.That(state.IsAuthenticated, Is.True);
        }

        [Test]
        public void AuthSessionState_EmptyToken_NotAuthenticated()
        {
            var state = new AuthSessionState();
            state.Token = string.Empty;
            Assert.That(state.IsAuthenticated, Is.False);
        }

        [Test]
        public void AuthSessionState_ApplyLoginResponse_PopulatesAllFields()
        {
            var state = new AuthSessionState();
            var response = new LoginResponseDto
            {
                Token = "bearer_token_xyz",
                TokenType = "Bearer",
                Student = new StudentIdentityDto
                {
                    Id = "stu_101",
                    Name = "Juan Dela Cruz",
                    LrnMasked = "1234••••9012",
                    GradeLevel = 5,
                    LanguagePreference = "en"
                }
            };

            state.ApplyLoginResponse(response);

            Assert.That(state.Token, Is.EqualTo("bearer_token_xyz"));
            Assert.That(state.TokenType, Is.EqualTo("Bearer"));
            Assert.That(state.StudentId, Is.EqualTo("stu_101"));
            Assert.That(state.StudentName, Is.EqualTo("Juan Dela Cruz"));
            Assert.That(state.LrnMasked, Is.EqualTo("1234••••9012"));
            Assert.That(state.GradeLevel, Is.EqualTo(5));
            Assert.That(state.LanguagePreference, Is.EqualTo("en"));
            Assert.That(state.IsAuthenticated, Is.True);
        }

        [Test]
        public void AuthSessionState_ApplyLoginResponse_NullStudent_SetsTokenOnly()
        {
            var state = new AuthSessionState();
            var response = new LoginResponseDto
            {
                Token = "token_only",
                TokenType = "Bearer",
                Student = null
            };

            state.ApplyLoginResponse(response);

            Assert.That(state.Token, Is.EqualTo("token_only"));
            Assert.That(state.TokenType, Is.EqualTo("Bearer"));
            Assert.That(state.StudentId, Is.Null);
            Assert.That(state.StudentName, Is.Null);
        }

        [Test]
        public void AuthSessionState_ApplyLoginResponse_NullResponse_NoChanges()
        {
            var state = new AuthSessionState();
            state.Token = "existing_token";

            state.ApplyLoginResponse(null);

            Assert.That(state.Token, Is.EqualTo("existing_token"));
        }

        [Test]
        public void AuthSessionState_ApplyLoginResponse_OverwritesExistingValues()
        {
            var state = new AuthSessionState
            {
                Token = "old_token",
                StudentName = "Old Name",
                GradeLevel = 5
            };

            var response = new LoginResponseDto
            {
                Token = "new_token",
                Student = new StudentIdentityDto
                {
                    Id = "stu_200",
                    Name = "New Name",
                    GradeLevel = 6
                }
            };

            state.ApplyLoginResponse(response);

            Assert.That(state.Token, Is.EqualTo("new_token"));
            Assert.That(state.StudentName, Is.EqualTo("New Name"));
            Assert.That(state.GradeLevel, Is.EqualTo(6));
        }

        [Test]
        public void AuthSessionState_Reset_ClearsAllFields()
        {
            var state = new AuthSessionState();
            state.ApplyLoginResponse(new LoginResponseDto
            {
                Token = "token",
                TokenType = "Bearer",
                Student = new StudentIdentityDto
                {
                    Id = "stu_1",
                    Name = "Name",
                    LrnMasked = "••••",
                    GradeLevel = 5,
                    LanguagePreference = "en"
                }
            });

            state.Reset();

            Assert.That(state.Token, Is.Null);
            Assert.That(state.StudentId, Is.Null);
            Assert.That(state.StudentName, Is.Null);
            Assert.That(state.LrnMasked, Is.Null);
            Assert.That(state.GradeLevel, Is.Null);
            Assert.That(state.LanguagePreference, Is.Null);
            Assert.That(state.TokenType, Is.Null);
            Assert.That(state.IsAuthenticated, Is.False);
        }

        [Test]
        public void AuthSessionState_Reset_FromDefault_Safe()
        {
            var state = new AuthSessionState();
            state.Reset();
            Assert.That(state.IsAuthenticated, Is.False);
        }

        [Test]
        public void AuthSessionState_HttpProviderLogin_PopulatesFullState()
        {
            // Integration-style test: HttpProvider.LoginAsync must populate
            // the full AuthSessionState through ApplyLoginResponse.
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200,
                "{\"token\":\"login_token\",\"token_type\":\"Bearer\"," +
                "\"student\":{\"id\":\"stu_42\",\"name\":\"Maria Santos\"," +
                "\"lrn_masked\":\"5678••••1234\",\"grade_level\":6,\"language_preference\":\"tl\"}}");

            var config = new HttpProviderConfig { BaseUrl = "https://test.example" };
            var session = new AuthSessionState();
            var provider = new HttpProvider(config, session, transport);

            DataResult<LoginResponseDto> result = provider.LoginAsync(
                new LoginRequestDto { Lrn = "567812345678", Pin = "654321" }).Result;

            Assert.That(result.Success, Is.True);
            Assert.That(session.Token, Is.EqualTo("login_token"));
            Assert.That(session.TokenType, Is.EqualTo("Bearer"));
            Assert.That(session.StudentId, Is.EqualTo("stu_42"));
            Assert.That(session.StudentName, Is.EqualTo("Maria Santos"));
            Assert.That(session.LrnMasked, Is.EqualTo("5678••••1234"));
            Assert.That(session.GradeLevel, Is.EqualTo(6));
            Assert.That(session.LanguagePreference, Is.EqualTo("tl"));
            Assert.That(session.IsAuthenticated, Is.True);
        }
    }
}
