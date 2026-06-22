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

    // ──────────────────────────────────────────────────────────────
    //  Quiz & Assessment System (Laravel quiz_first_laravel_1 REST)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Response item from quiz-list endpoints.
    /// </summary>
    public class QuizDto
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string? Id { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string? Title { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        [JsonProperty("subject_slug", NullValueHandling = NullValueHandling.Ignore)]
        public string? SubjectSlug { get; set; }

        [JsonProperty("term_number", NullValueHandling = NullValueHandling.Ignore)]
        public int? TermNumber { get; set; }

        [JsonProperty("grade_level", NullValueHandling = NullValueHandling.Ignore)]
        public int? GradeLevel { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string? State { get; set; }

        [JsonProperty("total_items", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalItems { get; set; }

        [JsonProperty("duration_minutes", NullValueHandling = NullValueHandling.Ignore)]
        public int? DurationMinutes { get; set; }

        [JsonProperty("is_available", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAvailable { get; set; }

        [JsonProperty("progress_percent", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ProgressPercent { get; set; }
    }

    /// <summary>
    /// Response from quiz-list endpoints.
    /// Science exploration-preview terms may return an empty list with preview mode.
    /// </summary>
    public class QuizListDto
    {
        [JsonProperty("subject_slug", NullValueHandling = NullValueHandling.Ignore)]
        public string? SubjectSlug { get; set; }

        [JsonProperty("grade_level", NullValueHandling = NullValueHandling.Ignore)]
        public int? GradeLevel { get; set; }

        [JsonProperty("term_number", NullValueHandling = NullValueHandling.Ignore)]
        public int? TermNumber { get; set; }

        [JsonProperty("quizzes")]
        public List<QuizDto> Quizzes { get; set; } = new List<QuizDto>();

        [JsonProperty("preview_mode", NullValueHandling = NullValueHandling.Ignore)]
        public string? PreviewMode { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; }
    }

    /// <summary>
    /// Response from GET /api/v1/student/quizzes/{quiz_id}.
    /// </summary>
    public class QuizDetailDto : QuizDto
    {
        [JsonProperty("instructions", NullValueHandling = NullValueHandling.Ignore)]
        public string? Instructions { get; set; }

        [JsonProperty("items")]
        public List<QuizItemDto> Items { get; set; } = new List<QuizItemDto>();
    }

    /// <summary>
    /// A question or task item inside a quiz.
    /// </summary>
    public class QuizItemDto
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string? Id { get; set; }

        [JsonProperty("quiz_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? QuizId { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string? Type { get; set; }

        [JsonProperty("prompt", NullValueHandling = NullValueHandling.Ignore)]
        public string? Prompt { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public List<QuizItemOptionDto>? Options { get; set; }

        [JsonProperty("order_index", NullValueHandling = NullValueHandling.Ignore)]
        public int? OrderIndex { get; set; }
    }

    /// <summary>
    /// A selectable option or match option for a quiz item.
    /// </summary>
    public class QuizItemOptionDto
    {
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string? Key { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string? Label { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request body for POST /api/v1/student/quizzes/{quiz_id}/attempts.
    /// </summary>
    public class QuizAttemptRequestDto
    {
        [JsonProperty("client_attempt_uuid")]
        public string? ClientAttemptUuid { get; set; }

        [JsonProperty("answers")]
        public Dictionary<string, object> Answers { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Response body for POST /api/v1/student/quizzes/{quiz_id}/attempts.
    /// </summary>
    public class QuizAttemptResponseDto
    {
        [JsonProperty("attempt_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? AttemptId { get; set; }

        [JsonProperty("client_attempt_uuid", NullValueHandling = NullValueHandling.Ignore)]
        public string? ClientAttemptUuid { get; set; }

        [JsonProperty("quiz_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? QuizId { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string? Status { get; set; }

        [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Score { get; set; }

        [JsonProperty("total_possible", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? TotalPossible { get; set; }

        [JsonProperty("percentage", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Percentage { get; set; }

        [JsonProperty("passed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Passed { get; set; }

        [JsonProperty("is_replay", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsReplay { get; set; }

        [JsonProperty("answers_feedback", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, QuizItemFeedbackDto>? AnswersFeedback { get; set; }

        [JsonProperty("progress_updated", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ProgressUpdated { get; set; }

        [JsonProperty("progress_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? ProgressRevision { get; set; }
    }

    /// <summary>
    /// Feedback for an individual quiz item.
    /// </summary>
    public class QuizItemFeedbackDto
    {
        [JsonProperty("is_correct", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsCorrect { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; }

        [JsonProperty("explanation", NullValueHandling = NullValueHandling.Ignore)]
        public string? Explanation { get; set; }

        [JsonProperty("hint_text", NullValueHandling = NullValueHandling.Ignore)]
        public string? HintText { get; set; }
    }

    /// <summary>
    /// Individual quiz result record.
    /// </summary>
    public class QuizResultDto
    {
        [JsonProperty("attempt_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? AttemptId { get; set; }

        [JsonProperty("quiz_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? QuizId { get; set; }

        [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Score { get; set; }

        [JsonProperty("total_possible", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? TotalPossible { get; set; }

        [JsonProperty("percentage", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Percentage { get; set; }

        [JsonProperty("passed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Passed { get; set; }

        [JsonProperty("completed_at", NullValueHandling = NullValueHandling.Ignore)]
        public string? CompletedAt { get; set; }

        [JsonProperty("answers", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object>? Answers { get; set; }
    }

    /// <summary>
    /// Response wrapper for GET /api/v1/student/quiz-results.
    /// </summary>
    public class QuizResultListDto
    {
        [JsonProperty("results")]
        public List<QuizResultDto> Results { get; set; } = new List<QuizResultDto>();
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

        [JsonProperty("total_quizzes_completed", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalQuizzesCompleted { get; set; }

        [JsonProperty("total_quizzes_available", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalQuizzesAvailable { get; set; }

        [JsonProperty("started_quizzes", NullValueHandling = NullValueHandling.Ignore)]
        public int? StartedQuizzes { get; set; }

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

        [JsonProperty("quizzes_completed", NullValueHandling = NullValueHandling.Ignore)]
        public int? QuizzesCompleted { get; set; }

        [JsonProperty("quizzes_available", NullValueHandling = NullValueHandling.Ignore)]
        public int? QuizzesAvailable { get; set; }

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

        [JsonProperty("quizzes_completed", NullValueHandling = NullValueHandling.Ignore)]
        public int? QuizzesCompleted { get; set; }

        [JsonProperty("quizzes_available", NullValueHandling = NullValueHandling.Ignore)]
        public int? QuizzesAvailable { get; set; }

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

        [JsonProperty("quiz_revision", NullValueHandling = NullValueHandling.Ignore)]
        public string? QuizRevision { get; set; }

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