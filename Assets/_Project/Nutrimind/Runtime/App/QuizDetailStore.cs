using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Typed store that caches detailed question data for the active quiz.
    /// Populated when a quiz is selected/loaded, and cleared on logout or transition.
    /// </summary>
    public class QuizDetailStore
    {
        /// <summary>The currently cached quiz details, if any.</summary>
        public QuizDetailDto? CurrentQuiz { get; set; }

        /// <summary>Resets all state to defaults.</summary>
        public void Reset()
        {
            CurrentQuiz = null;
        }
    }
}
