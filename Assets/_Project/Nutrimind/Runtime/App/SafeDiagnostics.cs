using System;
using System.Text.RegularExpressions;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Static utility class for safe diagnostic logging.
    /// Provides methods to redact sensitive data (tokens, PINs, answer keys,
    /// raw private strings) before they reach logs or telemetry.
    ///
    /// No dependency on UnityEngine.Debug — all output goes to
    /// <see cref="System.Console"/>.
    /// </summary>
    public static class SafeDiagnostics
    {
        // JWT-like token: three dot-separated base64 segments.
        private static readonly Regex s_tokenPattern = new(
            @"[a-zA-Z0-9_\-]{10,}\.[a-zA-Z0-9_\-]{4,}\.[a-zA-Z0-9_\-]+",
            RegexOptions.Compiled);

        // Consecutive digit sequences of length 4-8 that look like PINs.
        private static readonly Regex s_pinPattern = new(
            @"\b\d{4,8}\b",
            RegexOptions.Compiled);

        // Answer-key / correct-answer pattern: matches the label AND its
        // associated value, handling quoted multi-word strings or unquoted
        // scalars (e.g. "correct_answer":"New York", answer_key = "Fact or Opinion",
        // or correct_answer = 42).
        private static readonly Regex s_answerPattern = new("\"?\\b(answer_key|correct_answer)\\b\"?(\\s*[:=]\\s*(\"[^\"]*\"|'[^']*'|[^\\r\\n,;}]+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Stack-trace file-path snippet matching Windows absolute paths with
        // spaces, hyphens, or other path characters (e.g.
        //   "in C:\Dev-Env\Module.cs:line 42"
        //   "in C:\NutriMind Game\Runner.cs"
        //   "in C:/Dev-Environment/SomeDir/file.cs")
        private static readonly Regex s_stackPathPattern = new("in\\s+[A-Za-z]:[^\\r\\n]*?\\.cs(?::line\\s+\\d+)?", RegexOptions.Compiled);

        // .NET/Java stack-trace method-frame pattern matching lines like
        // "at NutriMind.Runtime.App.HttpProvider.SendWithRetryAsync(...)"
        // or "at SomeClass.SomeMethod()"
        private static readonly Regex s_stackFramePattern = new(
            @"\s*at\s+[a-zA-Z_][a-zA-Z0-9_.]*\.[a-zA-Z_][a-zA-Z0-9_<>]*\(.*?\)",
            RegexOptions.Compiled);

        // SQL-like content pattern: matches common SQL keywords and the
        // statement content up to the next period, semicolon, or newline.
        private static readonly Regex s_sqlPattern = new(
            @"\b(SELECT|INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|EXEC|EXECUTE)\b[^;.\n]*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Redacts JWT-like tokens from the given text.
        /// </summary>
        /// <param name="text">Text that may contain a token. Null-safe.</param>
        /// <returns>Text with token replaced by a safe placeholder; empty string if input was null.</returns>
        public static string RedactToken(string? text)
        {
            if (text == null) return string.Empty;
            return s_tokenPattern.Replace(text, "***REDACTED_TOKEN***");
        }

        /// <summary>
        /// Redacts PIN-like digit sequences from the given text.
        /// </summary>
        /// <param name="text">Text that may contain a PIN. Null-safe.</param>
        /// <returns>Text with PINs replaced by a safe placeholder; empty string if input was null.</returns>
        public static string RedactPin(string? text)
        {
            if (text == null) return string.Empty;
            return s_pinPattern.Replace(text, "***REDACTED***");
        }

        /// <summary>
        /// Comprehensive redaction: removes tokens, PINs, answer-key labels,
        /// correct-answer labels, stack-trace file-path snippets, stack frames,
        /// and SQL-like content.
        /// </summary>
        /// <param name="text">Text to sanitize. Null-safe.</param>
        /// <returns>Sanitized text; empty string if input was null.</returns>
        public static string RedactAll(string? text)
        {
            if (text == null) return string.Empty;
            string result = RedactToken(text);
            result = RedactPin(result);
            result = s_answerPattern.Replace(result, "***REDACTED_ANSWER***");
            result = s_stackPathPattern.Replace(result, "in ***REDACTED_PATH***");
            result = s_stackFramePattern.Replace(result, "***REDACTED_STACK_FRAME***");
            result = s_sqlPattern.Replace(result, "***REDACTED_SQL***");
            return result;
        }

        /// <summary>
        /// Synonym for <see cref="RedactAll"/>.
        /// </summary>
        public static string SanitizeDiagnostics(string? text)
        {
            return RedactAll(text);
        }

        /// <summary>
        /// Logs a message safely, ensuring all sensitive data is redacted
        /// before output.  Writes to <see cref="System.Console"/>.
        /// </summary>
        /// <param name="message">The message to log. Null-safe.</param>
        public static void LogSafe(string? message)
        {
            string safe = RedactAll(message);
            System.Console.WriteLine($"[NutriMind SafeDiagnostics] {safe}");
        }

        /// <summary>
        /// Synonym for <see cref="LogSafe"/>.
        /// </summary>
        public static void LogDiagnostic(string? message)
        {
            LogSafe(message);
        }
    }
}
