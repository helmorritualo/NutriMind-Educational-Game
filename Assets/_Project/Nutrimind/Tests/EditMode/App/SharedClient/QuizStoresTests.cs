using System.Collections.Generic;
using NUnit.Framework;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class QuizStoresTests
    {
        [Test]
        public void QuizAvailabilityStore_DefaultAndReset()
        {
            var store = new QuizAvailabilityStore();
            Assert.That(store.Quizzes, Is.Not.Null);
            Assert.That(store.Quizzes, Is.Empty);

            store.Quizzes.Add(new QuizDto { Id = "q1", Title = "Quiz 1" });
            Assert.That(store.Quizzes, Has.Count.EqualTo(1));

            store.Reset();
            Assert.That(store.Quizzes, Is.Empty);
        }

        [Test]
        public void QuizDetailStore_DefaultAndReset()
        {
            var store = new QuizDetailStore();
            Assert.That(store.CurrentQuiz, Is.Null);

            var detail = new QuizDetailDto { Id = "q1", Title = "Quiz 1", Instructions = "Read carefully" };
            store.CurrentQuiz = detail;
            Assert.That(store.CurrentQuiz, Is.SameAs(detail));

            store.Reset();
            Assert.That(store.CurrentQuiz, Is.Null);
        }

        [Test]
        public void QuizSessionStore_DefaultAndReset()
        {
            var store = new QuizSessionStore();
            Assert.That(store.ActiveQuizId, Is.Null);
            Assert.That(store.CurrentQuestionIndex, Is.EqualTo(0));
            Assert.That(store.IsActive, Is.False);
            Assert.That(store.IsSessionActive, Is.False);

            store.ActiveQuizId = "quiz_123";
            store.CurrentQuestionIndex = 3;
            store.IsActive = true;

            Assert.That(store.ActiveQuizId, Is.EqualTo("quiz_123"));
            Assert.That(store.CurrentQuestionIndex, Is.EqualTo(3));
            Assert.That(store.IsActive, Is.True);
            Assert.That(store.IsSessionActive, Is.True);

            store.IsSessionActive = false;
            Assert.That(store.IsActive, Is.False);

            store.Reset();
            Assert.That(store.ActiveQuizId, Is.Null);
            Assert.That(store.CurrentQuestionIndex, Is.EqualTo(0));
            Assert.That(store.IsActive, Is.False);
        }

        [Test]
        public void QuizResultStore_DefaultAndReset()
        {
            var store = new QuizResultStore();
            Assert.That(store.LastAttemptResult, Is.Null);
            Assert.That(store.Results, Is.Not.Null);
            Assert.That(store.Results, Is.Empty);

            var lastResult = new QuizAttemptResponseDto { AttemptId = "att_1", QuizId = "q1" };
            store.LastAttemptResult = lastResult;

            var historicalResult = new QuizResultDto { AttemptId = "att_0", QuizId = "q1" };
            store.Results.Add(historicalResult);

            Assert.That(store.LastAttemptResult, Is.SameAs(lastResult));
            Assert.That(store.Results, Has.Count.EqualTo(1));
            Assert.That(store.Results[0], Is.SameAs(historicalResult));

            store.Reset();
            Assert.That(store.LastAttemptResult, Is.Null);
            Assert.That(store.Results, Is.Empty);
        }
    }
}
