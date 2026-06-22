using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;
using UnityEditor;
using UnityEngine;

namespace NutriMind.Editor
{
    /// <summary>
    /// Editor-only generator that BUILDS the complete fabricated student demo
    /// fixture by constructing the real runtime DTO objects and serializing them
    /// with the same <see cref="JsonSettings.SafeDefaults"/> the HTTP provider
    /// uses. Constructing the actual DTOs guarantees field-name parity with the
    /// HTTP provider — the output round-trips into <see cref="DemoFixtureDto"/>.
    ///
    /// The fixture contains NO real student data, credentials, tokens, secrets,
    /// or production answer keys. All values are obviously fabricated demo data.
    /// </summary>
    public static class DemoFixtureGenerator
    {
        private const string OutputPath =
            "Assets/_Project/Nutrimind/Resources/DemoData/full-demo-student-data.json";

        private const string ContentRevision = "content-rev-1";
        private const string ServerTime = "2025-01-15T08:00:00Z";
        private const string StudentId = "demo-student-5-001";
        private const int Grade = 5;

        // ── Subject identity tables ──
        private static readonly string[] Slugs = { "literaquest", "healthquest", "sciencequest" };

        private static readonly Dictionary<string, string> SubjectNames = new()
        {
            { "literaquest", "LiteraQuest" },
            { "healthquest", "PE & Health Quest" },
            { "sciencequest", "ScienceQuest" }
        };

        private static readonly Dictionary<string, string> SubjectDescriptions = new()
        {
            { "literaquest", "Word and reading adventures." },
            { "healthquest", "Move, play, and grow healthy." },
            { "sciencequest", "Explore amazing science worlds." }
        };

        private static readonly Dictionary<string, string> MechanicFamily = new()
        {
            { "literaquest", "word_puzzle" },
            { "healthquest", "movement_rhythm" },
            { "sciencequest", "exploration" }
        };

        private static readonly Dictionary<string, string[]> WorldTitles = new()
        {
            { "literaquest", new[] { "Whispering Library", "Story Harbor", "Riddle Peaks" } },
            { "healthquest", new[] { "Sunrise Field", "Wellness Village", "Summit Trail" } },
            { "sciencequest", new[] { "Tidal Lagoon", "Ember Caverns", "Aurora Observatory" } }
        };

        private static readonly Dictionary<string, string[][]> EnvTags = new()
        {
            { "literaquest", new[] { new[] { "library", "whisper" }, new[] { "harbor", "story" }, new[] { "mountain", "riddle" } } },
            { "healthquest", new[] { new[] { "field", "sunrise" }, new[] { "village", "wellness" }, new[] { "summit", "trail" } } },
            { "sciencequest", new[] { new[] { "lagoon", "tidal" }, new[] { "cavern", "ember" }, new[] { "observatory", "aurora" } } }
        };

        // quiz: (slug, term, n, quiz_key, itemType1, itemType2)
        private static readonly (string slug, int term, int n, string key, string ct1, string ct2)[] Quizzes =
        {
            ("literaquest", 1, 1, "vocabulary_clue_trail", "multiple_choice", "fill_blank"),
            ("literaquest", 1, 2, "synonym_bridge", "matching", "multiple_choice"),
            ("literaquest", 2, 1, "context_clue_cove", "multiple_choice", "true_false"),
            ("literaquest", 2, 2, "main_idea_lighthouse", "scenario_choice", "multiple_choice"),
            ("literaquest", 3, 1, "inference_summit", "multiple_choice", "ordering"),
            ("literaquest", 3, 2, "figurative_peak", "matching", "fill_blank"),
            ("healthquest", 1, 1, "warmup_rhythm", "ordering", "multiple_choice"),
            ("healthquest", 1, 2, "hydration_station", "true_false", "multiple_choice"),
            ("healthquest", 2, 1, "food_group_market", "matching", "multiple_choice"),
            ("healthquest", 2, 2, "balance_beam", "multiple_choice", "true_false"),
            ("healthquest", 3, 1, "endurance_climb", "ordering", "scenario_choice"),
            ("healthquest", 3, 2, "teamwork_relay", "scenario_choice", "fill_blank")
        };

