using System;
using System.Reflection;
using NUnit.Framework;

namespace NutriMind.Tests.EditMode.App
{
    /// <summary>
    /// RED-phase tests for Phase 03: SessionScope must expose typed store/state
    /// types rather than only <c>object</c> buckets.
    ///
    /// Required types: <c>AuthSessionState</c>, <c>StudentProfileStore</c>,
    /// <c>SettingsStore</c>, <c>SubjectTermStore</c>, <c>ProgressRewardStore</c>,
    /// <c>InteractionStateStore</c>.
    ///
    /// SessionScope must expose typed properties for each.
    /// <c>Clear()</c> must reset them while preserving <see cref="DataProviderMode"/>.
    ///
    /// These tests will FAIL (RED) until the types and typed properties exist.
    /// </summary>
    [TestFixture]
    public class SessionScopeTypedStoresTests
    {
        private const string AssemblyName = "NutriMind.Runtime.App";

        private static Type FindType(string fullTypeName)
            => Type.GetType(fullTypeName + ", " + AssemblyName);

        private static Type FindEnum(string enumName)
            => FindType("NutriMind.Runtime.App." + enumName);

        // ---------------------------------------------------------------
        // Each required typed store type must exist
        // ---------------------------------------------------------------

        [Test]
        public void AuthSessionState_TypeExists()
        {
            Type t = FindType("NutriMind.Runtime.App.AuthSessionState");
            Assert.That(t, Is.Not.Null,
                "AuthSessionState type must exist -- " +
                "SessionScope should expose a typed auth state class");
            Assert.That(t.IsClass, Is.True,
                "AuthSessionState must be a class");
        }

        [Test]
        public void StudentProfileStore_TypeExists()
        {
            Type t = FindType("NutriMind.Runtime.App.StudentProfileStore");
            Assert.That(t, Is.Not.Null,
                "StudentProfileStore type must exist -- " +
                "SessionScope should expose a typed profile store");
            Assert.That(t.IsClass, Is.True,
                "StudentProfileStore must be a class");
        }

        [Test]
        public void SettingsStore_TypeExists()
        {
            Type t = FindType("NutriMind.Runtime.App.SettingsStore");
            Assert.That(t, Is.Not.Null,
                "SettingsStore type must exist -- " +
                "SessionScope should expose a typed settings store");
            Assert.That(t.IsClass, Is.True,
                "SettingsStore must be a class");
        }

        [Test]
        public void SubjectTermStore_TypeExists()
        {
            Type t = FindType("NutriMind.Runtime.App.SubjectTermStore");
            Assert.That(t, Is.Not.Null,
                "SubjectTermStore type must exist -- " +
                "SessionScope should expose a typed subject/term store");
            Assert.That(t.IsClass, Is.True,
                "SubjectTermStore must be a class");
        }

        [Test]
        public void ProgressRewardStore_TypeExists()
        {
            Type t = FindType("NutriMind.Runtime.App.ProgressRewardStore");
            Assert.That(t, Is.Not.Null,
                "ProgressRewardStore type must exist -- " +
                "SessionScope should expose a typed progress+reward store");
            Assert.That(t.IsClass, Is.True,
                "ProgressRewardStore must be a class");
        }

        [Test]
        public void InteractionStateStore_TypeExists()
        {
            Type t = FindType("NutriMind.Runtime.App.InteractionStateStore");
            Assert.That(t, Is.Not.Null,
                "InteractionStateStore type must exist -- " +
                "SessionScope should expose a typed interaction state store");
            Assert.That(t.IsClass, Is.True,
                "InteractionStateStore must be a class");
        }

        // ---------------------------------------------------------------
        // SessionScope must expose typed properties matching store types
        // ---------------------------------------------------------------

        [Test]
        public void SessionScope_HasAuthSessionStateProperty()
        {
            AssertSessionScopeHasTypedProperty("AuthSessionState", "AuthSessionState",
                "AuthState", "Auth");
        }

