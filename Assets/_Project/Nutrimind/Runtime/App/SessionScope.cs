namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Session-scoped context for the current user session.
    /// Holds the data provider mode and session-owned state properties
    /// that <see cref="Clear"/> resets on logout or session expiry.
    /// </summary>
    public class SessionScope
    {
        /// <summary>
        /// Active data provider mode for this session.
        /// Set once at startup and preserved across clear/logout.
        /// </summary>
        public DataProviderMode Mode { get; set; } = DataProviderMode.LocalDemoJson;

        // ──────────────────────────────────────────────────────────────
        //  Legacy session-owned state (object buckets) — reset on Clear()
        // ──────────────────────────────────────────────────────────────

        /// <summary>Current authentication token, if any.</summary>
        public string? AuthToken { get; set; }

        /// <summary>Cached user profile data.</summary>
        public object? ProfileData { get; set; }

        /// <summary>User settings / preferences.</summary>
        public object? Settings { get; set; }

        /// <summary>Currently selected subject, if any.</summary>
        public SubjectType? SelectedSubject { get; set; }

        /// <summary>Cached progress data for the selected subject.</summary>
        public object? ProgressData { get; set; }

        /// <summary>Earned rewards / achievements state.</summary>
        public object? RewardState { get; set; }

        /// <summary>Transient interaction state (e.g. current prompt).</summary>
        public object? InteractionState { get; set; }

        // ──────────────────────────────────────────────────────────────
        //  Typed store properties — reset on Clear()
        // ──────────────────────────────────────────────────────────────

        /// <summary>Typed authentication session state.</summary>
        public AuthSessionState? AuthSessionState { get; set; }

        /// <summary>Typed student profile store.</summary>
        public StudentProfileStore? StudentProfileStore { get; set; }

        /// <summary>Typed settings store.</summary>
        public SettingsStore? SettingsStore { get; set; }

        /// <summary>Typed subject/term store.</summary>
        public SubjectTermStore? SubjectTermStore { get; set; }

        /// <summary>Typed progress and reward store.</summary>
        public ProgressRewardStore? ProgressRewardStore { get; set; }

        /// <summary>Typed interaction state store.</summary>
        public InteractionStateStore? InteractionStateStore { get; set; }

        /// <summary>Typed quiz availability store.</summary>
        public QuizAvailabilityStore? QuizAvailabilityStore { get; set; }

        /// <summary>Typed quiz detail store.</summary>
        public QuizDetailStore? QuizDetailStore { get; set; }

        /// <summary>Typed quiz session store.</summary>
        public QuizSessionStore? QuizSessionStore { get; set; }

        /// <summary>Typed quiz answer draft store.</summary>
        public QuizAnswerDraftStore? QuizAnswerDraftStore { get; set; }

        /// <summary>Typed quiz result store.</summary>
        public QuizResultStore? QuizResultStore { get; set; }

        /// <summary>
        /// Resets all session-owned state to defaults.
        /// Called on logout and session expiry.
        /// The data provider <see cref="Mode"/> is intentionally preserved
        /// — it is chosen at application startup and must survive logout.
        /// </summary>
        public void Clear()
        {
            // Mode is preserved across clear.
            AuthToken = null;
            ProfileData = null;
            Settings = null;
            SelectedSubject = null;
            ProgressData = null;
            RewardState = null;
            InteractionState = null;

            // Reset typed stores.
            AuthSessionState = null;
            StudentProfileStore = null;
            SettingsStore = null;
            SubjectTermStore = null;
            ProgressRewardStore = null;
            InteractionStateStore = null;
            QuizAvailabilityStore = null;
            QuizDetailStore = null;
            QuizSessionStore = null;
            QuizAnswerDraftStore = null;
            QuizResultStore = null;
        }
    }
}
