using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;
using NutriMind.Runtime.App.Http;
using UnityEngine;

namespace NutriMind.Tests.EditMode.App
{
    /// <summary>
    /// Verifies the local demo provider against the fabricated fixture:
    /// JSON validity, DTO deserialization, provider parity with HTTP, reset,
    /// logout, settings, hints, discoveries, idempotent (duplicate) attempts,
    /// completion (unlocks + term crystal), reward use, safe error contracts,
    /// immutable source data, and the development-only guard.
    /// </summary>
    [TestFixture]
    public class LocalDemoJsonProviderTests
    {
        private const string Lrn = "000000000001";
        private const string Pin = "1234";
        private const string Term1Station1 = "literaquest-t1-s1";
        private const string Term1Station2 = "literaquest-t1-s2";

        private string _fixtureJson;
        private JObject _fixtureRoot;

        private static string FixturePath =>
            Path.Combine(Application.dataPath, "_Project", "Nutrimind", "Resources", "DemoData", "full-demo-student-data.json");

        [SetUp]
        public void SetUp()
        {
            Assert.That(File.Exists(FixturePath), Is.True,
                "Demo fixture must exist. Run NutriMind > Generate Demo Fixture. Path: " + FixturePath);
            _fixtureJson = File.ReadAllText(FixturePath);
            _fixtureRoot = JObject.Parse(_fixtureJson);
        }

        private LocalDemoJsonProvider New() => new LocalDemoJsonProvider(_fixtureJson);

        private LocalDemoJsonProvider NewLoggedIn()
        {
            var p = New();
            var login = p.LoginAsync(new LoginRequestDto { Lrn = Lrn, Pin = Pin }).Result;
            Assert.That(login.Success, Is.True, "Demo login should succeed: " + login.ErrorMessage);
            return p;
        }

        private JToken ExpectedAnswer(string challengeId)
        {
            var eval = _fixtureRoot["demo_only_evaluation"]?[challengeId]?["expected_answer"];
            Assert.That(eval, Is.Not.Null, "Fixture must define an expected answer for " + challengeId);
            return eval;
        }

        private List<string> ChallengeIds(string stationId)
        {
            var ids = new List<string>();
            var challenges = _fixtureRoot["station_content_by_id"]?[stationId]?["challenges"] as JArray;
            Assert.That(challenges, Is.Not.Null, "Fixture must define challenges for " + stationId);
            foreach (var c in challenges) ids.Add(c["challenge_id"].ToString());
            return ids;
        }

        private void CompleteAllChallenges(LocalDemoJsonProvider p, string stationId)
        {
            p.StartStationAsync(stationId).Wait();
            foreach (var cid in ChallengeIds(stationId))
            {
                var req = new AttemptRequestDto
                {
                    StationSessionId = "sess-" + stationId,
                    StationId = stationId,
                    ClientAttemptUuid = System.Guid.NewGuid().ToString("N"),
                    Answer = ExpectedAnswer(cid)
                };
                var r = p.SubmitAttemptAsync(cid, req).Result;
                Assert.That(r.Success, Is.True);
                Assert.That(r.Data.Correct, Is.True, "Expected-answer submission should be correct for " + cid);
            }
        }

        // ── JSON validation + DTO deserialization ─────────────────────

        [Test]
        public void Fixture_IsValidJson_AndIsLocalDemoOnly()
        {
            Assert.That(_fixtureRoot["mode"]?.ToString(), Is.EqualTo("local_demo_only"));
            Assert.That(_fixtureRoot["notice"], Is.Not.Null);
            Assert.That(_fixtureJson, Does.Not.Contain("\"access_token\""),
                "Fixture must use the canonical 'token' field, not the superseded 'access_token'.");
        }

        [Test]
        public void Fixture_DeserializesIntoDemoFixtureDto_WithExpectedCounts()
        {
            var fixture = JsonConvert.DeserializeObject<DemoFixtureDto>(_fixtureJson, JsonSettings.SafeDefaults);
            Assert.That(fixture, Is.Not.Null);
            Assert.That(fixture.Responses?.Subjects, Has.Count.EqualTo(3));
            Assert.That(fixture.TermsBySubject, Has.Count.EqualTo(3));
            Assert.That(fixture.StationContentById, Has.Count.EqualTo(12));
            Assert.That(fixture.StationStartById, Has.Count.EqualTo(12));
            Assert.That(fixture.CompletionResultByStationId, Has.Count.EqualTo(12));
            Assert.That(fixture.AttemptResultByChallengeId.Count, Is.GreaterThanOrEqualTo(12));
            Assert.That(fixture.ErrorFixtures, Is.Not.Null.And.Count.GreaterThan(0));
        }