        [MenuItem("NutriMind/Generate Demo Fixture")]
        public static void GenerateMenu() => Generate();

        /// <summary>
        /// Generates the fixture then re-deserializes it into <see cref="DemoFixtureDto"/>
        /// using <see cref="JsonSettings.SafeDefaults"/> to confirm it round-trips
        /// (field-name parity with the HTTP provider). Returns a human-readable
        /// report. Throws if deserialization fails.
        /// </summary>
        public static string GenerateAndVerify()
        {
            string path = Generate();

            bool exists = File.Exists(path);
            long size = exists ? new FileInfo(path).Length : -1;

            string text = File.ReadAllText(path);
            var fixture = JsonConvert.DeserializeObject<DemoFixtureDto>(text, JsonSettings.SafeDefaults);

            var sb = new StringBuilder();
            sb.AppendLine($"path={path}");
            sb.AppendLine($"exists={exists} size={size} bytes");
            sb.AppendLine($"round_trip_ok=true fixture_id={fixture.FixtureId} mode={fixture.Mode} format={fixture.FixtureFormatVersion}");
            sb.AppendLine($"terms_by_subject={fixture.TermsBySubject?.Count}");
            sb.AppendLine($"quizzes_by_scope={fixture.QuizzesByScope?.Count}");
            sb.AppendLine($"quiz_detail_by_id={fixture.QuizDetailById?.Count}");
            sb.AppendLine($"attempt_result_by_quiz_id={fixture.AttemptResultByQuizId?.Count}");
            sb.AppendLine($"demo_only_evaluation={fixture.DemoOnlyEvaluation?.Count}");
            sb.AppendLine($"error_fixtures={fixture.ErrorFixtures?.Count}");
            sb.AppendLine($"responses.subjects={fixture.Responses?.Subjects?.Count}");

            var sci = fixture.QuizzesByScope["sciencequest:5:1"];
            sb.AppendLine($"sciencequest:5:1 quizzes={sci.Quizzes.Count} preview_mode={sci.PreviewMode}");
            var lit = fixture.QuizzesByScope["literaquest:5:1"];
            sb.AppendLine($"literaquest:5:1 quizzes={lit.Quizzes.Count} first_state={lit.Quizzes[0].State} first_item_count={lit.Quizzes[0].TotalItems}");

            return sb.ToString();
        }

        public static string Generate()
        {
            var fixture = BuildFixture();

            string json = JsonConvert.SerializeObject(fixture, Formatting.Indented, JsonSettings.SafeDefaults);

            string dir = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(OutputPath, json, new UTF8Encoding(false));

            AssetDatabase.ImportAsset(OutputPath);
            AssetDatabase.Refresh();

            int byteCount = new UTF8Encoding(false).GetByteCount(json);
            Debug.Log($"[DemoFixtureGenerator] Wrote demo fixture to '{OutputPath}' ({byteCount} bytes).");

            return OutputPath;
        }

        // ──────────────────────────────────────────────────────────────
        //  Root builder
        // ──────────────────────────────────────────────────────────────

        private static DemoFixtureDto BuildFixture()
        {
            var fixture = new DemoFixtureDto
            {
                FixtureFormatVersion = "2.0",
                FixtureId = "nutrimind-demo-grade5-001",
                Mode = "local_demo_only",
                Notice = "Fabricated development fixture. Contains NO real student data, credentials, tokens, secrets, or production answer keys. Local-demo only.",
                DemoAuth = new DemoAuthDto
                {
                    Lrn = "000000000001",
                    Pin = "1234",
                    AllowDemoLoginButton = true,
                    DevelopmentBuildOnly = true
                },
                TermsBySubject = new Dictionary<string, List<TermDto>>(),
                QuizzesByScope = new Dictionary<string, QuizListDto>(),
                QuizDetailById = new Dictionary<string, QuizDetailDto>(),
                AttemptResultByQuizId = new Dictionary<string, DemoQuizAttemptFixtureDto>(),
                QuizResultByAttemptId = new Dictionary<string, QuizResultDto>(),
                DemoOnlyEvaluation = new Dictionary<string, JToken>(),
                ErrorFixtures = BuildErrorFixtures()
            };

            var subjects = BuildSubjects();
            fixture.Responses = BuildResponses(subjects);

            // terms_by_subject
            foreach (var slug in Slugs)
                fixture.TermsBySubject[slug] = BuildTerms(slug);

            // quizzes_by_scope (science = empty preview scopes)
            BuildQuizScopes(fixture);

            // playable quiz content / detail / attempts
            foreach (var q in Quizzes)
            {
                string quizId = $"{q.slug}-t{q.term}-q{q.n}";

                fixture.QuizDetailById[quizId] = BuildQuizDetail(q, quizId, fixture);
            }

            return fixture;
        }

