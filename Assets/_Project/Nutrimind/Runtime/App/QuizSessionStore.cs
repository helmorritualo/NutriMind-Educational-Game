namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Tracks active quiz runtime state (active quiz ID, current question index, session active flag).
    /// </summary>
    public class QuizSessionStore
    {
        /// <summary>The identifier of the active quiz, if any.</summary>
        public string? ActiveQuizId { get; set; }

        /// <summary>The index of the currently active question.</summary>
        public int CurrentQuestionIndex { get; set; }

        /// <summary>Whether the quiz session is currently active.</summary>
        public bool IsActive { get; set; }

        /// <summary>Whether the quiz session is currently active (alias/compatibility property).</summary>
        public bool IsSessionActive
        {
            get => IsActive;
            set => IsActive = value;
        }

        /// <summary>Resets all state to defaults.</summary>
        public void Reset()
        {
            ActiveQuizId = null;
            CurrentQuestionIndex = 0;
            IsActive = false;
        }
    }
}
