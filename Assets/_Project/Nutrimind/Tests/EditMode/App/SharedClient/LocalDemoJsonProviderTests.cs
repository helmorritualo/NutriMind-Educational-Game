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
    /// logout, settings, hints, idempotent (duplicate) attempts,
    /// result retrieval, reward balance retrieval, safe error contracts,
    /// immutable source data, and the development-only guard.
    /// </summary>
    [TestFixture]
    public class LocalDemoJsonProviderTests
    {
        private const string Lrn = "000000000001";
        private const string Pin = "1234";
        private const string Term1Quiz1 = "literaquest-t1-q1";
        private const string Term1Quiz2 = "literaquest-t1-q2";

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

        private JToken ExpectedAnswer(string itemId)
        {
            var eval = _fixtureRoot["demo_only_evaluation"]?[itemId]?["expected_answer"];
            Assert.That(eval, Is.Not.Null, "Fixture must define an expected answer for " + itemId);
            return eval;
        }

        private List<string> QuizItemIds(string quizId)
        {
            var ids = new List<string>();
            var items = _fixtureRoot["quiz_detail_by_id"]?[quizId]?["items"] as JArray;
            Assert.That(items, Is.Not.Null, "Fixture must define items for " + quizId);
            foreach (var item in items) ids.Add(item["id"].ToString());
            return ids;
        }

        private QuizAttemptRequestDto BuildCorrectAttempt(string quizId)
        {
            var req = new QuizAttemptRequestDto
            {
                ClientAttemptUuid = System.Guid.NewGuid().ToString("N"),
                Answers = new Dictionary<string, object>()
            };
            foreach (var itemId in QuizItemIds(quizId))
            {
                req.Answers[itemId] = ExpectedAnswer(itemId);
            }
            return req;
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
            Assert.That(fixture.QuizDetailById, Has.Count.EqualTo(12));
            Assert.That(fixture.AttemptResultByQuizId, Has.Count.EqualTo(12));
            Assert.That(fixture.DemoOnlyEvaluation, Is.Not.Null.And.Count.GreaterThan(0));
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
            var req = BuildCorrectAttempt(Term1Quiz1);
            var attempt = p.SubmitQuizAttemptAsync(Term1Quiz1, req).Result;
            Assert.That(attempt.Success, Is.True);

            var logout = p.LogoutAsync(CancellationToken.None).Result;
            Assert.That(logout.Success, Is.True);
            Assert.That(p.IsAuthenticated, Is.False);

            // After logout, auth is required again and progress is reset.
            Assert.That(p.GetProgressSummaryAsync().Result.Success, Is.False);
            var p2 = p; p2.LoginAsync(new LoginRequestDto { Lrn = Lrn, Pin = Pin }).Wait();
            var progress = p2.GetProgressSummaryAsync().Result;
            Assert.That(progress.Data.TotalQuizzesCompleted, Is.EqualTo(0));
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

        // ── Subjects / terms / quizzes / science preview ─────────────

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
        public void ScienceQuizzes_EmptyPreviewList_IsNotAnError()
        {
            var p = NewLoggedIn();
            var quizzes = p.GetQuizzesAsync("sciencequest", 1).Result;
            Assert.That(quizzes.Success, Is.True);
            Assert.That(quizzes.Data.Quizzes, Has.Count.EqualTo(0));
            Assert.That(quizzes.Data.PreviewMode, Is.EqualTo("exploration_only"));
        }

        [Test]
        public void PlayableQuizzes_AreReturnedForTerm()
        {
            var p = NewLoggedIn();
            var quizzes = p.GetQuizzesAsync("literaquest", 1).Result;
            Assert.That(quizzes.Success, Is.True);
            Assert.That(quizzes.Data.Quizzes, Has.Count.EqualTo(2));
        }

        // ── Hints / discoveries (quiz details) ─────────────────────

        [Test]
        public void QuizDetail_ExposesInstructionsAndItems()
        {
            var p = NewLoggedIn();
            var content = p.GetQuizDetailAsync(Term1Quiz1).Result;
            Assert.That(content.Success, Is.True);
            Assert.That(content.Data.Instructions, Is.Not.Null);
            Assert.That(content.Data.Items, Is.Not.Null.And.Count.GreaterThan(0));
        }

        [Test]
        public void WrongAnswer_ReturnsEncouragingSafeMistakeHint()
        {
            var p = NewLoggedIn();
            string itemId = QuizItemIds(Term1Quiz1)[0];
            var req = new QuizAttemptRequestDto
            {
                ClientAttemptUuid = System.Guid.NewGuid().ToString("N"),
                Answers = new Dictionary<string, object> { { itemId, "definitely_wrong_zzz" } }
            };
            var r = p.SubmitQuizAttemptAsync(Term1Quiz1, req).Result;

            Assert.That(r.Success, Is.True, "A wrong answer is still an accepted, recorded attempt.");
            Assert.That(r.Data.AnswersFeedback, Is.Not.Null);
            Assert.That(r.Data.AnswersFeedback.TryGetValue(itemId, out var fb), Is.True);
            Assert.That(fb.IsCorrect, Is.False);
            Assert.That(fb.HintText, Is.Not.Null.And.Not.Empty, "Safe mistake must offer a hint.");
            Assert.That(r.Data.Passed, Is.False);
        }

        // ── Attempts: correctness + idempotent duplicate replay ───────

        [Test]
        public void CorrectAnswer_IsAcceptedAndPassesQuiz()
        {
            var p = NewLoggedIn();
            var req = BuildCorrectAttempt(Term1Quiz1);
            var r = p.SubmitQuizAttemptAsync(Term1Quiz1, req).Result;

            Assert.That(r.Success, Is.True);
            Assert.That(r.Data.Passed, Is.True);
            Assert.That(r.Data.IsReplay, Is.Not.EqualTo(true));
            Assert.That(r.Data.Score, Is.EqualTo(r.Data.TotalPossible));
        }

        [Test]
        public void DuplicateAttemptUuid_WithSameAnswers_IsIdempotentReplay()
        {
            var p = NewLoggedIn();
            var req = BuildCorrectAttempt(Term1Quiz1);
            req.ClientAttemptUuid = "dup-uuid-1";

            var first = p.SubmitQuizAttemptAsync(Term1Quiz1, req).Result;
            var second = p.SubmitQuizAttemptAsync(Term1Quiz1, req).Result;

            Assert.That(first.Data.IsReplay, Is.Not.EqualTo(true));
            Assert.That(second.Data.IsReplay, Is.True, "Duplicate client_attempt_uuid must replay idempotently.");
            Assert.That(second.Data.Passed, Is.EqualTo(first.Data.Passed));
            Assert.That(second.Data.AttemptId, Is.EqualTo(first.Data.AttemptId));
        }

        [Test]
        public void DuplicateAttemptUuid_WithDifferentAnswers_FailsWithConflict()
        {
            var p = NewLoggedIn();
            var req1 = BuildCorrectAttempt(Term1Quiz1);
            req1.ClientAttemptUuid = "conflict-uuid-1";

            var first = p.SubmitQuizAttemptAsync(Term1Quiz1, req1).Result;
            Assert.That(first.Success, Is.True);

            var req2 = new QuizAttemptRequestDto
            {
                ClientAttemptUuid = "conflict-uuid-1",
                Answers = new Dictionary<string, object> { { QuizItemIds(Term1Quiz1)[0], "bad" } }
            };

            var second = p.SubmitQuizAttemptAsync(Term1Quiz1, req2).Result;
            Assert.That(second.Success, Is.False);
            Assert.That(second.Error.Code, Is.EqualTo("CONFLICT"));
        }

        // ── Progress: locks & completion ────────────────────────────────

        [Test]
        public void CompletingQuiz_UnlocksNextQuiz()
        {
            var p = NewLoggedIn();
            // Term 2 quiz 1 starts locked.
            var lockedStart = p.GetQuizDetailAsync("literaquest-t2-q1").Result;
            Assert.That(lockedStart.Success, Is.True);
            Assert.That(lockedStart.Data.State, Is.EqualTo("locked"));

            // Complete Term 1 Quiz 1
            var r1 = p.SubmitQuizAttemptAsync(Term1Quiz1, BuildCorrectAttempt(Term1Quiz1)).Result;
            Assert.That(r1.Success, Is.True);

            // Complete Term 1 Quiz 2
            var r2 = p.SubmitQuizAttemptAsync(Term1Quiz2, BuildCorrectAttempt(Term1Quiz2)).Result;
            Assert.That(r2.Success, Is.True);

            // Now Term 2 Quiz 1 should be unlocked
            var unlockedDetail = p.GetQuizDetailAsync("literaquest-t2-q1").Result;
            Assert.That(unlockedDetail.Success, Is.True);
            Assert.That(unlockedDetail.Data.State, Is.EqualTo("unlocked"));
        }

        // ── Reset + immutable source ──────────────────────────────────

        [Test]
        public void ResetDemoState_RestoresBaseline()
        {
            var p = NewLoggedIn();
            p.SubmitQuizAttemptAsync(Term1Quiz1, BuildCorrectAttempt(Term1Quiz1)).Wait();
            p.PatchSettingsAsync(new SettingsDto { MusicVolume = 0.1f }).Wait();

            p.ResetDemoState();
            Assert.That(p.IsAuthenticated, Is.False);

            p.LoginAsync(new LoginRequestDto { Lrn = Lrn, Pin = Pin }).Wait();
            Assert.That(p.GetProgressSummaryAsync().Result.Data.TotalQuizzesCompleted, Is.EqualTo(0));
        }

        [Test]
        public void SourceFixtureData_IsImmutable_AcrossInstances()
        {
            var p1 = NewLoggedIn();
            p1.SubmitQuizAttemptAsync(Term1Quiz1, BuildCorrectAttempt(Term1Quiz1)).Wait();

            // A fresh instance built from the SAME source JSON sees a clean baseline.
            var p2 = NewLoggedIn();
            p2.LoginAsync(new LoginRequestDto { Lrn = Lrn, Pin = Pin }).Wait();
            Assert.That(p2.GetProgressSummaryAsync().Result.Data.TotalQuizzesCompleted, Is.EqualTo(0),
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
            var p = New();
            Assert.That(p.IsReady, Is.True);
        }

        // ── Provider parity (Local vs HTTP use identical DTOs) ────────

        [Test]
        public void Parity_LocalAndHttpProviders_ProduceEquivalentQuizContent()
        {
            var local = NewLoggedIn();
            var localResult = local.GetQuizDetailAsync(Term1Quiz1).Result;
            Assert.That(localResult.Success, Is.True);

            string payload = JsonConvert.SerializeObject(localResult.Data, JsonSettings.SafeDefaults);
            var transport = new FakeHttpTransport();
            transport.EnqueueSuccess(200, payload);
            var http = new HttpProvider(
                new HttpProviderConfig { BaseUrl = "https://demo.nutrimind.example", MaxRetries = 0 },
                new AuthSessionState { Token = "demo" }, transport);

            var httpResult = http.GetQuizDetailAsync(Term1Quiz1).Result;
            Assert.That(httpResult.Success, Is.True);

            Assert.That(httpResult.Data.Id, Is.EqualTo(localResult.Data.Id));
            Assert.That(httpResult.Data.Title, Is.EqualTo(localResult.Data.Title));
            Assert.That(httpResult.Data.Items.Count, Is.EqualTo(localResult.Data.Items.Count));
            Assert.That(httpResult.Data.Instructions, Is.EqualTo(localResult.Data.Instructions));
        }
    }
}