        // ──────────────────────────────────────────────────────────────
        //  Subjects / terms / quizzes
        // ──────────────────────────────────────────────────────────────

        private static List<SubjectDto> BuildSubjects()
        {
            var list = new List<SubjectDto>();
            foreach (var slug in Slugs)
            {
                var dto = new SubjectDto
                {
                    Id = $"subj-{slug}",
                    Slug = slug,
                    Name = SubjectNames[slug],
                    Description = SubjectDescriptions[slug],
                    IconKey = $"icon_subject_{slug}",
                    GradeLevels = new List<int> { Grade },
                    IsAvailable = true,
                    ProgressPercent = 0m
                };
                if (slug == "sciencequest")
                    dto.PreviewMode = "exploration_only";
                list.Add(dto);
            }
            return list;
        }

        private static List<TermDto> BuildTerms(string slug)
        {
            var terms = new List<TermDto>();
            for (int term = 1; term <= 3; term++)
            {
                terms.Add(new TermDto
                {
                    Id = $"{slug}-t{term}",
                    TermNumber = term,
                    Title = WorldTitles[slug][term - 1],
                    Description = $"Term {term}: {WorldTitles[slug][term - 1]}.",
                    WorldMetadata = BuildWorld(slug, term),
                    IsAvailable = true,
                    ProgressPercent = 0m
                });
            }
            return terms;
        }

        private static WorldMetadataDto BuildWorld(string slug, int term)
        {
            string title = WorldTitles[slug][term - 1];
            string upper = char.ToUpperInvariant(slug[0]) + slug.Substring(1);
            return new WorldMetadataDto
            {
                WorldThemeKey = $"world_{slug}_t{term}",
                WorldTitle = title,
                UnitySceneKey = $"scene_world_{slug}_t{term}",
                UnitySceneName = $"World_{upper}_T{term}",
                SceneAddressKey = $"addr_world_{slug}_t{term}",
                EnvironmentTags = EnvTags[slug][term - 1].ToList(),
                MechanicFamily = MechanicFamily[slug]
            };
        }

        private static void BuildQuizScopes(DemoFixtureDto fixture)
        {
            // playable subjects: build scopes with 2 quizzes each
            foreach (var q in Quizzes)
            {
                string scope = $"{q.slug}:{Grade}:{q.term}";
                if (!fixture.QuizzesByScope.TryGetValue(scope, out var listDto))
                {
                    listDto = new QuizListDto
                    {
                        SubjectSlug = q.slug,
                        GradeLevel = Grade,
                        TermNumber = q.term,
                        Quizzes = new List<QuizDto>(),
                        PreviewMode = null,
                        Message = null
                    };
                    fixture.QuizzesByScope[scope] = listDto;
                }
                listDto.Quizzes.Add(BuildQuiz(q));
            }

            // sciencequest: exploration-only empty scopes for all 3 terms
            for (int term = 1; term <= 3; term++)
            {
                string scope = $"sciencequest:{Grade}:{term}";
                fixture.QuizzesByScope[scope] = new QuizListDto
                {
                    SubjectSlug = "sciencequest",
                    GradeLevel = Grade,
                    TermNumber = term,
                    Quizzes = new List<QuizDto>(),
                    PreviewMode = "exploration_only",
                    Message = "Science worlds are open for exploration. Playable quizzes are coming soon."
                };
            }
        }

