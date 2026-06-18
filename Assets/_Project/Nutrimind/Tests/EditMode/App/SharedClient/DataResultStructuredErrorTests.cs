using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;
using NutriMind.Runtime.App.Http;

namespace NutriMind.Tests.EditMode.App
{
    /// <summary>
    /// Tests that <c>DataResult&lt;T&gt;</c> exposes a structured
    /// <c>Error</c> property of type <c>DataProviderError</c>
    /// with full envelope fields, and that providers populate errors
    /// with a stable <c>"not_implemented"</c> code.
    ///
    /// Updated for Phase 04: providers now use async methods returning
    /// <c>Task&lt;DataResult&lt;T&gt;&gt;</c>.
    /// </summary>
    [TestFixture]
    public class DataResultStructuredErrorTests
    {
        private const string AssemblyName = "NutriMind.Runtime.App";

        private static Type FindType(string fullTypeName)
            => Type.GetType(fullTypeName + ", " + AssemblyName);

        // ---------------------------------------------------------------
        // DataResult<T>.Error property
        // ---------------------------------------------------------------

        [Test]
        public void DataResultOfT_HasErrorPropertyOfTypeDataProviderError()
        {
            Type openResult = FindType("NutriMind.Runtime.App.DataResult`1");
            Assert.That(openResult, Is.Not.Null,
                "Precondition: DataResult`1 generic type must exist");

            Type errorType = FindType("NutriMind.Runtime.App.DataProviderError");
            Assert.That(errorType, Is.Not.Null,
                "Precondition: DataProviderError type must exist");

            PropertyInfo errorProp = openResult.GetProperty("Error",
                BindingFlags.Public | BindingFlags.Instance);

            Assert.That(errorProp, Is.Not.Null,
                "DataResult<T> must expose a public instance property named 'Error' " +
                "of type DataProviderError");

            Assert.That(errorProp.PropertyType, Is.EqualTo(errorType),
                "DataResult<T>.Error must be of type DataProviderError");
        }

        [Test]
        public void DataResultOfT_ErrorMessage_StillAccessibleForCompatibility()
        {
            Type openResult = FindType("NutriMind.Runtime.App.DataResult`1");
            Assert.That(openResult, Is.Not.Null);

            PropertyInfo errorMsgProp = openResult.GetProperty("ErrorMessage",
                BindingFlags.Public | BindingFlags.Instance);

            Assert.That(errorMsgProp, Is.Not.Null,
                "DataResult<T> must retain ErrorMessage property for backward compatibility");
            Assert.That(errorMsgProp.PropertyType, Is.EqualTo(typeof(string)));
        }

        // ---------------------------------------------------------------
        // DataProviderError preserves all envelope fields
        // ---------------------------------------------------------------

