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
        private const string StartedAt = "2025-01-15T08:05:00Z";
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

        private static readonly Dictionary<string, string> LearningSkill = new()
        {
            { "literaquest", "reading comprehension" },
            { "healthquest", "physical literacy" }
        };

        private static readonly Dictionary<string, string> GuideNames = new()
        {
            { "literaquest", "Lumi the Owl" },
            { "healthquest", "Coach Rio" }
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

        // station: (slug, term, n, station_key, challengeType1, challengeType2)
        private static readonly (string slug, int term, int n, string key, string ct1, string ct2)[] Stations =
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
            sb.AppendLine($"stations_by_scope={fixture.StationsByScope?.Count}");
            sb.AppendLine($"station_content_by_id={fixture.StationContentById?.Count}");
            sb.AppendLine($"station_start_by_id={fixture.StationStartById?.Count}");
            sb.AppendLine($"attempt_result_by_challenge_id={fixture.AttemptResultByChallengeId?.Count}");
            sb.AppendLine($"completion_result_by_station_id={fixture.CompletionResultByStationId?.Count}");
            sb.AppendLine($"demo_only_evaluation={fixture.DemoOnlyEvaluation?.Count}");
            sb.AppendLine($"error_fixtures={fixture.ErrorFixtures?.Count}");
            sb.AppendLine($"responses.subjects={fixture.Responses?.Subjects?.Count}");

            var sci = fixture.StationsByScope["sciencequest:5:1"];
            sb.AppendLine($"sciencequest:5:1 stations={sci.Stations.Count} preview_mode={sci.PreviewMode}");
            var lit = fixture.StationsByScope["literaquest:5:1"];
            sb.AppendLine($"literaquest:5:1 stations={lit.Stations.Count} first_state={lit.Stations[0].State} first_ct={lit.Stations[0].ChallengeType}");

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
                StationsByScope = new Dictionary<string, StationListDto>(),
                StationContentById = new Dictionary<string, StationContentDto>(),
                StationStartById = new Dictionary<string, StationStartResponseDto>(),
                AttemptResultByChallengeId = new Dictionary<string, DemoAttemptFixtureDto>(),
                CompletionResultByStationId = new Dictionary<string, StationCompleteResponseDto>(),
                DemoOnlyEvaluation = new Dictionary<string, JToken>(),
                ErrorFixtures = BuildErrorFixtures()
            };

            var subjects = BuildSubjects();
            fixture.Responses = BuildResponses(subjects);

            // terms_by_subject
            foreach (var slug in Slugs)
                fixture.TermsBySubject[slug] = BuildTerms(slug);

            // stations_by_scope (science = empty preview scopes)
            BuildStationScopes(fixture);

            // playable station content / start / attempts / completion / evaluation
            foreach (var s in Stations)
            {
                string stationId = $"{s.slug}-t{s.term}-s{s.n}";

                fixture.StationContentById[stationId] = BuildStationContent(s, stationId, fixture);
                fixture.StationStartById[stationId] = BuildStationStart(stationId);
                fixture.CompletionResultByStationId[stationId] = BuildCompletion(stationId, s.key);
            }

            return fixture;
        }

        // ──────────────────────────────────────────────────────────────
        //  Subjects / terms / stations
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

        private static void BuildStationScopes(DemoFixtureDto fixture)
        {
            // playable subjects: build scopes with 2 stations each
            foreach (var s in Stations)
            {
                string scope = $"{s.slug}:{Grade}:{s.term}";
                if (!fixture.StationsByScope.TryGetValue(scope, out var listDto))
                {
                    listDto = new StationListDto
                    {
                        SubjectSlug = s.slug,
                        GradeLevel = Grade,
                        TermNumber = s.term,
                        Stations = new List<StationDto>(),
                        PreviewMode = null,
                        Message = null
                    };
                    fixture.StationsByScope[scope] = listDto;
                }
                listDto.Stations.Add(BuildStation(s));
            }

            // sciencequest: exploration-only empty scopes for all 3 terms
            for (int term = 1; term <= 3; term++)
            {
                string scope = $"sciencequest:{Grade}:{term}";
                fixture.StationsByScope[scope] = new StationListDto
                {
                    SubjectSlug = "sciencequest",
                    GradeLevel = Grade,
                    TermNumber = term,
                    Stations = new List<StationDto>(),
                    PreviewMode = "exploration_only",
                    Message = "Science worlds are open for exploration. Playable missions are coming soon."
                };
            }
        }

        private static StationDto BuildStation((string slug, int term, int n, string key, string ct1, string ct2) s)
        {
            var world = BuildWorld(s.slug, s.term);
            return new StationDto
            {
                Id = $"{s.slug}-t{s.term}-s{s.n}",
                StationKey = s.key,
                StationNumber = (s.term - 1) * 2 + s.n,
                Title = TitleCase(s.key),
                Description = $"A {TitleCase(s.key)} mission in {WorldTitles[s.slug][s.term - 1]}.",
                SubjectSlug = s.slug,
                GradeLevel = Grade,
                TermNumber = s.term,
                State = s.term == 1 ? "unlocked" : "locked",
                Required = true,
                ProgressPercent = 0m,
                ChallengeType = s.ct1,
                PortalKey = $"portal_{s.key}",
                UnitySceneKey = world.UnitySceneKey,
                ContentRevision = ContentRevision,
                WorldMetadata = world
            };
        }

        // ──────────────────────────────────────────────────────────────
        //  Station content (narrative + challenges + tasks)
        // ──────────────────────────────────────────────────────────────

        private static StationContentDto BuildStationContent(
            (string slug, int term, int n, string key, string ct1, string ct2) s,
            string stationId,
            DemoFixtureDto fixture)
        {
            var world = BuildWorld(s.slug, s.term);
            string title = TitleCase(s.key);
            string subjectName = SubjectNames[s.slug];

            var challenges = new List<ChallengeDto>();
            var worldTasks = new List<WorldTaskDto>();
            string[] types = { s.ct1, s.ct2 };

            for (int i = 1; i <= 2; i++)
            {
                string type = types[i - 1];
                string challengeId = $"{stationId}-c{i}";

                var (challenge, expected) = BuildChallenge(challengeId, type, i, title);
                challenges.Add(challenge);

                worldTasks.Add(new WorldTaskDto
                {
                    TaskId = $"{challengeId}-task",
                    TaskKey = $"task_{s.key}_{i}",
                    TaskType = "challenge",
                    PortalKey = $"portal_{s.key}",
                    InteractableKey = $"interactable_{s.key}_{i}",
                    PrefabKey = $"prefab_{s.slug}_task",
                    WorldPositionHint = $"node_{i}",
                    ChallengeId = challengeId,
                    Required = true
                });

                // attempt result + fabricated expected answer
                fixture.AttemptResultByChallengeId[challengeId] = BuildAttemptFixture(challengeId, s.slug);
                fixture.DemoOnlyEvaluation[challengeId] = new JObject { ["expected_answer"] = expected };
            }

            return new StationContentDto
            {
                StationId = stationId,
                StationKey = s.key,
                SubjectSlug = s.slug,
                GradeLevel = Grade,
                TermNumber = s.term,
                Title = $"{title}: A {subjectName} Mission",
                Description = $"Help restore {WorldTitles[s.slug][s.term - 1]} by completing the {title} challenges.",
                LearningSkill = LearningSkill[s.slug],
                StudentLearningGoal = $"I can practice {LearningSkill[s.slug]} while exploring {WorldTitles[s.slug][s.term - 1]}.",
                Instructions = "Talk to your guide, then solve each challenge. Mistakes are okay — keep trying!",
                ContentRevision = ContentRevision,
                WorldMetadata = world,
                StoryContext = $"Something has gone quiet in {WorldTitles[s.slug][s.term - 1]}. Your guide needs your help to bring it back to life. Every challenge you solve restores a little more of the world.",
                MissionTitle = $"The {title} Mission",
                MissionSummary = $"Solve the {title} challenges to restore part of {WorldTitles[s.slug][s.term - 1]}.",
                NpcGuides = new List<NpcGuideDto>
                {
                    new NpcGuideDto
                    {
                        NpcKey = $"guide_{s.slug}",
                        DisplayName = GuideNames[s.slug],
                        Role = "mentor",
                        AvatarKey = $"npc_{s.slug}",
                        IntroDialogue = "Welcome, explorer! Ready to bring this world back to life?",
                        CompletionDialogue = "Wonderful work! The world is shining again thanks to you."
                    }
                },
                LearningCycle = new LearningCycleDto
                {
                    Discover = "Look around and notice the clues in the world.",
                    Practice = "Try the first challenge and learn how it works.",
                    Apply = "Use what you learned to solve the next challenge.",
                    Review = "Think about what you discovered and how you solved it."
                },
                HintPolicy = new HintPolicyDto
                {
                    MaxHintTier = 3,
                    PreserveWorldProgress = true,
                    PenalizeOrdinaryMistake = false,
                    Tiers = new List<HintTierDto>
                    {
                        new HintTierDto { Tier = 1, Text = "Gentle nudge: read the question one more time." },
                        new HintTierDto { Tier = 2, Text = "Closer look: focus on the most important clue." },
                        new HintTierDto { Tier = 3, Text = "Almost there: you can narrow it down to two choices." }
                    }
                },
                Discoveries = new List<DiscoveryDto>
                {
                    new DiscoveryDto
                    {
                        DiscoveryKey = $"discovery_{s.key}",
                        Type = "lore",
                        Title = $"Secret of {title}",
                        Description = "An optional bit of world lore to discover. Totally up to you!",
                        Optional = true,
                        RewardPreview = new RewardPreviewDto
                        {
                            Code = "coin",
                            RewardType = "coin",
                            DisplayName = "Coin",
                            IconKey = "icon_coin",
                            Quantity = 2,
                            GrantScope = "discovery"
                        }
                    }
                },
                ReflectionPrompt = "What is one new thing you learned in this mission?",
                RewardPreview = new List<RewardPreviewDto>
                {
                    new RewardPreviewDto
                    {
                        Code = "coin",
                        RewardType = "coin",
                        DisplayName = "Coin",
                        IconKey = "icon_coin",
                        Quantity = 5,
                        GrantScope = "per_challenge"
                    },
                    new RewardPreviewDto
                    {
                        Code = $"crystal_{s.slug}",
                        RewardType = "crystal",
                        DisplayName = $"{subjectName} Crystal",
                        IconKey = $"icon_crystal_{s.slug}",
                        Quantity = 1,
                        GrantScope = "term_after_both_stations"
                    }
                },
                WorldRestorationState = new WorldRestorationStateDto
                {
                    StateKey = $"restored_{s.key}",
                    ApplyAfterAcceptedCompletion = true,
                    StateData = new JObject
                    {
                        ["vfx_key"] = $"vfx_restore_{s.slug}",
                        ["ambient_key"] = $"ambient_{s.slug}"
                    }
                },
                SuccessFeedback = new SuccessFeedbackDto
                {
                    Message = "You did it! The world feels brighter already.",
                    EncouragingPhrases = new List<string>
                    {
                        "Amazing thinking!",
                        "You kept going — that's what explorers do!",
                        "Your curiosity is your superpower!"
                    }
                },
                CompletionRule = new CompletionRuleDto
                {
                    Type = "complete_required_challenges",
                    RequiredCount = 2
                },
                Challenges = challenges,
                WorldTasks = worldTasks
            };
        }

        // Returns the challenge DTO plus the fabricated expected-answer token.
        private static (ChallengeDto challenge, JToken expected) BuildChallenge(
            string challengeId, string type, int orderIndex, string stationTitle)
        {
            var challenge = new ChallengeDto
            {
                ChallengeId = challengeId,
                ChallengeType = type,
                OrderIndex = orderIndex
            };

            JToken expected;

            switch (type)
            {
                case "multiple_choice":
                    challenge.Prompt = $"Which choice best fits the {stationTitle} clue?";
                    challenge.Options = new List<ChallengeOptionDto>
                    {
                        new ChallengeOptionDto { Key = "a", Label = "First idea", Description = "One possible answer." },
                        new ChallengeOptionDto { Key = "b", Label = "Second idea", Description = "Another possible answer." },
                        new ChallengeOptionDto { Key = "c", Label = "Third idea", Description = "A different answer." },
                        new ChallengeOptionDto { Key = "d", Label = "Fourth idea", Description = "Yet another answer." }
                    };
                    expected = new JValue("b");
                    break;

                case "scenario_choice":
                    challenge.Prompt = "What is the best thing to do in this situation?";
                    challenge.Options = new List<ChallengeOptionDto>
                    {
                        new ChallengeOptionDto { Key = "a", Label = "Help right away", Description = "Take helpful action now." },
                        new ChallengeOptionDto { Key = "b", Label = "Wait and see", Description = "Do nothing for now." },
                        new ChallengeOptionDto { Key = "c", Label = "Ask a friend", Description = "Get someone else to decide." }
                    };
                    expected = new JValue("a");
                    break;

                case "true_false":
                    challenge.Prompt = $"True or false: the {stationTitle} statement is correct.";
                    challenge.Options = new List<ChallengeOptionDto>
                    {
                        new ChallengeOptionDto { Key = "true", Label = "True", Description = "The statement is correct." },
                        new ChallengeOptionDto { Key = "false", Label = "False", Description = "The statement is not correct." }
                    };
                    expected = new JValue("true");
                    break;

                case "ordering":
                    challenge.Prompt = "Put these steps in the correct order.";
                    challenge.Options = new List<ChallengeOptionDto>
                    {
                        new ChallengeOptionDto { Key = "i1", Label = "Step one", Description = "The first step." },
                        new ChallengeOptionDto { Key = "i2", Label = "Step two", Description = "The second step." },
                        new ChallengeOptionDto { Key = "i3", Label = "Step three", Description = "The third step." }
                    };
                    expected = new JArray("i1", "i2", "i3");
                    break;

                case "matching":
                    challenge.Prompt = "Match each item on the left to its pair.";
                    challenge.Options = new List<ChallengeOptionDto>
                    {
                        new ChallengeOptionDto { Key = "a", Label = "Left item A", Description = "First item to match." },
                        new ChallengeOptionDto { Key = "b", Label = "Left item B", Description = "Second item to match." },
                        new ChallengeOptionDto { Key = "c", Label = "Left item C", Description = "Third item to match." }
                    };
                    expected = new JObject { ["a"] = "x", ["b"] = "y", ["c"] = "z" };
                    break;

                case "fill_blank":
                default:
                    challenge.Prompt = "Fill in the blank with the best word.";
                    challenge.Options = null;
                    expected = new JValue("noun");
                    break;
            }

            return (challenge, expected);
        }

        private static StationStartResponseDto BuildStationStart(string stationId)
        {
            return new StationStartResponseDto
            {
                StationSessionId = $"sess-{stationId}",
                StationId = stationId,
                Status = "in_progress",
                Resuming = false,
                StartedAt = StartedAt,
                ContentRevision = ContentRevision,
                ChallengeProgress = new Dictionary<string, object>()
            };
        }

        private static DemoAttemptFixtureDto BuildAttemptFixture(string challengeId, string slug)
        {
            return new DemoAttemptFixtureDto
            {
                ResponseTemplate = new AttemptResponseDto
                {
                    AttemptId = $"attempt-{challengeId}",
                    ClientAttemptUuid = null,
                    ChallengeId = challengeId,
                    Status = "accepted",
                    Accepted = true,
                    Correct = true,
                    IsReplay = null,
                    ReviewStatus = "auto_accepted",
                    Feedback = new AttemptFeedbackDto
                    {
                        IsCorrect = true,
                        Message = "Correct! Great thinking.",
                        Explanation = "You used the clues in the world to reach a good answer.",
                        EncouragingMessage = "You're doing great!",
                        RetryAction = null,
                        RetryAllowed = false
                    },
                    ScoreAwarded = 10m,
                    Progress = null,
                    RewardsGranted = new List<RewardGrantDto>
                    {
                        new RewardGrantDto
                        {
                            RewardCode = "coin",
                            RewardType = "coin",
                            DisplayName = "Coin",
                            Quantity = 5
                        }
                    },
                    ProgressUpdated = true,
                    ProgressRevision = null,
                    RewardWalletRevision = null
                },
                SafeMistake = new AttemptFeedbackDto
                {
                    IsCorrect = false,
                    Message = "Not quite — let's try once more.",
                    Explanation = null,
                    MisconceptionMessage = "Think about the clue again.",
                    EncouragingMessage = "Mistakes help us learn!",
                    RetryAction = "retry",
                    RetryAllowed = true,
                    RemainingAttempts = null,
                    CurrentHintTier = 1,
                    NextHintTier = 2,
                    HintText = "Look closely at the first part."
                }
            };
        }

        private static StationCompleteResponseDto BuildCompletion(string stationId, string stationKey)
        {
            return new StationCompleteResponseDto
            {
                StationId = stationId,
                Status = "completed",
                Completed = true,
                IsReplay = null,
                ScoreTotal = 20m,
                PortalState = "completed",
                Unlocks = null,
                TermCompletion = null,
                RewardsGranted = new List<RewardGrantDto>
                {
                    new RewardGrantDto
                    {
                        RewardCode = "coin",
                        RewardType = "coin",
                        DisplayName = "Coin",
                        Quantity = 5
                    }
                },
                WorldRestorationResult = new WorldRestorationResultDto
                {
                    StateKey = $"restored_{stationKey}",
                    Restored = true
                },
                ProgressSummary = null,
                ProgressRevision = null,
                RewardWalletRevision = null
            };
        }

        // ──────────────────────────────────────────────────────────────
        //  Fixed responses
        // ──────────────────────────────────────────────────────────────

        private static DemoResponsesDto BuildResponses(List<SubjectDto> subjects)
        {
            return new DemoResponsesDto
            {
                Ping = new PingResponseDto
                {
                    Status = "ok",
                    ServerTime = ServerTime,
                    ApiVersion = "v1"
                },
                Config = new ApiConfigDto
                {
                    ApiVersion = "v1",
                    ContractVersion = "2.0",
                    ServerTime = ServerTime,
                    MaintenanceMode = false,
                    MinimumUnityClientVersion = "1.0.0",
                    SupportedLanguages = new List<string> { "en", "fil" },
                    Polling = new PollingConfig
                    {
                        Enabled = true,
                        DefaultIntervalSeconds = 45,
                        MinimumIntervalSeconds = 15
                    },
                    Realtime = new RealtimeConfig
                    {
                        Enabled = false,
                        EventsAreMetadataOnly = true
                    }
                },
                Login = new LoginResponseDto
                {
                    Token = "demo-token-DO-NOT-USE",
                    TokenType = "Bearer",
                    Student = BuildIdentity()
                },
                Bootstrap = new BootstrapDto
                {
                    Student = BuildIdentity(),
                    Classroom = new ClassroomDto
                    {
                        Id = "demo-class-5A",
                        Name = "Grade 5 - Sampaguita",
                        GradeLevel = Grade,
                        Section = "Sampaguita"
                    },
                    Subjects = BuildSubjects(),
                    ProgressSummary = BuildInitialProgress(),
                    Rewards = BuildInitialWallet(),
                    Settings = BuildInitialSettings(),
                    SyncStatus = BuildInitialSync()
                },
                Profile = new StudentProfileDto
                {
                    Id = StudentId,
                    Name = "Maya Santos",
                    LrnMasked = "************4521",
                    GradeLevel = Grade,
                    LanguagePreference = "en",
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
                        StationsCompleted = 0,
                        StationsAvailable = science ? 0 : 2,
                        Percentage = 0m
                    });
                }
                subjectProgress.Add(new SubjectProgressDto
                {
                    SubjectSlug = slug,
                    SubjectName = SubjectNames[slug],
                    StationsCompleted = 0,
                    StationsAvailable = science ? 0 : 6,
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
                TotalStationsCompleted = 0,
                TotalStationsAvailable = 12,
                StartedStations = 0,
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
                StationUnlockRevision = "unlock-rev-1",
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
                ["station_locked"] = new DataProviderError
                {
                    Code = "STATION_LOCKED",
                    Message = "This station is still locked. Finish the earlier station first.",
                    Action = "return_to_menu",
                    Retryable = false
                },
                ["validation_error"] = new DataProviderError
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Please check your answer and try again.",
                    Action = "retry",
                    Retryable = false,
                    FieldErrors = new Dictionary<string, object> { { "answer", "This field is required." } }
                },
                ["rate_limited"] = new DataProviderError
                {
                    Code = "RATE_LIMITED",
                    Message = "You're going a bit fast. Please wait a moment.",
                    Action = "wait_then_retry",
                    Retryable = true,
                    RetryAfterSeconds = 5
                },
                ["unauthenticated"] = new DataProviderError
                {
                    Code = "UNAUTHENTICATED",
                    Message = "Your session ended. Please log in again.",
                    Action = "login_again",
                    Retryable = false
                },
                ["session_not_found"] = new DataProviderError
                {
                    Code = "SESSION_NOT_FOUND",
                    Message = "We couldn't find that activity session.",
                    Action = "return_to_menu",
                    Retryable = false
                },
                ["content_not_published"] = new DataProviderError
                {
                    Code = "CONTENT_NOT_PUBLISHED",
                    Message = "This activity isn't ready yet.",
                    Action = "return_to_menu",
                    Retryable = false
                }
            };
        }

        private static string TitleCase(string snake)
        {
            var parts = snake.Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0) continue;
                parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
            }
            return string.Join(" ", parts);
        }
    }
}