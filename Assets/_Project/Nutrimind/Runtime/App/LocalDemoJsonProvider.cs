using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NutriMind.Runtime.App.Dto;
using UnityEngine;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Development/demo data provider. Deserializes the fabricated demo fixture
    /// into the SAME DTOs the HTTP provider uses, so scenes and contracts behave
    /// identically to a real session. The fixture source is treated as immutable
    /// — every response is deep-cloned and a separate, resettable live session
    /// state holds mutations (settings, progress, wallet, unlocks, attempt and
    /// completion idempotency records).
    /// <para>
    /// This provider is editor/development-only. Release builds must not use it
    /// (see <see cref="CompositionRoot"/>'s production guard); the constructor
    /// also refuses to run outside the editor or a development build.
    /// </para>
    /// </summary>
    public sealed class LocalDemoJsonProvider : IGameDataProvider
    {
        /// <summary>Resources path (no extension) of the demo fixture.</summary>
        public const string ResourcesFixturePath = "DemoData/full-demo-student-data";

        private readonly DemoFixtureDto _fixture;        // immutable parsed source
        private readonly DataProviderError _loadError;   // non-null when fixture unavailable

        // ── Static topology derived from the immutable fixture ──
        private readonly Dictionary<string, (string Slug, int Term)> _stationScope = new();
        private readonly Dictionary<string, List<string>> _termStations = new();   // "slug:term" -> station ids (ordered)
        private readonly Dictionary<string, List<string>> _subjectStations = new(); // slug -> station ids
        private readonly Dictionary<string, List<string>> _stationChallenges = new(); // station id -> challenge ids
        private readonly Dictionary<string, string> _challengeStation = new();      // challenge id -> station id

        // ── Mutable, resettable live session state ──
        private bool _authenticated;
        private SettingsDto _settings;
        private RewardWalletDto _wallet;
        private readonly HashSet<string> _startedStations = new();
        private readonly HashSet<string> _completedStations = new();
        private readonly HashSet<string> _completedChallenges = new();
        private readonly Dictionary<string, string> _stationStates = new();         // station id -> live state
        private readonly Dictionary<string, AttemptResponseDto> _attemptsByUuid = new();
        private readonly Dictionary<string, StationCompleteResponseDto> _completionByStation = new();
        private int _progressRev = 1, _settingsRev = 1, _unlockRev = 1, _walletRev = 1;
        private const string ContentRevision = "content-rev-1";

        // ──────────────────────────────────────────────────────────────
        //  Construction
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates the provider. When <paramref name="fixtureJson"/> is null the
        /// fixture is loaded from Resources (<see cref="ResourcesFixturePath"/>).
        /// Tests may inject fixture JSON directly for determinism.
        /// </summary>
        public LocalDemoJsonProvider(string fixtureJson = null)
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            throw new InvalidOperationException(
                "LocalDemoJsonProvider is development-only and must not be constructed in release builds. " +
                "Use DataProviderMode.Http for production.");
#else
            try
            {
                if (string.IsNullOrEmpty(fixtureJson))
                {
                    var asset = Resources.Load<TextAsset>(ResourcesFixturePath);
                    fixtureJson = asset != null ? asset.text : null;
                }

                if (string.IsNullOrEmpty(fixtureJson))
                {
                    _loadError = new DataProviderError("CONFIGURATION_ERROR",
                        "The demo fixture could not be found. Generate it via NutriMind > Generate Demo Fixture.")
                    { Action = "show_offline_prompt" };
                    return;
                }

                _fixture = JsonConvert.DeserializeObject<DemoFixtureDto>(fixtureJson, JsonSettings.SafeDefaults);
                if (_fixture == null)
                {
                    _loadError = new DataProviderError("CONFIGURATION_ERROR",
                        "The demo fixture could not be read.") { Action = "show_offline_prompt" };
                    return;
                }

                BuildTopology();
                ResetDemoState();
            }
            catch (Exception)
            {
                // Never surface raw exception text to students.
                _loadError = new DataProviderError("CONFIGURATION_ERROR",
                    "The demo fixture could not be loaded.") { Action = "show_offline_prompt" };
            }
#endif
        }

        /// <summary>True when a valid demo fixture is loaded and ready.</summary>
        public bool IsReady => _fixture != null && _loadError == null;

        /// <summary>True when the demo session is currently logged in.</summary>
        public bool IsAuthenticated => _authenticated;

        // ──────────────────────────────────────────────────────────────
        //  Resettable live state
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Resets all live session state back to the immutable fixture baseline:
        /// logs out, restores baseline settings/wallet, and clears all progress,
        /// unlocks, and idempotency records. The fixture source is never modified.
        /// </summary>
        public void ResetDemoState()
        {
            if (_fixture == null) return;

            _authenticated = false;
            _settings = Clone(_fixture.Responses?.Settings) ?? new SettingsDto();
            _wallet = Clone(_fixture.Responses?.Rewards) ?? new RewardWalletDto { Rewards = new List<RewardBalanceDto>() };
            _startedStations.Clear();
            _completedStations.Clear();
            _completedChallenges.Clear();
            _attemptsByUuid.Clear();
            _completionByStation.Clear();
            _progressRev = _settingsRev = _unlockRev = _walletRev = 1;

            _stationStates.Clear();
            if (_fixture.StationsByScope != null)
            {
                foreach (var list in _fixture.StationsByScope.Values)
                {
                    if (list?.Stations == null) continue;
                    foreach (var s in list.Stations)
                        if (!string.IsNullOrEmpty(s?.Id))
                            _stationStates[s.Id] = s.State ?? "locked";
                }
            }
        }

        private void BuildTopology()
        {
            if (_fixture.StationsByScope != null)
            {
                foreach (var list in _fixture.StationsByScope.Values)
                {
                    if (list?.Stations == null) continue;
                    foreach (var s in list.Stations)
                    {
                        if (string.IsNullOrEmpty(s?.Id)) continue;
                        string slug = s.SubjectSlug ?? list.SubjectSlug ?? "";
                        int term = s.TermNumber ?? list.TermNumber ?? 0;
                        _stationScope[s.Id] = (slug, term);
                        Append(_termStations, $"{slug}:{term}", s.Id);
                        Append(_subjectStations, slug, s.Id);
                    }
                }
            }

            if (_fixture.StationContentById != null)
            {
                foreach (var kvp in _fixture.StationContentById)
                {
                    var content = kvp.Value;
                    if (content?.Challenges == null) continue;
                    foreach (var c in content.Challenges)
                    {
                        if (string.IsNullOrEmpty(c?.ChallengeId)) continue;
                        Append(_stationChallenges, kvp.Key, c.ChallengeId);
                        _challengeStation[c.ChallengeId] = kvp.Key;
                    }
                }
            }
        }

        private static void Append(Dictionary<string, List<string>> map, string key, string value)
        {
            if (!map.TryGetValue(key, out var list)) { list = new List<string>(); map[key] = list; }
            if (!list.Contains(value)) list.Add(value);
        }

        // ──────────────────────────────────────────────────────────────
        //  Connectivity & Config
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<PingResponseDto>> PingAsync(CancellationToken ct = default)
            => Result(_fixture?.Responses?.Ping ?? new PingResponseDto { Status = "ok" });

        public Task<DataResult<ApiConfigDto>> GetConfigAsync(CancellationToken ct = default)
            => Result(_fixture?.Responses?.Config);

        // ──────────────────────────────────────────────────────────────
        //  Auth
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
        {
            if (_loadError != null) return Fail<LoginResponseDto>(_loadError);

            var auth = _fixture.DemoAuth;
            bool matches = auth != null
                && request != null
                && string.Equals(request.Lrn, auth.Lrn, StringComparison.Ordinal)
                && string.Equals(request.Pin, auth.Pin, StringComparison.Ordinal);

            if (!matches)
            {
                return Fail<LoginResponseDto>(new DataProviderError("UNAUTHENTICATED",
                    "That LRN or PIN doesn't match. Please try again.") { Action = "login_again", Retryable = false });
            }

            _authenticated = true;
            return Result(_fixture.Responses?.Login);
        }

        public Task<DataResult<object>> LogoutAsync(CancellationToken ct = default)
        {
            // Logout always succeeds and returns the demo to a clean baseline.
            ResetDemoState();
            return Task.FromResult(DataResult<object>.Ok(new object()));
        }

        // ──────────────────────────────────────────────────────────────
        //  Bootstrap & Profile
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<BootstrapDto>> GetBootstrapAsync(CancellationToken ct = default)
        {
            if (!TryAuth<BootstrapDto>(out var fail)) return fail;
            var boot = Clone(_fixture.Responses?.Bootstrap) ?? new BootstrapDto();
            boot.ProgressSummary = BuildProgressSummary();
            boot.Rewards = Clone(_wallet);
            boot.Settings = Clone(_settings);
            boot.SyncStatus = BuildSyncStatus();
            return Task.FromResult(DataResult<BootstrapDto>.Ok(boot));
        }

        public Task<DataResult<StudentProfileDto>> GetProfileAsync(CancellationToken ct = default)
        {
            if (!TryAuth<StudentProfileDto>(out var fail)) return fail;
            return Result(_fixture.Responses?.Profile);
        }

        // ──────────────────────────────────────────────────────────────
        //  Settings
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<SettingsDto>> GetSettingsAsync(CancellationToken ct = default)
        {
            if (!TryAuth<SettingsDto>(out var fail)) return fail;
            return Task.FromResult(DataResult<SettingsDto>.Ok(Clone(_settings)));
        }

        public Task<DataResult<SettingsDto>> PatchSettingsAsync(SettingsDto settings, CancellationToken ct = default)
        {
            if (!TryAuth<SettingsDto>(out var fail)) return fail;
            if (settings == null)
                return Fail<SettingsDto>(new DataProviderError("VALIDATION_ERROR", "No settings were provided.") { Action = "retry" });

            // Merge only non-null incoming fields into the live settings.
            if (settings.Language != null) _settings.Language = settings.Language;
            if (settings.MasterVolume.HasValue) _settings.MasterVolume = settings.MasterVolume;
            if (settings.MusicVolume.HasValue) _settings.MusicVolume = settings.MusicVolume;
            if (settings.SfxVolume.HasValue) _settings.SfxVolume = settings.SfxVolume;
            if (settings.MuteAll.HasValue) _settings.MuteAll = settings.MuteAll;
            if (settings.SubtitlesEnabled.HasValue) _settings.SubtitlesEnabled = settings.SubtitlesEnabled;
            if (settings.ReducedMotion.HasValue) _settings.ReducedMotion = settings.ReducedMotion;
            if (settings.ShowHints.HasValue) _settings.ShowHints = settings.ShowHints;
            if (settings.TextSize != null) _settings.TextSize = settings.TextSize;
            if (settings.NotificationsEnabled.HasValue) _settings.NotificationsEnabled = settings.NotificationsEnabled;
            if (settings.AccessibilityPreferences != null) _settings.AccessibilityPreferences = settings.AccessibilityPreferences;

            _settingsRev++;
            _settings.Revision = $"settings-rev-{_settingsRev}";
            return Task.FromResult(DataResult<SettingsDto>.Ok(Clone(_settings)));
        }

        // ──────────────────────────────────────────────────────────────
        //  Subjects, Terms, Stations
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<List<SubjectDto>>> GetSubjectsAsync(CancellationToken ct = default)
        {
            if (!TryAuth<List<SubjectDto>>(out var fail)) return fail;
            return Task.FromResult(DataResult<List<SubjectDto>>.Ok(Clone(_fixture.Responses?.Subjects) ?? new List<SubjectDto>()));
        }

        public Task<DataResult<List<TermDto>>> GetTermsAsync(string subjectSlug, CancellationToken ct = default)
        {
            if (!TryAuth<List<TermDto>>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(subjectSlug))
                return Fail<List<TermDto>>(new DataProviderError("VALIDATION_ERROR", "Subject slug is required.") { Action = "retry" });

            if (_fixture.TermsBySubject != null && _fixture.TermsBySubject.TryGetValue(subjectSlug, out var terms))
                return Task.FromResult(DataResult<List<TermDto>>.Ok(Clone(terms) ?? new List<TermDto>()));

            return Fail<List<TermDto>>(new DataProviderError("NOT_FOUND", "That subject could not be found.") { Action = "return_to_menu" });
        }

        public Task<DataResult<StationListDto>> GetStationsAsync(string subjectSlug, int termNumber, CancellationToken ct = default)
        {
            if (!TryAuth<StationListDto>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(subjectSlug))
                return Fail<StationListDto>(new DataProviderError("VALIDATION_ERROR", "Subject slug is required.") { Action = "retry" });

            string key = $"{subjectSlug}:{termNumber}";
            // Stations scope keys are stored as "slug:grade:term"; match on slug+term.
            string scopeKey = _fixture.StationsByScope?.Keys
                .FirstOrDefault(k => k.StartsWith(subjectSlug + ":", StringComparison.Ordinal)
                                  && k.EndsWith(":" + termNumber, StringComparison.Ordinal));

            if (scopeKey == null || _fixture.StationsByScope == null
                || !_fixture.StationsByScope.TryGetValue(scopeKey, out var list))
            {
                return Fail<StationListDto>(new DataProviderError("NOT_FOUND", "That world could not be found.") { Action = "return_to_menu" });
            }

            var clone = Clone(list) ?? new StationListDto();
            // Overlay live station states (unlocks/completions) onto the response.
            if (clone.Stations != null)
            {
                foreach (var s in clone.Stations)
                {
                    if (s?.Id == null) continue;
                    if (_stationStates.TryGetValue(s.Id, out var live)) s.State = live;
                    if (_completedStations.Contains(s.Id)) { s.State = "completed"; s.ProgressPercent = 100m; }
                }
            }
            return Task.FromResult(DataResult<StationListDto>.Ok(clone));
        }

        // ──────────────────────────────────────────────────────────────
        //  Station Content & Session
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<StationContentDto>> GetStationContentAsync(string stationId, CancellationToken ct = default)
        {
            if (!TryAuth<StationContentDto>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(stationId))
                return Fail<StationContentDto>(new DataProviderError("VALIDATION_ERROR", "Station ID is required.") { Action = "retry" });

            if (_fixture.StationContentById != null && _fixture.StationContentById.TryGetValue(stationId, out var content))
                return Task.FromResult(DataResult<StationContentDto>.Ok(Clone(content)));

            return Fail<StationContentDto>(new DataProviderError("CONTENT_NOT_PUBLISHED", "This activity isn't ready yet.") { Action = "return_to_menu" });
        }

        public Task<DataResult<StationStartResponseDto>> StartStationAsync(string stationId, StationStartRequestDto request = null, CancellationToken ct = default)
        {
            if (!TryAuth<StationStartResponseDto>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(stationId))
                return Fail<StationStartResponseDto>(new DataProviderError("VALIDATION_ERROR", "Station ID is required.") { Action = "retry" });

            if (_fixture.StationStartById == null || !_fixture.StationStartById.TryGetValue(stationId, out var start))
                return Fail<StationStartResponseDto>(new DataProviderError("NOT_FOUND", "That activity could not be found.") { Action = "return_to_menu" });

            string state = _stationStates.TryGetValue(stationId, out var st) ? st : "locked";
            if (state == "locked")
                return Fail<StationStartResponseDto>(new DataProviderError("STATION_LOCKED", "This station is still locked. Finish the earlier station first.") { Action = "return_to_menu", Retryable = false });

            bool resuming = _startedStations.Contains(stationId);
            _startedStations.Add(stationId);
            if (!_completedStations.Contains(stationId) && state == "unlocked")
                _stationStates[stationId] = "started";

            var clone = Clone(start) ?? new StationStartResponseDto { StationId = stationId };
            clone.Resuming = resuming;
            clone.Status = _completedStations.Contains(stationId) ? "completed" : "in_progress";
            clone.ChallengeProgress = BuildChallengeProgress(stationId);
            return Task.FromResult(DataResult<StationStartResponseDto>.Ok(clone));
        }

        // ──────────────────────────────────────────────────────────────
        //  Attempts
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<AttemptResponseDto>> SubmitAttemptAsync(string challengeId, AttemptRequestDto request, CancellationToken ct = default)
        {
            if (!TryAuth<AttemptResponseDto>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(challengeId) || request == null)
                return Fail<AttemptResponseDto>(new DataProviderError("VALIDATION_ERROR", "A challenge attempt requires a challenge and an answer.") { Action = "retry" });

            // Idempotent replay: a previously processed client_attempt_uuid returns
            // the SAME result with is_replay=true — no double scoring or rewards.
            string uuid = request.ClientAttemptUuid;
            if (!string.IsNullOrEmpty(uuid) && _attemptsByUuid.TryGetValue(uuid, out var prior))
            {
                var replay = Clone(prior);
                replay.IsReplay = true;
                return Task.FromResult(DataResult<AttemptResponseDto>.Ok(replay));
            }

            if (_fixture.AttemptResultByChallengeId == null || !_fixture.AttemptResultByChallengeId.TryGetValue(challengeId, out var attemptFixture))
                return Fail<AttemptResponseDto>(new DataProviderError("NOT_FOUND", "That challenge could not be found.") { Action = "return_to_menu" });

            string stationId = _challengeStation.TryGetValue(challengeId, out var sid) ? sid : null;
            bool alreadyCorrect = _completedChallenges.Contains(challengeId);
            bool correct = alreadyCorrect || EvaluateAnswer(challengeId, request.Answer);

            AttemptResponseDto response;
            if (correct)
            {
                response = Clone(attemptFixture.ResponseTemplate) ?? new AttemptResponseDto();
                response.ChallengeId = challengeId;
                response.Status = "accepted";
                response.Accepted = true;
                response.Correct = true;
                response.IsReplay = false;

                bool firstTime = !alreadyCorrect;
                if (firstTime)
                {
                    _completedChallenges.Add(challengeId);
                    GrantRewards(response.RewardsGranted);   // grant coins once
                    _progressRev++;
                    response.ProgressUpdated = true;
                }
                else
                {
                    response.RewardsGranted = new List<RewardGrantDto>();
                    response.ProgressUpdated = false;
                    response.ScoreAwarded = 0m;
                }
            }
            else
            {
                // Safe mistake: encouraging, tiered hint; no penalty, world progress preserved.
                response = new AttemptResponseDto
                {
                    ChallengeId = challengeId,
                    Status = "rejected",
                    Accepted = true,     // the attempt was accepted/recorded
                    Correct = false,
                    IsReplay = false,
                    ScoreAwarded = 0m,
                    ProgressUpdated = false,
                    Feedback = Clone(attemptFixture.SafeMistake) ?? new AttemptFeedbackDto { IsCorrect = false, RetryAllowed = true },
                    RewardsGranted = new List<RewardGrantDto>()
                };
            }

            response.AttemptId = string.IsNullOrEmpty(response.AttemptId) ? $"attempt-{Guid.NewGuid():N}" : response.AttemptId;
            response.ClientAttemptUuid = uuid;
            response.Progress = BuildAttemptProgress(stationId);
            response.ProgressRevision = $"progress-rev-{_progressRev}";
            response.RewardWalletRevision = $"wallet-rev-{_walletRev}";

            if (!string.IsNullOrEmpty(uuid))
                _attemptsByUuid[uuid] = Clone(response);   // store canonical (non-replay) result

            return Task.FromResult(DataResult<AttemptResponseDto>.Ok(response));
        }

        // ──────────────────────────────────────────────────────────────
        //  Station Completion
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<StationCompleteResponseDto>> CompleteStationAsync(string stationId, StationCompleteRequestDto request = null, CancellationToken ct = default)
        {
            if (!TryAuth<StationCompleteResponseDto>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(stationId))
                return Fail<StationCompleteResponseDto>(new DataProviderError("VALIDATION_ERROR", "Station ID is required.") { Action = "retry" });

            // Idempotent replay: re-completing a finished station returns the SAME
            // result with is_replay=true — no double rewards, crystals, or unlocks.
            if (_completionByStation.TryGetValue(stationId, out var prior))
            {
                var replay = Clone(prior);
                replay.IsReplay = true;
                replay.ProgressSummary = BuildProgressSummary();
                return Task.FromResult(DataResult<StationCompleteResponseDto>.Ok(replay));
            }

            if (_fixture.CompletionResultByStationId == null || !_fixture.CompletionResultByStationId.TryGetValue(stationId, out var template))
                return Fail<StationCompleteResponseDto>(new DataProviderError("NOT_FOUND", "That activity could not be found.") { Action = "return_to_menu" });

            // Require all required challenges complete before finalizing.
            var challenges = _stationChallenges.TryGetValue(stationId, out var chs) ? chs : new List<string>();
            int required = challenges.Count;
            int done = challenges.Count(c => _completedChallenges.Contains(c));
            if (required > 0 && done < required)
                return Fail<StationCompleteResponseDto>(new DataProviderError("VALIDATION_ERROR", "Finish all the challenges before completing this station.") { Action = "retry", Retryable = false });

            var response = Clone(template) ?? new StationCompleteResponseDto { StationId = stationId };
            response.StationId = stationId;
            response.Status = "completed";
            response.Completed = true;
            response.IsReplay = false;

            // Apply completion once.
            _completedStations.Add(stationId);
            _stationStates[stationId] = "completed";
            GrantRewards(response.RewardsGranted);   // completion coins
            _wallet.TotalStars = (_wallet.TotalStars ?? 0) + 1;
            _walletRev++;
            _progressRev++;

            // Compute unlocks.
            response.Unlocks = ComputeUnlocks(stationId);
            if (response.Unlocks.Count > 0) _unlockRev++;

            // Term completion + subject crystal (granted once when both term stations done).
            response.TermCompletion = ComputeTermCompletion(stationId);

            // World restoration applies only after this accepted completion.
            if (response.WorldRestorationResult != null)
                response.WorldRestorationResult.Restored = true;

            response.ProgressSummary = BuildProgressSummary();
            response.ProgressRevision = $"progress-rev-{_progressRev}";
            response.RewardWalletRevision = $"wallet-rev-{_walletRev}";

            _completionByStation[stationId] = Clone(response);
            return Task.FromResult(DataResult<StationCompleteResponseDto>.Ok(response));
        }

        // ──────────────────────────────────────────────────────────────
        //  Progress & Rewards
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<ProgressSummaryDto>> GetProgressSummaryAsync(CancellationToken ct = default)
        {
            if (!TryAuth<ProgressSummaryDto>(out var fail)) return fail;
            return Task.FromResult(DataResult<ProgressSummaryDto>.Ok(BuildProgressSummary()));
        }

        public Task<DataResult<RewardWalletDto>> GetRewardsAsync(CancellationToken ct = default)
        {
            if (!TryAuth<RewardWalletDto>(out var fail)) return fail;
            return Task.FromResult(DataResult<RewardWalletDto>.Ok(BuildWallet()));
        }

        public Task<DataResult<UseRewardResponseDto>> UseRewardAsync(string rewardCode, UseRewardRequestDto request, CancellationToken ct = default)
        {
            if (!TryAuth<UseRewardResponseDto>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(rewardCode))
                return Fail<UseRewardResponseDto>(new DataProviderError("VALIDATION_ERROR", "A reward code is required.") { Action = "retry" });

            var entry = _wallet.Rewards?.FirstOrDefault(r => r.RewardCode == rewardCode);
            int qty = request?.Quantity ?? 1;
            if (qty < 1) qty = 1;

            if (entry == null || entry.IsUsable != true || (entry.Quantity ?? 0) < qty)
            {
                return Fail<UseRewardResponseDto>(new DataProviderError("VALIDATION_ERROR",
                    "You don't have enough of that reward to use right now.") { Action = "return_to_menu", Retryable = false });
            }

            entry.Quantity -= qty;
            if (entry.RewardType == "coin") _wallet.TotalCoins = entry.Quantity;
            _walletRev++;
            _wallet.Revision = $"wallet-rev-{_walletRev}";

            return Task.FromResult(DataResult<UseRewardResponseDto>.Ok(new UseRewardResponseDto
            {
                RewardCode = rewardCode,
                RemainingQuantity = entry.Quantity,
                Effect = entry.RewardCode == "hint_token" ? "hint_revealed" : "reward_used"
            }));
        }

        // ──────────────────────────────────────────────────────────────
        //  Sync
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<SyncStatusDto>> GetSyncStatusAsync(CancellationToken ct = default)
        {
            if (!TryAuth<SyncStatusDto>(out var fail)) return fail;
            return Task.FromResult(DataResult<SyncStatusDto>.Ok(BuildSyncStatus()));
        }

        // ──────────────────────────────────────────────────────────────
        //  Internal helpers
        // ──────────────────────────────────────────────────────────────

        private bool TryAuth<T>(out Task<DataResult<T>> failTask)
        {
            if (_loadError != null) { failTask = Fail<T>(_loadError); return false; }
            if (!_authenticated)
            {
                failTask = Fail<T>(new DataProviderError("UNAUTHENTICATED",
                    "Your session ended. Please log in again.") { Action = "login_again", Retryable = false });
                return false;
            }
            failTask = null;
            return true;
        }

        private Task<DataResult<T>> Result<T>(T data)
        {
            if (_loadError != null) return Fail<T>(_loadError);
            if (data == null)
                return Fail<T>(new DataProviderError("NOT_FOUND", "That information isn't available in the demo.") { Action = "return_to_menu" });
            return Task.FromResult(DataResult<T>.Ok(Clone(data)));
        }

        private static Task<DataResult<T>> Fail<T>(DataProviderError error)
            => Task.FromResult(DataResult<T>.Fail(error));

        private bool EvaluateAnswer(string challengeId, object answer)
        {
            if (_fixture.DemoOnlyEvaluation == null || !_fixture.DemoOnlyEvaluation.TryGetValue(challengeId, out var eval))
                return false;
            JToken expected = eval is JObject obj ? obj["expected_answer"] : eval;
            if (expected == null) return false;
            JToken actual = answer == null ? JValue.CreateNull() : JToken.FromObject(answer);
            // Unwrap a common {"answer": X} / {"selected_option": X} envelope.
            if (actual is JObject ao)
            {
                if (ao["expected_answer"] != null) actual = ao["expected_answer"];
                else if (ao["answer"] != null) actual = ao["answer"];
                else if (ao["selected_option"] != null) actual = ao["selected_option"];
            }
            return DeepMatch(expected, actual);
        }

        private static bool DeepMatch(JToken expected, JToken actual)
        {
            if (expected == null || actual == null) return false;
            if (expected.Type == JTokenType.Array && actual.Type == JTokenType.Array)
            {
                var e = (JArray)expected; var a = (JArray)actual;
                if (e.Count != a.Count) return false;
                for (int i = 0; i < e.Count; i++) if (!DeepMatch(e[i], a[i])) return false;
                return true;
            }
            if (expected.Type == JTokenType.Object && actual.Type == JTokenType.Object)
            {
                var e = (JObject)expected; var a = (JObject)actual;
                if (e.Count != a.Count) return false;
                foreach (var p in e)
                {
                    var av = a[p.Key];
                    if (av == null || !DeepMatch(p.Value, av)) return false;
                }
                return true;
            }
            return string.Equals(Scalar(expected), Scalar(actual), StringComparison.OrdinalIgnoreCase);
        }

        private static string Scalar(JToken t) => t == null || t.Type == JTokenType.Null ? string.Empty : t.ToString().Trim();

        private void GrantRewards(List<RewardGrantDto> grants)
        {
            if (grants == null || _wallet.Rewards == null) return;
            bool changed = false;
            foreach (var g in grants)
            {
                if (string.IsNullOrEmpty(g?.RewardCode)) continue;
                var entry = _wallet.Rewards.FirstOrDefault(r => r.RewardCode == g.RewardCode);
                int add = g.Quantity ?? 0;
                if (entry == null)
                {
                    entry = new RewardBalanceDto
                    {
                        RewardCode = g.RewardCode,
                        RewardType = g.RewardType,
                        DisplayName = g.DisplayName,
                        Quantity = add,
                        IsUsable = g.RewardType == "coin" ? false : true
                    };
                    _wallet.Rewards.Add(entry);
                }
                else entry.Quantity = (entry.Quantity ?? 0) + add;

                if (g.RewardType == "coin") _wallet.TotalCoins = (_wallet.TotalCoins ?? 0) + add;
                changed = true;
            }
            if (changed) { _walletRev++; _wallet.Revision = $"wallet-rev-{_walletRev}"; }
        }

        private List<StationUnlockDto> ComputeUnlocks(string stationId)
        {
            var unlocks = new List<StationUnlockDto>();
            if (!_stationScope.TryGetValue(stationId, out var scope)) return unlocks;

            // Unlock remaining stations in this term.
            if (_termStations.TryGetValue($"{scope.Slug}:{scope.Term}", out var termStations))
            {
                foreach (var sid in termStations)
                {
                    if (sid == stationId || _completedStations.Contains(sid)) continue;
                    if (_stationStates.TryGetValue(sid, out var s) && s == "locked")
                    {
                        _stationStates[sid] = "unlocked";
                        unlocks.Add(new StationUnlockDto { StationId = sid, State = "unlocked" });
                    }
                }
            }

            // If the term is now complete, unlock the first station of the next term.
            bool termComplete = _termStations.TryGetValue($"{scope.Slug}:{scope.Term}", out var ts)
                                && ts.All(_completedStations.Contains);
            if (termComplete && _termStations.TryGetValue($"{scope.Slug}:{scope.Term + 1}", out var next) && next.Count > 0)
            {
                string first = next[0];
                if (_stationStates.TryGetValue(first, out var s2) && s2 == "locked")
                {
                    _stationStates[first] = "unlocked";
                    unlocks.Add(new StationUnlockDto { StationId = first, State = "unlocked" });
                }
            }
            return unlocks;
        }

        private TermCompletionDto ComputeTermCompletion(string stationId)
        {
            if (!_stationScope.TryGetValue(stationId, out var scope)) return null;
            if (!_termStations.TryGetValue($"{scope.Slug}:{scope.Term}", out var termStations)) return null;

            bool complete = termStations.All(_completedStations.Contains);
            var tc = new TermCompletionDto { SubjectSlug = scope.Slug, TermNumber = scope.Term, Completed = complete };
            if (complete)
            {
                var crystal = new RewardGrantDto
                {
                    RewardCode = $"crystal_{scope.Slug}",
                    RewardType = "crystal",
                    DisplayName = $"{Capitalize(scope.Slug)} Crystal",
                    Quantity = 1
                };
                tc.Crystal = crystal;
                GrantRewards(new List<RewardGrantDto> { crystal });   // grant once (completion is idempotent)
            }
            return tc;
        }

        private Dictionary<string, object> BuildChallengeProgress(string stationId)
        {
            var map = new Dictionary<string, object>();
            if (_stationChallenges.TryGetValue(stationId, out var chs))
                foreach (var c in chs) map[c] = _completedChallenges.Contains(c) ? "completed" : "pending";
            return map;
        }

        private AttemptProgressDto BuildAttemptProgress(string stationId)
        {
            if (stationId == null || !_stationChallenges.TryGetValue(stationId, out var chs) || chs.Count == 0)
                return new AttemptProgressDto { CompletedChallenges = 0, RequiredChallenges = 0, StationProgressPercent = 0m };
            int done = chs.Count(c => _completedChallenges.Contains(c));
            return new AttemptProgressDto
            {
                CompletedChallenges = done,
                RequiredChallenges = chs.Count,
                StationProgressPercent = Math.Round((decimal)done / chs.Count * 100m, 1)
            };
        }

        private RewardWalletDto BuildWallet()
        {
            var w = Clone(_wallet) ?? new RewardWalletDto();
            w.Revision = $"wallet-rev-{_walletRev}";
            return w;
        }

        private SyncStatusDto BuildSyncStatus()
        {
            var baseSync = Clone(_fixture.Responses?.SyncStatus) ?? new SyncStatusDto();
            baseSync.StudentProgressRevision = $"progress-rev-{_progressRev}";
            baseSync.StudentSettingsRevision = $"settings-rev-{_settingsRev}";
            baseSync.StationUnlockRevision = $"unlock-rev-{_unlockRev}";
            baseSync.PublishedContentRevision = ContentRevision;
            baseSync.RewardWalletRevision = $"wallet-rev-{_walletRev}";
            return baseSync;
        }

        private ProgressSummaryDto BuildProgressSummary()
        {
            var summary = Clone(_fixture.Responses?.ProgressSummary) ?? new ProgressSummaryDto();
            int totalAvailable = _subjectStations.Values.Sum(v => v.Count);
            int totalCompleted = _completedStations.Count;

            summary.TotalStationsAvailable = totalAvailable;
            summary.TotalStationsCompleted = totalCompleted;
            summary.StartedStations = _startedStations.Count;
            summary.OverallPercentage = totalAvailable == 0 ? 0m : Math.Round((decimal)totalCompleted / totalAvailable * 100m, 1);
            summary.Stars = _wallet.TotalStars ?? 0;
            summary.Coins = _wallet.TotalCoins ?? 0;
            summary.Revision = $"progress-rev-{_progressRev}";

            if (summary.Subjects != null)
            {
                foreach (var sp in summary.Subjects)
                {
                    string slug = sp.SubjectSlug ?? "";
                    var stations = _subjectStations.TryGetValue(slug, out var list) ? list : new List<string>();
                    int avail = stations.Count;
                    int done = stations.Count(_completedStations.Contains);
                    sp.StationsAvailable = avail;
                    sp.StationsCompleted = done;
                    sp.Percentage = avail == 0 ? 0m : Math.Round((decimal)done / avail * 100m, 1);
                    sp.ProgressPercent = sp.Percentage;

                    if (sp.Terms != null)
                    {
                        foreach (var tp in sp.Terms)
                        {
                            int term = tp.TermNumber ?? 0;
                            var ts = _termStations.TryGetValue($"{slug}:{term}", out var tlist) ? tlist : new List<string>();
                            int tAvail = ts.Count;
                            int tDone = ts.Count(_completedStations.Contains);
                            tp.StationsAvailable = tAvail;
                            tp.StationsCompleted = tDone;
                            tp.Percentage = tAvail == 0 ? 0m : Math.Round((decimal)tDone / tAvail * 100m, 1);
                        }
                    }
                }
            }
            return summary;
        }

        private static string Capitalize(string s)
            => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        /// <summary>
        /// Deep-clones a DTO via a JSON round-trip so the immutable fixture source
        /// is never exposed by reference and callers cannot mutate it.
        /// </summary>
        private static T Clone<T>(T src)
            => src == null ? default : JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(src, JsonSettings.SafeDefaults), JsonSettings.SafeDefaults);
    }
}