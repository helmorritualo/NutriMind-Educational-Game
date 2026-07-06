namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Typed store for the currently selected subject and term.
    /// Used during subject/term selection flow and cleared on logout.
    /// Includes term-related state such as <see cref="CurrentTerm"/>.
    /// </summary>
    public class SubjectTermStore
    {
        /// <summary>The currently selected subject, if any.</summary>
        public SubjectType? SelectedSubject { get; set; }

        /// <summary>The currently selected term identifier, if any.</summary>
        public string? CurrentTerm { get; set; }

        /// <summary>Scene registry key for LoadingTransition to load after preload (e.g. mission gameplay).</summary>
        public string? PendingSceneKey { get; set; }

        /// <summary>Resets all state to defaults.</summary>
        public void Reset()
        {
            SelectedSubject = null;
            CurrentTerm = null;
            PendingSceneKey = null;
        }
    }
}
