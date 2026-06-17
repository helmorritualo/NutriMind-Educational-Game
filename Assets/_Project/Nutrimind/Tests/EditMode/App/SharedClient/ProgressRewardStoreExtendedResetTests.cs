using NUnit.Framework;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    /// <summary>
    /// RED-phase tests verifying that ProgressRewardStore extended with
    /// typed restoration state and reward previews resets correctly.
    /// </summary>
    [TestFixture]
    public class ProgressRewardStoreExtendedResetTests
    {
        [Test]
        public void RestorationState_DefaultsToNull()
        {
            var store = new ProgressRewardStore();
            Assert.That(store.RestorationState, Is.Null,
                "RestorationState must default to null");
        }

        [Test]
        public void RewardPreviews_DefaultsToEmpty()
        {
            var store = new ProgressRewardStore();
            Assert.That(store.RewardPreviews, Is.Not.Null,
                "RewardPreviews must default to non-null list");
            Assert.That(store.RewardPreviews.Count, Is.EqualTo(0),
                "RewardPreviews must default to empty list");
        }

        [Test]
        public void Reset_ClearsRestorationState()
        {
            var store = new ProgressRewardStore();
            store.RestorationState = new WorldRestorationState();
            store.Reset();
            Assert.That(store.RestorationState, Is.Null,
                "Reset() must clear RestorationState");
        }

        [Test]
        public void Reset_ClearsRewardPreviews()
        {
            var store = new ProgressRewardStore();
            store.RewardPreviews.Add(new RewardPreview());
            store.Reset();
            Assert.That(store.RewardPreviews.Count, Is.EqualTo(0),
                "Reset() must clear RewardPreviews list");
        }

        [Test]
        public void Reset_StillClearsProgressData()
        {
            var store = new ProgressRewardStore();
            store.ProgressData = new object();
            store.Reset();
            Assert.That(store.ProgressData, Is.Null,
                "Reset() must still null ProgressData (backward compat)");
        }

        [Test]
        public void Reset_StillClearsRewardState()
        {
            var store = new ProgressRewardStore();
            store.RewardState = new object();
            store.Reset();
            Assert.That(store.RewardState, Is.Null,
                "Reset() must still null RewardState (backward compat)");
        }

        [Test]
        public void RewardPreviews_Mutable()
        {
            var store = new ProgressRewardStore();
            store.RewardPreviews.Add(new RewardPreview { RewardKey = "test_key" });
            Assert.That(store.RewardPreviews.Count, Is.EqualTo(1));

            store.RewardPreviews.Clear();
            Assert.That(store.RewardPreviews.Count, Is.EqualTo(0));
        }
    }
}