        private static QuizDto BuildQuiz((string slug, int term, int n, string key, string ct1, string ct2) q)
        {
            return new QuizDto
            {
                Id = $"{q.slug}-t{q.term}-q{q.n}",
                Title = TitleCase(q.key),
                Description = $"A {TitleCase(q.key)} quiz in {WorldTitles[q.slug][q.term - 1]}.",
                SubjectSlug = q.slug,
                GradeLevel = Grade,
                TermNumber = q.term,
                State = q.term == 1 ? "unlocked" : "locked",
                TotalItems = 2,
                DurationMinutes = 15,
                IsAvailable = true,
                ProgressPercent = 0m
            };
        }

        // ──────────────────────────────────────────────────────────────
        //  Quiz Detail & Items
        // ──────────────────────────────────────────────────────────────

        private static QuizDetailDto BuildQuizDetail(
            (string slug, int term, int n, string key, string ct1, string ct2) q,
            string quizId,
            DemoFixtureDto fixture)
        {
            string title = TitleCase(q.key);

            var items = new List<QuizItemDto>();
            string[] types = { q.ct1, q.ct2 };

            var safeMistakes = new Dictionary<string, QuizItemFeedbackDto>();

            for (int i = 1; i <= 2; i++)
            {
                string type = types[i - 1];
                string itemId = $"{quizId}-item{i}";

                var (quizItem, expected) = BuildQuizItem(quizId, itemId, type, i, title);
                items.Add(quizItem);

                // Evaluation expectation
                fixture.DemoOnlyEvaluation[itemId] = new JObject { ["expected_answer"] = expected };

                // Build a safe mistake feedback
                safeMistakes[itemId] = new QuizItemFeedbackDto
                {
                    IsCorrect = false,
                    Message = "Not quite — let's try once more.",
                    Explanation = "Think about the clue and choose carefully.",
                    HintText = "Gentle nudge: re-read the options."
                };
            }

            // Create attempts fixture
            fixture.AttemptResultByQuizId[quizId] = new DemoQuizAttemptFixtureDto
            {
                ResponseTemplate = new QuizAttemptResponseDto
                {
                    AttemptId = $"attempt-{quizId}",
                    ClientAttemptUuid = null,
                    QuizId = quizId,
                    Status = "completed",
                    Score = 2m,
                    TotalPossible = 2m,
                    Percentage = 100m,
                    Passed = true,
                    IsReplay = null,
                    ProgressUpdated = true
                },
                SafeMistakes = safeMistakes
            };

            return new QuizDetailDto
            {
                Id = quizId,
                Title = title,
                Description = $"Practice your skills in {title}.",
                SubjectSlug = q.slug,
                TermNumber = q.term,
                GradeLevel = Grade,
                State = q.term == 1 ? "unlocked" : "locked",
                TotalItems = 2,
                DurationMinutes = 15,
                IsAvailable = true,
                ProgressPercent = 0m,
                Instructions = "Solve each item carefully. You must score 75% or higher to pass!",
                Items = items
            };
        }

