using System;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Non-static service layer that redacts sensitive text (JWTs, PINs, answer keys, paths)
    /// delegating to SafeDiagnostics.
    /// </summary>
    public class SafeErrorService
    {
        /// <summary>
        /// Redacts JWT-like tokens from the given text.
        /// </summary>
        public string RedactToken(string? text)
        {
            return SafeDiagnostics.RedactToken(text);
        }

        /// <summary>
        /// Redacts PIN-like digit sequences from the given text.
        /// </summary>
        public string RedactPin(string? text)
        {
            return SafeDiagnostics.RedactPin(text);
        }

        /// <summary>
        /// Comprehensive redaction: removes tokens, PINs, answer-key labels,
        /// correct-answer labels, stack-trace file-path snippets, stack frames,
        /// and SQL-like content.
        /// </summary>
        public string RedactAll(string? text)
        {
            return SafeDiagnostics.RedactAll(text);
        }

        /// <summary>
        /// Synonym for RedactAll.
        /// </summary>
        public string SanitizeDiagnostics(string? text)
        {
            return SafeDiagnostics.SanitizeDiagnostics(text);
        }

        /// <summary>
        /// Logs a message safely, ensuring all sensitive data is redacted.
        /// </summary>
        public void LogSafe(string? message)
        {
            SafeDiagnostics.LogSafe(message);
        }

        /// <summary>
        /// Synonym for LogSafe.
        /// </summary>
        public void LogDiagnostic(string? message)
        {
            SafeDiagnostics.LogDiagnostic(message);
        }
    }
}
