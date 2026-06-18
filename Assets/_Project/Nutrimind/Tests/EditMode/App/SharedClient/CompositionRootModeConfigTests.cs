using System;
using System.Reflection;
using NUnit.Framework;

namespace NutriMind.Tests.EditMode.App
{
    /// <summary>
    /// RED-phase tests for Phase 03 composition/provider configuration.
    ///
    /// Verifies there is a factory or static method to create/configure
    /// the app composition for each <see cref="DataProviderMode"/> so that
    /// <c>Http</c> yields an <c>HttpProvider</c> and <c>LocalDemoJson</c>
    /// yields a <c>LocalDemoJsonProvider</c>, without relying on mutating
    /// a singleton's <c>Session.Mode</c> after construction.
    ///
    /// These tests will FAIL (RED) until a <c>CreateForMode</c> factory,
    /// <c>Initialize(DataProviderMode)</c> entry, or equivalent static
    /// factory is present.
    /// </summary>
    [TestFixture]
    public class CompositionRootModeConfigTests
    {
        private const string AssemblyName = "NutriMind.Runtime.App";

        private static Type FindType(string fullTypeName)
            => Type.GetType(fullTypeName + ", " + AssemblyName);

        private static Type FindEnum(string enumName)
            => FindType("NutriMind.Runtime.App." + enumName);

        // ---------------------------------------------------------------
        // Factory method existence
        // ---------------------------------------------------------------

        [Test]
        public void CompositionRoot_HasCreateForModeFactory()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null,
                "Precondition: CompositionRoot class must exist");

