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
        /// Get quiz list for a subject/term. Requires auth.
        /// <c>GET /api/v1/student/quizzes</c>
        /// </summary>
        Task<DataResult<QuizListDto>> GetQuizzesAsync(string subjectSlug, int termNumber, CancellationToken ct = default);

        /// <summary>
        /// Get detailed content for a quiz. Requires auth.
        /// <c>GET /api/v1/student/quizzes/{quiz_id}</c>
        /// </summary>
        Task<DataResult<QuizDetailDto>> GetQuizDetailAsync(string quizId, CancellationToken ct = default);

        /// <summary>
        /// Submit quiz attempt. Requires auth.
        /// <c>POST /api/v1/student/quizzes/{quiz_id}/attempts</c>
        /// </summary>
        Task<DataResult<QuizAttemptResponseDto>> SubmitQuizAttemptAsync(string quizId, QuizAttemptRequestDto request, CancellationToken ct = default);

        /// <summary>
        /// Get all quiz attempt results for the student. Requires auth.
        /// <c>GET /api/v1/student/quiz-results</c>
        /// </summary>
        Task<DataResult<QuizResultListDto>> GetQuizResultsAsync(CancellationToken ct = default);

        /// <summary>
        /// Get a specific quiz attempt result. Requires auth.
        /// <c>GET /api/v1/student/quiz-results/{attempt_id}</c>
        /// </summary>
        Task<DataResult<QuizResultDto>> GetQuizResultAsync(string attemptId, CancellationToken ct = default);

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