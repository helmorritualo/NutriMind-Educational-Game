using NUnit.Framework;
using UnityEngine;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class AppBootstrapTests
    {
        private GameObject _bootstrapGo;
        private AppBootstrap _bootstrap;
        private SceneRegistry _testRegistry;

        [SetUp]
        public void SetUp()
        {
            _bootstrapGo = new GameObject("Test-Bootstrap");
            _bootstrap = _bootstrapGo.AddComponent<AppBootstrap>();
            _testRegistry = new SceneRegistry();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_bootstrapGo);
        }

        [Test]
        public void AppBootstrap_RegistersAllActiveAppScenes()
        {
            _bootstrap.RegisterScenes(_testRegistry);

            Assert.That(_testRegistry.Count, Is.EqualTo(9), "Expected exactly 9 registered scene keys.");

            string[] expectedKeys = { "Bootstrap", "SplashScreen", "Login", "MainMenu", "Profile", "Settings", "Worldhub", "LiteraQuestTerms", "HealthQuestTerms" };
            foreach (string key in expectedKeys)
            {
                Assert.That(_testRegistry.GetScene(key), Is.Not.Null, $"Expected key '{key}' to be registered.");
            }

            // Verify specific path mappings
            Assert.That(_testRegistry.GetScene("Bootstrap"), Is.EqualTo("Assets/_Project/Nutrimind/Scenes/App/Bootstrap.unity"));
            Assert.That(_testRegistry.GetScene("SplashScreen"), Is.EqualTo("Assets/_Project/Nutrimind/Scenes/App/SplashScreen.unity"));
            Assert.That(_testRegistry.GetScene("Login"), Is.EqualTo("Assets/_Project/Nutrimind/Scenes/App/Login.unity"));
            Assert.That(_testRegistry.GetScene("MainMenu"), Is.EqualTo("Assets/_Project/Nutrimind/Scenes/App/MainMenu.unity"));
        }

        [Test]
        public void AppNavigation_LoadsScenesThroughNavigationService()
        {
            // Verify that CompositionRoot Instance is active
            var root = CompositionRoot.Instance;
            Assert.That(root, Is.Not.Null);

            // Access scene registry through Singleton and ensure Bootstrap registers scenes on it
            _bootstrap.RegisterScenes(root.SceneRegistry);

            // Navigation service should resolve "SplashScreen" key successfully
            var result = root.NavigationService.Navigate("SplashScreen");
            Assert.That(result.IsAvailable, Is.True);
            Assert.That(result.ScenePath, Is.EqualTo("Assets/_Project/Nutrimind/Scenes/App/SplashScreen.unity"));
        }
    }
}
