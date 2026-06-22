using NUnit.Framework;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class QuizAnswerDraftStoreTests
    {
        [Test]
        public void DefaultState_IsEmpty()
        {
            var store = new QuizAnswerDraftStore();
            Assert.That(store.Answers, Is.Not.Null);
            Assert.That(store.Answers, Is.Empty);
        }

        [Test]
        public void SetAndGetAnswer_WorksCorrectly()
        {
            var store = new QuizAnswerDraftStore();
            store.SetAnswer("item_1", "Answer A");
            store.SetAnswer("item_2", 42);

            Assert.That(store.HasAnswer("item_1"), Is.True);
            Assert.That(store.HasAnswer("item_2"), Is.True);
            Assert.That(store.HasAnswer("item_3"), Is.False);

            Assert.That(store.GetAnswer("item_1"), Is.EqualTo("Answer A"));
            Assert.That(store.GetAnswer("item_2"), Is.EqualTo(42));
            Assert.That(store.GetAnswer("item_3"), Is.Null);
        }

        [Test]
        public void RemoveAnswer_RemovesCorrectly()
        {
            var store = new QuizAnswerDraftStore();
            store.SetAnswer("item_1", "Answer A");

            Assert.That(store.HasAnswer("item_1"), Is.True);

            bool removed = store.RemoveAnswer("item_1");
            Assert.That(removed, Is.True);
            Assert.That(store.HasAnswer("item_1"), Is.False);
            Assert.That(store.GetAnswer("item_1"), Is.Null);

            bool removedAgain = store.RemoveAnswer("item_1");
            Assert.That(removedAgain, Is.False);
        }

        [Test]
        public void Reset_ClearsAllAnswers()
        {
            var store = new QuizAnswerDraftStore();
            store.SetAnswer("item_1", "Answer A");
            store.SetAnswer("item_2", "Answer B");

            Assert.That(store.Answers, Has.Count.EqualTo(2));

            store.Reset();
            Assert.That(store.Answers, Is.Empty);
            Assert.That(store.HasAnswer("item_1"), Is.False);
            Assert.That(store.GetAnswer("item_1"), Is.Null);
        }
    }
}
