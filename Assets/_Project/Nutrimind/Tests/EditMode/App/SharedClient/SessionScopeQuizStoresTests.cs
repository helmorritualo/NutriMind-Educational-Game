using NUnit.Framework;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class SessionScopeQuizStoresTests
    {
        [Test]
        public void SessionScope_HasQuizStoresProperties()
        {
            var session = new SessionScope();

            // Verify they are declared on the SessionScope class and initially null/default
            Assert.That(session.QuizAvailabilityStore, Is.Null);
            Assert.That(session.QuizDetailStore, Is.Null);
            Assert.That(session.QuizSessionStore, Is.Null);
            Assert.That(session.QuizAnswerDraftStore, Is.Null);
            Assert.That(session.QuizResultStore, Is.Null);
        }

        [Test]
        public void SessionScope_CanAssignAndRetrieveQuizStores()
        {
            var session = new SessionScope();

            var availabilityStore = new QuizAvailabilityStore();
            var detailStore = new QuizDetailStore();
            var sessionStore = new QuizSessionStore();
            var draftStore = new QuizAnswerDraftStore();
            var resultStore = new QuizResultStore();

            session.QuizAvailabilityStore = availabilityStore;
            session.QuizDetailStore = detailStore;
            session.QuizSessionStore = sessionStore;
            session.QuizAnswerDraftStore = draftStore;
            session.QuizResultStore = resultStore;

            Assert.That(session.QuizAvailabilityStore, Is.SameAs(availabilityStore));
            Assert.That(session.QuizDetailStore, Is.SameAs(detailStore));
            Assert.That(session.QuizSessionStore, Is.SameAs(sessionStore));
            Assert.That(session.QuizAnswerDraftStore, Is.SameAs(draftStore));
            Assert.That(session.QuizResultStore, Is.SameAs(resultStore));
        }

        [Test]
        public void SessionScope_Clear_ResetsAllQuizStoresToNull()
        {
            var session = new SessionScope();

            session.QuizAvailabilityStore = new QuizAvailabilityStore();
            session.QuizDetailStore = new QuizDetailStore();
            session.QuizSessionStore = new QuizSessionStore();
            session.QuizAnswerDraftStore = new QuizAnswerDraftStore();
            session.QuizResultStore = new QuizResultStore();

            // Verify they are not null before Clear
            Assert.That(session.QuizAvailabilityStore, Is.Not.Null);
            Assert.That(session.QuizDetailStore, Is.Not.Null);
            Assert.That(session.QuizSessionStore, Is.Not.Null);
            Assert.That(session.QuizAnswerDraftStore, Is.Not.Null);
            Assert.That(session.QuizResultStore, Is.Not.Null);

            // Act
            session.Clear();

            // Assert
            Assert.That(session.QuizAvailabilityStore, Is.Null, "QuizAvailabilityStore should be reset to null after Clear().");
            Assert.That(session.QuizDetailStore, Is.Null, "QuizDetailStore should be reset to null after Clear().");
            Assert.That(session.QuizSessionStore, Is.Null, "QuizSessionStore should be reset to null after Clear().");
            Assert.That(session.QuizAnswerDraftStore, Is.Null, "QuizAnswerDraftStore should be reset to null after Clear().");
            Assert.That(session.QuizResultStore, Is.Null, "QuizResultStore should be reset to null after Clear().");
        }
    }
}