        // Returns the quiz item DTO plus the expected answer JToken
        private static (QuizItemDto quizItem, JToken expected) BuildQuizItem(
            string quizId, string itemId, string type, int orderIndex, string quizTitle)
        {
            var item = new QuizItemDto
            {
                Id = itemId,
                QuizId = quizId,
                Type = type,
                OrderIndex = orderIndex
            };

            JToken expected;

            switch (type)
            {
                case "multiple_choice":
                    item.Prompt = $"Which choice best fits the {quizTitle} clue?";
                    item.Options = new List<QuizItemOptionDto>
                    {
                        new QuizItemOptionDto { Key = "a", Label = "First idea", Description = "One possible answer." },
                        new QuizItemOptionDto { Key = "b", Label = "Second idea", Description = "Another possible answer." },
                        new QuizItemOptionDto { Key = "c", Label = "Third idea", Description = "A different answer." },
                        new QuizItemOptionDto { Key = "d", Label = "Fourth idea", Description = "Yet another answer." }
                    };
                    expected = new JValue("b");
                    break;

                case "scenario_choice":
                    item.Prompt = "What is the best thing to do in this situation?";
                    item.Options = new List<QuizItemOptionDto>
                    {
                        new QuizItemOptionDto { Key = "a", Label = "Help right away", Description = "Take helpful action now." },
                        new QuizItemOptionDto { Key = "b", Label = "Wait and see", Description = "Do nothing for now." },
                        new QuizItemOptionDto { Key = "c", Label = "Ask a friend", Description = "Get someone else to decide." }
                    };
                    expected = new JValue("a");
                    break;

                case "true_false":
                    item.Prompt = $"True or false: the {quizTitle} statement is correct.";
                    item.Options = new List<QuizItemOptionDto>
                    {
                        new QuizItemOptionDto { Key = "true", Label = "True", Description = "The statement is correct." },
                        new QuizItemOptionDto { Key = "false", Label = "False", Description = "The statement is not correct." }
                    };
                    expected = new JValue("true");
                    break;

                case "ordering":
                    item.Prompt = "Put these steps in the correct order.";
                    item.Options = new List<QuizItemOptionDto>
                    {
                        new QuizItemOptionDto { Key = "i1", Label = "Step one", Description = "The first step." },
                        new QuizItemOptionDto { Key = "i2", Label = "Step two", Description = "The second step." },
                        new QuizItemOptionDto { Key = "i3", Label = "Step three", Description = "The third step." }
                    };
                    expected = new JArray("i1", "i2", "i3");
                    break;

                case "matching":
                    item.Prompt = "Match each item on the left to its pair.";
                    item.Options = new List<QuizItemOptionDto>
                    {
                        new QuizItemOptionDto { Key = "a", Label = "Left item A", Description = "First item to match." },
                        new QuizItemOptionDto { Key = "b", Label = "Left item B", Description = "Second item to match." },
                        new QuizItemOptionDto { Key = "c", Label = "Left item C", Description = "Third item to match." }
                    };
                    expected = new JObject { ["a"] = "x", ["b"] = "y", ["c"] = "z" };
                    break;

                case "fill_blank":
                default:
                    item.Prompt = "Fill in the blank with the best word.";
                    item.Options = null;
                    expected = new JValue("noun");
                    break;
            }

            return (item, expected);
        }

        // ──────────────────────────────────────────────────────────────
        //  HTTP response-wrappers template
        // ──────────────────────────────────────────────────────────────

        private static DemoResponsesDto BuildResponses(List<SubjectDto> subjects)
        {
            return new DemoResponsesDto
            {
                Ping = new PingResponseDto { Status = "ok", ServerTime = ServerTime },
                Config = new ApiConfigDto
                {
                    ApiVersion = "1.0.0",
                    ContractVersion = "quiz_first_laravel_1",
                    MinimumUnityClientVersion = "1.0.0",
                    MaintenanceMode = false
                },
                Login = new LoginResponseDto
                {
                    Token = "fake-jwt-token-0001",
                    Student = BuildIdentity()
                },
                Bootstrap = new BootstrapDto
                {
                    Student = BuildIdentity(),
                    Subjects = subjects,
                    Classroom = new ClassroomDto
                    {
                        Id = "class-5a",
                        Name = "Grade 5 Section A",
                        GradeLevel = Grade,
                        Section = "A"
                    }
                },
                Profile = new StudentProfileDto
                {
                    Id = StudentId,
                    Name = "Maya",
                    GradeLevel = Grade,
                    AvatarKey = "avatar_explorer_teal"
                },
                Settings = BuildInitialSettings(),
                Subjects = subjects,
                ProgressSummary = BuildInitialProgress(),
                Rewards = BuildInitialWallet(),
                SyncStatus = BuildInitialSync()
            };
        }

        private static StudentIdentityDto BuildIdentity()
        {
            return new StudentIdentityDto
            {
                Id = StudentId,
                Name = "Maya Santos",
                LrnMasked = "************4521",
                GradeLevel = Grade,
                LanguagePreference = "en"
            };
        }

