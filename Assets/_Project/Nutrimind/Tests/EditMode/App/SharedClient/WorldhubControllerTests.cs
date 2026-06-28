using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class WorldhubControllerTests
    {
        private GameObject _holder;
        private WorldhubController _controller;

        private Button _litButton;
        private Button _heButton;
        private Button _sciButton;
        private Button _backButton;
        private CanvasGroup _canvasGroup;
        private GraphicRaycaster _graphicRaycaster;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("WorldhubControllerTestHolder");
            _controller = _holder.AddComponent<WorldhubController>();

            _litButton = new GameObject("lit").AddComponent<Button>();
            _heButton = new GameObject("he").AddComponent<Button>();
            _sciButton = new GameObject("sci").AddComponent<Button>();
            _backButton = new GameObject("back").AddComponent<Button>();
            _canvasGroup = _holder.AddComponent<CanvasGroup>();
            _graphicRaycaster = _holder.AddComponent<GraphicRaycaster>();

            _controller.SetLitButton(_litButton);
            _controller.SetHeButton(_heButton);
            _controller.SetSciButton(_sciButton);
            _controller.SetBackButton(_backButton);
            _controller.SetMainCanvasGroup(_canvasGroup);
            _controller.SetGraphicRaycaster(_graphicRaycaster);
        }

        [TearDown]
        public void TearDown()
        {
            if (_holder != null) Object.DestroyImmediate(_holder);
            if (_litButton != null) Object.DestroyImmediate(_litButton.gameObject);
            if (_heButton != null) Object.DestroyImmediate(_heButton.gameObject);
            if (_sciButton != null) Object.DestroyImmediate(_sciButton.gameObject);
            if (_backButton != null) Object.DestroyImmediate(_backButton.gameObject);
        }

        [Test]
        public void WorldhubController_PublicSetters_SuccessfullyAssignFields()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            var litField = typeof(WorldhubController).GetField("_litButton", bindingFlags);
            Assert.That(litField.GetValue(_controller), Is.SameAs(_litButton));

            var backField = typeof(WorldhubController).GetField("_backButton", bindingFlags);
            Assert.That(backField.GetValue(_controller), Is.SameAs(_backButton));
        }

        [Test]
        public void WorldhubController_Awake_DisablesScienceButton()
        {
            // Call Awake on controller via reflection (since MonoBehaviours don't have public Awake)
            var awakeMethod = typeof(WorldhubController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(awakeMethod, Is.Not.Null);

            // Initially interactable is true
            _sciButton.interactable = true;

            awakeMethod.Invoke(_controller, null);

            // Verify that ScienceQuest button is now non-interactable
            Assert.That(_sciButton.interactable, Is.False, "ScienceQuest button must be disabled as it is deferred.");
        }
    }
}
