using System;
using System.Collections.Generic;
using NUnit.Framework;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class QuizItemPresenterRegistryTests
    {
        private QuizItemPresenterRegistry _registry;

        private class FakePresenter : IQuizItemPresenter
        {
            public QuizItemDto Item { get; private set; }
            public QuizAnswerDraftStore DraftStore { get; private set; }

            public void Bind(QuizItemDto item, QuizAnswerDraftStore draftStore)
            {
                Item = item;
                DraftStore = draftStore;
            }
        }

        [SetUp]
        public void Setup()
        {
            _registry = new QuizItemPresenterRegistry();
        }

        [Test]
        public void Register_NullOrEmptyItemType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _registry.Register(null, () => new FakePresenter()));
            Assert.Throws<ArgumentException>(() => _registry.Register("", () => new FakePresenter()));
        }

        [Test]
        public void Register_NullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _registry.Register("TypeA", null));
        }

        [Test]
        public void RegisterAndHasPresenter_WorksCaseInsensitively()
        {
            Assert.That(_registry.HasPresenter("MultipleChoice"), Is.False);

            _registry.Register("MultipleChoice", () => new FakePresenter());

            Assert.That(_registry.HasPresenter("MultipleChoice"), Is.True);
            Assert.That(_registry.HasPresenter("multiplechoice"), Is.True);
            Assert.That(_registry.HasPresenter("MULTIPLECHOICE"), Is.True);
            Assert.That(_registry.HasPresenter("OtherType"), Is.False);
            Assert.That(_registry.HasPresenter(null), Is.False);
            Assert.That(_registry.HasPresenter(""), Is.False);
        }

        [Test]
        public void CreatePresenter_NullOrEmptyItemType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _registry.CreatePresenter(null));
            Assert.Throws<ArgumentException>(() => _registry.CreatePresenter(""));
        }

        [Test]
        public void CreatePresenter_UnregisteredType_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => _registry.CreatePresenter("UnregisteredType"));
        }

        [Test]
        public void CreatePresenter_RegisteredType_ReturnsNewInstanceViaFactory()
        {
            int factoryCalls = 0;
            _registry.Register("TypeA", () =>
            {
                factoryCalls++;
                return new FakePresenter();
            });

            var p1 = _registry.CreatePresenter("TypeA");
            var p2 = _registry.CreatePresenter("typea"); // Case-insensitivity check

            Assert.That(p1, Is.Not.Null);
            Assert.That(p2, Is.Not.Null);
            Assert.That(p1, Is.Not.SameAs(p2)); // Each call should generate a new instance
            Assert.That(factoryCalls, Is.EqualTo(2));
        }

        [Test]
        public void Unregister_NullOrEmpty_ReturnsFalse()
        {
            Assert.That(_registry.Unregister(null), Is.False);
            Assert.That(_registry.Unregister(""), Is.False);
        }

        [Test]
        public void Unregister_RegisteredType_RemovesAndReturnsTrue()
        {
            _registry.Register("TypeA", () => new FakePresenter());
            Assert.That(_registry.HasPresenter("TypeA"), Is.True);

            bool removed = _registry.Unregister("typea"); // Case-insensitivity check
            Assert.That(removed, Is.True);
            Assert.That(_registry.HasPresenter("TypeA"), Is.False);

            bool removedAgain = _registry.Unregister("TypeA");
            Assert.That(removedAgain, Is.False);
        }

        [Test]
        public void Clear_RemovesAllFactories()
        {
            _registry.Register("TypeA", () => new FakePresenter());
            _registry.Register("TypeB", () => new FakePresenter());

            Assert.That(_registry.HasPresenter("TypeA"), Is.True);
            Assert.That(_registry.HasPresenter("TypeB"), Is.True);

            _registry.Clear();

            Assert.That(_registry.HasPresenter("TypeA"), Is.False);
            Assert.That(_registry.HasPresenter("TypeB"), Is.False);
        }

        [Test]
        public void PresenterBind_SavesReferencesCorrectly()
        {
            var presenter = new FakePresenter();
            var item = new QuizItemDto { Id = "item_1", Type = "TypeA" };
            var draftStore = new QuizAnswerDraftStore();

            presenter.Bind(item, draftStore);

            Assert.That(presenter.Item, Is.SameAs(item));
            Assert.That(presenter.DraftStore, Is.SameAs(draftStore));
        }
    }
}
