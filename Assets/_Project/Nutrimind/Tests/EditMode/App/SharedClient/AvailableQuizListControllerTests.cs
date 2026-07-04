using System.Collections.Generic;
using NUnit.Framework;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class AvailableQuizListControllerTests
    {
        [Test]
        public void FilterQuizzes_All_ReturnsEveryQuiz()
        {
            var quizzes = new List<QuizDto>
            {
                new() { Id = "1", SubjectSlug = "literaquest" },
                new() { Id = "2", SubjectSlug = "healthquest" },
                new() { Id = "3", SubjectSlug = "sciencequest" }
            };

            var filtered = AvailableQuizListController.FilterQuizzes(quizzes, QuizSubjectFilters.All);
            Assert.That(filtered, Has.Count.EqualTo(3));
        }

        [Test]
        public void FilterQuizzes_LiteraQuest_ReturnsOnlyLiteraQuest()
        {
            var quizzes = new List<QuizDto>
            {
                new() { Id = "1", SubjectSlug = "literaquest" },
                new() { Id = "2", SubjectSlug = "healthquest" }
            };

            var filtered = AvailableQuizListController.FilterQuizzes(quizzes, QuizSubjectFilters.LiteraQuest);
            Assert.That(filtered, Has.Count.EqualTo(1));
            Assert.That(filtered[0].Id, Is.EqualTo("1"));
        }

        [Test]
        public void QuizListActionResolver_Pagination_FivePerPage()
        {
            Assert.That(QuizListActionResolver.GetTotalPages(0), Is.EqualTo(1));
            Assert.That(QuizListActionResolver.GetTotalPages(1), Is.EqualTo(1));
            Assert.That(QuizListActionResolver.GetTotalPages(5), Is.EqualTo(1));
            Assert.That(QuizListActionResolver.GetTotalPages(6), Is.EqualTo(2));

            var slice = QuizListActionResolver.GetPageSlice(2, 6);
            Assert.That(slice.startIndex, Is.EqualTo(5));
            Assert.That(slice.endIndexExclusive, Is.EqualTo(6));
            Assert.That(QuizListActionResolver.FormatPageSummary(2, 6), Is.EqualTo("Showing 6-6 of 6 quizzes"));
        }

        [Test]
        public void QuizListActionResolver_MapsRowActions()
        {
            Assert.That(QuizListActionResolver.Resolve(new QuizDto { State = "unlocked", IsAvailable = true }),
                Is.EqualTo(QuizListRowActionKind.Start));
            Assert.That(QuizListActionResolver.Resolve(new QuizDto { State = "locked" }),
                Is.EqualTo(QuizListRowActionKind.View));
            Assert.That(QuizListActionResolver.Resolve(new QuizDto { State = "completed" }),
                Is.EqualTo(QuizListRowActionKind.ViewResult));
            Assert.That(QuizListActionResolver.Resolve(new QuizDto { State = "unlocked", IsAvailable = false }),
                Is.EqualTo(QuizListRowActionKind.View));
        }

        [Test]
        public void QuizAvailabilityStore_PreservesListUiStateUntilReset()
        {
            var store = new QuizAvailabilityStore
            {
                SelectedSubjectFilter = QuizSubjectFilters.Science,
                ScrollPosition = 0.42f,
                CurrentPage = 3,
                SelectedQuizId = "literaquest-t1-q1",
                IsLoaded = true
            };

            Assert.That(store.SelectedSubjectFilter, Is.EqualTo(QuizSubjectFilters.Science));
            Assert.That(store.ScrollPosition, Is.EqualTo(0.42f).Within(0.001f));
            Assert.That(store.CurrentPage, Is.EqualTo(3));
            Assert.That(store.SelectedQuizId, Is.EqualTo("literaquest-t1-q1"));

            store.Reset();
            Assert.That(store.SelectedSubjectFilter, Is.EqualTo(QuizSubjectFilters.All));
            Assert.That(store.ScrollPosition, Is.EqualTo(1f).Within(0.001f));
            Assert.That(store.CurrentPage, Is.EqualTo(1));
            Assert.That(store.SelectedQuizId, Is.Null);
            Assert.That(store.IsLoaded, Is.False);
        }

        [Test]
        public void ShouldShowEmptyState_True_WhenLoadedAndEmptyAndNoError()
            => Assert.That(AvailableQuizListController.ShouldShowEmptyState(0, true, false), Is.True);

        [Test]
        public void ShouldShowEmptyState_False_WhenNotLoaded()
            => Assert.That(AvailableQuizListController.ShouldShowEmptyState(0, false, false), Is.False);

        [Test]
        public void ShouldShowEmptyState_False_WhenNotEmpty()
            => Assert.That(AvailableQuizListController.ShouldShowEmptyState(3, true, false), Is.False);

        [Test]
        public void ShouldShowEmptyState_False_WhenLoadErrorPresent()
            => Assert.That(AvailableQuizListController.ShouldShowEmptyState(0, true, true), Is.False);
    }
}
