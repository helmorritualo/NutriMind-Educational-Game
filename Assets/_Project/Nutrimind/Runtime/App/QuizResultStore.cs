using System.Collections.Generic;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Typed store that stores completed quiz results returned by the server.
    /// Includes both the last submitted attempt and historical results.
    /// </summary>
    public class QuizResultStore
    {
        /// <summary>
        /// The result of the last submitted quiz attempt, if any.
        /// </summary>
        public QuizAttemptResponseDto? LastAttemptResult { get; set; }

        /// <summary>
        /// Historical quiz results for the student.
        /// </summary>
        public List<QuizResultDto> Results { get; set; } = new();

        /// <summary>Resets all state to defaults.</summary>
        public void Reset()
        {
            LastAttemptResult = null;
            Results.Clear();
        }
    }
}
