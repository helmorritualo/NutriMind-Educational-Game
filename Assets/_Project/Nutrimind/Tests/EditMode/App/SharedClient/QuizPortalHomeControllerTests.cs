using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class QuizPortalHomeControllerTests
    {
        private GameObject _holder;
        private QuizPortalHomeController _homeController;
        private QuizPortalNavigationController _navigationController;
        private TextMeshProUGUI _welcomeText;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("QuizPortalHomeControllerTestHolder");
            _navigationController = _holder.AddComponent<QuizPortalNavigationController>();
            _homeController = _holder.AddComponent<QuizPortalHomeController>();

            _welcomeText = new GameObject("WelcomeText").AddComponent<TextMeshProUGUI>();
            _homeController.SetWelcomeText(_welcomeText);
            _homeController.SetNavigation(_navigationController);
        }

        [TearDown]
        public void TearDown()
        {
            if (_welcomeText != null) Object.DestroyImmediate(_welcomeText.gameObject);
            if (_holder != null) Object.DestroyImmediate(_holder);
        }

        [Test]
        public void QuizPortalHomeController_PublicSetters_SuccessfullyAssignFields()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var welcomeField = typeof(QuizPortalHomeController).GetField("_welcomeText", bindingFlags);
            Assert.That(welcomeField.GetValue(_homeController), Is.SameAs(_welcomeText));
        }

        [Test]
        public void BindWelcomeText_UsesFallbackWhenUnauthenticated()
        {
            CompositionRoot.Instance?.AuthSession?.Reset();

            _homeController.BindWelcomeText();
            Assert.That(_welcomeText.text, Is.EqualTo("Welcome, Explorer!"));
        }

        [Test]
        public void QuizPortalNavigationController_ShowAvailableQuizList_DoesNotThrowWhenPanelMissing()
        {
            Assert.DoesNotThrow(() => _navigationController.ShowAvailableQuizList());
        }

        [Test]
        public void AppBootstrap_RegistersQuizPortalScene()
        {
            var bootstrapGo = new GameObject("Test-Bootstrap-QuizPortal");
            var bootstrap = bootstrapGo.AddComponent<AppBootstrap>();
            var registry = new SceneRegistry();

            bootstrap.RegisterScenes(registry);

            Assert.That(registry.GetScene("QuizPortal"),
                Is.EqualTo("Assets/_Project/Nutrimind/Scenes/App/QuizPortalScene.unity"));

            Object.DestroyImmediate(bootstrapGo);
        }
    }
}
