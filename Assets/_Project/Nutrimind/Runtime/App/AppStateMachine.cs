using System.Collections.Generic;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Pure C# state machine for the NutriMind game application flow.
    /// Encodes all canonical transitions, subject-aware rules,
    /// and provides <see cref="InvalidateInFlightOperations"/> for
    /// pause/resume cancellation modeling.
    /// </summary>
    /// <remarks>
    /// This is a shared foundation — no Unity lifecycle, scene, network,
    /// or UI dependencies. Used by both <see cref="DataProviderMode.LocalDemoJson"/>
    /// and <see cref="DataProviderMode.Http"/> providers.
    /// </remarks>
    public class AppStateMachine
    {
        private AppState _currentState = AppState.Starting;
        private SubjectType? _selectedSubject;
        private int _operationGeneration;

        // ──────────────────────────────────────────────────────────────
        //  Transition table — valid (from, to) pairs
        // ──────────────────────────────────────────────────────────────
        private static readonly HashSet<(AppState, AppState)> s_allowed = new()
        {
            // Starting — entry point
            (AppState.Starting, AppState.Starting),
            (AppState.Starting, AppState.CheckingServer),
            (AppState.Starting, AppState.LoggedOut),
            (AppState.Starting, AppState.FatalConfigurationError),

            // Checking server
            (AppState.CheckingServer, AppState.CheckingServer),
            (AppState.CheckingServer, AppState.LoggedOut),
            (AppState.CheckingServer, AppState.MaintenanceBlocked),
            (AppState.CheckingServer, AppState.UpdateRequired),

            // Logged out — unauthenticated base
            (AppState.LoggedOut, AppState.LoggedOut),
            (AppState.LoggedOut, AppState.Authenticating),

            // Authenticating
            (AppState.Authenticating, AppState.Authenticating),
            (AppState.Authenticating, AppState.Bootstrapping),
            (AppState.Authenticating, AppState.LoggedOut),

            // Bootstrapping
            (AppState.Bootstrapping, AppState.Bootstrapping),
            (AppState.Bootstrapping, AppState.MainMenu),

            // Main menu
            (AppState.MainMenu, AppState.MainMenu),
            (AppState.MainMenu, AppState.SelectingSubject),
            (AppState.MainMenu, AppState.LoggedOut),
            (AppState.MainMenu, AppState.SessionExpired),

            // Subject selection
            (AppState.SelectingSubject, AppState.SelectingSubject),
            (AppState.SelectingSubject, AppState.SelectingTerm),

            // Term selection
            (AppState.SelectingTerm, AppState.SelectingTerm),
            (AppState.SelectingTerm, AppState.LoadingWorld),

            // World loading → entry or error
            (AppState.LoadingWorld, AppState.LoadingWorld),
            (AppState.LoadingWorld, AppState.InWorld),
            (AppState.LoadingWorld, AppState.ConnectionUnavailable),
            (AppState.LoadingWorld, AppState.SessionExpired),

            // In world
            (AppState.InWorld, AppState.InWorld),
            (AppState.InWorld, AppState.StartingStation),
            (AppState.InWorld, AppState.ConnectionUnavailable),
            (AppState.InWorld, AppState.SessionExpired),

            // Station flow
            (AppState.StartingStation, AppState.StartingStation),
            (AppState.StartingStation, AppState.InStationTask),
            (AppState.StartingStation, AppState.ShowingMissionBrief),
            (AppState.StartingStation, AppState.SessionExpired),

            (AppState.ShowingMissionBrief, AppState.ShowingMissionBrief),
            (AppState.ShowingMissionBrief, AppState.InStationTask),
            (AppState.ShowingMissionBrief, AppState.SessionExpired),

            (AppState.InStationTask, AppState.InStationTask),
            (AppState.InStationTask, AppState.SubmittingAttempt),
            (AppState.InStationTask, AppState.ShowingHintOverlay),
            (AppState.InStationTask, AppState.SessionExpired),

            (AppState.ShowingHintOverlay, AppState.ShowingHintOverlay),
            (AppState.ShowingHintOverlay, AppState.InStationTask),
            (AppState.ShowingHintOverlay, AppState.SessionExpired),

            (AppState.SubmittingAttempt, AppState.SubmittingAttempt),
            (AppState.SubmittingAttempt, AppState.ShowingFeedback),
            (AppState.SubmittingAttempt, AppState.SessionExpired),

            (AppState.ShowingFeedback, AppState.ShowingFeedback),
            (AppState.ShowingFeedback, AppState.InStationTask),
            (AppState.ShowingFeedback, AppState.ShowingReflection),
            (AppState.ShowingFeedback, AppState.RefreshingProgress),
            (AppState.ShowingFeedback, AppState.SessionExpired),

            (AppState.ShowingReflection, AppState.ShowingReflection),
            (AppState.ShowingReflection, AppState.ShowingRewardPresentation),
            (AppState.ShowingReflection, AppState.RefreshingProgress),
            (AppState.ShowingReflection, AppState.SessionExpired),

            (AppState.ShowingRewardPresentation, AppState.ShowingRewardPresentation),
            (AppState.ShowingRewardPresentation, AppState.RefreshingProgress),
            (AppState.ShowingRewardPresentation, AppState.SessionExpired),

            (AppState.RefreshingProgress, AppState.RefreshingProgress),
            (AppState.RefreshingProgress, AppState.ReturningToWorld),
            (AppState.RefreshingProgress, AppState.SessionExpired),

            // Returning to world
            (AppState.ReturningToWorld, AppState.ReturningToWorld),
            (AppState.ReturningToWorld, AppState.InWorld),
            (AppState.ReturningToWorld, AppState.SessionExpired),

            // Session expired → re-authenticate
            (AppState.SessionExpired, AppState.SessionExpired),
            (AppState.SessionExpired, AppState.LoggedOut),

            // Connection unavailable → return to menu
            (AppState.ConnectionUnavailable, AppState.ConnectionUnavailable),
            (AppState.ConnectionUnavailable, AppState.MainMenu),

            // Fatal error — terminal unless app restarts externally
            (AppState.FatalConfigurationError, AppState.FatalConfigurationError),
        };

        /// <summary>
        /// The current application state.
        /// </summary>
        public AppState CurrentState => _currentState;

        /// <summary>
        /// Monotonically increasing generation counter.  Incremented by
        /// <see cref="InvalidateInFlightOperations"/> to signal that
        /// previously initiated async operations should be considered
        /// stale (e.g. after a pause/resume cycle).
        /// </summary>
        public int OperationGeneration => _operationGeneration;

        /// <summary>
        /// Attempts a transition to <paramref name="targetState"/>.
        /// Returns true if the transition is allowed by the encoded rules
        /// and was applied; returns false and leaves <see cref="CurrentState"/>
        /// unchanged otherwise.
        /// </summary>
        public bool TryTransition(AppState targetState)
        {
            var key = (_currentState, targetState);

            if (!s_allowed.Contains(key))
                return false;

            // Only LiteraQuest and HealthQuest may enter a station.
            if (targetState == AppState.StartingStation &&
                _selectedSubject != SubjectType.LiteraQuest &&
                _selectedSubject != SubjectType.HealthQuest)
            {
                return false;
            }

            _currentState = targetState;
            return true;
        }

        /// <summary>
        /// Registers the currently selected subject for subject-aware
        /// transition rules.  Only succeeds when the machine is in
        /// <see cref="AppState.SelectingSubject"/>.
        /// </summary>
        /// <returns>True if the subject was registered; false if the
        /// machine is not in SelectingSubject.</returns>
        public bool TrySelectSubject(SubjectType subject)
        {
            if (_currentState != AppState.SelectingSubject)
                return false;

            _selectedSubject = subject;
            return true;
        }

        /// <summary>
        /// Invalidates any in-flight operations (e.g. after a pause
        /// or cancellation event so stale completions are ignored).
        /// Increments <see cref="OperationGeneration"/> each invocation.
        /// </summary>
        public void InvalidateInFlightOperations()
        {
            _operationGeneration++;
        }
    }
}