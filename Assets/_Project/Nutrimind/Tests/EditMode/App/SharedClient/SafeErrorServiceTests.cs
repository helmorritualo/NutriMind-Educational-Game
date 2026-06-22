using NUnit.Framework;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class SafeErrorServiceTests
    {
        private SafeErrorService _service;

        [SetUp]
        public void Setup()
        {
            _service = new SafeErrorService();
        }

        [Test]
        public void RedactToken_WithJwtLikeToken_RedactsToken()
        {
            string tokenText = "My token is abcdefghij.klmnop.qrstuvwxyz";
            string result = _service.RedactToken(tokenText);
            Assert.That(result, Contains.Substring("***REDACTED_TOKEN***"));
            Assert.That(result, Does.Not.Contain("abcdefghij"));
        }

        [Test]
        public void RedactToken_WithNull_ReturnsEmptyString()
        {
            string result = _service.RedactToken(null);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void RedactPin_WithDigitPin_RedactsPin()
        {
            string pinText = "The secret PIN is 12345";
            string result = _service.RedactPin(pinText);
            Assert.That(result, Contains.Substring("***REDACTED***"));
            Assert.That(result, Does.Not.Contain("12345"));
        }

        [Test]
        public void RedactPin_WithNull_ReturnsEmptyString()
        {
            string result = _service.RedactPin(null);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void RedactAll_WithTokensAndPins_RedactsBoth()
        {
            string mixedText = "Token: abcdefghij.klmnop.qrstuvwxyz and PIN: 43216";
            string result = _service.RedactAll(mixedText);
            Assert.That(result, Contains.Substring("***REDACTED_TOKEN***"));
            Assert.That(result, Contains.Substring("***REDACTED***"));
        }

        [Test]
        public void RedactAll_WithAnswerKeys_RedactsAnswers()
        {
            string answerText = "The answer is correct_answer: \"New York\" and answer_key = \"Fact or Opinion\"";
            string result = _service.RedactAll(answerText);
            Assert.That(result, Contains.Substring("***REDACTED_ANSWER***"));
            Assert.That(result, Does.Not.Contain("New York"));
            Assert.That(result, Does.Not.Contain("Fact or Opinion"));
        }

        [Test]
        public void RedactAll_WithStackPathsAndFrames_RedactsPathsAndFrames()
        {
            string stackText = "in C:\\Dev-Env\\Module.cs:line 42\n  at NutriMind.Runtime.App.HttpProvider.SendWithRetryAsync()";
            string result = _service.RedactAll(stackText);
            Assert.That(result, Contains.Substring("in ***REDACTED_PATH***"));
            Assert.That(result, Contains.Substring("***REDACTED_STACK_FRAME***"));
        }

        [Test]
        public void RedactAll_WithSqlContent_RedactsSql()
        {
            string sqlText = "Executing query SELECT * FROM Users WHERE Id = 5";
            string result = _service.RedactAll(sqlText);
            Assert.That(result, Contains.Substring("***REDACTED_SQL***"));
            Assert.That(result, Does.Not.Contain("SELECT"));
            Assert.That(result, Does.Not.Contain("Users"));
        }

        [Test]
        public void SanitizeDiagnostics_DelegatesToRedactAll()
        {
            string mixedText = "Token: abcdefghij.klmnop.qrstuvwxyz and PIN: 43216";
            string result = _service.SanitizeDiagnostics(mixedText);
            Assert.That(result, Contains.Substring("***REDACTED_TOKEN***"));
            Assert.That(result, Contains.Substring("***REDACTED***"));
        }

        [Test]
        public void LogMethods_DoNotThrow()
        {
            Assert.DoesNotThrow(() => _service.LogSafe("Safe message with PIN 1234"));
            Assert.DoesNotThrow(() => _service.LogDiagnostic("Diagnostic message with Token abcdefghij.klmnop.qrstuvwxyz"));
        }
    }
}
