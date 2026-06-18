using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NutriMind.Runtime.App.Dto
{
    // ──────────────────────────────────────────────────────────────
    //  Demo Fixture Root (development/demo container — NOT an API response)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Typed root for the local demo fixture
    /// (<c>Assets/_Project/Nutrimind/Resources/DemoData/full-demo-student-data.json</c>,
    /// loaded at runtime via <c>Resources.Load</c> in editor/development builds).
    /// <para>
    /// The outer object is only a fixture container. Every nested payload
    /// deserializes into the <b>same</b> DTO used by the corresponding HTTP
    /// endpoint, guaranteeing provider parity. This fixture must contain no real
    /// student data, credentials, tokens, secrets, or production answer keys.
    /// </para>
    /// </summary>
    public class DemoFixtureDto
    {
        [JsonProperty("fixture_format_version")]
        public string? FixtureFormatVersion { get; set; }

        [JsonProperty("fixture_id")]
        public string? FixtureId { get; set; }

        [JsonProperty("mode")]
        public string? Mode { get; set; }

        [JsonProperty("notice")]
        public string? Notice { get; set; }

        [JsonProperty("demo_auth")]
        public DemoAuthDto? DemoAuth { get; set; }

        [JsonProperty("responses")]
        public DemoResponsesDto? Responses { get; set; }

        /// <summary>subject_slug → term list (same shape as the HTTP terms endpoint).</summary>
        [JsonProperty("terms_by_subject")]
        public Dictionary<string, List<TermDto>>? TermsBySubject { get; set; }

        /// <summary>"subject:grade:term" → station list (same shape as the HTTP stations endpoint).</summary>
        [JsonProperty("stations_by_scope")]
        public Dictionary<string, StationListDto>? StationsByScope { get; set; }

        /// <summary>station_id → station content (same shape as the HTTP station-content endpoint).</summary>
        [JsonProperty("station_content_by_id")]
        public Dictionary<string, StationContentDto>? StationContentById { get; set; }

        /// <summary>station_id → start/resume response (same shape as the HTTP start endpoint).</summary>
        [JsonProperty("station_start_by_id")]
        public Dictionary<string, StationStartResponseDto>? StationStartById { get; set; }

        /// <summary>challenge_id → simulated attempt-result template + safe-mistake feedback.</summary>
        [JsonProperty("attempt_result_by_challenge_id")]
        public Dictionary<string, DemoAttemptFixtureDto>? AttemptResultByChallengeId { get; set; }

        /// <summary>station_id → simulated completion result (same shape as the HTTP complete endpoint).</summary>
        [JsonProperty("completion_result_by_station_id")]
        public Dictionary<string, StationCompleteResponseDto>? CompletionResultByStationId { get; set; }

        /// <summary>
        /// Fixture-only fabricated expected answers used by the local evaluator.
        /// These are fake development values; they are never exposed through
        /// student DTOs and must never contain production answer keys.
        /// </summary>
        [JsonProperty("demo_only_evaluation")]
        public Dictionary<string, JToken>? DemoOnlyEvaluation { get; set; }

        /// <summary>Representative safe error envelopes (same shape as HTTP errors).</summary>
        [JsonProperty("error_fixtures")]
        public Dictionary<string, DataProviderError>? ErrorFixtures { get; set; }

        [JsonProperty("demo_scope", NullValueHandling = NullValueHandling.Ignore)]
        public JObject? DemoScope { get; set; }

        [JsonProperty("gameplay_design", NullValueHandling = NullValueHandling.Ignore)]
        public JObject? GameplayDesign { get; set; }
    }

    /// <summary>
    /// Development-only login credentials for the demo. Obviously fabricated and
    /// rejected outside editor/development-demo mode.
    /// </summary>
    public class DemoAuthDto
    {
        [JsonProperty("lrn")]
        public string? Lrn { get; set; }

        [JsonProperty("pin")]
        public string? Pin { get; set; }

        [JsonProperty("allow_demo_login_button", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AllowDemoLoginButton { get; set; }

        [JsonProperty("development_build_only", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DevelopmentBuildOnly { get; set; }
    }

    /// <summary>
    /// The fixed (non-scoped) endpoint responses in the demo bundle. Each value
    /// is the exact DTO returned by the matching HTTP endpoint.
    /// </summary>
    public class DemoResponsesDto
    {
        [JsonProperty("ping")]
        public PingResponseDto? Ping { get; set; }

        [JsonProperty("config")]
        public ApiConfigDto? Config { get; set; }

        [JsonProperty("login")]
        public LoginResponseDto? Login { get; set; }

        [JsonProperty("bootstrap")]
        public BootstrapDto? Bootstrap { get; set; }

        [JsonProperty("profile")]
        public StudentProfileDto? Profile { get; set; }

        [JsonProperty("settings")]
        public SettingsDto? Settings { get; set; }

        [JsonProperty("subjects")]
        public List<SubjectDto>? Subjects { get; set; }

        [JsonProperty("progress_summary")]
        public ProgressSummaryDto? ProgressSummary { get; set; }

        [JsonProperty("rewards")]
        public RewardWalletDto? Rewards { get; set; }

        [JsonProperty("sync_status")]
        public SyncStatusDto? SyncStatus { get; set; }
    }

    /// <summary>
    /// A simulated attempt result template for one challenge. <see cref="ResponseTemplate"/>
    /// is the accepted/correct result (its <c>client_attempt_uuid</c> is filled from the
    /// request at runtime). <see cref="SafeMistake"/> is the encouraging, tiered-hint
    /// feedback returned when the submitted answer does not match the fixture's
    /// fabricated expected answer.
    /// </summary>
    public class DemoAttemptFixtureDto
    {
        [JsonProperty("response_template")]
        public AttemptResponseDto? ResponseTemplate { get; set; }

        [JsonProperty("safe_mistake")]
        public AttemptFeedbackDto? SafeMistake { get; set; }
    }
}