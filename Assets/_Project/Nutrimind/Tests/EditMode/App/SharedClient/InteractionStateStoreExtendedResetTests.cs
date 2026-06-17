using NUnit.Framework;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    /// <summary>
    /// RED-phase tests verifying that InteractionStateStore extended with
    /// typed learning/narrative/feedback properties resets correctly.
    /// </summary>
    [TestFixture]
    public class InteractionStateStoreExtendedResetTests
    {
        [Test]
        public void CurrentStationNarrative_DefaultsToNull()
        {
            var store = new InteractionStateStore();
            Assert.That(store.CurrentStationNarrative, Is.Null,
                "CurrentStationNarrative must default to null");
        }

        [Test]
        public void CurrentPhase_DefaultsToDiscover()
        {
            var store = new InteractionStateStore();
            Assert.That(store.CurrentPhase, Is.EqualTo(LearningCyclePhase.Discover),
                "CurrentPhase must default to Discover");
        }

        [Test]
        public void LastFeedback_DefaultsToNull()
        {
            var store = new InteractionStateStore();
            Assert.That(store.LastFeedback, Is.Null,
                "LastFeedback must default to null");
        }

        [Test]
        public void Reset_ClearsCurrentStationNarrative()
        {
            var store = new InteractionStateStore();
            store.CurrentStationNarrative = new StationNarrative();
            store.Reset();
            Assert.That(store.CurrentStationNarrative, Is.Null,
                "Reset() must clear CurrentStationNarrative");
        }

        [Test]
        public void Reset_ResetsCurrentPhaseToDiscover()
        {
            var store = new InteractionStateStore();
            store.CurrentPhase = LearningCyclePhase.Review;
            store.Reset();
            Assert.That(store.CurrentPhase, Is.EqualTo(LearningCyclePhase.Discover),
                "Reset() must reset CurrentPhase to Discover");
        }

        [Test]
        public void Reset_ClearsLastFeedback()
        {
            var store = new InteractionStateStore();
            store.LastFeedback = new MistakeFeedback();
            store.Reset();
            Assert.That(store.LastFeedback, Is.Null,
                "Reset() must clear LastFeedback");
        }

        [Test]
        public void Reset_StillClearsCurrentPrompt()
        {
            var store = new InteractionStateStore();
            store.CurrentPrompt = "some prompt";
            store.Reset();
            Assert.That(store.CurrentPrompt, Is.Null,
                "Reset() must still null CurrentPrompt (backward compat)");
        }

        [Test]
        public void Reset_StillClearsContextData()
        {
            var store = new InteractionStateStore();
            store.ContextData = new object();
            store.Reset();
            Assert.That(store.ContextData, Is.Null,
                "Reset() must still null ContextData (backward compat)");
        }
    }
}