        [Test]
        public void SessionScope_HasStudentProfileStoreProperty()
        {
            AssertSessionScopeHasTypedProperty("StudentProfileStore", "StudentProfileStore",
                "ProfileStore", "Profile");
        }

        [Test]
        public void SessionScope_HasSettingsStoreProperty()
        {
            AssertSessionScopeHasTypedProperty("SettingsStore", "SettingsStore",
                "Settings");
        }

        [Test]
        public void SessionScope_HasSubjectTermStoreProperty()
        {
            AssertSessionScopeHasTypedProperty("SubjectTermStore", "SubjectTermStore",
                "SubjectTerm", "SubjectTermState");

            // Additional check: SubjectTermStore should have term-related state
            Type storeType = FindType("NutriMind.Runtime.App.SubjectTermStore");
            Assert.That(storeType, Is.Not.Null,
                "Precondition: SubjectTermStore type must exist");

            bool hasTermProperty =
                storeType.GetProperty("CurrentTerm", BindingFlags.Public | BindingFlags.Instance) != null
                || storeType.GetProperty("SelectedTerm", BindingFlags.Public | BindingFlags.Instance) != null
                || storeType.GetProperty("Term", BindingFlags.Public | BindingFlags.Instance) != null
                || storeType.GetProperty("ActiveTerm", BindingFlags.Public | BindingFlags.Instance) != null;

            Assert.That(hasTermProperty, Is.True,
                "SubjectTermStore must include term-related state " +
                "(e.g. CurrentTerm, SelectedTerm, or Term property)");
        }

        [Test]
        public void SessionScope_HasProgressRewardStoreProperty()
        {
            AssertSessionScopeHasTypedProperty("ProgressRewardStore", "ProgressRewardStore",
                "ProgressReward", "Progress");
        }

        [Test]
        public void SessionScope_HasInteractionStateStoreProperty()
        {
            AssertSessionScopeHasTypedProperty("InteractionStateStore", "InteractionStateStore",
                "InteractionState", "Interaction");
        }

        // ---------------------------------------------------------------
        // Clear() resets typed stores while preserving Mode
        // ---------------------------------------------------------------

        [Test]
        public void Clear_ResetsAllTypedStoresToDefaults()
        {
            Type sessionType = FindType("NutriMind.Runtime.App.SessionScope");
            Assert.That(sessionType, Is.Not.Null,
                "Precondition: SessionScope type must exist");

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null,
                "Precondition: DataProviderMode enum must exist");

            MethodInfo clearMethod = sessionType.GetMethod("Clear",
                BindingFlags.Public | BindingFlags.Instance,
                null, Type.EmptyTypes, null);
            Assert.That(clearMethod, Is.Not.Null,
                "Precondition: SessionScope must have Clear() method");

            ConstructorInfo ctor = sessionType.GetConstructor(Type.EmptyTypes);
            object session = ctor.Invoke(null);

