using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;
using Newtonsoft.Json;

namespace NutriMind.Tests.EditMode.App.SharedClient
{
    /// <summary>
    /// Reflection-based contract sanity tests.
    /// Ensures that all API signatures and JSON property mappings in DTOs
    /// conform to the 'quiz_first_laravel_1' Laravel specifications,
    /// preventing accidental field renaming or wrong Task-wrapped generics.
    /// </summary>
    [TestFixture]
    public class GameDataProviderContractTests
    {
        // ──────────────────────────────────────────────────────────────
        //  IGameDataProvider contract signatures
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void IGameDataProvider_HasTaskAndCancellationTokenCorrectlyTyped()
        {
            Type type = typeof(IGameDataProvider);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] params_ = method.GetParameters();
                bool hasCt = false;
                foreach (ParameterInfo p in params_)
                {
                    if (p.ParameterType == typeof(CancellationToken) && p.IsOptional)
                    {
                        hasCt = true;
                        break;
                    }
                }

                Assert.That(hasCt, Is.True,
                    "Method {0}.{1} must have an optional CancellationToken parameter.",
                    type.Name, method.Name);

                Assert.That(typeof(Task).IsAssignableFrom(method.ReturnType), Is.True,
                    "Method {0}.{1} must return a Task or Task<T>.",
                    type.Name, method.Name);

                if (method.ReturnType.IsGenericType)
                {
                    Type genericDef = method.ReturnType.GetGenericTypeDefinition();
                    Assert.That(genericDef, Is.EqualTo(typeof(Task<>)),
                        "Method {0}.{1} must return a generic Task<T>.",
                        type.Name, method.Name);

                    Type resultType = method.ReturnType.GetGenericArguments()[0];
                    Type genericResultDef = resultType.IsGenericType ? resultType.GetGenericTypeDefinition() : null;
                    Assert.That(genericResultDef, Is.EqualTo(typeof(DataResult<>)),
                        "Method {0}.{1} must wrap its result in a DataResult<T> envelope.",
                        type.Name, method.Name);
                }
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Snake case / JSON Property attribute mapping checks
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void QuizDto_HasSnakeCaseJsonProperties()
        {
            AssertDtoHasJsonProperty<QuizDto>("Id", "id");
            AssertDtoHasJsonProperty<QuizDto>("Title", "title");
            AssertDtoHasJsonProperty<QuizDto>("Description", "description");
            AssertDtoHasJsonProperty<QuizDto>("SubjectSlug", "subject_slug");
            AssertDtoHasJsonProperty<QuizDto>("TermNumber", "term_number");
            AssertDtoHasJsonProperty<QuizDto>("GradeLevel", "grade_level");
            AssertDtoHasJsonProperty<QuizDto>("State", "state");
            AssertDtoHasJsonProperty<QuizDto>("TotalItems", "total_items");
            AssertDtoHasJsonProperty<QuizDto>("DurationMinutes", "duration_minutes");
            AssertDtoHasJsonProperty<QuizDto>("IsAvailable", "is_available");
            AssertDtoHasJsonProperty<QuizDto>("ProgressPercent", "progress_percent");
        }

        [Test]
        public void QuizListDto_HasSnakeCaseJsonProperties()
        {
            AssertDtoHasJsonProperty<QuizListDto>("SubjectSlug", "subject_slug");
            AssertDtoHasJsonProperty<QuizListDto>("GradeLevel", "grade_level");
            AssertDtoHasJsonProperty<QuizListDto>("TermNumber", "term_number");
            AssertDtoHasJsonProperty<QuizListDto>("Quizzes", "quizzes");
            AssertDtoHasJsonProperty<QuizListDto>("PreviewMode", "preview_mode");
            AssertDtoHasJsonProperty<QuizListDto>("Message", "message");
        }

        [Test]
        public void QuizDetailDto_HasSnakeCaseJsonProperties()
        {
            AssertDtoHasJsonProperty<QuizDetailDto>("Instructions", "instructions");
            AssertDtoHasJsonProperty<QuizDetailDto>("Items", "items");
        }

        [Test]
        public void QuizItemDto_HasSnakeCaseJsonProperties()
        {
            AssertDtoHasJsonProperty<QuizItemDto>("Id", "id");
            AssertDtoHasJsonProperty<QuizItemDto>("QuizId", "quiz_id");
            AssertDtoHasJsonProperty<QuizItemDto>("Type", "type");
            AssertDtoHasJsonProperty<QuizItemDto>("Prompt", "prompt");
            AssertDtoHasJsonProperty<QuizItemDto>("Options", "options");
            AssertDtoHasJsonProperty<QuizItemDto>("OrderIndex", "order_index");
        }

