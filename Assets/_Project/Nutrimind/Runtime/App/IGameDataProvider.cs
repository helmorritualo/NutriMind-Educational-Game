using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Contract for all game-data providers (local demo JSON or remote HTTP).
    /// Consumers depend solely on this interface and never branch on
    /// <see cref="DataProviderMode"/>.
    /// <para>
    /// All methods return <see cref="Task{DataResult{T}}"/> so that
    /// HTTP providers can perform genuine async I/O, and callers can
    /// cancel via <see cref="CancellationToken"/>.
    /// </para>
    /// <para>
    /// Local demo providers deserialize fixture JSON into the same DTOs
    /// that the HTTP provider uses. The outer demo bundle is a fixture
    /// container — it is not itself an API response.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Each method returns a <see cref="DataResult{T}"/> that wraps success/failure
    /// uniformly, allowing the caller to handle errors without exceptions.
    /// The structured <see cref="DataProviderError"/> on failure carries a
    /// stable <see cref="DataProviderError.Code"/> for programmatic handling.
    /// </remarks>
    public interface IGameDataProvider
    {
        // ──────────────────────────────────────────────────────────────
        //  Connectivity & Config
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Lightweight connectivity check. No auth required.
        /// <c>GET /api/v1/student/ping</c>
        /// </summary>
        Task<DataResult<PingResponseDto>> PingAsync(CancellationToken ct = default);

        /// <summary>
        /// Connection/config contract. No auth required.
        /// <c>GET /api/v1/student/config</c>
        /// </summary>
        Task<DataResult<ApiConfigDto>> GetConfigAsync(CancellationToken ct = default);

        // ──────────────────────────────────────────────────────────────
        //  Auth
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Student login. No auth required (the session is being established).
        /// <c>POST /api/v1/student/auth/login</c>
        /// </summary>
        Task<DataResult<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default);

        /// <summary>
        /// End student session. Requires auth.
        /// <c>POST /api/v1/student/auth/logout</c>
        /// </summary>
        Task<DataResult<object>> LogoutAsync(CancellationToken ct = default);

        // ──────────────────────────────────────────────────────────────
        //  Bootstrap & Profile
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Initial student/classroom/subject/wallet snapshot. Requires auth.
        /// <c>GET /api/v1/student/bootstrap</c>
        /// </summary>
        Task<DataResult<BootstrapDto>> GetBootstrapAsync(CancellationToken ct = default);

        /// <summary>
        /// Student-safe profile and progress-summary data. Requires auth.
        /// <c>GET /api/v1/student/profile</c>
        /// </summary>
        Task<DataResult<StudentProfileDto>> GetProfileAsync(CancellationToken ct = default);

        // ──────────────────────────────────────────────────────────────
        //  Settings
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Load game settings. Requires auth.
        /// <c>GET /api/v1/student/settings</c>
        /// </summary>
        Task<DataResult<SettingsDto>> GetSettingsAsync(CancellationToken ct = default);

        /// <summary>
        /// Persist game settings. Requires auth.
        /// <c>PATCH /api/v1/student/settings</c>
        /// </summary>
        Task<DataResult<SettingsDto>> PatchSettingsAsync(SettingsDto settings, CancellationToken ct = default);

        // ──────────────────────────────────────────────────────────────
        //  Subjects, Terms, Stations
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Available subject platforms. Requires auth.
        /// <c>GET /api/v1/student/subjects</c>
        /// </summary>
        Task<DataResult<List<SubjectDto>>> GetSubjectsAsync(CancellationToken ct = default);

        /// <summary>
        /// Terms and world metadata for a subject. Requires auth.
        /// <c>GET /api/v1/student/subjects/{subject_slug}/terms</c>
        /// </summary>
        Task<DataResult<List<TermDto>>> GetTermsAsync(string subjectSlug, CancellationToken ct = default);

        /// <summary>
        /// Station state list for a subject/term. Requires auth.
        /// <c>GET /api/v1/student/subjects/{subject_slug}/terms/{term_number}/stations</c>
        /// <para>
        /// Returns a <see cref="StationListDto"/> object (not a bare array) so
        /// Science exploration-preview terms can return an empty
        /// <see cref="StationListDto.Stations"/> array with a
        /// <see cref="StationListDto.PreviewMode"/> marker. An empty list is a
        /// valid preview state, not an error.
        /// </para>
        /// </summary>
        Task<DataResult<StationListDto>> GetStationsAsync(string subjectSlug, int termNumber, CancellationToken ct = default);

        // ──────────────────────────────────────────────────────────────
        //  Station Content & Session
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Approved station content and world tasks. Requires auth.
        /// <c>GET /api/v1/student/stations/{station_id}/content</c>
        /// </summary>
        Task<DataResult<StationContentDto>> GetStationContentAsync(string stationId, CancellationToken ct = default);

        /// <summary>
        /// Start or resume station session. Requires auth.
        /// <c>POST /api/v1/student/stations/{station_id}/start</c>
        /// </summary>
        Task<DataResult<StationStartResponseDto>> StartStationAsync(string stationId, StationStartRequestDto request = null, CancellationToken ct = default);

        // ──────────────────────────────────────────────────────────────
        //  Attempts
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Submit answer attempt. Requires auth.
        /// <c>POST /api/v1/student/challenges/{challenge_id}/attempts</c>
        /// <para>
        /// The <see cref="AttemptRequestDto.ClientAttemptUuid"/> is sent as
        /// <c>client_attempt_uuid</c> for idempotent retry. Duplicate
        /// UUIDs resolve to the existing attempt result.
        /// </para>
        /// </summary>
        Task<DataResult<AttemptResponseDto>> SubmitAttemptAsync(string challengeId, AttemptRequestDto request, CancellationToken ct = default);

        // ──────────────────────────────────────────────────────────────
        //  Station Completion
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Finalize station completion where separate completion is required. Requires auth.
        /// <c>POST /api/v1/student/stations/{station_id}/complete</c>
        /// </summary>
        Task<DataResult<StationCompleteResponseDto>> CompleteStationAsync(string stationId, StationCompleteRequestDto request = null, CancellationToken ct = default);

        // ──────────────────────────────────────────────────────────────
        //  Progress & Rewards
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Progress overview. Requires auth.
        /// <c>GET /api/v1/student/progress/summary</c>
        /// </summary>
        Task<DataResult<ProgressSummaryDto>> GetProgressSummaryAsync(CancellationToken ct = default);

        /// <summary>
        /// Reward wallet. Requires auth.
        /// <c>GET /api/v1/student/rewards</c>
        /// </summary>
        Task<DataResult<RewardWalletDto>> GetRewardsAsync(CancellationToken ct = default);

        /// <summary>
        /// Use a server-approved reward. Requires auth.
        /// <c>POST /api/v1/student/rewards/{reward_code}/use</c>
        /// </summary>
        Task<DataResult<UseRewardResponseDto>> UseRewardAsync(string rewardCode, UseRewardRequestDto request, CancellationToken ct = default);

        // ──────────────────────────────────────────────────────────────
        //  Sync
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Revision-based refresh metadata. Requires auth.
        /// <c>GET /api/v1/student/sync/status</c>
        /// </summary>
        Task<DataResult<SyncStatusDto>> GetSyncStatusAsync(CancellationToken ct = default);
    }
}