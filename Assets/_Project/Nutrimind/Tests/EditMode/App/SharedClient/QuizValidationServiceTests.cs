using System.Collections.Generic;
using NUnit.Framework;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class QuizValidationServiceTests
    {
        private QuizValidationService _service;
        private QuizAnswerDraftStore _draftStore;

        [SetUp]
        public void Setup()
        {
            _service = new QuizValidationService();
            _draftStore = new QuizAnswerDraftStore();
        }

        [Test]
        public void ValidateDraft_NullQuizDetail_ReturnsFalse()
        {
            bool result = _service.ValidateDraft(null, _draftStore, out var unanswered);
            Assert.That(result, Is.False);
            Assert.That(unanswered, Is.Not.Null);
            Assert.That(unanswered, Is.Empty);
        }

        [Test]
        public void ValidateDraft_NullItemsList_ReturnsTrue()
        {
            var quiz = new QuizDetailDto { Items = null };
            bool result = _service.ValidateDraft(quiz, _draftStore, out var unanswered);
            Assert.That(result, Is.True);
            Assert.That(unanswered, Is.Not.Null);
            Assert.That(unanswered, Is.Empty);
        }

        [Test]
        public void ValidateDraft_EmptyItemsList_ReturnsTrue()
        {
            var quiz = new QuizDetailDto { Items = new List<QuizItemDto>() };
            bool result = _service.ValidateDraft(quiz, _draftStore, out var unanswered);
            Assert.That(result, Is.True);
            Assert.That(unanswered, Is.Not.Null);
            Assert.That(unanswered, Is.Empty);
        }

        [Test]
        public void ValidateDraft_NullOrEmptyItemIds_AreSkipped()
        {
            var quiz = new QuizDetailDto
            {
                Items = new List<QuizItemDto>
                {
                    null,
                    new QuizItemDto { Id = null },
                    new QuizItemDto { Id = "" }
                }
            };
            bool result = _service.ValidateDraft(quiz, _draftStore, out var unanswered);
            Assert.That(result, Is.True);
            Assert.That(unanswered, Is.Not.Null);
            Assert.That(unanswered, Is.Empty);
        }

        [Test]
        public void ValidateDraft_UnansweredItems_ReturnsFalseAndListsIds()
        {
            var quiz = new QuizDetailDto
            {
                Items = new List<QuizItemDto>
                {
                    new QuizItemDto { Id = "q1" },
                    new QuizItemDto { Id = "q2" },
                    new QuizItemDto { Id = "q3" }
                }
            };

            // None of the items are answered in the draft store yet.
            bool result = _service.ValidateDraft(quiz, _draftStore, out var unanswered);
            Assert.That(result, Is.False);
            Assert.That(unanswered, Is.EquivalentTo(new[] { "q1", "q2", "q3" }));
        }

        [Test]
        public void ValidateDraft_InvalidAnswers_AreConsideredUnanswered()
        {
            var quiz = new QuizDetailDto
            {
                Items = new List<QuizItemDto>
                {
                    new QuizItemDto { Id = "q1" }, // Null answer
                    new QuizItemDto { Id = "q2" }, // Empty string
                    new QuizItemDto { Id = "q3" }, // Whitespace string
                    new QuizItemDto { Id = "q4" }  // Empty collection
                }
            };

            _draftStore.SetAnswer("q1", null);
            _draftStore.SetAnswer("q2", "");
            _draftStore.SetAnswer("q3", "   ");
            _draftStore.SetAnswer("q4", new List<string>());

            bool result = _service.ValidateDraft(quiz, _draftStore, out var unanswered);
            Assert.That(result, Is.False);
            Assert.That(unanswered, Is.EquivalentTo(new[] { "q1", "q2", "q3", "q4" }));
        }

        [Test]
        public void ValidateDraft_AllValidAnswers_ReturnsTrue()
        {
            var quiz = new QuizDetailDto
            {
                Items = new List<QuizItemDto>
                {
                    new QuizItemDto { Id = "q1" }, // String
                    new QuizItemDto { Id = "q2" }, // Collection with items
                    new QuizItemDto { Id = "q3" }, // Integer/Other object
                    new QuizItemDto { Id = "q4" }  // Boolean
                }
            };

            _draftStore.SetAnswer("q1", "A valid answer");
            _draftStore.SetAnswer("q2", new List<int> { 1, 2 });
            _draftStore.SetAnswer("q3", 42);
            _draftStore.SetAnswer("q4", true);

            bool result = _service.ValidateDraft(quiz, _draftStore, out var unanswered);
            Assert.That(result, Is.True);
            Assert.That(unanswered, Is.Empty);
        }

        [Test]
        public void ValidateDraft_NullDraftStore_TreatsAllAsUnanswered()
        {
            var quiz = new QuizDetailDto
            {
                Items = new List<QuizItemDto>
                {
                    new QuizItemDto { Id = "q1" }
                }
            };

            bool result = _service.ValidateDraft(quiz, null, out var unanswered);
            Assert.That(result, Is.False);
            Assert.That(unanswered, Is.EquivalentTo(new[] { "q1" }));
        }
    }
}