        private static ProgressSummaryDto BuildInitialProgress()
        {
            var subjectProgress = new List<SubjectProgressDto>();
            foreach (var slug in Slugs)
            {
                bool science = slug == "sciencequest";
                var terms = new List<TermProgressDto>();
                for (int term = 1; term <= 3; term++)
                {
                    terms.Add(new TermProgressDto
                    {
                        TermNumber = term,
                        QuizzesCompleted = 0,
                        QuizzesAvailable = science ? 0 : 2,
                        Percentage = 0m
                    });
                }
                subjectProgress.Add(new SubjectProgressDto
                {
                    SubjectSlug = slug,
                    SubjectName = SubjectNames[slug],
                    QuizzesCompleted = 0,
                    QuizzesAvailable = science ? 0 : 6,
                    Percentage = 0m,
                    ProgressPercent = 0m,
                    PreviewMode = science ? "exploration_only" : null,
                    Terms = terms
                });
            }

            return new ProgressSummaryDto
            {
                StudentId = StudentId,
                Subjects = subjectProgress,
                TotalQuizzesCompleted = 0,
                TotalQuizzesAvailable = 12,
                StartedQuizzes = 0,
                OverallPercentage = 0m,
                Stars = 0,
                Coins = 0,
                Revision = "progress-rev-1"
            };
        }

        private static RewardWalletDto BuildInitialWallet()
        {
            return new RewardWalletDto
            {
                StudentId = StudentId,
                Rewards = new List<RewardBalanceDto>
                {
                    new RewardBalanceDto
                    {
                        RewardCode = "coin",
                        RewardType = "coin",
                        DisplayName = "Coin",
                        IconKey = "icon_coin",
                        Quantity = 0,
                        IsUsable = false,
                        Description = "Earned by solving challenges."
                    },
                    new RewardBalanceDto
                    {
                        RewardCode = "hint_token",
                        RewardType = "consumable",
                        DisplayName = "Hint Token",
                        IconKey = "icon_hint",
                        Quantity = 3,
                        IsUsable = true,
                        Description = "Reveal a helpful hint."
                    },
                    new RewardBalanceDto
                    {
                        RewardCode = "skip_token",
                        RewardType = "consumable",
                        DisplayName = "Skip Token",
                        IconKey = "icon_skip",
                        Quantity = 1,
                        IsUsable = true,
                        Description = "Skip one challenge."
                    }
                },
                TotalCoins = 0,
                TotalStars = 0,
                Revision = "wallet-rev-1"
            };
        }

        private static SettingsDto BuildInitialSettings()
        {
            return new SettingsDto
            {
                Language = "en",
                MasterVolume = 0.8f,
                MusicVolume = 0.7f,
                SfxVolume = 0.9f,
                MuteAll = false,
                SubtitlesEnabled = true,
                ReducedMotion = false,
                ShowHints = true,
                TextSize = "medium",
                NotificationsEnabled = true,
                AccessibilityPreferences = new Dictionary<string, object> { { "high_contrast", false } },
                Revision = "settings-rev-1"
            };
        }

        private static SyncStatusDto BuildInitialSync()
        {
            return new SyncStatusDto
            {
                StudentProgressRevision = "progress-rev-1",
                StudentSettingsRevision = "settings-rev-1",
                QuizRevision = "unlock-rev-1",
                PublishedContentRevision = ContentRevision,
                RewardWalletRevision = "wallet-rev-1",
                ServerTime = ServerTime,
                NextPollAfterSeconds = 45
            };
        }

        private static Dictionary<string, DataProviderError> BuildErrorFixtures()
        {
            return new Dictionary<string, DataProviderError>
            {
                ["quiz_locked"] = new DataProviderError
                {
                    Code = "QUIZ_LOCKED",
                    Message = "This quiz is still locked. Finish the earlier quiz first.",
                    Action = "return_to_menu",
                    Retryable = false
                },
                ["invalid_session"] = new DataProviderError
                {
                    Code = "UNAUTHENTICATED",
                    Message = "Your session ended. Please log in again.",
                    Action = "login_again",
                    Retryable = false
                }
            };
        }

        // ──────────────────────────────────────────────────────────────
        //  Helpers
        // ──────────────────────────────────────────────────────────────

        private static string TitleCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var parts = s.Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0) continue;
                parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
            }
            return string.Join(" ", parts);
        }
    }
}