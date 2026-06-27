using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class SettingsControllerTests
    {
        private GameObject _holder;
        private SettingsController _controller;

        private Slider _volumeSlider;
        private Slider _musicSlider;
        private TMP_Dropdown _languageDropdown;
        private TMP_Dropdown _textSizeDropdown;
        private TMP_Dropdown _accessibilityDropdown;
        private Button _logoutButton;
        private Button _saveButton;
        private CanvasGroup _canvasGroup;
        private GraphicRaycaster _graphicRaycaster;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("SettingsControllerTestHolder");
            _controller = _holder.AddComponent<SettingsController>();

            _volumeSlider = new GameObject("volume").AddComponent<Slider>();
            _musicSlider = new GameObject("music").AddComponent<Slider>();
            _languageDropdown = new GameObject("lang").AddComponent<TMP_Dropdown>();
            _textSizeDropdown = new GameObject("size").AddComponent<TMP_Dropdown>();
            _accessibilityDropdown = new GameObject("acc").AddComponent<TMP_Dropdown>();
            _logoutButton = new GameObject("logout").AddComponent<Button>();
            _saveButton = new GameObject("save").AddComponent<Button>();
            _canvasGroup = _holder.AddComponent<CanvasGroup>();
            _graphicRaycaster = _holder.AddComponent<GraphicRaycaster>();

            _controller.SetVolumeSlider(_volumeSlider);
            _controller.SetMusicSlider(_musicSlider);
            _controller.SetLanguageDropdown(_languageDropdown);
            _controller.SetTextSizeDropdown(_textSizeDropdown);
            _controller.SetAccessibilityDropdown(_accessibilityDropdown);
            _controller.SetLogoutButton(_logoutButton);
            _controller.SetSaveButton(_saveButton);
            _controller.SetMainCanvasGroup(_canvasGroup);
            _controller.SetGraphicRaycaster(_graphicRaycaster);
        }

        [TearDown]
        public void TearDown()
        {
            if (_holder != null) Object.DestroyImmediate(_holder);
            if (_volumeSlider != null) Object.DestroyImmediate(_volumeSlider.gameObject);
            if (_musicSlider != null) Object.DestroyImmediate(_musicSlider.gameObject);
            if (_languageDropdown != null) Object.DestroyImmediate(_languageDropdown.gameObject);
            if (_textSizeDropdown != null) Object.DestroyImmediate(_textSizeDropdown.gameObject);
            if (_accessibilityDropdown != null) Object.DestroyImmediate(_accessibilityDropdown.gameObject);
            if (_logoutButton != null) Object.DestroyImmediate(_logoutButton.gameObject);
            if (_saveButton != null) Object.DestroyImmediate(_saveButton.gameObject);
        }

        [Test]
        public void SettingsController_PublicSetters_SuccessfullyAssignFields()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            var volumeSliderField = typeof(SettingsController).GetField("_volumeSlider", bindingFlags);
            Assert.That(volumeSliderField.GetValue(_controller), Is.SameAs(_volumeSlider));

            var musicSliderField = typeof(SettingsController).GetField("_musicSlider", bindingFlags);
            Assert.That(musicSliderField.GetValue(_controller), Is.SameAs(_musicSlider));

            var saveButtonField = typeof(SettingsController).GetField("_saveButton", bindingFlags);
            Assert.That(saveButtonField.GetValue(_controller), Is.SameAs(_saveButton));
        }

        [Test]
        public void SettingsController_ApplySettingsDtoToUi_CorrectlyUpdatesControls()
        {
            var method = typeof(SettingsController).GetMethod("ApplySettingsDtoToUi", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var dto = new SettingsDto
            {
                SfxVolume = 0.75f,
                MusicVolume = 0.5f,
                Language = "fil",
                TextSize = "large",
                ReducedMotion = true
            };

            method.Invoke(_controller, new object[] { dto });

            Assert.That(_volumeSlider.value, Is.EqualTo(0.75f));
            Assert.That(_musicSlider.value, Is.EqualTo(0.5f));
            Assert.That(_languageDropdown.value, Is.EqualTo(1)); // fil -> 1
            Assert.That(_textSizeDropdown.value, Is.EqualTo(2)); // large -> 2
            Assert.That(_accessibilityDropdown.value, Is.EqualTo(1)); // true -> 1
        }
    }
}
