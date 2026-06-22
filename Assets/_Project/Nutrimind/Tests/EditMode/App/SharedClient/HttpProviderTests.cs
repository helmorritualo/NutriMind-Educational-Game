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
    /// transport. No real network calls are made.
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

        // ──────────────────────────────────────────────────────────────
        //  Error envelope parsing
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void HttpProvider_ParsesServerErrorEnvelope()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueError(403, "{\"message\":\"Quiz is locked.\",\"code\":\"QUIZ_LOCKED\",\"request_id\":\"req_abc\",\"retryable\":false,\"details\":{},\"field_errors\":{},\"retry_after_seconds\":null,\"action\":\"refresh_sync_status\"}");

            var config = new HttpProviderConfig
            {
                BaseUrl = BaseUrl,
                MaxRetries = 0,
                DefaultRetryDelayMs = 10
            };
            var session = new AuthSessionState { Token = "token" };
            var provider = new HttpProvider(config, session, transport);

            DataResult<QuizDetailDto> result = provider.GetQuizDetailAsync("quiz_1").Result;

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("QUIZ_LOCKED"));
            Assert.That(result.Error.Message, Is.EqualTo("Quiz is locked."));
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
            Assert.That(result.Error.Code, Is.EqualTo("FUTURE_CODE"));
            Assert.That(result.Error.Message, Is.EqualTo("An unexpected error occurred."));
            Assert.That(result.Error.RequestId, Is.EqualTo("req_1"));
        }

        [Test]
        public void HttpProvider_KnownCodeWithSensitiveContent_Redacted()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueError(403,
                "{\"code\":\"QUIZ_LOCKED\",\"message\":\"Token eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNrvP5T7lB7FrhTNPi and PIN 123456 found\"," +
                "\"details\":{\"sql\":\"SELECT * FROM users\"}," +
                "\"field_errors\":{\"field1\":\"at SomeClass.SomeMethod()\"}," +
                "\"request_id\":\"req_2\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<QuizDetailDto> result = provider.GetQuizDetailAsync("quiz_1").Result;

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("QUIZ_LOCKED"));
            Assert.That(result.Error.Message, Does.Not.Contain("eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNrvP5T7lB7FrhTNPi"));
            Assert.That(result.Error.Message, Does.Not.Contain("123456"));
            Assert.That(result.Error.Message, Does.Contain("Token"));
        }

        private static readonly string[] KnownSafeServerCodes =
        {
            "UNAUTHENTICATED", "TOKEN_EXPIRED", "STUDENT_INACTIVE", "VALIDATION_ERROR",
            "RATE_LIMITED", "SYNC_RATE_LIMITED", "SERVER_UNAVAILABLE", "SERVER_TIMEOUT",
            "MAINTENANCE_MODE", "QUIZ_LOCKED", "CONTENT_NOT_PUBLISHED", "SESSION_NOT_FOUND",
            "CLIENT_VERSION_UNSUPPORTED", "CONFIG_VERSION_UNSUPPORTED", "NOT_FOUND"
        };

        [TestCaseSource(nameof(KnownSafeServerCodes))]
        public void HttpProvider_KnownSafeCode_PreservesServerMessage_RedactsSensitiveContent(string errorCode)
        {
            var transport = new FakeHttpTransport();
            string jwtToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNrvP5T7lB7FrhTNPi";
            string serverMsg = $"Server says: token {jwtToken} found for {errorCode}";
            string errorBody = $"{{\"code\":\"{errorCode}\",\"message\":\"{serverMsg}\",\"request_id\":\"req_{errorCode}\"}}";

            transport.EnqueueError(400, errorBody);

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<PingResponseDto> result = provider.PingAsync().Result;

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(errorCode));
            Assert.That(result.Error.Message, Is.Not.EqualTo("An unexpected error occurred."));
            Assert.That(result.Error.Message, Does.Contain("Server says:"));
            Assert.That(result.Error.Message, Does.Not.Contain(jwtToken));
            Assert.That(result.Error.Message, Does.Contain("***REDACTED_TOKEN***"));
            Assert.That(result.Error.RequestId, Is.EqualTo($"req_{errorCode}"));
        }

        [Test]
        public void HttpProvider_GetQuizzes_UsesCorrectQueryParams()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{\"quizzes\":[]}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<QuizListDto> result = provider.GetQuizzesAsync("litera_quest", 2).Result;

            Assert.That(result.Success, Is.True);
            Assert.That(transport.Requests[0].Url, Does.Contain("/student/quizzes?subject_slug=litera_quest&term_number=2"));
        }

        [Test]
        public void HttpProvider_SubmitQuizAttempt_UsesCorrectEndpoint()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{\"attempt_id\":\"att_1\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<QuizAttemptResponseDto> result = provider.SubmitQuizAttemptAsync("quiz_1", new QuizAttemptRequestDto
            {
                ClientAttemptUuid = "uuid-1",
                Answers = new Dictionary<string, object> { { "item_1", "b" } }
            }).Result;

            Assert.That(result.Success, Is.True);
            Assert.That(transport.Requests[0].Url, Does.Contain("/student/quizzes/quiz_1/attempts"));
            Assert.That(transport.Requests[0].BodyJson, Does.Contain("\"client_attempt_uuid\""));
            Assert.That(transport.Requests[0].BodyJson, Does.Contain("\"uuid-1\""));
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
        public void HttpProvider_QuizAttemptRetriesAndReusesClientAttemptUuid()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueNetworkError();
            transport.EnqueueSuccess(200, "{\"attempt_id\":\"att_1\",\"client_attempt_uuid\":\"uuid-1\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<QuizAttemptResponseDto> result = provider.SubmitQuizAttemptAsync("quiz_1", new QuizAttemptRequestDto
            {
                ClientAttemptUuid = "uuid-1",
                Answers = new Dictionary<string, object> { { "item_1", "b" } }
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
            Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThan(900));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(4000));
        }

        [Test]
        public void HttpProvider_PatchDoesNotRetryOnTransientError()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueError(503, "{\"code\":\"SERVER_UNAVAILABLE\",\"message\":\"Down for maintenance\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<SettingsDto> result = provider.PatchSettingsAsync(new SettingsDto()).Result;

            Assert.That(result.Success, Is.False);
            Assert.That(transport.RequestCount, Is.EqualTo(1));
        }

        // ──────────────────────────────────────────────────────────────
        //  Optional narrative fields
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void HttpProvider_QuizDetail_InstructionsAbsentStillSucceeds()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, "{\"id\":\"quiz_1\",\"title\":\"Minimal\"}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<QuizDetailDto> result = provider.GetQuizDetailAsync("quiz_1").Result;

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data.Title, Is.EqualTo("Minimal"));
            Assert.That(result.Data.Instructions, Is.Null);
        }

        [Test]
        public void HttpProvider_QuizDetail_InstructionsPresentParsed()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200,
                "{\"id\":\"quiz_1\",\"title\":\"Rich\"," +
                "\"instructions\":\"Read carefully.\"," +
                "\"items\":[{\"id\":\"item_1\",\"type\":\"multiple_choice\",\"prompt\":\"Q1\"}]}");

            var session = new AuthSessionState { Token = "token" };
            var provider = CreateProvider(session, transport);

            DataResult<QuizDetailDto> result = provider.GetQuizDetailAsync("quiz_1").Result;

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data.Instructions, Is.EqualTo("Read carefully."));
            Assert.That(result.Data.Items, Has.Count.EqualTo(1));
            Assert.That(result.Data.Items[0].Id, Is.EqualTo("item_1"));
            Assert.That(result.Data.Items[0].Prompt, Is.EqualTo("Q1"));
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

    [TestFixture]
    public class AuthSessionStateTests
    {
        [Test]
        public void AuthSessionState_DefaultsUnauthenticated()
        {
            var session = new AuthSessionState();
            Assert.That(session.IsAuthenticated, Is.False);
            Assert.That(session.Token, Is.Null);
            Assert.That(session.StudentId, Is.Null);
        }

        [Test]
        public void AuthSessionState_ApplyLogin_Authenticated()
        {
            var session = new AuthSessionState();
            var login = new LoginResponseDto
            {
                Token = "jwt_token",
                Student = new StudentIdentityDto { Id = "student_1", Name = "Maya" }
            };

            session.ApplyLoginResponse(login);

            Assert.That(session.IsAuthenticated, Is.True);
            Assert.That(session.Token, Is.EqualTo("jwt_token"));
            Assert.That(session.StudentId, Is.EqualTo("student_1"));
            Assert.That(session.StudentName, Is.EqualTo("Maya"));
        }

        [Test]
        public void AuthSessionState_Reset_ClearsAuth()
        {
            var session = new AuthSessionState();
            session.ApplyLoginResponse(new LoginResponseDto { Token = "jwt" });
            Assert.That(session.IsAuthenticated, Is.True);

            session.Reset();

            Assert.That(session.IsAuthenticated, Is.False);
            Assert.That(session.Token, Is.Null);
            Assert.That(session.StudentId, Is.Null);
        }
    }
}