using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using NutriMind.Runtime.App;
using NutriMind.Runtime.UI;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class LoadingTransitionControllerTests
    {
        private GameObject _holderGo;
        private LoadingTransitionController _controller;
        private UIDocument _uiDocument;

        [SetUp]
        public void SetUp()
        {
            _holderGo = new GameObject("LoadingTransitionTestHolder");
            _uiDocument = _holderGo.AddComponent<UIDocument>();
            _controller = _holderGo.AddComponent<LoadingTransitionController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_holderGo);
        }

        [Test]
        public void LoadingTransitionController_CanBeAddedAndInitialized()
        {
            Assert.That(_controller, Is.Not.Null, "Expected LoadingTransitionController component to be added successfully.");
        }

        [Test]
        public void LoadingTransitionController_DefaultDisplayTimeIsPositive()
        {
            var serializedObject = new UnityEditor.SerializedObject(_controller);
            var minDisplayTimeProp = serializedObject.FindProperty("_minimumDisplayTime");
            
            Assert.That(minDisplayTimeProp, Is.Not.Null);
            Assert.That(minDisplayTimeProp.floatValue, Is.GreaterThanOrEqualTo(1.5f), "Minimum display time should be at least 1.5 seconds.");
        }
    }
}