        // ── Auth / logout ─────────────────────────────────────────────

        [Test]
        public void Login_WithDemoCredentials_Succeeds()
        {
            var p = New();
            var r = p.LoginAsync(new LoginRequestDto { Lrn = Lrn, Pin = Pin }).Result;
            Assert.That(r.Success, Is.True);
            Assert.That(r.Data.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(p.IsAuthenticated, Is.True);
        }

        [Test]
        public void Login_WithWrongCredentials_FailsSafely()
        {
            var p = New();
            var r = p.LoginAsync(new LoginRequestDto { Lrn = "bad", Pin = "bad" }).Result;
            Assert.That(r.Success, Is.False);
            Assert.That(r.Error.Code, Is.EqualTo("UNAUTHENTICATED"));
            Assert.That(p.IsAuthenticated, Is.False);
        }

        [Test]
        public void UnauthenticatedCall_ReturnsUnauthenticated()
        {
            var p = New();
            var r = p.GetBootstrapAsync(CancellationToken.None).Result;
            Assert.That(r.Success, Is.False);
            Assert.That(r.Error.Code, Is.EqualTo("UNAUTHENTICATED"));
            Assert.That(r.Error.ResolvedAction, Is.EqualTo(ErrorAction.LoginAgain));
        }

        [Test]
        public void Logout_ResetsToCleanBaseline()
        {
            var p = NewLoggedIn();
            CompleteAllChallenges(p, Term1Station1);
            p.CompleteStationAsync(Term1Station1).Wait();

            var logout = p.LogoutAsync(CancellationToken.None).Result;
            Assert.That(logout.Success, Is.True);
            Assert.That(p.IsAuthenticated, Is.False);

            // After logout, auth is required again and progress is reset.
            Assert.That(p.GetProgressSummaryAsync().Result.Success, Is.False);
            var p2 = p; p2.LoginAsync(new LoginRequestDto { Lrn = Lrn, Pin = Pin }).Wait();
            var progress = p2.GetProgressSummaryAsync().Result;
            Assert.That(progress.Data.TotalStationsCompleted, Is.EqualTo(0));
        }

        // ── Settings ──────────────────────────────────────────────────

        [Test]
        public void PatchSettings_MergesAndBumpsRevision()
        {
            var p = NewLoggedIn();
            var before = p.GetSettingsAsync().Result.Data;
            string revBefore = before.Revision;

            var patched = p.PatchSettingsAsync(new SettingsDto { MusicVolume = 0.2f, ReducedMotion = true }).Result;
            Assert.That(patched.Success, Is.True);
            Assert.That(patched.Data.MusicVolume, Is.EqualTo(0.2f).Within(0.001f));
            Assert.That(patched.Data.ReducedMotion, Is.True);
            Assert.That(patched.Data.Revision, Is.Not.EqualTo(revBefore));
            // Unspecified fields are preserved.
            Assert.That(patched.Data.Language, Is.EqualTo(before.Language));
        }

        // ── Subjects / terms / stations / science preview ─────────────

        [Test]
        public void Subjects_ReturnsThree_WithScienceExplorationPreview()
        {
            var p = NewLoggedIn();
            var subjects = p.GetSubjectsAsync().Result;
            Assert.That(subjects.Success, Is.True);
            Assert.That(subjects.Data, Has.Count.EqualTo(3));
            var science = subjects.Data.Find(s => s.Slug == "sciencequest");
            Assert.That(science, Is.Not.Null);
            Assert.That(science.PreviewMode, Is.EqualTo("exploration_only"));
        }

        [Test]
        public void ScienceStations_EmptyPreviewList_IsNotAnError()
        {
            var p = NewLoggedIn();
            var stations = p.GetStationsAsync("sciencequest", 1).Result;
            Assert.That(stations.Success, Is.True);
            Assert.That(stations.Data.Stations, Has.Count.EqualTo(0));
            Assert.That(stations.Data.PreviewMode, Is.EqualTo("exploration_only"));
        }

        [Test]
        public void PlayableStations_AreReturnedForTerm()
        {
            var p = NewLoggedIn();
            var stations = p.GetStationsAsync("literaquest", 1).Result;
            Assert.That(stations.Success, Is.True);
            Assert.That(stations.Data.Stations, Has.Count.EqualTo(2));
        }

        // ── Hints / discoveries (station content) ─────────────────────

        [Test]
        public void StationContent_ExposesHintPolicyAndOptionalDiscovery()
        {
            var p = NewLoggedIn();
            var content = p.GetStationContentAsync(Term1Station1).Result;
            Assert.That(content.Success, Is.True);
            Assert.That(content.Data.HintPolicy, Is.Not.Null);
            Assert.That(content.Data.HintPolicy.PreserveWorldProgress, Is.True);
            Assert.That(content.Data.HintPolicy.PenalizeOrdinaryMistake, Is.False);
            Assert.That(content.Data.Discoveries, Is.Not.Null.And.Count.GreaterThan(0));
            Assert.That(content.Data.Discoveries[0].Optional, Is.True,
                "Discoveries must be optional (never required to complete a station).");
            Assert.That(content.Data.LearningCycle, Is.Not.Null);
            Assert.That(content.Data.LearningCycle.Discover, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void WrongAnswer_ReturnsEncouragingSafeMistakeHint_NoPenalty()
        {
            var p = NewLoggedIn();
            string cid = ChallengeIds(Term1Station1)[0];
            p.StartStationAsync(Term1Station1).Wait();
            var r = p.SubmitAttemptAsync(cid, new AttemptRequestDto
            {
                StationSessionId = "sess",
                ClientAttemptUuid = System.Guid.NewGuid().ToString("N"),
                Answer = "definitely_wrong_zzz"
            }).Result;

            Assert.That(r.Success, Is.True, "A wrong answer is still an accepted, recorded attempt.");
            Assert.That(r.Data.Correct, Is.False);
            Assert.That(r.Data.Feedback, Is.Not.Null);
            Assert.That(r.Data.Feedback.RetryAllowed, Is.True);
            Assert.That(r.Data.Feedback.HintText, Is.Not.Null.And.Not.Empty, "Safe mistake must offer a hint.");
            Assert.That(r.Data.ScoreAwarded, Is.EqualTo(0m), "Ordinary mistakes are not penalized with score.");
            // World progress preserved: no challenge marked complete.
            Assert.That(r.Data.Progress.CompletedChallenges, Is.EqualTo(0));
        }

        // ── Attempts: correctness + idempotent duplicate replay ───────

        [Test]
        public void CorrectAnswer_IsAcceptedAndGrantsReward()
        {
            var p = NewLoggedIn();
            string cid = ChallengeIds(Term1Station1)[0];
            p.StartStationAsync(Term1Station1).Wait();
            var r = p.SubmitAttemptAsync(cid, new AttemptRequestDto
            {
                StationSessionId = "sess",
                ClientAttemptUuid = System.Guid.NewGuid().ToString("N"),
                Answer = ExpectedAnswer(cid)
            }).Result;

            Assert.That(r.Success, Is.True);
            Assert.That(r.Data.Correct, Is.True);
            Assert.That(r.Data.Accepted, Is.True);
            Assert.That(r.Data.IsReplay, Is.Not.EqualTo(true));
            Assert.That(r.Data.RewardsGranted, Is.Not.Null.And.Count.GreaterThan(0));
            Assert.That(r.Data.Progress.CompletedChallenges, Is.EqualTo(1));
        }

        [Test]
        public void DuplicateAttemptUuid_IsIdempotentReplay_NoDoubleReward()
        {
            var p = NewLoggedIn();
            string cid = ChallengeIds(Term1Station1)[0];
            p.StartStationAsync(Term1Station1).Wait();
            var req = new AttemptRequestDto
            {
                StationSessionId = "sess",
                StationId = Term1Station1,
                ClientAttemptUuid = "dup-uuid-1",
                Answer = ExpectedAnswer(cid)
            };

            var first = p.SubmitAttemptAsync(cid, req).Result;
            int coinsAfterFirst = p.GetRewardsAsync().Result.Data.TotalCoins ?? 0;

            var second = p.SubmitAttemptAsync(cid, req).Result;
            int coinsAfterSecond = p.GetRewardsAsync().Result.Data.TotalCoins ?? 0;

            Assert.That(first.Data.IsReplay, Is.Not.EqualTo(true));
            Assert.That(second.Data.IsReplay, Is.True, "Duplicate client_attempt_uuid must replay idempotently.");
            Assert.That(second.Data.Correct, Is.EqualTo(first.Data.Correct));
            Assert.That(second.Data.AttemptId, Is.EqualTo(first.Data.AttemptId));
            Assert.That(coinsAfterSecond, Is.EqualTo(coinsAfterFirst), "Replay must not grant rewards twice.");
        }

        // ── Completion: unlocks, term crystal, idempotency ────────────

        [Test]
        public void CompleteStation_FinalizesUnlocksAndUpdatesProgress()
        {
            var p = NewLoggedIn();
            CompleteAllChallenges(p, Term1Station1);
            var done = p.CompleteStationAsync(Term1Station1).Result;

            Assert.That(done.Success, Is.True);
            Assert.That(done.Data.Completed, Is.True);
            Assert.That(done.Data.IsReplay, Is.Not.EqualTo(true));
            Assert.That(done.Data.PortalState, Is.EqualTo("completed"));
            Assert.That(done.Data.WorldRestorationResult.Restored, Is.True);
            Assert.That(done.Data.ProgressSummary.TotalStationsCompleted, Is.EqualTo(1));
            Assert.That(done.Data.ProgressRevision, Is.Not.Null);
        }

        [Test]
        public void CompleteStation_BeforeAllChallenges_FailsSafely()
        {
            var p = NewLoggedIn();
            p.StartStationAsync(Term1Station1).Wait();
            var done = p.CompleteStationAsync(Term1Station1).Result;
            Assert.That(done.Success, Is.False);
            Assert.That(done.Error.Code, Is.EqualTo("VALIDATION_ERROR"));
        }

        [Test]
        public void CompleteStation_Duplicate_IsIdempotentReplay()
        {
            var p = NewLoggedIn();
            CompleteAllChallenges(p, Term1Station1);
            var first = p.CompleteStationAsync(Term1Station1).Result;
            int starsAfterFirst = p.GetProgressSummaryAsync().Result.Data.Stars ?? 0;

            var second = p.CompleteStationAsync(Term1Station1).Result;
            int starsAfterSecond = p.GetProgressSummaryAsync().Result.Data.Stars ?? 0;

            Assert.That(first.Data.IsReplay, Is.Not.EqualTo(true));
            Assert.That(second.Data.IsReplay, Is.True);
            Assert.That(starsAfterSecond, Is.EqualTo(starsAfterFirst), "Re-completion must not award stars twice.");
        }

        [Test]
        public void CompletingBothTermStations_AwardsSubjectCrystalOnce()
        {
            var p = NewLoggedIn();
            CompleteAllChallenges(p, Term1Station1);
            p.CompleteStationAsync(Term1Station1).Wait();

            CompleteAllChallenges(p, Term1Station2);
            var done = p.CompleteStationAsync(Term1Station2).Result;

            Assert.That(done.Data.TermCompletion, Is.Not.Null);
            Assert.That(done.Data.TermCompletion.Completed, Is.True);
            Assert.That(done.Data.TermCompletion.Crystal, Is.Not.Null);
            Assert.That(done.Data.TermCompletion.Crystal.RewardType, Is.EqualTo("crystal"));

            // Crystal is present in the wallet exactly once.
            var wallet = p.GetRewardsAsync().Result.Data;
            int crystals = 0;
            foreach (var r in wallet.Rewards) if (r.RewardType == "crystal") crystals += r.Quantity ?? 0;
            Assert.That(crystals, Is.EqualTo(1));
        }

        [Test]
        public void CompletingFirstStation_UnlocksNextStation_WhereApplicable()
        {
            var p = NewLoggedIn();
            // Term 2 station 1 starts locked.
            var lockedStart = p.StartStationAsync("literaquest-t2-s1").Result;
            Assert.That(lockedStart.Success, Is.False);
            Assert.That(lockedStart.Error.Code, Is.EqualTo("STATION_LOCKED"));

            // Complete both term-1 stations -> term complete -> unlock term-2 station 1.
            CompleteAllChallenges(p, Term1Station1);
            p.CompleteStationAsync(Term1Station1).Wait();
            CompleteAllChallenges(p, Term1Station2);
            p.CompleteStationAsync(Term1Station2).Wait();

            var nowStart = p.StartStationAsync("literaquest-t2-s1").Result;
            Assert.That(nowStart.Success, Is.True, "Next term's first station should unlock after term completion.");
        }

        // ── Reward use ────────────────────────────────────────────────

        [Test]
        public void UseReward_DecrementsBalance_AndOverUseFails()
        {
            var p = NewLoggedIn();
            var before = p.GetRewardsAsync().Result.Data;
            var hint = before.Rewards.Find(r => r.RewardCode == "hint_token");
            Assert.That(hint, Is.Not.Null.And.Property("Quantity").GreaterThan(0));
            int qty = hint.Quantity ?? 0;

            var use = p.UseRewardAsync("hint_token", new UseRewardRequestDto { Quantity = 1 }).Result;
            Assert.That(use.Success, Is.True);
            Assert.That(use.Data.RemainingQuantity, Is.EqualTo(qty - 1));

            var over = p.UseRewardAsync("hint_token", new UseRewardRequestDto { Quantity = 9999 }).Result;
            Assert.That(over.Success, Is.False);
            Assert.That(over.Error.Code, Is.EqualTo("VALIDATION_ERROR"));
        }

        // ── Reset + immutable source ──────────────────────────────────

        [Test]
        public void ResetDemoState_RestoresBaseline()
        {
            var p = NewLoggedIn();
            CompleteAllChallenges(p, Term1Station1);
            p.CompleteStationAsync(Term1Station1).Wait();
            p.PatchSettingsAsync(new SettingsDto { MusicVolume = 0.1f }).Wait();

            p.ResetDemoState();
            Assert.That(p.IsAuthenticated, Is.False);

            p.LoginAsync(new LoginRequestDto { Lrn = Lrn, Pin = Pin }).Wait();
            Assert.That(p.GetProgressSummaryAsync().Result.Data.TotalStationsCompleted, Is.EqualTo(0));
            Assert.That(p.GetRewardsAsync().Result.Data.TotalCoins, Is.EqualTo(0));
        }

        [Test]
        public void SourceFixtureData_IsImmutable_AcrossInstances()
        {
            var p1 = NewLoggedIn();
            CompleteAllChallenges(p1, Term1Station1);
            p1.CompleteStationAsync(Term1Station1).Wait();

            // A fresh instance built from the SAME source JSON sees a clean baseline.
            var p2 = NewLoggedIn();
            Assert.That(p2.GetProgressSummaryAsync().Result.Data.TotalStationsCompleted, Is.EqualTo(0),
                "Mutations must not leak into the immutable source fixture.");
        }

        // ── Error contract ────────────────────────────────────────────

        [Test]
        public void ErrorFixtures_AreStudentSafe()
        {
            var fixture = JsonConvert.DeserializeObject<DemoFixtureDto>(_fixtureJson, JsonSettings.SafeDefaults);
            Assert.That(fixture.ErrorFixtures, Is.Not.Null.And.Count.GreaterThan(0));
            foreach (var kvp in fixture.ErrorFixtures)
            {
                var err = kvp.Value;
                Assert.That(err.Code, Is.Not.Null.And.Not.Empty, "Error '" + kvp.Key + "' must carry a stable code.");
                Assert.That(err.Message, Is.Not.Null.And.Not.Empty);
                Assert.That(err.Message.ToLowerInvariant(), Does.Not.Contain("answer_key"));
                Assert.That(err.Message.ToLowerInvariant(), Does.Not.Contain("correct_answer"));
                Assert.That(err.Message.ToLowerInvariant(), Does.Not.Contain("exception"));
            }
        }

        // ── Production / development guard ─────────────────────────────

        [Test]
        public void DevelopmentGuard_EditorAllowsLocalDemoProvider()
        {
            // In editor/development the provider constructs and loads cleanly.
            // Release builds reject it (CompositionRoot throws + provider #if guard);
            // that path cannot execute under the editor test runner.
            var p = New();
            Assert.That(p.IsReady, Is.True);
        }

        // ── Provider parity (Local vs HTTP use identical DTOs) ────────

        [Test]
        public void Parity_LocalAndHttpProviders_ProduceEquivalentStationContent()
        {
            // The local provider serves a station-content DTO from the fixture.
            var local = NewLoggedIn();
            var localResult = local.GetStationContentAsync(Term1Station1).Result;
            Assert.That(localResult.Success, Is.True);

            // Feed the SAME payload through the HTTP provider's deserialization path.
            string payload = JsonConvert.SerializeObject(localResult.Data, JsonSettings.SafeDefaults);
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, payload);
            var http = new HttpProvider(
                new HttpProviderConfig { BaseUrl = "https://demo.nutrimind.example", MaxRetries = 0 },
                new AuthSessionState { Token = "demo" }, transport);

            var httpResult = http.GetStationContentAsync(Term1Station1).Result;
            Assert.That(httpResult.Success, Is.True);

            // Both providers yield an equivalent DTO graph.
            Assert.That(httpResult.Data.StationId, Is.EqualTo(localResult.Data.StationId));
            Assert.That(httpResult.Data.MissionTitle, Is.EqualTo(localResult.Data.MissionTitle));
            Assert.That(httpResult.Data.Challenges.Count, Is.EqualTo(localResult.Data.Challenges.Count));
            Assert.That(httpResult.Data.LearningCycle.Discover, Is.EqualTo(localResult.Data.LearningCycle.Discover));
            Assert.That(httpResult.Data.HintPolicy.MaxHintTier, Is.EqualTo(localResult.Data.HintPolicy.MaxHintTier));
            Assert.That(httpResult.Data.WorldTasks.Count, Is.EqualTo(localResult.Data.WorldTasks.Count));
        }
    }
}