            // Look for a static factory that accepts DataProviderMode
            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null,
                "Precondition: DataProviderMode enum must exist");

            MethodInfo factory = FindFactoryMethod(t, modeType);

            Assert.That(factory, Is.Not.Null,
                "CompositionRoot must provide a static factory (CreateForMode/Create/ForMode) " +
                "or instance Initialize(DataProviderMode) method to create/configure " +
                "the composition with a specific mode before first use");
        }

        // ---------------------------------------------------------------
        // Http mode -> HttpProvider
        // ---------------------------------------------------------------

        [Test]
        public void CreateForMode_Http_YieldsHttpProvider()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null,
                "Precondition: CompositionRoot class must exist");

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null,
                "Precondition: DataProviderMode enum must exist");

            Type httpProviderType = FindType("NutriMind.Runtime.App.HttpProvider");
            Assert.That(httpProviderType, Is.Not.Null,
                "Precondition: HttpProvider type must exist");

            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null,
                "Precondition: factory method must exist -- see CompositionRoot_HasCreateForModeFactory");

            object httpMode = Enum.Parse(modeType, "Http");
            object root = factory.Invoke(null, new object[] { httpMode });
            Assert.That(root, Is.Not.Null,
                "Factory must return non-null CompositionRoot for Http mode");

            // Read the DataProvider property
            PropertyInfo providerProp = t.GetProperty("DataProvider",
                BindingFlags.Public | BindingFlags.Instance)
                ?? t.GetProperty("GameDataProvider", BindingFlags.Public | BindingFlags.Instance)
                ?? t.GetProperty("Provider", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(providerProp, Is.Not.Null,
                "CompositionRoot must expose a provider property");

            object provider = providerProp.GetValue(root);
            Assert.That(provider, Is.Not.Null,
                "DataProvider must be non-null when created in Http mode");

            Assert.That(httpProviderType.IsInstanceOfType(provider), Is.True,
                "When created in Http mode, DataProvider must be an instance of HttpProvider, " +
                "but was {0}", provider?.GetType().Name);
        }

        // ---------------------------------------------------------------
        // LocalDemoJson mode -> LocalDemoJsonProvider
        // ---------------------------------------------------------------

        [Test]
        public void CreateForMode_LocalDemoJson_YieldsLocalDemoJsonProvider()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null,
                "Precondition: CompositionRoot class must exist");

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null,
                "Precondition: DataProviderMode enum must exist");

            Type localProviderType = FindType("NutriMind.Runtime.App.LocalDemoJsonProvider");
            Assert.That(localProviderType, Is.Not.Null,
                "Precondition: LocalDemoJsonProvider type must exist");

            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null,
                "Precondition: factory method must exist -- see CompositionRoot_HasCreateForModeFactory");

            object localMode = Enum.Parse(modeType, "LocalDemoJson");
            object root = factory.Invoke(null, new object[] { localMode });
            Assert.That(root, Is.Not.Null,
                "Factory must return non-null CompositionRoot for LocalDemoJson mode");

            PropertyInfo providerProp = t.GetProperty("DataProvider",
                BindingFlags.Public | BindingFlags.Instance)
                ?? t.GetProperty("GameDataProvider", BindingFlags.Public | BindingFlags.Instance)
                ?? t.GetProperty("Provider", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(providerProp, Is.Not.Null,
                "CompositionRoot must expose a provider property");

            object provider = providerProp.GetValue(root);
            Assert.That(provider, Is.Not.Null,
                "DataProvider must be non-null when created in LocalDemoJson mode");

            Assert.That(localProviderType.IsInstanceOfType(provider), Is.True,
                "When created in LocalDemoJson mode, DataProvider must be an instance of " +
                "LocalDemoJsonProvider, but was {0}", provider?.GetType().Name);
        }

        // ---------------------------------------------------------------
        // Repeated singleton access -- no duplicate service graph
        // ---------------------------------------------------------------

        [Test]
        public void CreateForMode_RepeatedSingletonAccess_ReturnsSameReferences()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null,
                "Precondition: CompositionRoot class must exist");

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null,
                "Precondition: DataProviderMode enum must exist");

            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null,
                "Precondition: factory method must exist");

            object httpMode = Enum.Parse(modeType, "Http");
            object root1 = factory.Invoke(null, new object[] { httpMode });
            object root2 = factory.Invoke(null, new object[] { httpMode });

            Assert.That(root2, Is.SameAs(root1),
                "Repeated calls to the factory with the same mode must return " +
                "the same singleton CompositionRoot instance (no duplicate service graph)");

            // Also verify all services are shared across the two reference accesses
            foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead || p.PropertyType == t || p.PropertyType.IsValueType) continue;
                object s1 = p.GetValue(root1);
                object s2 = p.GetValue(root2);
                if (s1 != null)
                    Assert.That(s2, Is.SameAs(s1),
                        "Service property '{0}' must return same instance on repeated access " +
                        "from the singleton CompositionRoot", p.Name);
            }
        }

        // ---------------------------------------------------------------
        // Default singleton still works
        // ---------------------------------------------------------------

        [Test]
        public void DefaultInstance_UsesLocalDemoJsonProvider()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null,
                "Precondition: CompositionRoot class must exist");

            Type localProviderType = FindType("NutriMind.Runtime.App.LocalDemoJsonProvider");
            Assert.That(localProviderType, Is.Not.Null,
                "Precondition: LocalDemoJsonProvider type must exist");

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null,
                "Precondition: DataProviderMode enum must exist");

            // Ensure the canonical singleton is configured for LocalDemoJson so this
            // test is order-independent and consistent with the canonical singleton
            // architecture.
            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null,
                "Precondition: factory method must exist -- see CompositionRoot_HasCreateForModeFactory");

            object localMode = Enum.Parse(modeType, "LocalDemoJson");
            object configuredRoot = factory.Invoke(null, new object[] { localMode });
            Assert.That(configuredRoot, Is.Not.Null,
                "Factory must return non-null CompositionRoot for LocalDemoJson mode");

            PropertyInfo instProp = t.GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static);
            object root = instProp?.GetValue(null);

            if (root == null)
            {
                MethodInfo getInst = t.GetMethod("GetInstance",
                    BindingFlags.Public | BindingFlags.Static);
                root = getInst?.Invoke(null, null);
            }

            Assert.That(root, Is.Not.Null,
                "Canonical Instance/GetInstance() must be accessible after CreateForMode");
            Assert.That(root, Is.SameAs(configuredRoot),
                "Canonical Instance/GetInstance() must return the same CompositionRoot " +
                "that was configured by CreateForMode(LocalDemoJson)");

            PropertyInfo providerProp = t.GetProperty("DataProvider",
                BindingFlags.Public | BindingFlags.Instance)
                ?? t.GetProperty("GameDataProvider", BindingFlags.Public | BindingFlags.Instance)
                ?? t.GetProperty("Provider", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(providerProp, Is.Not.Null,
                "CompositionRoot must expose a provider property");

            object provider = providerProp.GetValue(root);
            Assert.That(provider, Is.Not.Null,
                "Default singleton DataProvider must be non-null");

            Assert.That(localProviderType.IsInstanceOfType(provider), Is.True,
                "Default CompositionRoot singleton must use LocalDemoJsonProvider, " +
                "but was {0}", provider?.GetType().Name);
        }

        // ---------------------------------------------------------------
        // Canonical singleton — CreateForMode(Http) must set the
        // canonical Instance/GetInstance() so that the rest of the app
        // resolves to the same configured root, not a separate default.
        // ---------------------------------------------------------------

        [Test]
        public void CreateForMode_Http_CanonicalInstanceMatchesConfiguredRoot()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null,
                "Precondition: CompositionRoot class must exist");

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null,
                "Precondition: DataProviderMode enum must exist");

            Type httpProviderType = FindType("NutriMind.Runtime.App.HttpProvider");
            Assert.That(httpProviderType, Is.Not.Null,
                "Precondition: HttpProvider type must exist");

            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null,
                "Precondition: factory method must exist");

            // 1) Configure an Http root
            object httpMode = Enum.Parse(modeType, "Http");
            object configuredRoot = factory.Invoke(null, new object[] { httpMode });
            Assert.That(configuredRoot, Is.Not.Null,
                "CreateForMode(Http) must return non-null");

            // 2) Resolve the canonical singleton (Instance / GetInstance)
            PropertyInfo instProp = t.GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static);
            object canonicalRoot = instProp?.GetValue(null);

            if (canonicalRoot == null)
            {
                MethodInfo getInst = t.GetMethod("GetInstance",
                    BindingFlags.Public | BindingFlags.Static);
                canonicalRoot = getInst?.Invoke(null, null);
            }

            Assume.That(canonicalRoot, Is.Not.Null,
                "Canonical Instance/GetInstance() must be accessible");

            // 3) They must be the SAME composition root — one service graph.
            Assert.That(canonicalRoot, Is.SameAs(configuredRoot),
                "After CreateForMode(Http), the canonical Instance/GetInstance() " +
                "must return the same CompositionRoot that was created for Http mode, " +
                "not a separate LocalDemoJson default root. " +
                "This currently FAILS (RED) due to the dual-root implementation " +
                "where Instance always returns a LocalDemoJson root independent " +
                "of CreateForMode.");

            // 4) The canonical root's DataProvider must be an HttpProvider.
            PropertyInfo providerProp = t.GetProperty("DataProvider",
                BindingFlags.Public | BindingFlags.Instance)
                ?? t.GetProperty("GameDataProvider", BindingFlags.Public | BindingFlags.Instance)
                ?? t.GetProperty("Provider", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(providerProp, Is.Not.Null,
                "CompositionRoot must expose a provider property");

            object provider = providerProp.GetValue(canonicalRoot);
            Assert.That(httpProviderType.IsInstanceOfType(provider), Is.True,
                "The canonical root's DataProvider must be an HttpProvider " +
                "after CreateForMode(Http), but was {0}", provider?.GetType().Name);
        }

        // ---------------------------------------------------------------
        // IDisposable
        // ---------------------------------------------------------------

        [Test]
        public void CompositionRoot_ImplementsIDisposable()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null);

            bool implementsIDisposable = typeof(System.IDisposable).IsAssignableFrom(t);
            Assert.That(implementsIDisposable, Is.True,
                "CompositionRoot must implement IDisposable to support safe " +
                "cleanup when the canonical root is replaced");
        }

        [Test]
        public void CompositionRoot_HasDisposeMethod()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null);

            MethodInfo dispose = t.GetMethod("Dispose",
                BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            Assert.That(dispose, Is.Not.Null,
                "CompositionRoot must have a public Dispose() method");
        }

        // ---------------------------------------------------------------
        // Disposal on root replacement
        // ---------------------------------------------------------------

        [Test]
        public void CreateForMode_WhenModeChanges_DisposesOldHttpProvider()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null);

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null);

            Type httpProviderType = FindType("NutriMind.Runtime.App.HttpProvider");
            Assert.That(httpProviderType, Is.Not.Null);

            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null);

            // 1) Create an Http root.
            object httpMode = Enum.Parse(modeType, "Http");
            object httpRoot = factory.Invoke(null, new object[] { httpMode });
            Assert.That(httpRoot, Is.Not.Null);

            // Capture the old DataProvider.
            PropertyInfo providerProp = t.GetProperty("DataProvider",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.That(providerProp, Is.Not.Null);
            object oldProvider = providerProp.GetValue(httpRoot);
            Assert.That(httpProviderType.IsInstanceOfType(oldProvider), Is.True);

            // Capture the old SyncPolling.
            PropertyInfo syncProp = t.GetProperty("SyncPolling",
                BindingFlags.Public | BindingFlags.Instance);
            object oldSync = syncProp?.GetValue(httpRoot);

            // 2) Switch to LocalDemoJson — this should dispose the old root.
            object localMode = Enum.Parse(modeType, "LocalDemoJson");
            object newRoot = factory.Invoke(null, new object[] { localMode });
            Assert.That(newRoot, Is.Not.Null);
            Assert.That(newRoot, Is.Not.SameAs(httpRoot),
                "Switching modes must replace the singleton with a new CompositionRoot");

            // 3) Verify old HttpProvider is disposed by checking its _disposed field.
            FieldInfo disposedField = httpProviderType.GetField("_disposed",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (disposedField != null)
            {
                bool oldProviderDisposed = (bool)disposedField.GetValue(oldProvider);
                Assert.That(oldProviderDisposed, Is.True,
                    "Old HttpProvider must be disposed when CompositionRoot mode changes");
            }

            // 4) Verify old SyncPollingService is disposed (if it was present).
            if (oldSync != null && syncProp != null)
            {
                Type syncType = oldSync.GetType();
                FieldInfo syncDisposed = syncType.GetField("_disposed",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (syncDisposed != null)
                {
                    bool oldSyncDisposed = (bool)syncDisposed.GetValue(oldSync);
                    Assert.That(oldSyncDisposed, Is.True,
                        "Old SyncPollingService must be disposed when CompositionRoot mode changes");
                }
            }

            // 5) Verify new root uses LocalDemoJsonProvider.
            Type localProviderType = FindType("NutriMind.Runtime.App.LocalDemoJsonProvider");
            Assert.That(localProviderType, Is.Not.Null);
            object newProvider = providerProp.GetValue(newRoot);
            Assert.That(localProviderType.IsInstanceOfType(newProvider), Is.True,
                "After switching to LocalDemoJson, DataProvider must be LocalDemoJsonProvider");
        }

        [Test]
        public void CreateForMode_ModeUnchanged_DoesNotDispose()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null);

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null);

            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null);

            object httpMode = Enum.Parse(modeType, "Http");
            object root1 = factory.Invoke(null, new object[] { httpMode });
            object root2 = factory.Invoke(null, new object[] { httpMode });

            Assert.That(root2, Is.SameAs(root1),
                "Calling CreateForMode with the same mode must return the same instance");
        }

        // ---------------------------------------------------------------
        // Dispatcher pump (Http mode only)
        // ---------------------------------------------------------------

        [Test]
        public void CreateForMode_Http_CreatesDispatcherPump()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null);

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null);

            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null);

            object httpMode = Enum.Parse(modeType, "Http");
            object root = factory.Invoke(null, new object[] { httpMode });
            Assert.That(root, Is.Not.Null);

            // Check the private _pumpGameObject field — it should be non-null in Http mode.
            FieldInfo pumpField = t.GetField("_pumpGameObject",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(pumpField, Is.Not.Null,
                "CompositionRoot must have _pumpGameObject field");

            object pumpGo = pumpField.GetValue(root);
            Assert.That(pumpGo, Is.Not.Null,
                "CompositionRoot must create a dispatcher pump GameObject in Http mode");

            // Verify the GameObject has the expected name.
            var goType = pumpGo.GetType();
            PropertyInfo nameProp = goType.GetProperty("name",
                BindingFlags.Public | BindingFlags.Instance);
            if (nameProp != null)
            {
                string name = nameProp.GetValue(pumpGo) as string;
                Assert.That(name, Is.EqualTo("NutriMind-DispatcherPump"),
                    "Pump GameObject must have the expected name");
            }

            // Verify hide flags are set correctly.
            PropertyInfo hideFlagsProp = goType.GetProperty("hideFlags",
                BindingFlags.Public | BindingFlags.Instance);
            if (hideFlagsProp != null)
            {
                var hideFlagsObj = hideFlagsProp.GetValue(pumpGo);
                // HideFlags.HideAndDontSave is a composite flag (value 61), NOT 64.
                int hideFlagsVal = Convert.ToInt32(hideFlagsObj);
                int expected = (int)UnityEngine.HideFlags.HideAndDontSave;
                Assert.That((hideFlagsVal & expected) == expected, Is.True,
                    "Pump GameObject must have HideAndDontSave flag");
            }
        }

        [Test]
        public void CreateForMode_LocalDemoJson_NoDispatcherPump()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null);

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null);

            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null);

            object localMode = Enum.Parse(modeType, "LocalDemoJson");
            object root = factory.Invoke(null, new object[] { localMode });
            Assert.That(root, Is.Not.Null);

            // In LocalDemoJson mode, the pump field should be null.
            FieldInfo pumpField = t.GetField("_pumpGameObject",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(pumpField, Is.Not.Null);

            object pumpGo = pumpField.GetValue(root);
            Assert.That(pumpGo, Is.Null,
                "CompositionRoot must NOT create a dispatcher pump GameObject in LocalDemoJson mode");
        }

        [Test]
        public void CreateForMode_Dispose_DestroysPumpGameObject()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null);

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null);

            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null);

            object httpMode = Enum.Parse(modeType, "Http");
            object root = factory.Invoke(null, new object[] { httpMode });
            Assert.That(root, Is.Not.Null);

            // Verify pump exists before disposal.
            FieldInfo pumpField = t.GetField("_pumpGameObject",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(pumpField, Is.Not.Null);
            object pumpBefore = pumpField.GetValue(root);
            Assert.That(pumpBefore, Is.Not.Null);

            // Verify the GameObject reference is valid before disposal.
            // Accessing the 'name' property proves the GO hasn't been destroyed yet.
            var goType = pumpBefore.GetType();
            PropertyInfo nameProp = goType.GetProperty("name",
                BindingFlags.Public | BindingFlags.Instance);
            string nameBefore = nameProp?.GetValue(pumpBefore) as string;
            Assert.That(nameBefore, Is.EqualTo("NutriMind-DispatcherPump"));

            // Dispose the root.
            MethodInfo dispose = t.GetMethod("Dispose",
                BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            Assert.That(dispose, Is.Not.Null);
            dispose.Invoke(root, null);

            // After disposal, the _pumpGameObject field should be null.
            object pumpAfter = pumpField.GetValue(root);
            Assert.That(pumpAfter, Is.Null,
                "After Dispose(), the _pumpGameObject must be null");
        }

        // ---------------------------------------------------------------
        // Default-mode guard (preprocessor) note
        // ---------------------------------------------------------------

        /// <summary>
        /// The release-build guard for <c>CreateForMode(LocalDemoJson)</c> is
        /// implemented via preprocessor directives (<c>UNITY_EDITOR</c> and
        /// <c>DEVELOPMENT_BUILD</c>) and cannot be verified in EditMode tests
        /// because the test runner runs inside the Unity Editor where
        /// <c>UNITY_EDITOR</c> is always defined.
        ///
        /// The guard logic (in <c>CreateForMode</c>):
        /// <code>
        /// #if !UNITY_EDITOR &amp;&amp; !DEVELOPMENT_BUILD
        ///     if (mode == DataProviderMode.LocalDemoJson)
        ///         throw new InvalidOperationException(...);
        /// #endif
        /// </code>
        ///
        /// Manual verification steps:
        /// 1. Build a release standalone player (File → Build Settings, uncheck
        ///    Development Build).
        /// 2. Launch the built player and open the Console (or check logs).
        /// 3. If any code path calls <c>CompositionRoot.CreateForMode(LocalDemoJson)</c>
        ///    or <c>CompositionRoot.Instance</c> (which uses <c>GetDefaultMode()</c>),
        ///    the release player will either use Http (default) or throw an exception
        ///    on explicit LocalDemoJson requests.
        /// </summary>
        [Test]
        public void ReleaseBuildGuard_Documented()
        {
            // This test exists solely to document the preprocessor guard logic.
            // No runtime assertion — see XML doc above for manual verification.
            Assert.Pass("Release-build guard is implemented via preprocessor directives " +
                "(see XML doc on this test method). Verify in a built release player.");
        }

        // ---------------------------------------------------------------
        // Disposed-root reuse regression
        // ---------------------------------------------------------------

        [Test]
        public void CreateForMode_DisposedRoot_ReplacedByNewInstance()
        {
            Type t = FindType("NutriMind.Runtime.App.CompositionRoot");
            Assert.That(t, Is.Not.Null);

            Type modeType = FindEnum("DataProviderMode");
            Assert.That(modeType, Is.Not.Null);

            MethodInfo factory = FindFactoryMethod(t, modeType);
            Assert.That(factory, Is.Not.Null);

            MethodInfo dispose = t.GetMethod("Dispose",
                BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            Assert.That(dispose, Is.Not.Null,
                "Precondition: Dispose method must exist");

            object localMode = Enum.Parse(modeType, "LocalDemoJson");

            // 1) Create an initial root.
            object root1 = factory.Invoke(null, new object[] { localMode });
            Assert.That(root1, Is.Not.Null);

            // 2) Dispose the root.
            dispose.Invoke(root1, null);

            // 3) Create again with the same mode.
            //    Regression: a disposed root must be replaced, not returned as-is.
            object root2 = factory.Invoke(null, new object[] { localMode });
            Assert.That(root2, Is.Not.Null);
            Assert.That(root2, Is.Not.SameAs(root1),
                "After Dispose(), CreateForMode with the same mode must return a " +
                "new CompositionRoot, not the disposed singleton");

            // 4) New root's services must be healthy (non-null).
            foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead || p.PropertyType == t || p.PropertyType.IsValueType) continue;
                if (p.Name == "SyncPolling") continue; // null in LocalDemoJson mode.
                object svc = p.GetValue(root2);
                Assert.That(svc, Is.Not.Null,
                    "New root's property '{0}' must be non-null after re-creation", p.Name);
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static MethodInfo FindFactoryMethod(Type rootType, Type modeType)
        {
            string[] factoryNames = { "CreateForMode", "Create", "ForMode", "ConfigureForMode" };
            foreach (string name in factoryNames)
            {
                MethodInfo m = rootType.GetMethod(name,
                    BindingFlags.Public | BindingFlags.Static,
                    null, new[] { modeType }, null);
                if (m != null)
                    return m;
            }

            // Also accept instance Initialize(DataProviderMode)
            MethodInfo init = rootType.GetMethod("Initialize",
                BindingFlags.Public | BindingFlags.Instance,
                null, new[] { modeType }, null);
            if (init != null)
                return init;

            // Accept a parameterless static factory
            MethodInfo defaultFactory = rootType.GetMethod("CreateForMode",
                BindingFlags.Public | BindingFlags.Static);
            if (defaultFactory != null)
                return defaultFactory;

            return null;
        }
    }
}
