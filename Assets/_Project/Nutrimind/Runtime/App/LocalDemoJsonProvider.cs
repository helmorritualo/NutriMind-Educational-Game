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
        private readonly Dictionary<string, (string Slug, int Term)> _quizScope = new();
        private readonly Dictionary<string, List<string>> _termQuizzes = new();   // "slug:term" -> quiz ids (ordered)
        private readonly Dictionary<string, List<string>> _subjectQuizzes = new(); // slug -> quiz ids

        // ── Mutable, resettable live session state ──
        private bool _authenticated;
        private SettingsDto _settings;
        private RewardWalletDto _wallet;
        private readonly HashSet<string> _startedQuizzes = new();
        private readonly HashSet<string> _completedQuizzes = new();
        private readonly Dictionary<string, string> _quizStates = new();         // quiz id -> live state
        private readonly Dictionary<string, QuizAttemptResponseDto> _attemptsByUuid = new();
        private readonly Dictionary<string, QuizResultDto> _quizResultByAttemptId = new();
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
            _startedQuizzes.Clear();
            _completedQuizzes.Clear();
            _attemptsByUuid.Clear();
            _quizResultByAttemptId.Clear();
            _progressRev = _settingsRev = _unlockRev = _walletRev = 1;

            _quizStates.Clear();
            if (_fixture.QuizzesByScope != null)
            {
                foreach (var list in _fixture.QuizzesByScope.Values)
                {
                    if (list?.Quizzes == null) continue;
                    foreach (var s in list.Quizzes)
                        if (!string.IsNullOrEmpty(s?.Id))
                            _quizStates[s.Id] = s.State ?? "locked";
                }
            }
        }

        private void BuildTopology()
        {
            if (_fixture.QuizzesByScope != null)
            {
                foreach (var list in _fixture.QuizzesByScope.Values)
                {
                    if (list?.Quizzes == null) continue;
                    foreach (var s in list.Quizzes)
                    {
                        if (string.IsNullOrEmpty(s?.Id)) continue;
                        string slug = s.SubjectSlug ?? list.SubjectSlug ?? "";
                        int term = s.TermNumber ?? list.TermNumber ?? 0;
                        _quizScope[s.Id] = (slug, term);
                        Append(_termQuizzes, $"{slug}:{term}", s.Id);
                        Append(_subjectQuizzes, slug, s.Id);
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
        //  Subjects, Terms, Quizzes
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

        public Task<DataResult<QuizListDto>> GetQuizzesAsync(string subjectSlug, int termNumber, CancellationToken ct = default)
        {
            if (!TryAuth<QuizListDto>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(subjectSlug))
                return Fail<QuizListDto>(new DataProviderError("VALIDATION_ERROR", "Subject slug is required.") { Action = "retry" });

            string key = $"{subjectSlug}:{termNumber}";
            string scopeKey = _fixture.QuizzesByScope?.Keys
                .FirstOrDefault(k => k.StartsWith(subjectSlug + ":", StringComparison.Ordinal)
                                  && k.EndsWith(":" + termNumber, StringComparison.Ordinal));

            if (scopeKey == null || _fixture.QuizzesByScope == null
                || !_fixture.QuizzesByScope.TryGetValue(scopeKey, out var list))
            {
                return Fail<QuizListDto>(new DataProviderError("NOT_FOUND", "That world could not be found.") { Action = "return_to_menu" });
            }

            var clone = Clone(list) ?? new QuizListDto();
            if (clone.Quizzes != null)
            {
                foreach (var q in clone.Quizzes)
                {
                    if (q?.Id == null) continue;
                    if (_quizStates.TryGetValue(q.Id, out var live)) q.State = live;
                    if (_completedQuizzes.Contains(q.Id)) { q.State = "completed"; q.ProgressPercent = 100m; }
                }
            }
            return Task.FromResult(DataResult<QuizListDto>.Ok(clone));
        }

        // ──────────────────────────────────────────────────────────────
        //  Quiz Detail & Attempts (Laravel quiz_first_laravel_1 REST)
        // ──────────────────────────────────────────────────────────────

        public Task<DataResult<QuizDetailDto>> GetQuizDetailAsync(string quizId, CancellationToken ct = default)
        {
            if (!TryAuth<QuizDetailDto>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(quizId))
                return Fail<QuizDetailDto>(new DataProviderError("VALIDATION_ERROR", "Quiz ID is required.") { Action = "retry" });

            if (_fixture.QuizDetailById != null && _fixture.QuizDetailById.TryGetValue(quizId, out var content))
            {
                var clone = Clone(content);
                if (_completedQuizzes.Contains(quizId)) { clone.State = "completed"; clone.ProgressPercent = 100m; }
                else if (_quizStates.TryGetValue(quizId, out var live)) { clone.State = live; }
                return Task.FromResult(DataResult<QuizDetailDto>.Ok(clone));
            }

            return Fail<QuizDetailDto>(new DataProviderError("CONTENT_NOT_PUBLISHED", "This quiz content isn't ready yet.") { Action = "return_to_menu" });
        }

        public Task<DataResult<QuizAttemptResponseDto>> SubmitQuizAttemptAsync(string quizId, QuizAttemptRequestDto request, CancellationToken ct = default)
        {
            if (!TryAuth<QuizAttemptResponseDto>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(quizId) || request == null)
                return Fail<QuizAttemptResponseDto>(new DataProviderError("VALIDATION_ERROR", "A quiz attempt requires a quiz and answer sheet.") { Action = "retry" });

            string uuid = request.ClientAttemptUuid;

            // Check for duplicate client_attempt_uuid replay.
            if (!string.IsNullOrEmpty(uuid) && _attemptsByUuid.TryGetValue(uuid, out var prior))
            {
                // Idempotent reply: if the answers match the prior attempt, replay the exact response with is_replay = true.
                // If the answers do NOT match, return a duplicate client_attempt_uuid conflict.
                bool matchesPriorAnswers = MatchAnswers(prior.QuizId, prior.AttemptId, request.Answers);
                if (matchesPriorAnswers)
                {
                    var replay = Clone(prior);
                    replay.IsReplay = true;
                    return Task.FromResult(DataResult<QuizAttemptResponseDto>.Ok(replay));
                }
                else
                {
                    return Fail<QuizAttemptResponseDto>(new DataProviderError("CONFLICT", "An attempt with this UUID already exists but contains different answers.") { Action = "retry", Retryable = false });
                }
            }

            if (_fixture.AttemptResultByQuizId == null || !_fixture.AttemptResultByQuizId.TryGetValue(quizId, out var attemptFixture))
                return Fail<QuizAttemptResponseDto>(new DataProviderError("NOT_FOUND", "That quiz could not be found.") { Action = "return_to_menu" });

            var responseTemplate = Clone(attemptFixture.ResponseTemplate) ?? new QuizAttemptResponseDto();

            // Evaluate answers.
            bool correct = EvaluateQuizAnswers(quizId, request.Answers, out var feedbackMap, out decimal score, out decimal totalPossible);

            var response = new QuizAttemptResponseDto
            {
                AttemptId = string.IsNullOrEmpty(responseTemplate.AttemptId) ? $"attempt-{Guid.NewGuid():N}" : responseTemplate.AttemptId,
                QuizId = quizId,
                Status = "completed",
                Score = score,
                TotalPossible = totalPossible,
                Percentage = totalPossible == 0 ? 100m : Math.Round(score / totalPossible * 100m, 1),
                Passed = totalPossible == 0 || (score / totalPossible >= 0.75m), // standard 75% pass mark
                IsReplay = false,
                AnswersFeedback = feedbackMap,
                ProgressUpdated = true
            };

            response.ProgressRevision = $"progress-rev-{_progressRev}";

            _startedQuizzes.Add(quizId);
            if (response.Passed == true)
            {
                _completedQuizzes.Add(quizId);
                _quizStates[quizId] = "completed";
                _progressRev++;
                ComputeQuizUnlocks(quizId);
            }

            if (!string.IsNullOrEmpty(uuid))
            {
                response.ClientAttemptUuid = uuid;
                _attemptsByUuid[uuid] = Clone(response);
            }

            // Also record the attempt result so it can be retrieved by id
            var resultRecord = new QuizResultDto
            {
                AttemptId = response.AttemptId,
                QuizId = quizId,
                Score = response.Score,
                TotalPossible = response.TotalPossible,
                Percentage = response.Percentage,
                Passed = response.Passed,
                CompletedAt = DateTime.UtcNow.ToString("o"),
                Answers = request.Answers
            };
            _quizResultByAttemptId[response.AttemptId] = resultRecord;

            return Task.FromResult(DataResult<QuizAttemptResponseDto>.Ok(response));
        }

        public Task<DataResult<QuizResultListDto>> GetQuizResultsAsync(CancellationToken ct = default)
        {
            if (!TryAuth<QuizResultListDto>(out var fail)) return fail;
            var list = new QuizResultListDto { Results = _quizResultByAttemptId.Values.ToList() };
            return Task.FromResult(DataResult<QuizResultListDto>.Ok(list));
        }

        public Task<DataResult<QuizResultDto>> GetQuizResultAsync(string attemptId, CancellationToken ct = default)
        {
            if (!TryAuth<QuizResultDto>(out var fail)) return fail;
            if (string.IsNullOrWhiteSpace(attemptId))
                return Fail<QuizResultDto>(new DataProviderError("VALIDATION_ERROR", "Attempt ID is required.") { Action = "retry" });

            if (_quizResultByAttemptId.TryGetValue(attemptId, out var result))
                return Task.FromResult(DataResult<QuizResultDto>.Ok(Clone(result)));

            return Fail<QuizResultDto>(new DataProviderError("NOT_FOUND", "That quiz result could not be found.") { Action = "return_to_menu" });
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

        private bool MatchAnswers(string quizId, string attemptId, Dictionary<string, object> incoming)
        {
            if (attemptId != null && _quizResultByAttemptId.TryGetValue(attemptId, out var priorResult))
            {
                if (priorResult.Answers == null || incoming == null) return priorResult.Answers == incoming;
                if (priorResult.Answers.Count != incoming.Count) return false;
                foreach (var kvp in priorResult.Answers)
                {
                    if (!incoming.TryGetValue(kvp.Key, out var value)) return false;
                    JToken expected = kvp.Value == null ? JValue.CreateNull() : JToken.FromObject(kvp.Value);
                    JToken actual = value == null ? JValue.CreateNull() : JToken.FromObject(value);
                    if (!DeepMatch(expected, actual)) return false;
                }
                return true;
            }
            return false;
        }

        private bool EvaluateQuizAnswers(string quizId, Dictionary<string, object> answers, out Dictionary<string, QuizItemFeedbackDto> feedbackMap, out decimal score, out decimal totalPossible)
        {
            feedbackMap = new Dictionary<string, QuizItemFeedbackDto>();
            score = 0m;
            totalPossible = 0m;

            if (_fixture.QuizDetailById == null || !_fixture.QuizDetailById.TryGetValue(quizId, out var detail))
                return false;

            if (detail.Items == null) return true;

            totalPossible = detail.Items.Count;
            bool allCorrect = true;

            foreach (var item in detail.Items)
            {
                if (string.IsNullOrEmpty(item.Id)) continue;

                bool correct = false;
                object answerObj = null;
                if (answers != null) answers.TryGetValue(item.Id, out answerObj);

                if (_fixture.DemoOnlyEvaluation != null && _fixture.DemoOnlyEvaluation.TryGetValue(item.Id, out var expectedVal))
                {
                    JToken expected = expectedVal is JObject obj && obj["expected_answer"] != null ? obj["expected_answer"] : expectedVal;
                    JToken actual = answerObj == null ? JValue.CreateNull() : JToken.FromObject(answerObj);
                    if (actual is JObject ao)
                    {
                        if (ao["expected_answer"] != null) actual = ao["expected_answer"];
                        else if (ao["answer"] != null) actual = ao["answer"];
                        else if (ao["selected_option"] != null) actual = ao["selected_option"];
                    }
                    correct = DeepMatch(expected, actual);
                }
                else
                {
                    // Fallback if no evaluation defined (e.g., matching options, default correct is true for placeholder)
                    correct = answerObj != null;
                }

                if (correct)
                {
                    score += 1m;
                }
                else
                {
                    allCorrect = false;
                }

                // Build feedback (use safe mistakes from the fixture if available)
                var fb = new QuizItemFeedbackDto { IsCorrect = correct };
                if (_fixture.AttemptResultByQuizId != null && _fixture.AttemptResultByQuizId.TryGetValue(quizId, out var attemptFixture))
                {
                    if (attemptFixture.SafeMistakes != null && attemptFixture.SafeMistakes.TryGetValue(item.Id, out var itemFb))
                    {
                        fb.Message = itemFb.Message;
                        fb.Explanation = itemFb.Explanation;
                        fb.HintText = itemFb.HintText;
                    }
                }

                feedbackMap[item.Id] = fb;
            }

            return allCorrect;
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

        private void ComputeQuizUnlocks(string quizId)
        {
            if (!_quizScope.TryGetValue(quizId, out var scope)) return;

            // Unlock remaining quizzes in this term.
            if (_termQuizzes.TryGetValue($"{scope.Slug}:{scope.Term}", out var termQuizzes))
            {
                foreach (var qid in termQuizzes)
                {
                    if (qid == quizId || _completedQuizzes.Contains(qid)) continue;
                    if (_quizStates.TryGetValue(qid, out var s) && s == "locked")
                    {
                        _quizStates[qid] = "unlocked";
                        _unlockRev++;
                    }
                }
            }

            // If all quizzes of this term are complete, unlock the first quiz of the next term.
            bool termComplete = _termQuizzes.TryGetValue($"{scope.Slug}:{scope.Term}", out var ts)
                                && ts.All(_completedQuizzes.Contains);
            if (termComplete && _termQuizzes.TryGetValue($"{scope.Slug}:{scope.Term + 1}", out var next) && next.Count > 0)
            {
                string first = next[0];
                if (_quizStates.TryGetValue(first, out var s2) && s2 == "locked")
                {
                    _quizStates[first] = "unlocked";
                    _unlockRev++;
                }
            }
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
            baseSync.QuizRevision = $"unlock-rev-{_unlockRev}";
            baseSync.PublishedContentRevision = ContentRevision;
            baseSync.RewardWalletRevision = $"wallet-rev-{_walletRev}";
            return baseSync;
        }

        private ProgressSummaryDto BuildProgressSummary()
        {
            var summary = Clone(_fixture.Responses?.ProgressSummary) ?? new ProgressSummaryDto();
            int totalAvailable = _subjectQuizzes.Values.Sum(v => v.Count);
            int totalCompleted = _completedQuizzes.Count;

            summary.TotalQuizzesAvailable = totalAvailable;
            summary.TotalQuizzesCompleted = totalCompleted;
            summary.StartedQuizzes = _startedQuizzes.Count;
            summary.OverallPercentage = totalAvailable == 0 ? 0m : Math.Round((decimal)totalCompleted / totalAvailable * 100m, 1);
            summary.Stars = _wallet.TotalStars ?? 0;
            summary.Coins = _wallet.TotalCoins ?? 0;
            summary.Revision = $"progress-rev-{_progressRev}";

            if (summary.Subjects != null)
            {
                foreach (var sp in summary.Subjects)
                {
                    string slug = sp.SubjectSlug ?? "";
                    var quizzes = _subjectQuizzes.TryGetValue(slug, out var list) ? list : new List<string>();
                    int avail = quizzes.Count;
                    int done = quizzes.Count(_completedQuizzes.Contains);
                    sp.QuizzesAvailable = avail;
                    sp.QuizzesCompleted = done;
                    sp.Percentage = avail == 0 ? 0m : Math.Round((decimal)done / avail * 100m, 1);
                    sp.ProgressPercent = sp.Percentage;

                    if (sp.Terms != null)
                    {
                        foreach (var tp in sp.Terms)
                        {
                            int term = tp.TermNumber ?? 0;
                            var ts = _termQuizzes.TryGetValue($"{slug}:{term}", out var tlist) ? tlist : new List<string>();
                            int tAvail = ts.Count;
                            int tDone = ts.Count(_completedQuizzes.Contains);
                            tp.QuizzesAvailable = tAvail;
                            tp.QuizzesCompleted = tDone;
                            tp.Percentage = tAvail == 0 ? 0m : Math.Round((decimal)tDone / tAvail * 100m, 1);
                        }
                    }
                }
            }
            return summary;
        }

        /// <summary>
        /// Deep-clones a DTO via a JSON round-trip so the immutable fixture source
        /// is never exposed by reference and callers cannot mutate it.
        /// </summary>
        private static T Clone<T>(T src)
            => src == null ? default : JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(src, JsonSettings.SafeDefaults), JsonSettings.SafeDefaults);
    }
}