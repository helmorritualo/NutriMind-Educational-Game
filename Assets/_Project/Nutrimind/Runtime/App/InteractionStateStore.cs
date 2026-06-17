using System.Collections.Generic;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Typed store for transient interaction state.
    /// Holds ephemeral data such as the current prompt, selected options,
    /// or UI interaction context that is reset on station transitions
    /// or logout.
    /// </summary>
    public class InteractionStateStore
    {
        /// <summary>Current prompt or question text being shown.</summary>
        public string? CurrentPrompt { get; set; }

        /// <summary>Arbitrary interaction context object.</summary>
        public object? ContextData { get; set; }

        // ──────────────────────────────────────────────────────────────
        //  Phase 04A: learning / narrative / feedback state
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// The current station's full narrative metadata from the provider.
        /// Null when no station is active.
        /// </summary>
        public StationNarrative? CurrentStationNarrative { get; set; }

        /// <summary>
        /// Current learning-cycle phase within the active station.
        /// Defaults to <see cref="LearningCyclePhase.Discover"/>.
        /// This is a presentation hint — the provider's <see cref="StationNarrative.LearningCycle"/>
        /// list is the authoritative sequence.
        /// </summary>
        public LearningCyclePhase CurrentPhase { get; set; } = LearningCyclePhase.Discover;

        /// <summary>
        /// The most recent mistake/attempt feedback from the provider.
        /// Null when no attempt has been submitted or after a successful attempt.
        /// Presentation-only; no scoring/correctness authority.
        /// </summary>
        public MistakeFeedback? LastFeedback { get; set; }

        /// <summary>Resets all state to defaults.</summary>
        public void Reset()
        {
            CurrentPrompt = null;
            ContextData = null;
            CurrentStationNarrative = null;
            CurrentPhase = LearningCyclePhase.Discover;
            LastFeedback = null;
        }
    }
}
