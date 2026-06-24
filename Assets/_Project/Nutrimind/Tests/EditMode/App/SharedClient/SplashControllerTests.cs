using NUnit.Framework;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class SplashControllerTests
    {
        private GameObject _splashGo;
        private SplashController _controller;

        [SetUp]
        public void SetUp()
        {
            _splashGo = new GameObject("Test-Splash");
            _controller = _splashGo.AddComponent<SplashController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_splashGo);
        }

        [Test]
        public void SplashController_CanBeAddedAndInitialized()
        {
            Assert.That(_controller, Is.Not.Null, "Expected SplashController component to be added successfully.");
        }
    }
}