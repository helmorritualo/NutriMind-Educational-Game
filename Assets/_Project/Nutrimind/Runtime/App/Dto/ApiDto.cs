using System.Collections.Generic;
using Newtonsoft.Json;

namespace NutriMind.Runtime.App.Dto
{
    // ──────────────────────────────────────────────────────────────
    //  Config & Connectivity
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Response from <c>GET /api/v1/student/ping</c>.
    /// Lightweight connectivity check — no auth required.
    /// </summary>
    public class PingResponseDto
    {
        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("server_time", NullValueHandling = NullValueHandling.Ignore)]
        public string? ServerTime { get; set; }

        [JsonProperty("api_version", NullValueHandling = NullValueHandling.Ignore)]
        public string? ApiVersion { get; set; }
    }

    /// <summary>
    /// Response from <c>GET /api/v1/student/config</c>.
    /// Public contract so Unity does not hardcode fragile assumptions.
    /// </summary>
    public class ApiConfigDto
    {
        [JsonProperty("api_version")]
        public string? ApiVersion { get; set; }

        [JsonProperty("contract_version", NullValueHandling = NullValueHandling.Ignore)]
        public string? ContractVersion { get; set; }

        [JsonProperty("server_time", NullValueHandling = NullValueHandling.Ignore)]
        public string? ServerTime { get; set; }

        [JsonProperty("maintenance_mode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MaintenanceMode { get; set; }

        [JsonProperty("minimum_unity_client_version", NullValueHandling = NullValueHandling.Ignore)]
        public string? MinimumUnityClientVersion { get; set; }

        [JsonProperty("supported_languages", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? SupportedLanguages { get; set; }

        [JsonProperty("polling", NullValueHandling = NullValueHandling.Ignore)]
        public PollingConfig? Polling { get; set; }

        [JsonProperty("realtime", NullValueHandling = NullValueHandling.Ignore)]
        public RealtimeConfig? Realtime { get; set; }
    }

    /// <summary>
    /// Polling configuration from the server config endpoint.
    /// </summary>
    public class PollingConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("default_interval_seconds", NullValueHandling = NullValueHandling.Ignore)]
        public int? DefaultIntervalSeconds { get; set; }

        [JsonProperty("minimum_interval_seconds", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinimumIntervalSeconds { get; set; }
    }

    /// <summary>
    /// Realtime configuration from the server config endpoint.
    /// WSS is optional metadata only; HTTPS REST must support core gameplay.
    /// </summary>
    public class RealtimeConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("transport", NullValueHandling = NullValueHandling.Ignore)]
        public string? Transport { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string? Url { get; set; }

        [JsonProperty("events_are_metadata_only", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EventsAreMetadataOnly { get; set; }
    }

    // ──────────────────────────────────────────────────────────────
    //  Auth
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Request body for <c>POST /api/v1/student/auth/login</c>.
    /// </summary>
    public class LoginRequestDto
    {
        [JsonProperty("lrn")]
        public string? Lrn { get; set; }

        [JsonProperty("pin")]
        public string? Pin { get; set; }

        [JsonProperty("device_name", NullValueHandling = NullValueHandling.Ignore)]
        public string? DeviceName { get; set; }

        [JsonProperty("client_version", NullValueHandling = NullValueHandling.Ignore)]
        public string? ClientVersion { get; set; }
    }

    /// <summary>
    /// Response from <c>POST /api/v1/student/auth/login</c>.
    /// Token is returned only once at login. Stored token hashes
    /// must not be exposed in dashboards or APIs.
    /// </summary>
    public class LoginResponseDto
    {
        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("token_type", NullValueHandling = NullValueHandling.Ignore)]
        public string? TokenType { get; set; }

        [JsonProperty("student", NullValueHandling = NullValueHandling.Ignore)]
        public StudentIdentityDto? Student { get; set; }
    }

    /// <summary>
    /// Student identity returned inside a login response.
    /// Student-safe: never includes raw PIN, tokens, or stack traces.
    /// </summary>
    public class StudentIdentityDto
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

        [JsonProperty("lrn_masked", NullValueHandling = NullValueHandling.Ignore)]
        public string? LrnMasked { get; set; }

        [JsonProperty("grade_level", NullValueHandling = NullValueHandling.Ignore)]
        public int? GradeLevel { get; set; }

        [JsonProperty("language_preference", NullValueHandling = NullValueHandling.Ignore)]
        public string? LanguagePreference { get; set; }
    }

    /// <summary>
    /// Request body for <c>POST /api/v1/student/auth/logout</c>.
    /// </summary>
    public class LogoutRequestDto
    {
        // Logout may carry an empty body or require only the auth token.
        // This DTO exists for future extensibility.
    }

    // ──────────────────────────────────────────────────────────────
    //  Bootstrap & Profile
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Response from <c>GET /api/v1/student/bootstrap</c>.
    /// Initial student/classroom/subject/wallet snapshot.
    /// </summary>
    public class BootstrapDto
    {
        [JsonProperty("student", NullValueHandling = NullValueHandling.Ignore)]
        public StudentIdentityDto? Student { get; set; }

        [JsonProperty("classroom", NullValueHandling = NullValueHandling.Ignore)]
        public ClassroomDto? Classroom { get; set; }

        [JsonProperty("subjects", NullValueHandling = NullValueHandling.Ignore)]
        public List<SubjectDto>? Subjects { get; set; }

        [JsonProperty("progress_summary", NullValueHandling = NullValueHandling.Ignore)]
        public ProgressSummaryDto? ProgressSummary { get; set; }

        [JsonProperty("rewards", NullValueHandling = NullValueHandling.Ignore)]
        public RewardWalletDto? Rewards { get; set; }

        [JsonProperty("settings", NullValueHandling = NullValueHandling.Ignore)]
        public SettingsDto? Settings { get; set; }

        [JsonProperty("sync_status", NullValueHandling = NullValueHandling.Ignore)]
        public SyncStatusDto? SyncStatus { get; set; }
    }

    /// <summary>
    /// Classroom summary within bootstrap.
    /// </summary>
    public class ClassroomDto
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string? Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

        [JsonProperty("grade_level", NullValueHandling = NullValueHandling.Ignore)]
        public int? GradeLevel { get; set; }

        [JsonProperty("section", NullValueHandling = NullValueHandling.Ignore)]
        public string? Section { get; set; }
    }

    /// <summary>
    /// Response from <c>GET /api/v1/student/profile</c>.
    /// Student-safe profile; never exposes raw tokens, PINs,
    /// answer keys, scoring rules, or private data.
    /// </summary>
    public class StudentProfileDto
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

        [JsonProperty("lrn_masked", NullValueHandling = NullValueHandling.Ignore)]
        public string? LrnMasked { get; set; }

        [JsonProperty("grade_level", NullValueHandling = NullValueHandling.Ignore)]
        public int? GradeLevel { get; set; }

        [JsonProperty("language_preference", NullValueHandling = NullValueHandling.Ignore)]
        public string? LanguagePreference { get; set; }

        [JsonProperty("avatar_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? AvatarKey { get; set; }
    }

    // ──────────────────────────────────────────────────────────────
    //  Settings
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Response from <c>GET /api/v1/student/settings</c> and
    /// request/response for <c>PATCH /api/v1/student/settings</c>.
    /// </summary>
    public class SettingsDto
    {
        [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
        public string? Language { get; set; }

        [JsonProperty("master_volume", NullValueHandling = NullValueHandling.Ignore)]
        public float? MasterVolume { get; set; }

        [JsonProperty("music_volume", NullValueHandling = NullValueHandling.Ignore)]
        public float? MusicVolume { get; set; }

        [JsonProperty("sfx_volume", NullValueHandling = NullValueHandling.Ignore)]
        public float? SfxVolume { get; set; }

        [JsonProperty("mute_all", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MuteAll { get; set; }

        [JsonProperty("subtitles_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubtitlesEnabled { get; set; }

        [JsonProperty("reduced_motion", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ReducedMotion { get; set; }

        [JsonProperty("show_hints", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowHints { get; set; }

        [JsonProperty("text_size", NullValueHandling = NullValueHandling.Ignore)]
        public string? TextSize { get; set; }

        [JsonProperty("notifications_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NotificationsEnabled { get; set; }

        [JsonProperty("accessibility_preferences", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object>? AccessibilityPreferences { get; set; }

        /// <summary>
        /// Opaque settings revision string. Unity compares equality only and
        /// uses it for targeted sync refresh after a settings PATCH.
        /// </summary>
        [JsonProperty("revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? Revision { get; set; }
    }

    // ──────────────────────────────────────────────────────────────
    //  Subjects, Terms, Stations
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Response item from <c>GET /api/v1/student/subjects</c>.
    /// </summary>
    public class SubjectDto
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string? Id { get; set; }

        [JsonProperty("slug")]
        public string? Slug { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        [JsonProperty("icon_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? IconKey { get; set; }

        [JsonProperty("grade_levels", NullValueHandling = NullValueHandling.Ignore)]
        public List<int>? GradeLevels { get; set; }

        [JsonProperty("is_available", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAvailable { get; set; }

        [JsonProperty("progress_percent", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ProgressPercent { get; set; }

        /// <summary>
        /// Optional preview marker for exploration-only subjects
        /// (e.g. <c>"exploration_only"</c> for Science in the current milestone).
        /// </summary>
        [JsonProperty("preview_mode", NullValueHandling = NullValueHandling.Ignore)]
        public string? PreviewMode { get; set; }
    }

    /// <summary>
    /// Response item from <c>GET /api/v1/student/subjects/{subject_slug}/terms</c>.
    /// </summary>
    public class TermDto
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string? Id { get; set; }

        [JsonProperty("term_number")]
        public int? TermNumber { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string? Title { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        [JsonProperty("world_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public WorldMetadataDto? WorldMetadata { get; set; }

        [JsonProperty("is_available", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAvailable { get; set; }

        [JsonProperty("progress_percent", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ProgressPercent { get; set; }
    }

    /// <summary>
    /// World metadata returned inside term and station responses.
    /// Provides identity for Unity scene loading.
    /// </summary>
    public class WorldMetadataDto
    {
        [JsonProperty("world_theme_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? WorldThemeKey { get; set; }

        [JsonProperty("world_title", NullValueHandling = NullValueHandling.Ignore)]
        public string? WorldTitle { get; set; }

        [JsonProperty("unity_scene_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? UnitySceneKey { get; set; }

        [JsonProperty("unity_scene_name", NullValueHandling = NullValueHandling.Ignore)]
        public string? UnitySceneName { get; set; }

        [JsonProperty("scene_address_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? SceneAddressKey { get; set; }

        [JsonProperty("environment_tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? EnvironmentTags { get; set; }

        [JsonProperty("mechanic_family", NullValueHandling = NullValueHandling.Ignore)]
        public string? MechanicFamily { get; set; }
    }

    /// <summary>
    /// Response item from station-list endpoints.
    /// </summary>
    public class StationDto
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string? Id { get; set; }

        /// <summary>Stable station key (e.g. <c>vocabulary_clue_trail</c>).</summary>
        [JsonProperty("station_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationKey { get; set; }

        [JsonProperty("station_number", NullValueHandling = NullValueHandling.Ignore)]
        public int? StationNumber { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string? Title { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        [JsonProperty("subject_slug", NullValueHandling = NullValueHandling.Ignore)]
        public string? SubjectSlug { get; set; }

        [JsonProperty("grade_level", NullValueHandling = NullValueHandling.Ignore)]
        public int? GradeLevel { get; set; }

        [JsonProperty("term_number", NullValueHandling = NullValueHandling.Ignore)]
        public int? TermNumber { get; set; }

        /// <summary>
        /// Station availability/progress state. Maps to <see cref="StationState"/>
        /// via the safe enum converter; unknown values fall back safely.
        /// </summary>
        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string? State { get; set; }

        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Required { get; set; }

        [JsonProperty("progress_percent", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ProgressPercent { get; set; }

        [JsonProperty("challenge_type", NullValueHandling = NullValueHandling.Ignore)]
        public string? ChallengeType { get; set; }

        /// <summary>Stable portal/interactable key used to enter the station.</summary>
        [JsonProperty("portal_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? PortalKey { get; set; }

        /// <summary>Stable local scene key for the station gameplay scene.</summary>
        [JsonProperty("unity_scene_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? UnitySceneKey { get; set; }

        [JsonProperty("content_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? ContentRevision { get; set; }

        [JsonProperty("world_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public WorldMetadataDto? WorldMetadata { get; set; }
    }

    /// <summary>
    /// Response from station-list endpoints — wraps the station scope and the
    /// station array. Science exploration-preview terms return an empty
    /// <see cref="Stations"/> array with <see cref="PreviewMode"/> set; this is
    /// a valid no-station preview state, not an error.
    /// </summary>
    public class StationListDto
    {
        [JsonProperty("subject_slug", NullValueHandling = NullValueHandling.Ignore)]
        public string? SubjectSlug { get; set; }

        [JsonProperty("grade_level", NullValueHandling = NullValueHandling.Ignore)]
        public int? GradeLevel { get; set; }

        [JsonProperty("term_number", NullValueHandling = NullValueHandling.Ignore)]
        public int? TermNumber { get; set; }

        [JsonProperty("stations")]
        public List<StationDto> Stations { get; set; } = new List<StationDto>();

        /// <summary>
        /// Exploration-preview marker (e.g. <c>"exploration_only"</c>) for
        /// Science terms that intentionally expose no playable stations.
        /// </summary>
        [JsonProperty("preview_mode", NullValueHandling = NullValueHandling.Ignore)]
        public string? PreviewMode { get; set; }

        /// <summary>Optional student-safe message describing a preview state.</summary>
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; }
    }

    // ──────────────────────────────────────────────────────────────
    //  Station Content & Session
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Response from <c>GET /api/v1/student/stations/{station_id}/content</c>.
    /// Student-safe station content and world task metadata.
    /// Never exposes answer keys, scoring rules, or teacher notes.
    /// </summary>
    public class StationContentDto
    {
        [JsonProperty("station_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationId { get; set; }

        [JsonProperty("station_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationKey { get; set; }

        [JsonProperty("subject_slug", NullValueHandling = NullValueHandling.Ignore)]
        public string? SubjectSlug { get; set; }

        [JsonProperty("grade_level", NullValueHandling = NullValueHandling.Ignore)]
        public int? GradeLevel { get; set; }

        [JsonProperty("term_number", NullValueHandling = NullValueHandling.Ignore)]
        public int? TermNumber { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string? Title { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        [JsonProperty("learning_skill", NullValueHandling = NullValueHandling.Ignore)]
        public string? LearningSkill { get; set; }

        [JsonProperty("student_learning_goal", NullValueHandling = NullValueHandling.Ignore)]
        public string? StudentLearningGoal { get; set; }

        [JsonProperty("instructions", NullValueHandling = NullValueHandling.Ignore)]
        public string? Instructions { get; set; }

        [JsonProperty("completion_rule", NullValueHandling = NullValueHandling.Ignore)]
        public CompletionRuleDto? CompletionRule { get; set; }

        [JsonProperty("world_tasks", NullValueHandling = NullValueHandling.Ignore)]
        public List<WorldTaskDto>? WorldTasks { get; set; }

        [JsonProperty("challenges", NullValueHandling = NullValueHandling.Ignore)]
        public List<ChallengeDto>? Challenges { get; set; }

        [JsonProperty("content_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? ContentRevision { get; set; }

        [JsonProperty("world_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public WorldMetadataDto? WorldMetadata { get; set; }

        // ── Optional learning-gameplay / narrative fields ──

        [JsonProperty("story_context", NullValueHandling = NullValueHandling.Ignore)]
        public string? StoryContext { get; set; }

        [JsonProperty("mission_title", NullValueHandling = NullValueHandling.Ignore)]
        public string? MissionTitle { get; set; }

        [JsonProperty("mission_summary", NullValueHandling = NullValueHandling.Ignore)]
        public string? MissionSummary { get; set; }

        [JsonProperty("npc_guides", NullValueHandling = NullValueHandling.Ignore)]
        public List<NpcGuideDto>? NpcGuides { get; set; }

        [JsonProperty("learning_cycle", NullValueHandling = NullValueHandling.Ignore)]
        public LearningCycleDto? LearningCycle { get; set; }

        [JsonProperty("hint_policy", NullValueHandling = NullValueHandling.Ignore)]
        public HintPolicyDto? HintPolicy { get; set; }

        [JsonProperty("discoveries", NullValueHandling = NullValueHandling.Ignore)]
        public List<DiscoveryDto>? Discoveries { get; set; }

        [JsonProperty("reflection_prompt", NullValueHandling = NullValueHandling.Ignore)]
        public string? ReflectionPrompt { get; set; }

        [JsonProperty("reward_preview", NullValueHandling = NullValueHandling.Ignore)]
        public List<RewardPreviewDto>? RewardPreview { get; set; }

        [JsonProperty("world_restoration_state", NullValueHandling = NullValueHandling.Ignore)]
        public WorldRestorationStateDto? WorldRestorationState { get; set; }

        [JsonProperty("success_feedback", NullValueHandling = NullValueHandling.Ignore)]
        public SuccessFeedbackDto? SuccessFeedback { get; set; }
    }

    /// <summary>
    /// A challenge within station content.
    /// Answer details are student-safe; never includes answer keys.
    /// </summary>
    public class ChallengeDto
    {
        [JsonProperty("challenge_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? ChallengeId { get; set; }

        [JsonProperty("challenge_type", NullValueHandling = NullValueHandling.Ignore)]
        public string? ChallengeType { get; set; }

        [JsonProperty("prompt", NullValueHandling = NullValueHandling.Ignore)]
        public string? Prompt { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public List<ChallengeOptionDto>? Options { get; set; }

        [JsonProperty("order_index", NullValueHandling = NullValueHandling.Ignore)]
        public int? OrderIndex { get; set; }
    }

    /// <summary>
    /// A choice/option within a challenge.
    /// Never includes correct/incorrect flag.
    /// </summary>
    public class ChallengeOptionDto
    {
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string? Key { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string? Label { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Canonical four-step learning cycle (Discover → Practice → Apply → Review).
    /// Each phase holds short, student-safe guidance text. All phases optional.
    /// </summary>
    public class LearningCycleDto
    {
        [JsonProperty("discover", NullValueHandling = NullValueHandling.Ignore)]
        public string? Discover { get; set; }

        [JsonProperty("practice", NullValueHandling = NullValueHandling.Ignore)]
        public string? Practice { get; set; }

        [JsonProperty("apply", NullValueHandling = NullValueHandling.Ignore)]
        public string? Apply { get; set; }

        [JsonProperty("review", NullValueHandling = NullValueHandling.Ignore)]
        public string? Review { get; set; }
    }

    /// <summary>
    /// Station completion rule (e.g. <c>complete_required_challenges</c>).
    /// </summary>
    public class CompletionRuleDto
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string? Type { get; set; }

        [JsonProperty("required_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? RequiredCount { get; set; }
    }

    /// <summary>
    /// A world task / interactable within station content. Carries the stable
    /// local presentation keys (portal, interactable, prefab) used by Unity to
    /// place and drive the learning interaction, and links to its challenge.
    /// </summary>
    public class WorldTaskDto
    {
        [JsonProperty("task_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? TaskId { get; set; }

        [JsonProperty("task_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? TaskKey { get; set; }

        [JsonProperty("task_type", NullValueHandling = NullValueHandling.Ignore)]
        public string? TaskType { get; set; }

        [JsonProperty("portal_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? PortalKey { get; set; }

        [JsonProperty("interactable_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? InteractableKey { get; set; }

        [JsonProperty("prefab_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? PrefabKey { get; set; }

        [JsonProperty("world_position_hint", NullValueHandling = NullValueHandling.Ignore)]
        public string? WorldPositionHint { get; set; }

        [JsonProperty("challenge_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? ChallengeId { get; set; }

        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Required { get; set; }
    }

    /// <summary>
    /// NPC guide DTO within station content. Provides concise, child-friendly
    /// mission guidance (intro + completion). Stable <see cref="NpcKey"/> allows
    /// a UI-Toolkit dialogue fallback when a character/voice asset is missing.
    /// </summary>
    public class NpcGuideDto
    {
        [JsonProperty("npc_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? NpcKey { get; set; }

        [JsonProperty("display_name", NullValueHandling = NullValueHandling.Ignore)]
        public string? DisplayName { get; set; }

        [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
        public string? Role { get; set; }

        [JsonProperty("avatar_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? AvatarKey { get; set; }

        [JsonProperty("intro_dialogue", NullValueHandling = NullValueHandling.Ignore)]
        public string? IntroDialogue { get; set; }

        [JsonProperty("completion_dialogue", NullValueHandling = NullValueHandling.Ignore)]
        public string? CompletionDialogue { get; set; }
    }

    /// <summary>
    /// Hint policy configuration within station content. Encourages safe
    /// mistakes: ordinary mistakes are not punished and world progress is
    /// preserved. Tiered hints scaffold without exposing answer keys.
    /// </summary>
    public class HintPolicyDto
    {
        [JsonProperty("max_hint_tier", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxHintTier { get; set; }

        [JsonProperty("preserve_world_progress", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PreserveWorldProgress { get; set; }

        [JsonProperty("penalize_ordinary_mistake", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PenalizeOrdinaryMistake { get; set; }

        [JsonProperty("tiers", NullValueHandling = NullValueHandling.Ignore)]
        public List<HintTierDto>? Tiers { get; set; }
    }

    /// <summary>
    /// A single hint tier within a hint policy.
    /// </summary>
    public class HintTierDto
    {
        [JsonProperty("tier", NullValueHandling = NullValueHandling.Ignore)]
        public int? Tier { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string? Text { get; set; }
    }

    /// <summary>
    /// Optional discovery entry within station content. Discoveries are never
    /// required to complete a station and must not become grind.
    /// </summary>
    public class DiscoveryDto
    {
        [JsonProperty("discovery_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? DiscoveryKey { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string? Type { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string? Title { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        [JsonProperty("optional", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Optional { get; set; }

        [JsonProperty("reward_preview", NullValueHandling = NullValueHandling.Ignore)]
        public RewardPreviewDto? RewardPreview { get; set; }
    }

    /// <summary>
    /// Reward preview within station content.
    /// Does NOT represent an earned reward — only motivation display.
    /// </summary>
    public class RewardPreviewDto
    {
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string? Code { get; set; }

        [JsonProperty("reward_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardKey { get; set; }

        [JsonProperty("reward_type", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardType { get; set; }

        [JsonProperty("display_name", NullValueHandling = NullValueHandling.Ignore)]
        public string? DisplayName { get; set; }

        [JsonProperty("icon_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? IconKey { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public int? Quantity { get; set; }

        /// <summary>
        /// When a reward is granted (e.g. <c>term_after_both_stations</c> for a
        /// subject crystal). Presentation/preview only — grants are provider-owned.
        /// </summary>
        [JsonProperty("grant_scope", NullValueHandling = NullValueHandling.Ignore)]
        public string? GrantScope { get; set; }
    }

    /// <summary>
    /// World restoration state within station content. Restoration is applied
    /// only after a provider-accepted completion.
    /// </summary>
    public class WorldRestorationStateDto
    {
        [JsonProperty("state_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? StateKey { get; set; }

        [JsonProperty("apply_after_accepted_completion", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ApplyAfterAcceptedCompletion { get; set; }

        [JsonProperty("state_data", NullValueHandling = NullValueHandling.Ignore)]
        public object? StateData { get; set; }
    }

    /// <summary>
    /// Success feedback within station content.
    /// Presentation-only; no scoring/correctness authority.
    /// </summary>
    public class SuccessFeedbackDto
    {
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; }

        [JsonProperty("encouraging_phrases", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? EncouragingPhrases { get; set; }
    }

    // ──────────────────────────────────────────────────────────────
    //  Station Start/Resume
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Request body for <c>POST /api/v1/student/stations/{station_id}/start</c>.
    /// </summary>
    public class StationStartRequestDto
    {
        [JsonProperty("client_version", NullValueHandling = NullValueHandling.Ignore)]
        public string? ClientVersion { get; set; }
    }

    /// <summary>
    /// Response from <c>POST /api/v1/student/stations/{station_id}/start</c>.
    /// Returns station session for starting or resuming.
    /// </summary>
    public class StationStartResponseDto
    {
        [JsonProperty("station_session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationSessionId { get; set; }

        [JsonProperty("station_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationId { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string? Status { get; set; }

        [JsonProperty("resuming", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Resuming { get; set; }

        [JsonProperty("started_at", NullValueHandling = NullValueHandling.Ignore)]
        public string? StartedAt { get; set; }

        [JsonProperty("content_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? ContentRevision { get; set; }

        [JsonProperty("challenge_progress", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object>? ChallengeProgress { get; set; }
    }

    // ──────────────────────────────────────────────────────────────
    //  Attempts
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Request body for <c>POST /api/v1/student/challenges/{challenge_id}/attempts</c>.
    /// The <see cref="ClientAttemptUuid"/> is sent as <c>client_attempt_uuid</c>
    /// for idempotent retry behaviour — duplicate UUIDs resolve to the same
    /// attempt result rather than creating double scores.
    /// </summary>
    public class AttemptRequestDto
    {
        [JsonProperty("station_session_id")]
        public string? StationSessionId { get; set; }

        [JsonProperty("station_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationId { get; set; }

        [JsonProperty("client_attempt_uuid")]
        public string? ClientAttemptUuid { get; set; }

        [JsonProperty("answer")]
        public object? Answer { get; set; }

        [JsonProperty("time_spent_seconds", NullValueHandling = NullValueHandling.Ignore)]
        public int? TimeSpentSeconds { get; set; }

        [JsonProperty("used_rewards", NullValueHandling = NullValueHandling.Ignore)]
        public List<UsedRewardDto>? UsedRewards { get; set; }
    }

    /// <summary>
    /// A single reward used during an attempt.
    /// </summary>
    public class UsedRewardDto
    {
        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public int? Quantity { get; set; }
    }

    /// <summary>
    /// Response from <c>POST /api/v1/student/challenges/{challenge_id}/attempts</c>.
    /// Student-safe feedback; never includes answer keys or hidden scoring.
    /// </summary>
    public class AttemptResponseDto
    {
        [JsonProperty("attempt_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? AttemptId { get; set; }

        [JsonProperty("client_attempt_uuid", NullValueHandling = NullValueHandling.Ignore)]
        public string? ClientAttemptUuid { get; set; }

        [JsonProperty("challenge_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? ChallengeId { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string? Status { get; set; }

        /// <summary>Server/provider accepted the submission as a valid attempt.</summary>
        [JsonProperty("accepted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Accepted { get; set; }

        /// <summary>Authoritative correctness — never inferred locally.</summary>
        [JsonProperty("correct", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Correct { get; set; }

        /// <summary>
        /// True when this result is an idempotent replay of a previously
        /// processed <c>client_attempt_uuid</c> (no double scoring/rewards).
        /// </summary>
        [JsonProperty("is_replay", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsReplay { get; set; }

        [JsonProperty("review_status", NullValueHandling = NullValueHandling.Ignore)]
        public string? ReviewStatus { get; set; }

        [JsonProperty("feedback", NullValueHandling = NullValueHandling.Ignore)]
        public AttemptFeedbackDto? Feedback { get; set; }

        [JsonProperty("score_awarded", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ScoreAwarded { get; set; }

        [JsonProperty("progress", NullValueHandling = NullValueHandling.Ignore)]
        public AttemptProgressDto? Progress { get; set; }

        [JsonProperty("rewards_granted", NullValueHandling = NullValueHandling.Ignore)]
        public List<RewardGrantDto>? RewardsGranted { get; set; }

        [JsonProperty("progress_updated", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ProgressUpdated { get; set; }

        [JsonProperty("progress_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? ProgressRevision { get; set; }

        [JsonProperty("reward_wallet_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardWalletRevision { get; set; }
    }

    /// <summary>
    /// Per-attempt station progress snapshot returned with an attempt result.
    /// </summary>
    public class AttemptProgressDto
    {
        [JsonProperty("completed_challenges", NullValueHandling = NullValueHandling.Ignore)]
        public int? CompletedChallenges { get; set; }

        [JsonProperty("required_challenges", NullValueHandling = NullValueHandling.Ignore)]
        public int? RequiredChallenges { get; set; }

        [JsonProperty("station_progress_percent", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? StationProgressPercent { get; set; }
    }

    /// <summary>
    /// Feedback returned within an attempt response.
    /// Presentation-only; no answer keys or hidden scoring data.
    /// </summary>
    public class AttemptFeedbackDto
    {
        [JsonProperty("is_correct", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsCorrect { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; }

        [JsonProperty("explanation", NullValueHandling = NullValueHandling.Ignore)]
        public string? Explanation { get; set; }

        [JsonProperty("misconception_message", NullValueHandling = NullValueHandling.Ignore)]
        public string? MisconceptionMessage { get; set; }

        [JsonProperty("encouraging_message", NullValueHandling = NullValueHandling.Ignore)]
        public string? EncouragingMessage { get; set; }

        [JsonProperty("retry_action", NullValueHandling = NullValueHandling.Ignore)]
        public string? RetryAction { get; set; }

        [JsonProperty("retry_allowed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RetryAllowed { get; set; }

        [JsonProperty("remaining_attempts", NullValueHandling = NullValueHandling.Ignore)]
        public int? RemainingAttempts { get; set; }

        [JsonProperty("current_hint_tier", NullValueHandling = NullValueHandling.Ignore)]
        public int? CurrentHintTier { get; set; }

        [JsonProperty("next_hint_tier", NullValueHandling = NullValueHandling.Ignore)]
        public int? NextHintTier { get; set; }

        [JsonProperty("hint_text", NullValueHandling = NullValueHandling.Ignore)]
        public string? HintText { get; set; }
    }

    /// <summary>
    /// A reward granted after a successful attempt.
    /// </summary>
    public class RewardGrantDto
    {
        [JsonProperty("reward_code", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardCode { get; set; }

        [JsonProperty("reward_type", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardType { get; set; }

        [JsonProperty("display_name", NullValueHandling = NullValueHandling.Ignore)]
        public string? DisplayName { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public int? Quantity { get; set; }
    }

    // ──────────────────────────────────────────────────────────────
    //  Station Completion
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Request body for <c>POST /api/v1/student/stations/{station_id}/complete</c>.
    /// </summary>
    public class StationCompleteRequestDto
    {
        [JsonProperty("station_session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationSessionId { get; set; }
    }

    /// <summary>
    /// Response from <c>POST /api/v1/student/stations/{station_id}/complete</c>.
    /// </summary>
    public class StationCompleteResponseDto
    {
        [JsonProperty("station_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationId { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string? Status { get; set; }

        [JsonProperty("completed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Completed { get; set; }

        /// <summary>
        /// True when this completion is an idempotent replay (already completed);
        /// no double scoring, double rewards, or duplicate progress is applied.
        /// </summary>
        [JsonProperty("is_replay", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsReplay { get; set; }

        [JsonProperty("score_total", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ScoreTotal { get; set; }

        /// <summary>Portal completion state for the originating world (e.g. <c>completed</c>).</summary>
        [JsonProperty("portal_state", NullValueHandling = NullValueHandling.Ignore)]
        public string? PortalState { get; set; }

        /// <summary>Stations unlocked as a result of this completion.</summary>
        [JsonProperty("unlocks", NullValueHandling = NullValueHandling.Ignore)]
        public List<StationUnlockDto>? Unlocks { get; set; }

        /// <summary>Term-completion result, including a subject crystal/badge when earned.</summary>
        [JsonProperty("term_completion", NullValueHandling = NullValueHandling.Ignore)]
        public TermCompletionDto? TermCompletion { get; set; }

        [JsonProperty("rewards_granted", NullValueHandling = NullValueHandling.Ignore)]
        public List<RewardGrantDto>? RewardsGranted { get; set; }

        /// <summary>Provider-confirmed world restoration result applied on completion.</summary>
        [JsonProperty("world_restoration_result", NullValueHandling = NullValueHandling.Ignore)]
        public WorldRestorationResultDto? WorldRestorationResult { get; set; }

        [JsonProperty("progress_summary", NullValueHandling = NullValueHandling.Ignore)]
        public ProgressSummaryDto? ProgressSummary { get; set; }

        [JsonProperty("progress_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? ProgressRevision { get; set; }

        [JsonProperty("reward_wallet_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardWalletRevision { get; set; }
    }

    /// <summary>
    /// A station unlocked by a completion result.
    /// </summary>
    public class StationUnlockDto
    {
        [JsonProperty("station_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationId { get; set; }

        [JsonProperty("station_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationKey { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string? State { get; set; }
    }

    /// <summary>
    /// Term-completion result. When both required term stations are complete the
    /// provider may award a subject-themed crystal and/or badge.
    /// </summary>
    public class TermCompletionDto
    {
        [JsonProperty("subject_slug", NullValueHandling = NullValueHandling.Ignore)]
        public string? SubjectSlug { get; set; }

        [JsonProperty("term_number", NullValueHandling = NullValueHandling.Ignore)]
        public int? TermNumber { get; set; }

        [JsonProperty("completed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Completed { get; set; }

        [JsonProperty("crystal", NullValueHandling = NullValueHandling.Ignore)]
        public RewardGrantDto? Crystal { get; set; }

        [JsonProperty("badge", NullValueHandling = NullValueHandling.Ignore)]
        public RewardGrantDto? Badge { get; set; }
    }

    /// <summary>
    /// Provider-confirmed world restoration result. Only applied after an
    /// accepted completion — Unity owns the animation, not the state authority.
    /// </summary>
    public class WorldRestorationResultDto
    {
        [JsonProperty("state_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? StateKey { get; set; }

        [JsonProperty("restored", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Restored { get; set; }
    }

    // ──────────────────────────────────────────────────────────────
    //  Progress & Rewards
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Response from <c>GET /api/v1/student/progress/summary</c>.
    /// </summary>
    public class ProgressSummaryDto
    {
        [JsonProperty("student_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? StudentId { get; set; }

        [JsonProperty("subjects", NullValueHandling = NullValueHandling.Ignore)]
        public List<SubjectProgressDto>? Subjects { get; set; }

        [JsonProperty("total_stations_completed", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalStationsCompleted { get; set; }

        [JsonProperty("total_stations_available", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalStationsAvailable { get; set; }

        [JsonProperty("started_stations", NullValueHandling = NullValueHandling.Ignore)]
        public int? StartedStations { get; set; }

        [JsonProperty("overall_percentage", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? OverallPercentage { get; set; }

        [JsonProperty("stars", NullValueHandling = NullValueHandling.Ignore)]
        public int? Stars { get; set; }

        [JsonProperty("coins", NullValueHandling = NullValueHandling.Ignore)]
        public int? Coins { get; set; }

        [JsonProperty("revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? Revision { get; set; }
    }

    /// <summary>
    /// Progress summary for a single subject.
    /// </summary>
    public class SubjectProgressDto
    {
        [JsonProperty("subject_slug", NullValueHandling = NullValueHandling.Ignore)]
        public string? SubjectSlug { get; set; }

        [JsonProperty("subject_name", NullValueHandling = NullValueHandling.Ignore)]
        public string? SubjectName { get; set; }

        [JsonProperty("stations_completed", NullValueHandling = NullValueHandling.Ignore)]
        public int? StationsCompleted { get; set; }

        [JsonProperty("stations_available", NullValueHandling = NullValueHandling.Ignore)]
        public int? StationsAvailable { get; set; }

        [JsonProperty("percentage", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Percentage { get; set; }

        [JsonProperty("progress_percent", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ProgressPercent { get; set; }

        [JsonProperty("preview_mode", NullValueHandling = NullValueHandling.Ignore)]
        public string? PreviewMode { get; set; }

        [JsonProperty("terms", NullValueHandling = NullValueHandling.Ignore)]
        public List<TermProgressDto>? Terms { get; set; }
    }

    /// <summary>
    /// Progress summary for a single term.
    /// </summary>
    public class TermProgressDto
    {
        [JsonProperty("term_number", NullValueHandling = NullValueHandling.Ignore)]
        public int? TermNumber { get; set; }

        [JsonProperty("stations_completed", NullValueHandling = NullValueHandling.Ignore)]
        public int? StationsCompleted { get; set; }

        [JsonProperty("stations_available", NullValueHandling = NullValueHandling.Ignore)]
        public int? StationsAvailable { get; set; }

        [JsonProperty("percentage", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Percentage { get; set; }
    }

    /// <summary>
    /// Response from <c>GET /api/v1/student/rewards</c>.
    /// </summary>
    public class RewardWalletDto
    {
        [JsonProperty("student_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? StudentId { get; set; }

        [JsonProperty("rewards", NullValueHandling = NullValueHandling.Ignore)]
        public List<RewardBalanceDto>? Rewards { get; set; }

        [JsonProperty("total_coins", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalCoins { get; set; }

        [JsonProperty("total_stars", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalStars { get; set; }

        [JsonProperty("revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? Revision { get; set; }
    }

    /// <summary>
    /// A single reward balance entry.
    /// </summary>
    public class RewardBalanceDto
    {
        [JsonProperty("reward_code", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardCode { get; set; }

        [JsonProperty("reward_type", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardType { get; set; }

        [JsonProperty("display_name", NullValueHandling = NullValueHandling.Ignore)]
        public string? DisplayName { get; set; }

        [JsonProperty("icon_key", NullValueHandling = NullValueHandling.Ignore)]
        public string? IconKey { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public int? Quantity { get; set; }

        [JsonProperty("is_usable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsUsable { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request body for <c>POST /api/v1/student/rewards/{reward_code}/use</c>.
    /// </summary>
    public class UseRewardRequestDto
    {
        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public int? Quantity { get; set; }

        [JsonProperty("station_session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationSessionId { get; set; }
    }

    /// <summary>
    /// Response from <c>POST /api/v1/student/rewards/{reward_code}/use</c>.
    /// </summary>
    public class UseRewardResponseDto
    {
        [JsonProperty("reward_code", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardCode { get; set; }

        [JsonProperty("remaining_quantity", NullValueHandling = NullValueHandling.Ignore)]
        public int? RemainingQuantity { get; set; }

        [JsonProperty("effect", NullValueHandling = NullValueHandling.Ignore)]
        public string? Effect { get; set; }
    }

    // ──────────────────────────────────────────────────────────────
    //  Sync Status
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Response from <c>GET /api/v1/student/sync/status</c>.
    /// Revision-based metadata for targeted refresh.
    /// Revision values are opaque strings — Unity compares equality only.
    /// </summary>
    public class SyncStatusDto
    {
        [JsonProperty("student_progress_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? StudentProgressRevision { get; set; }

        [JsonProperty("student_settings_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? StudentSettingsRevision { get; set; }

        [JsonProperty("station_unlock_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? StationUnlockRevision { get; set; }

        [JsonProperty("published_content_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? PublishedContentRevision { get; set; }

        [JsonProperty("reward_wallet_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardWalletRevision { get; set; }

        [JsonProperty("server_time", NullValueHandling = NullValueHandling.Ignore)]
        public string? ServerTime { get; set; }

        [JsonProperty("next_poll_after_seconds", NullValueHandling = NullValueHandling.Ignore)]
        public int? NextPollAfterSeconds { get; set; }
    }

    // ──────────────────────────────────────────────────────────────
    //  Challenge Answer Type Enum
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Known challenge answer families as documented by the server.
    /// Unknown values from the server are handled as
    /// <see cref="Unknown"/> and must not crash the client.
    /// </summary>
    public enum ChallengeAnswerType
    {
        Unknown,
        MultipleChoice,
        TrueFalse,
        Matching,
        Sorting,
        Ordering,
        FillBlank,
        ShortResponse,
        ScenarioChoice
    }
}