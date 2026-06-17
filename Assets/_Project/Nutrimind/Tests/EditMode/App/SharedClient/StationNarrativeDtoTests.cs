using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class StationNarrativeDtoTests
    {
        private const string AssemblyName = "NutriMind.Runtime.App";

        private static Type FindType(string fullTypeName)
            => Type.GetType(fullTypeName + ", " + AssemblyName);

        [Test]
        public void StationNarrative_TypeExists()
        {
            Type t = FindType("NutriMind.Runtime.App.StationNarrative");
            Assert.That(t, Is.Not.Null);
        }

        [Test]
        public void StationNarrative_HasExpectedStringProperties()
        {
            Type t = FindType("NutriMind.Runtime.App.StationNarrative");
            Assert.That(t, Is.Not.Null);
            AssertHasNullableStringProperty(t, "StoryContext");
            AssertHasNullableStringProperty(t, "MissionTitle");
            AssertHasNullableStringProperty(t, "MissionSummary");
            AssertHasNullableStringProperty(t, "ReflectionPrompt");
        }

        [Test]
        public void StationNarrative_HasNpcGuidesList()
        {
            Type t = FindType("NutriMind.Runtime.App.StationNarrative");
            Assert.That(t, Is.Not.Null);
            PropertyInfo prop = t.GetProperty("NpcGuides", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null);
            Type expectedListType = typeof(List<>).MakeGenericType(FindType("NutriMind.Runtime.App.NpcGuide"));
            Assert.That(prop.PropertyType, Is.EqualTo(expectedListType));
            object instance = Activator.CreateInstance(t);
            Assert.That(prop.GetValue(instance), Is.Not.Null, "NpcGuides must default to empty list");
        }

        [Test]
        public void StationNarrative_HasDiscoveriesList()
        {
            Type t = FindType("NutriMind.Runtime.App.StationNarrative");
            Assert.That(t, Is.Not.Null);
            PropertyInfo prop = t.GetProperty("Discoveries", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null);
            Type expectedListType = typeof(List<>).MakeGenericType(FindType("NutriMind.Runtime.App.DiscoveryEntry"));
            Assert.That(prop.PropertyType, Is.EqualTo(expectedListType));
            object instance = Activator.CreateInstance(t);
            Assert.That(prop.GetValue(instance), Is.Not.Null, "Discoveries must default to empty list");
        }

        [Test]
        public void StationNarrative_HasRewardPreviewsList()
        {
            Type t = FindType("NutriMind.Runtime.App.StationNarrative");
            Assert.That(t, Is.Not.Null);
            PropertyInfo prop = t.GetProperty("RewardPreviews", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null);
            Type expectedListType = typeof(List<>).MakeGenericType(FindType("NutriMind.Runtime.App.RewardPreview"));
            Assert.That(prop.PropertyType, Is.EqualTo(expectedListType));
            object instance = Activator.CreateInstance(t);
            Assert.That(prop.GetValue(instance), Is.Not.Null, "RewardPreviews must default to empty list");
        }

        [Test]
        public void StationNarrative_HasLearningCycleList()
        {
            Type t = FindType("NutriMind.Runtime.App.StationNarrative");
            Assert.That(t, Is.Not.Null);
            PropertyInfo prop = t.GetProperty("LearningCycle", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop.PropertyType, Is.EqualTo(typeof(List<LearningCyclePhase>)));
            object instance = Activator.CreateInstance(t);
            Assert.That(prop.GetValue(instance), Is.Not.Null, "LearningCycle must default to empty list");
        }

        [Test]
        public void StationNarrative_HasCurrentPhaseDefaultDiscover()
        {
            Type t = FindType("NutriMind.Runtime.App.StationNarrative");
            Assert.That(t, Is.Not.Null);
            PropertyInfo prop = t.GetProperty("CurrentPhase", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop.PropertyType, Is.EqualTo(typeof(LearningCyclePhase)));
            object instance = Activator.CreateInstance(t);
            Assert.That((LearningCyclePhase)prop.GetValue(instance), Is.EqualTo(LearningCyclePhase.Discover));
        }

        [Test]
        public void StationNarrative_HasHintPolicy()
        {
            Type t = FindType("NutriMind.Runtime.App.StationNarrative");
            Assert.That(t, Is.Not.Null);
            Type hintPolicyType = FindType("NutriMind.Runtime.App.HintPolicy");
            Assume.That(hintPolicyType, Is.Not.Null);
            PropertyInfo prop = t.GetProperty("HintPolicy", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop.PropertyType, Is.EqualTo(hintPolicyType));
            object instance = Activator.CreateInstance(t);
            Assert.That(prop.GetValue(instance), Is.Null);
        }

        [Test]
        public void StationNarrative_HasWorldRestorationState()
        {
            Type t = FindType("NutriMind.Runtime.App.StationNarrative");
            Assert.That(t, Is.Not.Null);
            Type rtype = FindType("NutriMind.Runtime.App.WorldRestorationState");
            Assume.That(rtype, Is.Not.Null);
            PropertyInfo prop = t.GetProperty("WorldRestorationState", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null);
            object instance = Activator.CreateInstance(t);
            Assert.That(prop.GetValue(instance), Is.Null);
        }

        [Test]
        public void StationNarrative_HasSuccessAndMistakeFeedback()
        {
            Type t = FindType("NutriMind.Runtime.App.StationNarrative");
            Assert.That(t, Is.Not.Null);
            Type st = FindType("NutriMind.Runtime.App.SuccessFeedback");
            Type mt = FindType("NutriMind.Runtime.App.MistakeFeedback");
            Assume.That(st, Is.Not.Null);
            Assume.That(mt, Is.Not.Null);
            Assert.That(t.GetProperty("SuccessFeedback", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(t.GetProperty("MistakeFeedback", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            object instance = Activator.CreateInstance(t);
            Assert.That(t.GetProperty("SuccessFeedback").GetValue(instance), Is.Null);
            Assert.That(t.GetProperty("MistakeFeedback").GetValue(instance), Is.Null);
        }

        [Test]
        public void StationNarrative_LearningCycleList_Mutable()
        {
            object narrative = Activator.CreateInstance(FindType("NutriMind.Runtime.App.StationNarrative"));
            var list = (List<LearningCyclePhase>)narrative.GetType().GetProperty("LearningCycle").GetValue(narrative);
            list.Add(LearningCyclePhase.Practice);
            list.Add(LearningCyclePhase.Apply);
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list, Does.Contain(LearningCyclePhase.Practice));
            Assert.That(list, Does.Contain(LearningCyclePhase.Apply));
            list.Clear();
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [Test]
        public void StationNarrative_Collections_Mutable()
        {
            object narrative = Activator.CreateInstance(FindType("NutriMind.Runtime.App.StationNarrative"));
            Type npcType = FindType("NutriMind.Runtime.App.NpcGuide");
            var npcList = (System.Collections.IList)narrative.GetType().GetProperty("NpcGuides").GetValue(narrative);
            npcList.Add(Activator.CreateInstance(npcType));
            Assert.That(npcList.Count, Is.EqualTo(1));
            Type discType = FindType("NutriMind.Runtime.App.DiscoveryEntry");
            var discList = (System.Collections.IList)narrative.GetType().GetProperty("Discoveries").GetValue(narrative);
            discList.Add(Activator.CreateInstance(discType));
            Assert.That(discList.Count, Is.EqualTo(1));
            Type rewType = FindType("NutriMind.Runtime.App.RewardPreview");
            var rewList = (System.Collections.IList)narrative.GetType().GetProperty("RewardPreviews").GetValue(narrative);
            rewList.Add(Activator.CreateInstance(rewType));
            Assert.That(rewList.Count, Is.EqualTo(1));
        }

        [Test]
        public void NpcGuide_TypeExists() { Assert.That(FindType("NutriMind.Runtime.App.NpcGuide"), Is.Not.Null); }

        [Test]
        public void NpcGuide_HasNullableStringProperties()
        {
            Type t = FindType("NutriMind.Runtime.App.NpcGuide");
            Assert.That(t, Is.Not.Null);
            AssertHasNullableStringProperty(t, "GuideKey");
            AssertHasNullableStringProperty(t, "Name");
            AssertHasNullableStringProperty(t, "AvatarKey");
            AssertHasNullableStringProperty(t, "Dialogue");
        }

        [Test]
        public void NpcGuide_DefaultsAreNull()
        {
            object i = Activator.CreateInstance(FindType("NutriMind.Runtime.App.NpcGuide"));
            Assert.That(GetStringProp(i, "GuideKey"), Is.Null);
            Assert.That(GetStringProp(i, "Name"), Is.Null);
            Assert.That(GetStringProp(i, "AvatarKey"), Is.Null);
            Assert.That(GetStringProp(i, "Dialogue"), Is.Null);
        }

        [Test]
        public void HintPolicy_TypeExists() { Assert.That(FindType("NutriMind.Runtime.App.HintPolicy"), Is.Not.Null); }

        [Test]
        public void HintPolicy_HasMaxTiersAndTiersList()
        {
            Type t = FindType("NutriMind.Runtime.App.HintPolicy");
            Assert.That(t, Is.Not.Null);
            PropertyInfo maxProp = t.GetProperty("MaxTiers", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(maxProp, Is.Not.Null);
            Assert.That(maxProp.PropertyType, Is.EqualTo(typeof(int)));
            object instance = Activator.CreateInstance(t);
            Assert.That((int)maxProp.GetValue(instance), Is.EqualTo(0));
            Type ht = FindType("NutriMind.Runtime.App.HintTier");
            Assume.That(ht, Is.Not.Null);
            PropertyInfo tiersProp = t.GetProperty("Tiers", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(tiersProp, Is.Not.Null);
            Assert.That(tiersProp.PropertyType, Is.EqualTo(typeof(List<>).MakeGenericType(ht)));
            Assert.That(tiersProp.GetValue(instance), Is.Not.Null);
        }

        [Test]
        public void HintTier_TypeExists() { Assert.That(FindType("NutriMind.Runtime.App.HintTier"), Is.Not.Null); }

        [Test]
        public void HintTier_HasTierAndText()
        {
            Type t = FindType("NutriMind.Runtime.App.HintTier");
            Assert.That(t, Is.Not.Null);
            PropertyInfo tierProp = t.GetProperty("Tier", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(tierProp, Is.Not.Null);
            Assert.That(tierProp.PropertyType, Is.EqualTo(typeof(int)));
            object instance = Activator.CreateInstance(t);
            Assert.That((int)tierProp.GetValue(instance), Is.EqualTo(0));
            AssertHasNullableStringProperty(t, "Text");
            Assert.That(GetStringProp(instance, "Text"), Is.Null);
        }

        [Test]
        public void DiscoveryEntry_TypeExists() { Assert.That(FindType("NutriMind.Runtime.App.DiscoveryEntry"), Is.Not.Null); }

        [Test]
        public void DiscoveryEntry_HasNullableStringProperties()
        {
            Type t = FindType("NutriMind.Runtime.App.DiscoveryEntry");
            Assert.That(t, Is.Not.Null);
            AssertHasNullableStringProperty(t, "DiscoveryKey");
            AssertHasNullableStringProperty(t, "Title");
            AssertHasNullableStringProperty(t, "Description");
        }

        [Test]
        public void DiscoveryEntry_DefaultsAreNull()
        {
            object i = Activator.CreateInstance(FindType("NutriMind.Runtime.App.DiscoveryEntry"));
            Assert.That(GetStringProp(i, "DiscoveryKey"), Is.Null);
            Assert.That(GetStringProp(i, "Title"), Is.Null);
            Assert.That(GetStringProp(i, "Description"), Is.Null);
        }

        [Test]
        public void RewardPreview_TypeExists() { Assert.That(FindType("NutriMind.Runtime.App.RewardPreview"), Is.Not.Null); }

        [Test]
        public void RewardPreview_HasNullableStringProperties()
        {
            Type t = FindType("NutriMind.Runtime.App.RewardPreview");
            Assert.That(t, Is.Not.Null);
            AssertHasNullableStringProperty(t, "RewardKey");
            AssertHasNullableStringProperty(t, "RewardType");
            AssertHasNullableStringProperty(t, "DisplayName");
            AssertHasNullableStringProperty(t, "IconKey");
        }

        [Test]
        public void RewardPreview_DefaultsAreNull()
        {
            object i = Activator.CreateInstance(FindType("NutriMind.Runtime.App.RewardPreview"));
            Assert.That(GetStringProp(i, "RewardKey"), Is.Null);
            Assert.That(GetStringProp(i, "RewardType"), Is.Null);
            Assert.That(GetStringProp(i, "DisplayName"), Is.Null);
            Assert.That(GetStringProp(i, "IconKey"), Is.Null);
        }

        [Test]
        public void WorldRestorationState_TypeExists() { Assert.That(FindType("NutriMind.Runtime.App.WorldRestorationState"), Is.Not.Null); }

        [Test]
        public void WorldRestorationState_HasStateKeyAndStateData()
        {
            Type t = FindType("NutriMind.Runtime.App.WorldRestorationState");
            Assert.That(t, Is.Not.Null);
            AssertHasNullableStringProperty(t, "StateKey");
            PropertyInfo dp = t.GetProperty("StateData", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(dp, Is.Not.Null);
            Assert.That(dp.PropertyType, Is.EqualTo(typeof(object)));
            object instance = Activator.CreateInstance(t);
            Assert.That(GetStringProp(instance, "StateKey"), Is.Null);
            Assert.That(dp.GetValue(instance), Is.Null);
        }

        [Test]
        public void SuccessFeedback_TypeExists() { Assert.That(FindType("NutriMind.Runtime.App.SuccessFeedback"), Is.Not.Null); }

        [Test]
        public void SuccessFeedback_HasMessageAndEncouragingPhrases()
        {
            Type t = FindType("NutriMind.Runtime.App.SuccessFeedback");
            Assert.That(t, Is.Not.Null);
            AssertHasNullableStringProperty(t, "Message");
            PropertyInfo pp = t.GetProperty("EncouragingPhrases", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(pp, Is.Not.Null);
            Assert.That(pp.PropertyType, Is.EqualTo(typeof(List<string>)));
            object instance = Activator.CreateInstance(t);
            Assert.That(GetStringProp(instance, "Message"), Is.Null);
            var phrases = (List<string>)pp.GetValue(instance);
            Assert.That(phrases, Is.Not.Null);
            Assert.That(phrases.Count, Is.EqualTo(0));
        }

        [Test]
        public void MistakeFeedback_TypeExists() { Assert.That(FindType("NutriMind.Runtime.App.MistakeFeedback"), Is.Not.Null); }

        [Test]
        public void MistakeFeedback_HasAllExpectedProperties()
        {
            Type t = FindType("NutriMind.Runtime.App.MistakeFeedback");
            Assert.That(t, Is.Not.Null);
            AssertHasNullableStringProperty(t, "MisconceptionMessage");
            AssertHasNullableStringProperty(t, "EncouragingMessage");
            AssertHasNullableStringProperty(t, "RetryAction");
            PropertyInfo ht = t.GetProperty("CurrentHintTier", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(ht, Is.Not.Null);
            Assert.That(ht.PropertyType, Is.EqualTo(typeof(int?)));
            PropertyInfo ra = t.GetProperty("RemainingAttempts", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(ra, Is.Not.Null);
            Assert.That(ra.PropertyType, Is.EqualTo(typeof(int?)));
        }

        [Test]
        public void MistakeFeedback_DefaultsAreNull()
        {
            object i = Activator.CreateInstance(FindType("NutriMind.Runtime.App.MistakeFeedback"));
            Assert.That(GetStringProp(i, "MisconceptionMessage"), Is.Null);
            Assert.That(GetStringProp(i, "EncouragingMessage"), Is.Null);
            Assert.That(GetStringProp(i, "RetryAction"), Is.Null);
            Assert.That(i.GetType().GetProperty("CurrentHintTier").GetValue(i), Is.Null);
            Assert.That(i.GetType().GetProperty("RemainingAttempts").GetValue(i), Is.Null);
        }

        [Test]
        public void LearningCyclePhase_RepresentsFourPhases()
        {
            Assert.That(Enum.IsDefined(typeof(LearningCyclePhase), LearningCyclePhase.Discover), Is.True);
            Assert.That(Enum.IsDefined(typeof(LearningCyclePhase), LearningCyclePhase.Practice), Is.True);
            Assert.That(Enum.IsDefined(typeof(LearningCyclePhase), LearningCyclePhase.Apply), Is.True);
            Assert.That(Enum.IsDefined(typeof(LearningCyclePhase), LearningCyclePhase.Review), Is.True);
        }

        [Test]
        public void StationNarrative_CurrentPhase_CanSetAllPhases()
        {
            Type type = FindType("NutriMind.Runtime.App.StationNarrative");
            object narrative = Activator.CreateInstance(type);
            PropertyInfo pp = type.GetProperty("CurrentPhase");
            pp.SetValue(narrative, LearningCyclePhase.Discover);
            Assert.That((LearningCyclePhase)pp.GetValue(narrative), Is.EqualTo(LearningCyclePhase.Discover));
            pp.SetValue(narrative, LearningCyclePhase.Practice);
            Assert.That((LearningCyclePhase)pp.GetValue(narrative), Is.EqualTo(LearningCyclePhase.Practice));
            pp.SetValue(narrative, LearningCyclePhase.Apply);
            Assert.That((LearningCyclePhase)pp.GetValue(narrative), Is.EqualTo(LearningCyclePhase.Apply));
            pp.SetValue(narrative, LearningCyclePhase.Review);
            Assert.That((LearningCyclePhase)pp.GetValue(narrative), Is.EqualTo(LearningCyclePhase.Review));
        }

        private static void AssertHasNullableStringProperty(Type type, string propName)
        {
            PropertyInfo prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null, "{0} must have property '{1}'", type.Name, propName);
            Assert.That(prop.PropertyType, Is.EqualTo(typeof(string)), "{0}.{1} must be string", type.Name, propName);
            object instance = Activator.CreateInstance(type);
            Assert.That(prop.GetValue(instance), Is.Null, "{0}.{1} must default to null", type.Name, propName);
        }

        private static string GetStringProp(object instance, string propName)
        {
            PropertyInfo prop = instance.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(instance) as string;
        }
    }
}
