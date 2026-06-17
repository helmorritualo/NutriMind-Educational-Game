using System.Collections.Generic;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Root provider-agnostic container for all optional station/learning narrative
    /// metadata returned by the data provider.  All fields are nullable or default
    /// to empty collections so that missing optional provider data is safe.
    /// </summary>
    public class StationNarrative
    {
        /// <summary>Optional story/world context for this station.</summary>
        public string? StoryContext { get; set; }

        /// <summary>Optional mission title shown to the student.</summary>
        public string? MissionTitle { get; set; }

        /// <summary>Optional mission summary / briefing text.</summary>
        public string? MissionSummary { get; set; }

        /// <summary>
        /// Ordered learning-cycle phases for this station as provided by the
        /// data provider.  Defaults to empty; never null.
        /// </summary>
        public List<LearningCyclePhase> LearningCycle { get; set; } = new();

        /// <summary>
        /// The current learning-cycle phase.  Defaults to <see cref="LearningCyclePhase.Discover"/>.
        /// Consumers may update this based on the student's progress through the
        /// station without modifying the provider-driven <see cref="LearningCycle"/> list.
        /// </summary>
        public LearningCyclePhase CurrentPhase { get; set; } = LearningCyclePhase.Discover;

        /// <summary>
        /// Optional NPC dialogue guides for this station.
        /// Defaults to empty; never null.
        /// </summary>
        public List<NpcGuide> NpcGuides { get; set; } = new();

        /// <summary>Optional hint policy configuration.</summary>
        public HintPolicy? HintPolicy { get; set; }

        /// <summary>
        /// Optional list of discoveries the student has made.
        /// Defaults to empty; never null.
        /// </summary>
        public List<DiscoveryEntry> Discoveries { get; set; } = new();

        /// <summary>Optional reflection prompt shown after station completion.</summary>
        public string? ReflectionPrompt { get; set; }

        /// <summary>
        /// Optional reward previews for motivation.
        /// Defaults to empty; never null.
        /// </summary>
        public List<RewardPreview> RewardPreviews { get; set; } = new();

        /// <summary>Optional world restoration state for returning to the 3D world.</summary>
        public WorldRestorationState? WorldRestorationState { get; set; }

        /// <summary>Optional success feedback shown on task completion.</summary>
        public SuccessFeedback? SuccessFeedback { get; set; }

        /// <summary>Optional mistake/attempt feedback shown on incorrect answers.</summary>
        public MistakeFeedback? MistakeFeedback { get; set; }
    }

    /// <summary>
    /// Represents an NPC guide who provides dialogue and guidance
    /// during a station's narrative flow.
    /// </summary>
    public class NpcGuide
    {
        /// <summary>Stable key for referencing this guide (provider-driven).</summary>
        public string? GuideKey { get; set; }

        /// <summary>Display name of the NPC.</summary>
        public string? Name { get; set; }

        /// <summary>Key or identifier for the NPC's avatar/portrait asset.</summary>
        public string? AvatarKey { get; set; }

        /// <summary>Dialogue text spoken by this NPC.</summary>
        public string? Dialogue { get; set; }
    }

    /// <summary>
    /// Configuration for the hint system within a station.
    /// </summary>
    public class HintPolicy
    {
        /// <summary>Maximum number of hint tiers available.</summary>
        public int MaxTiers { get; set; }

        /// <summary>
        /// Ordered list of hint tiers.  Defaults to empty; never null.
        /// </summary>
        public List<HintTier> Tiers { get; set; } = new();
    }

    /// <summary>
    /// A single tier within a hint hierarchy.
    /// </summary>
    public class HintTier
    {
        /// <summary>Zero-based tier index (0 = first/general hint).</summary>
        public int Tier { get; set; }

        /// <summary>The hint text shown at this tier.</summary>
        public string? Text { get; set; }
    }

    /// <summary>
    /// A discovery entry recorded during station gameplay.
    /// </summary>
    public class DiscoveryEntry
    {
        /// <summary>Stable key for referencing this discovery (provider-driven).</summary>
        public string? DiscoveryKey { get; set; }

        /// <summary>Title of the discovery.</summary>
        public string? Title { get; set; }

        /// <summary>Description of what was discovered.</summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// A preview of a reward that can be earned, for motivational display.
    /// Does NOT represent an earned reward — <see cref="ProgressRewardStore"/> holds actual rewards.
    /// </summary>
    public class RewardPreview
    {
        /// <summary>Stable key for referencing this reward (provider-driven).</summary>
        public string? RewardKey { get; set; }

        /// <summary>Optional category/type of reward (e.g. ""badge"", ""item"", ""unlock"").</summary>
        public string? RewardType { get; set; }

        /// <summary>Display name shown to the student.</summary>
        public string? DisplayName { get; set; }

        /// <summary>Key or identifier for the reward's icon asset.</summary>
        public string? IconKey { get; set; }
    }

    /// <summary>
    /// Describes the state the 3D world should return to after a station,
    /// e.g. which objects are restored, which NPCs have moved, etc.
    /// </summary>
    public class WorldRestorationState
    {
        /// <summary>Stable key identifying this restoration state (provider-driven).</summary>
        public string? StateKey { get; set; }

        /// <summary>Arbitrary restoration payload (provider-specific).</summary>
        public object? StateData { get; set; }
    }

    /// <summary>
    /// Feedback presented to the student on successful task completion.
    /// Presentation-only; no scoring/correctness authority.
    /// </summary>
    public class SuccessFeedback
    {
        /// <summary>Primary success message.</summary>
        public string? Message { get; set; }

        /// <summary>
        /// Additional encouraging phrases.  Defaults to empty; never null.
        /// </summary>
        public List<string> EncouragingPhrases { get; set; } = new();
    }

    /// <summary>
    /// Feedback presented to the student on an incorrect attempt.
    /// Presentation-only; no scoring/correctness authority.
    /// </summary>
    public class MistakeFeedback
    {
        /// <summary>Optional misconception-aware message.</summary>
        public string? MisconceptionMessage { get; set; }

        /// <summary>Encouraging message to keep the student motivated.</summary>
        public string? EncouragingMessage { get; set; }

        /// <summary>Safe retry action label (e.g. ""Try Again"", ""Review Hints"").</summary>
        public string? RetryAction { get; set; }

        /// <summary>Current hint tier the student is on (null if no hint has been shown).</summary>
        public int? CurrentHintTier { get; set; }

        /// <summary>Remaining attempts before lockout (null if unlimited).</summary>
        public int? RemainingAttempts { get; set; }
    }
}
