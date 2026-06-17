using NUnit.Framework;
using NutriMind.Runtime.UI;

namespace NutriMind.Tests.EditMode.UI
{
    [TestFixture]
    public class NutriMindAppShellLayerTests
    {
        private string _appShellContent;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string fullPath = System.IO.Path.GetFullPath(NutriMindAssetPaths.AppShell);
            _appShellContent = System.IO.File.ReadAllText(fullPath);
        }

        [Test]
        public void AppShell_HasBackgroundLayer()
        {
            Assert.That(_appShellContent, Does.Contain("name=\"background-layer\""),
                "AppShell must define a 'background-layer' element for the app background");
            Assert.That(_appShellContent, Does.Contain("class=\"nm-app-background\""),
                "background-layer must use the .nm-app-background class");
        }

        [Test]
        public void AppShell_HasScreenLayer()
        {
            Assert.That(_appShellContent, Does.Contain("name=\"screen-layer\""),
                "AppShell must define a 'screen-layer' element for screen content");
            Assert.That(_appShellContent, Does.Contain("class=\"nm-app-screen\""),
                "screen-layer must use the .nm-app-screen class");
        }

        [Test]
        public void AppShell_HasWorldHudLayer()
        {
            Assert.That(_appShellContent, Does.Contain("name=\"world-hud-layer\""),
                "AppShell must define a 'world-hud-layer' element for world-space HUD overlays. " +
                "This layer has not been created yet.");
        }

        [Test]
        public void AppShell_HasModalLayer()
        {
            Assert.That(_appShellContent, Does.Contain("name=\"modal-layer\""),
                "AppShell must define a 'modal-layer' element for modal dialogs");
        }

        [Test]
        public void AppShell_HasBlockingLayer()
        {
            Assert.That(_appShellContent, Does.Contain("name=\"blocking-layer\""),
                "AppShell must define a 'blocking-layer' element for loading/blocking overlays");
        }

        [Test]
        public void AppShell_HasToastLayer()
        {
            Assert.That(_appShellContent, Does.Contain("name=\"toast-layer\""),
                "AppShell must define a 'toast-layer' element for toast notifications");
        }

        [Test]
        public void AppShell_HasAccessibilityAnnouncer()
        {
            Assert.That(_appShellContent, Does.Contain("name=\"accessibility-announcer\""),
                "AppShell must define an 'accessibility-announcer' element");
            Assert.That(_appShellContent, Does.Contain("class=\"nm-sr-only\""),
                "accessibility-announcer must use the .nm-sr-only class for screen-reader-only positioning");
        }
    }
}
