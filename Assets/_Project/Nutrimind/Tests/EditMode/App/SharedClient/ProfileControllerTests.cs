using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Reflection;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class ProfileControllerTests
    {
        private GameObject _holder;
        private ProfileController _controller;

        private TextMeshProUGUI _nameText;
        private TextMeshProUGUI _gradeSectionText;
        private TextMeshProUGUI _lrnText;
        private TextMeshProUGUI _roomText;
        private TextMeshProUGUI _lvlText;
        private TextMeshProUGUI _coinsText;
        private TextMeshProUGUI _badgesText;
        private Button _backButton;
        private CanvasGroup _canvasGroup;
        private GraphicRaycaster _graphicRaycaster;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("ProfileControllerTestHolder");
            _controller = _holder.AddComponent<ProfileController>();

            // Instantiate UI parts
            _nameText = new GameObject("name").AddComponent<TextMeshProUGUI>();
            _gradeSectionText = new GameObject("Grade").AddComponent<TextMeshProUGUI>();
            _lrnText = new GameObject("lrn").AddComponent<TextMeshProUGUI>();
            _roomText = new GameObject("room").AddComponent<TextMeshProUGUI>();
            _lvlText = new GameObject("lvl").AddComponent<TextMeshProUGUI>();
            _coinsText = new GameObject("coins").AddComponent<TextMeshProUGUI>();
            _badgesText = new GameObject("badges").AddComponent<TextMeshProUGUI>();
            _backButton = new GameObject("back").AddComponent<Button>();
            _canvasGroup = _holder.AddComponent<CanvasGroup>();
            _graphicRaycaster = _holder.AddComponent<GraphicRaycaster>();

            // Setup setters
            _controller.SetStudentNameText(_nameText);
            _controller.SetGradeSectionText(_gradeSectionText);
            _controller.SetLrnText(_lrnText);
            _controller.SetRoomText(_roomText);
            _controller.SetLevelNumberText(_lvlText);
            _controller.SetCoinsText(_coinsText);
            _controller.SetBadgesText(_badgesText);
            _controller.SetBackButton(_backButton);
            _controller.SetMainCanvasGroup(_canvasGroup);
            _controller.SetGraphicRaycaster(_graphicRaycaster);
        }

        [TearDown]
        public void TearDown()
        {
            if (_holder != null) Object.DestroyImmediate(_holder);
            if (_nameText != null) Object.DestroyImmediate(_nameText.gameObject);
            if (_gradeSectionText != null) Object.DestroyImmediate(_gradeSectionText.gameObject);
            if (_lrnText != null) Object.DestroyImmediate(_lrnText.gameObject);
            if (_roomText != null) Object.DestroyImmediate(_roomText.gameObject);
            if (_lvlText != null) Object.DestroyImmediate(_lvlText.gameObject);
            if (_coinsText != null) Object.DestroyImmediate(_coinsText.gameObject);
            if (_badgesText != null) Object.DestroyImmediate(_badgesText.gameObject);
            if (_backButton != null) Object.DestroyImmediate(_backButton.gameObject);
        }

        [Test]
        public void ProfileController_PublicSetters_SuccessfullyAssignFields()
        {
            // Verify that fields can be correctly accessed via reflection or tested directly
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            
            var nameField = typeof(ProfileController).GetField("_studentNameText", bindingFlags);
            Assert.That(nameField.GetValue(_controller), Is.SameAs(_nameText));

            var backField = typeof(ProfileController).GetField("_backButton", bindingFlags);
            Assert.That(backField.GetValue(_controller), Is.SameAs(_backButton));
        }

        [Test]
        public void ProfileController_SetTextValues_UpdatesLabels()
        {
            // Use reflection to call the private SetTextValues method
            var method = typeof(ProfileController).GetMethod("SetTextValues", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            method.Invoke(_controller, new object[] { "Test Maya", "Grade 5 - A", "123456", "Room A", "5", "500", "2 / 12" });

            Assert.That(_nameText.text, Is.EqualTo("Test Maya"));
            Assert.That(_gradeSectionText.text, Is.EqualTo("Grade 5 - A"));
            Assert.That(_lrnText.text, Is.EqualTo("123456"));
            Assert.That(_roomText.text, Is.EqualTo("Room A"));
            Assert.That(_lvlText.text, Is.EqualTo("5"));
            Assert.That(_coinsText.text, Is.EqualTo("500"));
            Assert.That(_badgesText.text, Is.EqualTo("2 / 12"));
        }
    }
}