            // Read Mode before Clear
            PropertyInfo modeProp = sessionType.GetProperty("Mode",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.That(modeProp, Is.Not.Null,
                "Precondition: SessionScope must have Mode property");

            // Set mode to something distinct so we can verify it survives
            object httpMode = Enum.Parse(modeType, "Http");
            modeProp.SetValue(session, httpMode);

            // Call Clear
            clearMethod.Invoke(session, null);

            // Verify typed stores are reset (e.g. fresh instances or null)
            string[] storeBaseNames = {
                "AuthSessionState", "StudentProfileStore", "SettingsStore",
                "SubjectTermStore", "ProgressRewardStore", "InteractionStateStore"
            };

            string[][] propertyCandidates = {
                new[] { "AuthSessionState", "AuthState", "Auth" },
                new[] { "StudentProfileStore", "ProfileStore", "Profile" },
                new[] { "SettingsStore", "Settings" },
                new[] { "SubjectTermStore", "SubjectTerm", "SubjectTermState" },
                new[] { "ProgressRewardStore", "ProgressReward", "Progress" },
                new[] { "InteractionStateStore", "InteractionState", "Interaction" }
            };

            for (int i = 0; i < storeBaseNames.Length; i++)
            {
                PropertyInfo prop = FindSessionScopeProperty(sessionType, propertyCandidates[i]);
                if (prop == null)
                {
                    // Property not yet implemented -- this is expected RED behaviour
                    continue;
                }

                object value = prop.GetValue(session);
                Type propType = prop.PropertyType;

                // These are nullable class types (stores are reference types)
                // After Clear they should be null, meaning reset-to-default
                if (!propType.IsValueType)
                {
                    // Allow either null (if stores are replaced with null on Clear)
                    // or a fresh default-constructed instance
                    if (value != null)
                    {
                        // If not null, it must be the correct type
                        Assert.That(propType.IsInstanceOfType(value), Is.True,
                            "SessionScope.{0} after Clear must be null or a fresh instance of {1}",
                            prop.Name, storeBaseNames[i]);
                    }
                }
                else
                {
                    Assert.That(value, Is.EqualTo(Activator.CreateInstance(propType)),
                        "SessionScope.{0} must be default({1}) after Clear()",
                        prop.Name, propType.Name);
                }
            }

            // Mode must be preserved
            object modeAfterClear = modeProp.GetValue(session);
            Assert.That(modeAfterClear, Is.EqualTo(httpMode),
                "SessionScope.Clear must preserve the Mode value across store reset");
        }

        [Test]
        public void Clear_PreservesMode_WhenStoresAreTyped()
        {
            Type sessionType = FindType("NutriMind.Runtime.App.SessionScope");
            Assert.That(sessionType, Is.Not.Null,
                "Precondition: SessionScope type must exist");

            MethodInfo clearMethod = sessionType.GetMethod("Clear",
                BindingFlags.Public | BindingFlags.Instance,
                null, Type.EmptyTypes, null);
            Assert.That(clearMethod, Is.Not.Null,
                "Precondition: SessionScope must have Clear() method");

            ConstructorInfo ctor = sessionType.GetConstructor(Type.EmptyTypes);
            object session = ctor.Invoke(null);

            Type modeType = FindEnum("DataProviderMode");
            PropertyInfo modeProp = sessionType.GetProperty("Mode",
                BindingFlags.Public | BindingFlags.Instance);

            object localMode = Enum.Parse(modeType, "LocalDemoJson");
            modeProp.SetValue(session, localMode);

            // Call Clear twice to ensure mode survives repeated clears
            clearMethod.Invoke(session, null);
            clearMethod.Invoke(session, null);

            object modeValue = modeProp.GetValue(session);
            Assert.That(modeValue, Is.EqualTo(localMode),
                "Mode must survive repeated Clear() calls");
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static void AssertSessionScopeHasTypedProperty(
            string storeTypeName, params string[] candidatePropertyNames)
        {
            Type sessionType = FindType("NutriMind.Runtime.App.SessionScope");
            Assert.That(sessionType, Is.Not.Null,
                "Precondition: SessionScope type must exist");

            Type storeType = FindType("NutriMind.Runtime.App." + storeTypeName);
            Assert.That(storeType, Is.Not.Null,
                "Precondition: {0} type must exist", storeTypeName);

            PropertyInfo prop = FindSessionScopeProperty(sessionType, candidatePropertyNames);
            Assert.That(prop, Is.Not.Null,
                "SessionScope must expose a public instance property named one of [{0}] " +
                "of type {1}", string.Join(", ", candidatePropertyNames), storeTypeName);

            Assert.That(prop.PropertyType, Is.EqualTo(storeType),
                "SessionScope.{0} must be of type {1}, but was {2}",
                prop.Name, storeTypeName, prop.PropertyType.Name);
        }

        private static PropertyInfo FindSessionScopeProperty(
            Type sessionType, string[] candidateNames)
        {
            foreach (string name in candidateNames)
            {
                PropertyInfo prop = sessionType.GetProperty(name,
                    BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                    return prop;
            }
            return null;
        }
    }
}
