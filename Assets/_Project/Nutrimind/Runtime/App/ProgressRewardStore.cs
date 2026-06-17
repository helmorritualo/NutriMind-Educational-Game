using System.Collections.Generic;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Typed store for the student's progress data and earned rewards.
    /// Populated by the data-provider layer and cleared on logout.
    /// No scoring/progress/reward authority — data is read-only from the server.
    /// </summary>
    public class ProgressRewardStore
    {
        /// <summary>Cached progress data (serialized from the provider).</summary>
        public object? ProgressData { get; set; }

        /// <summary>Cached reward/achievement state.</summary>
        public object? RewardState { get; set; }

        // ──────────────────────────────────────────────────────────────
        //  Phase 04A: typed reward and restoration state
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Provider-confirmed world restoration state for returning to the 3D world
        /// after completing a station.  Null when no restoration is pending.
        /// </summary>
        public WorldRestorationState? RestorationState { get; set; }

        /// <summary>
        /// Reward previews provided by the provider for motivational display.
        /// Defaults to empty; never null.
        /// These are NOT earned rewards — they are previews for presentation only.
        /// </summary>
        public List<RewardPreview> RewardPreviews { get; set; } = new();

        /// <summary>Resets all state to defaults.</summary>
        public void Reset()
        {
            ProgressData = null;
            RewardState = null;
            RestorationState = null;
            RewardPreviews.Clear();
        }
    }
}