        [Test]
        public void QuizItemOptionDto_HasSnakeCaseJsonProperties()
        {
            AssertDtoHasJsonProperty<QuizItemOptionDto>("Key", "key");
            AssertDtoHasJsonProperty<QuizItemOptionDto>("Label", "label");
            AssertDtoHasJsonProperty<QuizItemOptionDto>("Description", "description");
        }

        [Test]
        public void QuizAttemptRequestDto_HasSnakeCaseJsonProperties()
        {
            AssertDtoHasJsonProperty<QuizAttemptRequestDto>("ClientAttemptUuid", "client_attempt_uuid");
            AssertDtoHasJsonProperty<QuizAttemptRequestDto>("Answers", "answers");
        }

        [Test]
        public void QuizAttemptResponseDto_HasSnakeCaseJsonProperties()
        {
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("AttemptId", "attempt_id");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("ClientAttemptUuid", "client_attempt_uuid");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("QuizId", "quiz_id");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("Status", "status");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("Score", "score");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("TotalPossible", "total_possible");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("Percentage", "percentage");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("Passed", "passed");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("IsReplay", "is_replay");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("AnswersFeedback", "answers_feedback");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("ProgressUpdated", "progress_updated");
            AssertDtoHasJsonProperty<QuizAttemptResponseDto>("ProgressRevision", "progress_revision");
        }

        [Test]
        public void SyncStatusDto_HasSnakeCaseJsonProperties()
        {
            AssertDtoHasJsonProperty<SyncStatusDto>("StudentProgressRevision", "student_progress_revision");
            AssertDtoHasJsonProperty<SyncStatusDto>("StudentSettingsRevision", "student_settings_revision");
            AssertDtoHasJsonProperty<SyncStatusDto>("QuizRevision", "quiz_revision");
            AssertDtoHasJsonProperty<SyncStatusDto>("PublishedContentRevision", "published_content_revision");
            AssertDtoHasJsonProperty<SyncStatusDto>("RewardWalletRevision", "reward_wallet_revision");
            AssertDtoHasJsonProperty<SyncStatusDto>("ServerTime", "server_time");
            AssertDtoHasJsonProperty<SyncStatusDto>("NextPollAfterSeconds", "next_poll_after_seconds");
        }

        [Test]
        public void DataProviderError_HasSnakeCaseJsonProperties()
        {
            AssertDtoHasJsonProperty<DataProviderError>("Code", "code");
            AssertDtoHasJsonProperty<DataProviderError>("Message", "message");
            AssertDtoHasJsonProperty<DataProviderError>("Action", "action");
            AssertDtoHasJsonProperty<DataProviderError>("Retryable", "retryable");
        }

        // ──────────────────────────────────────────────────────────────
        //  Default constructors return clean null baselines
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void LoginResponseDto_OptionalFieldsAreNullByDefault()
        {
            var dto = new LoginResponseDto();
            Assert.That(dto.Token, Is.Null);
            Assert.That(dto.TokenType, Is.Null);
            Assert.That(dto.Student, Is.Null);
        }

        [Test]
        public void QuizAttemptRequestDto_OptionalFieldsAreNullByDefault()
        {
            var dto = new QuizAttemptRequestDto();
            Assert.That(dto.ClientAttemptUuid, Is.Null);
            Assert.That(dto.Answers, Is.Not.Null);
            Assert.That(dto.Answers, Is.Empty);
        }

        [Test]
        public void SyncStatusDto_OptionalFieldsAreNullByDefault()
        {
            var dto = new SyncStatusDto();
            Assert.That(dto.StudentProgressRevision, Is.Null);
            Assert.That(dto.StudentSettingsRevision, Is.Null);
            Assert.That(dto.QuizRevision, Is.Null);
            Assert.That(dto.PublishedContentRevision, Is.Null);
            Assert.That(dto.RewardWalletRevision, Is.Null);
            Assert.That(dto.ServerTime, Is.Null);
            Assert.That(dto.NextPollAfterSeconds, Is.Null);
        }

        // ──────────────────────────────────────────────────────────────
        //  Assertion Helpers
        // ──────────────────────────────────────────────────────────────

        private static void AssertDtoHasJsonProperty<T>(string propertyName, string expectedJsonName)
        {
            Type type = typeof(T);
            PropertyInfo prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null,
                "DTO '{0}' must contain a public property named '{1}'.", type.Name, propertyName);

            var jsonAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
            Assert.That(jsonAttr, Is.Not.Null,
                "Property '{0}.{1}' must be decorated with a [JsonProperty] attribute.", type.Name, propertyName);

            Assert.That(jsonAttr.PropertyName, Is.EqualTo(expectedJsonName),
                "{0}.{1} [JsonProperty] must map to '{2}' but was '{3}'",
                type.Name, propertyName, expectedJsonName, jsonAttr.PropertyName);
        }
    }
}