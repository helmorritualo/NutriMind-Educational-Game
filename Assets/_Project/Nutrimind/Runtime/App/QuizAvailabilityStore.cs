using System.Collections.Generic;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Typed store for available quizzes for the selected subject/term.
    /// Populated by the data-provider layer and cleared on logout.
    /// </summary>
    public class QuizAvailabilityStore
    {
        /// <summary>The list of available quizzes.</summary>
        public List<QuizDto> Quizzes { get; set; } = new();

        /// <summary>Resets all state to defaults.</summary>
        public void Reset()
        {
            Quizzes.Clear();
        }
    }
}
