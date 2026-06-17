using NUnit.Framework;
using NutriMind.Runtime.UI;

namespace NutriMind.Tests.EditMode.UI
{
    [TestFixture]
    public class NutriMindComponentStyleTests
    {
        private string _componentsUss;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string fullPath = System.IO.Path.GetFullPath(NutriMindAssetPaths.StyleComponents);
            _componentsUss = System.IO.File.ReadAllText(fullPath);
        }

        // ===============================================================
        // Buttons: nm-btn variants
        // ===============================================================

        [Test]
        public void ComponentsUss_HasBaseButtonClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-btn"),
                "Components.uss must define the .nm-btn base button class");
        }

        [Test]
        public void ComponentsUss_HasPrimaryButtonClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-btn-primary"),
                "Components.uss must define the .nm-btn-primary variant class");
        }

        [Test]
        public void ComponentsUss_HasSecondaryButtonClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-btn-secondary"),
                "Components.uss must define the .nm-btn-secondary variant class");
        }

        [Test]
        public void ComponentsUss_HasBackButtonClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-btn-back"),
                "Components.uss must define the .nm-btn-back navigation button class. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasDestructiveButtonClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-btn-destructive"),
                "Components.uss must define the .nm-btn-destructive danger button class. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasIconButtonClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-btn-icon"),
                "Components.uss must define the .nm-btn-icon class for icon-only buttons. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        // ===============================================================
        // Navigation / Header classes
        // ===============================================================

        [Test]
        public void ComponentsUss_HasNavigationClasses()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-navbar"),
                "Components.uss must define the .nm-navbar class for top navigation bars");
            Assert.That(_componentsUss, Does.Contain(".nm-bottom-nav"),
                "Components.uss must define the .nm-bottom-nav class for bottom navigation");
        }

        // ===============================================================
        // Card variant classes: subject, term, station
        // ===============================================================

        [Test]
        public void ComponentsUss_HasBaseCardClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-card"),
                "Components.uss must define the .nm-card base card class");
        }

        [Test]
        public void ComponentsUss_HasSubjectCardClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-card-subject"),
                "Components.uss must define .nm-card-subject for subject selection cards. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasTermCardClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-card-term"),
                "Components.uss must define .nm-card-term for term selection cards. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasStationCardClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-card-station"),
                "Components.uss must define .nm-card-station for station cards. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        // ===============================================================
        // Specialized container classes
        // ===============================================================

        [Test]
        public void ComponentsUss_HasProfileSummaryClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-profile-summary"),
                "Components.uss must define .nm-profile-summary for profile summaries. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasSettingsRowClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-settings-row"),
                "Components.uss must define .nm-settings-row for settings rows. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasFieldClasses()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-field"),
                "Components.uss must define .nm-field for labeled input fields. " +
                "This is a Phase 09 addition not yet implemented.");
            Assert.That(_componentsUss, Does.Contain(".nm-pin-field"),
                "Components.uss must define .nm-pin-field for PIN input fields. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasSelectionControlClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-selection-control"),
                "Components.uss must define .nm-selection-control for radio/checkbox-style controls. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasProgressClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-progress"),
                "Components.uss must define .nm-progress for progress bars/rings. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasConnectionBadgeClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-connection-badge"),
                "Components.uss must define .nm-connection-badge for connection status indicators. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        // ===============================================================
        // Overlay / dialog / state classes
        // ===============================================================

        [Test]
        public void ComponentsUss_HasOverlayClasses()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-overlay"),
                "Components.uss must define .nm-overlay for generic overlays");
            Assert.That(_componentsUss, Does.Contain(".nm-dialog"),
                "Components.uss must define .nm-dialog for dialog popups");
            Assert.That(_componentsUss, Does.Contain(".nm-loading-overlay"),
                "Components.uss must define .nm-loading-overlay for loading state overlays");
        }

        [Test]
        public void ComponentsUss_HasEmptyStateClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-empty"),
                "Components.uss must define .nm-empty for empty state displays. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasFeedbackClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-feedback"),
                "Components.uss must define .nm-feedback for feedback/toast messages. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasRewardClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-reward"),
                "Components.uss must define .nm-reward for reward celebration displays. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasInteractionPromptClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-interaction-prompt"),
                "Components.uss must define .nm-interaction-prompt for tap/click prompts. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        [Test]
        public void ComponentsUss_HasTouchJoystickClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-touch-joystick"),
                "Components.uss must define .nm-touch-joystick for touch joystick controls. " +
                "This is a Phase 09 addition not yet implemented.");
        }

        // ===============================================================
        // Shared state classes — all 10 states
        // ===============================================================

        [Test]
        public void ComponentsUss_HasAllStateClasses()
        {
            Assert.That(_componentsUss, Does.Contain(NutriMindUiState.ClassNormal));
            Assert.That(_componentsUss, Does.Contain(NutriMindUiState.ClassFocused));
            Assert.That(_componentsUss, Does.Contain(NutriMindUiState.ClassPressed));
            Assert.That(_componentsUss, Does.Contain(NutriMindUiState.ClassSelected));
            Assert.That(_componentsUss, Does.Contain(NutriMindUiState.ClassDisabled));
            Assert.That(_componentsUss, Does.Contain(NutriMindUiState.ClassLoading));
            Assert.That(_componentsUss, Does.Contain(NutriMindUiState.ClassError));
            Assert.That(_componentsUss, Does.Contain(NutriMindUiState.ClassSuccess));
            Assert.That(_componentsUss, Does.Contain(NutriMindUiState.ClassLocked));
            Assert.That(_componentsUss, Does.Contain(NutriMindUiState.ClassCompleted));
        }

        // ===============================================================
        // Phase 04A — Mission / NPC / Hint / Discovery / Reflection / Feedback
        // ===============================================================

        [Test]
        public void ComponentsUss_HasMissionBriefClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-mission-brief"),
                "Components.uss must define .nm-mission-brief for mission brief overlays");
        }

        [Test]
        public void ComponentsUss_HasHintClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-hint"),
                "Components.uss must define .nm-hint for hint overlays");
        }

        [Test]
        public void ComponentsUss_HasReflectionClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-reflection"),
                "Components.uss must define .nm-reflection for reflection overlays");
        }

        [Test]
        public void ComponentsUss_HasNpcDialogueClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-npc-dialogue"),
                "Components.uss must define .nm-npc-dialogue for NPC dialogue displays");
        }

        [Test]
        public void ComponentsUss_HasNpcPortraitClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-npc-portrait"),
                "Components.uss must define .nm-npc-portrait for NPC portrait containers");
        }

        [Test]
        public void ComponentsUss_HasDiscoveryClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-discovery"),
                "Components.uss must define .nm-discovery for discovery cards");
        }

        [Test]
        public void ComponentsUss_HasFeedbackPanelClass()
        {
            Assert.That(_componentsUss, Does.Contain(".nm-feedback-panel"),
                "Components.uss must define .nm-feedback-panel for feedback panels");
        }
    }
}