        [Test]
        public void DataProviderError_HasStableCode()
        {
            Type errorType = FindType("NutriMind.Runtime.App.DataProviderError");
            Assert.That(errorType, Is.Not.Null);

            PropertyInfo codeProp = errorType.GetProperty("Code",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.That(codeProp, Is.Not.Null);
            Assert.That(codeProp.PropertyType, Is.EqualTo(typeof(string)));
        }

        [Test]
        public void DataProviderError_HasStableMessage()
        {
            Type errorType = FindType("NutriMind.Runtime.App.DataProviderError");
            Assert.That(errorType, Is.Not.Null);

            PropertyInfo msgProp = errorType.GetProperty("Message",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.That(msgProp, Is.Not.Null);
            Assert.That(msgProp.PropertyType, Is.EqualTo(typeof(string)));
        }

        // ---------------------------------------------------------------
        // HttpProvider failures populate structured error
        // ---------------------------------------------------------------

        [Test]
        public void HttpProvider_FailureResult_HasNonNullStructuredError()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueError(500, "{\"code\":\"SERVER_UNAVAILABLE\",\"message\":\"Server down.\"}");

            var config = new HttpProviderConfig { BaseUrl = "https://test.example" };
            var session = new AuthSessionState { Token = "token" };
            var provider = new HttpProvider(config, session, transport);

            // Use PingAsync rather than LogoutAsync because LogoutAsync
            // deliberately swallows the server response (best-effort design).
            DataResult<PingResponseDto> result = provider.PingAsync(CancellationToken.None).Result;

            Assert.That(result, Is.Not.Null, "PingAsync must return a non-null result");
            Assert.That(result.Success, Is.False, "Failed request must return failure");
            Assert.That(result.Error, Is.Not.Null,
                "HttpProvider failure must have non-null Error");
            Assert.That(result.Error.Code, Is.Not.Null.And.Not.Empty,
                "Error.Code must be non-empty");
            Assert.That(result.Error.Message, Is.Not.Null.And.Not.Empty,
                "Error.Message must be non-empty");
        }

        [Test]
        public void HttpProvider_ServerError_MapsToStructuredError()
        {
            var transport = new FakeHttpTransport();
            transport.EnqueueError(503, "{\"code\":\"SERVER_UNAVAILABLE\",\"message\":\"Temporarily unavailable.\",\"request_id\":\"req_123\",\"retryable\":true,\"retry_after_seconds\":2}");

            var config = new HttpProviderConfig { BaseUrl = "https://test.example", MaxRetries = 0 };
            var session = new AuthSessionState { Token = "token" };
            var provider = new HttpProvider(config, session, transport);

            DataResult<PingResponseDto> result = provider.PingAsync(CancellationToken.None).Result;

            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error.Code, Is.EqualTo("SERVER_UNAVAILABLE"));
            Assert.That(result.Error.RequestId, Is.EqualTo("req_123"));
            Assert.That(result.Error.Retryable, Is.True);
            Assert.That(result.Error.RetryAfterSeconds, Is.EqualTo(2));
        }

        // ---------------------------------------------------------------
        // LocalDemoJsonProvider failures populate structured error
        // ---------------------------------------------------------------

        [Test]
        public void LocalDemoJsonProvider_UnauthenticatedCall_HasNonNullStructuredError()
        {
            var provider = new LocalDemoJsonProvider();
            // An auth-gated call before login must fail with a structured, safe error.
            DataResult<StudentProfileDto> result = provider.GetProfileAsync(CancellationToken.None).Result;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.False, "Auth-gated call before login must fail");
            Assert.That(result.Error, Is.Not.Null,
                "LocalDemoJsonProvider failure must have non-null Error");
            Assert.That(result.Error.Code, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Error.Message, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void LocalDemoJsonProvider_NeverReportsNotImplemented()
        {
            var provider = new LocalDemoJsonProvider();
            // Public endpoint succeeds...
            DataResult<ApiConfigDto> config = provider.GetConfigAsync(CancellationToken.None).Result;
            // ...auth-gated endpoint fails with a real safe code, never the stub 'not_implemented'.
            DataResult<SettingsDto> settings = provider.GetSettingsAsync(CancellationToken.None).Result;

            Assert.That(settings.Success, Is.False);
            Assert.That(settings.Error.Code, Is.Not.EqualTo("not_implemented"),
                "Implemented LocalDemoJsonProvider must never report the stub 'not_implemented' code");
            if (config.Success)
                Assert.That(config.Data, Is.Not.Null);
        }

        // ---------------------------------------------------------------
        // Named method backward compat: Fail(string) still works
        // ---------------------------------------------------------------

        [Test]
        public void DataResult_Fail_WithMessage_CreatesNotImplementedCode()
        {
            var result = DataResult<string>.Fail("test failure message");
            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error.Code, Is.EqualTo("not_implemented"));
            Assert.That(result.ErrorMessage, Is.EqualTo("test failure message"));
        }

        [Test]
        public void DataResult_Fail_WithDataProviderError_PreservesCode()
        {
            var error = new DataProviderError("STATION_LOCKED", "Station is locked.");
            var result = DataResult<string>.Fail(error);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("STATION_LOCKED"));
            Assert.That(result.Error.Message, Is.EqualTo("Station is locked."));
        }

        [Test]
        public void DataResult_Ok_CarriesData()
        {
            var result = DataResult<string>.Ok("hello");
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.EqualTo("hello"));
            Assert.That(result.Error, Is.Null);
        }
    }
}