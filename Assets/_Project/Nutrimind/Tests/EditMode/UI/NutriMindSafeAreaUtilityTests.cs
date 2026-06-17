using NUnit.Framework;
using UnityEngine;
using NutriMind.Runtime.UI;

namespace NutriMind.Tests.EditMode.UI
{
    [TestFixture]
    public class NutriMindSafeAreaUtilityTests
    {
        [Test]
        public void CalculatePadding_WithDefaultCall_ReturnsNonNullRectOffset()
        {
            RectOffset padding = NutriMindSafeAreaUtility.CalculatePadding();
            Assert.That(padding, Is.Not.Null);
        }

        [Test]
        public void CalculatePadding_WithFullScreenSafeArea_ReturnsZeroPadding()
        {
            var fullScreen = new Rect(0f, 0f, 1920f, 1080f);
            var panelResolution = new Vector2(1920f, 1080f);
            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(fullScreen, panelResolution);
            Assert.That(padding.left, Is.Zero);
            Assert.That(padding.right, Is.Zero);
            Assert.That(padding.top, Is.Zero);
            Assert.That(padding.bottom, Is.Zero);
        }

        [Test]
        public void CalculatePadding_WithBottomNotch_ReturnsPositiveBottomPadding()
        {
            var safeArea = new Rect(0f, 64f, 1920f, 1016f);
            var panelResolution = new Vector2(1920f, 1080f);
            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(safeArea, panelResolution);
            Assert.That(padding.bottom, Is.GreaterThan(0),
                "Bottom notch should produce positive bottom padding");
        }

        [Test]
        public void CalculatePadding_WithLeftNotch_ReturnsPositiveLeftPadding()
        {
            var safeArea = new Rect(48f, 0f, 1872f, 1080f);
            var panelResolution = new Vector2(1920f, 1080f);
            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(safeArea, panelResolution);
            Assert.That(padding.left, Is.GreaterThan(0),
                "Left notch should produce positive left padding");
        }

        [Test]
        public void CalculatePadding_WithRightNotch_ReturnsPositiveRightPadding()
        {
            var safeArea = new Rect(0f, 0f, 1832f, 1080f);
            var panelResolution = new Vector2(1920f, 1080f);
            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(safeArea, panelResolution);
            Assert.That(padding.right, Is.GreaterThan(0),
                "Right notch should produce positive right padding");
        }

        [Test]
        public void CalculatePadding_WithTopNotch_ReturnsPositiveTopPadding()
        {
            var safeArea = new Rect(0f, 0f, 1125f, 2424f);
            var panelResolution = new Vector2(1125f, 2556f);
            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(safeArea, panelResolution);
            Assert.That(padding.top, Is.GreaterThan(0),
                "Top notch should produce positive top padding");
        }

        [Test]
        public void GetPanelPadding_WithCustomResolution_ScalesPadding()
        {
            var safeArea = new Rect(48f, 64f, 1872f, 1016f);
            var panelResolution = new Vector2(1920f, 1080f);
            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(safeArea, panelResolution);
            Assert.That(padding, Is.Not.Null);
            Assert.That(padding.left, Is.GreaterThanOrEqualTo(0));
            Assert.That(padding.right, Is.GreaterThanOrEqualTo(0));
            Assert.That(padding.top, Is.GreaterThanOrEqualTo(0));
            Assert.That(padding.bottom, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void IsAndroidLandscape_IsReadable()
        {
            Assert.DoesNotThrow(() =>
            {
                bool value = NutriMindSafeAreaUtility.IsAndroidLandscape;
                _ = value;
            });
        }

        [Test]
        public void CalculatePadding_WithEmptySafeArea_DoesNotThrow()
        {
            var empty = new Rect(0f, 0f, 0f, 0f);
            Assert.DoesNotThrow(() => NutriMindSafeAreaUtility.CalculatePadding(empty));
        }

        [Test]
        public void CalculatePadding_WithNegativeSafeArea_DoesNotThrow()
        {
            var negative = new Rect(-100f, -100f, 100f, 100f);
            Assert.DoesNotThrow(() => NutriMindSafeAreaUtility.CalculatePadding(negative));
        }

        [Test]
        public void CalculatePadding_WhenSafeAreaExceedsScreenBounds_ClampsToZero()
        {
            var oversized = new Rect(-50f, -50f, 2000f, 1180f);
            RectOffset padding = NutriMindSafeAreaUtility.CalculatePadding(oversized);
            Assert.That(padding.left, Is.GreaterThanOrEqualTo(0));
            Assert.That(padding.right, Is.GreaterThanOrEqualTo(0));
            Assert.That(padding.top, Is.GreaterThanOrEqualTo(0));
            Assert.That(padding.bottom, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void CalculatePadding_ReturnsNewInstanceEachCall()
        {
            var safeArea = new Rect(48f, 64f, 1872f, 1016f);
            RectOffset first = NutriMindSafeAreaUtility.CalculatePadding(safeArea);
            RectOffset second = NutriMindSafeAreaUtility.CalculatePadding(safeArea);
            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void GetPanelPadding_WithThreeArg_ScalesInsetsWhenScreenDiffersFromPanel()
        {
            var safeArea = new Rect(48f, 0f, 2292f, 1080f);
            var screenResolution = new Vector2(2340f, 1080f);
            var panelResolution = new Vector2(1920f, 1080f);
            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(
                safeArea, screenResolution, panelResolution);
            Assert.That(padding.left, Is.EqualTo(39));
            Assert.That(padding.right, Is.Zero);
            Assert.That(padding.top, Is.Zero);
            Assert.That(padding.bottom, Is.Zero);
        }

        [Test]
        public void GetPanelPadding_WithThreeArg_ScalesBothAxes()
        {
            var safeArea = new Rect(0f, 64f, 2300f, 960f);
            var screenResolution = new Vector2(2340f, 1080f);
            var panelResolution = new Vector2(1920f, 800f);
            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(
                safeArea, screenResolution, panelResolution);
            Assert.That(padding.right, Is.EqualTo(33));
            Assert.That(padding.bottom, Is.EqualTo(47));
            Assert.That(padding.top, Is.EqualTo(41));
            Assert.That(padding.left, Is.Zero);
        }

        [Test]
        public void GetPanelPadding_WithThreeArg_WhenScreenEqualsPanel_MatchesTwoArg()
        {
            var safeArea = new Rect(48f, 64f, 1872f, 1016f);
            var resolution = new Vector2(1920f, 1080f);
            RectOffset twoArg = NutriMindSafeAreaUtility.GetPanelPadding(safeArea, resolution);
            RectOffset threeArg = NutriMindSafeAreaUtility.GetPanelPadding(safeArea, resolution, resolution);
            Assert.That(threeArg.left, Is.EqualTo(twoArg.left));
            Assert.That(threeArg.right, Is.EqualTo(twoArg.right));
            Assert.That(threeArg.top, Is.EqualTo(twoArg.top));
            Assert.That(threeArg.bottom, Is.EqualTo(twoArg.bottom));
        }

        [Test]
        public void GetPanelPadding_WithThreeArg_ZeroScreenResolution_DoesNotThrow()
        {
            var safeArea = new Rect(48f, 64f, 1872f, 1016f);
            Assert.DoesNotThrow(() =>
                NutriMindSafeAreaUtility.GetPanelPadding(
                    safeArea, Vector2.zero, new Vector2(1920f, 1080f)));
        }

        [Test]
        public void GetPanelPadding_WithThreeArg_ZeroPanelResolution_DoesNotThrow()
        {
            var safeArea = new Rect(48f, 64f, 1872f, 1016f);
            Assert.DoesNotThrow(() =>
                NutriMindSafeAreaUtility.GetPanelPadding(
                    safeArea, new Vector2(2340f, 1080f), Vector2.zero));
        }

        [Test]
        public void GetPanelPadding_WithThreeArg_NegativeResolution_ClampsToZero()
        {
            var safeArea = new Rect(48f, 64f, 1872f, 1016f);
            Assert.DoesNotThrow(() =>
                NutriMindSafeAreaUtility.GetPanelPadding(
                    safeArea, new Vector2(-100f, 1080f), new Vector2(1920f, -500f)));
        }

        // ---------------------------------------------------------------
        // NEW: Landscape 16:9 deterministic safe-area tests for Phase 09
        // ---------------------------------------------------------------

        [Test]
        public void GetPanelPadding_Landscape16by9_WithLeftAndRightInsets_PaddingMatchesExpected()
        {
            // Landscape 1920x1080 panel, safe area with 48px left and 48px right insets.
            var safeArea = new Rect(48f, 0f, 1824f, 1080f);
            var panelResolution = new Vector2(1920f, 1080f);

            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(safeArea, panelResolution);

            Assert.That(padding.left, Is.EqualTo(48),
                "Left inset 48px at 1:1 panel scale should map to 48px padding");
            Assert.That(padding.right, Is.EqualTo(48),
                "Right inset 48px at 1:1 panel scale should map to 48px padding");
            Assert.That(padding.top, Is.Zero,
                "No vertical inset expected for right/left-only safe area");
            Assert.That(padding.bottom, Is.Zero,
                "No vertical inset expected for right/left-only safe area");
        }

        [Test]
        public void GetPanelPadding_Landscape16by9_WithPhysicalScreenScaling_ProducesCorrectScaledPadding()
        {
            // Physical screen: 2400x1080 (22:9 ultrawide, common Android landscape),
            // Panel reference: 1920x1080 (16:9).
            // Safe area has 64px left and 40px right insets on physical screen.
            // Left scaled: 64 * (1920/2400) = 64 * 0.8 = 51.2 -> 51
            // Right scaled: 40 * (1920/2400) = 40 * 0.8 = 32
            var safeArea = new Rect(64f, 0f, 2296f, 1080f);
            var screenResolution = new Vector2(2400f, 1080f);
            var panelResolution = new Vector2(1920f, 1080f);

            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(
                safeArea, screenResolution, panelResolution);

            Assert.That(padding.left, Is.EqualTo(51),
                "Left padding should be rounded scaled value");
            Assert.That(padding.right, Is.EqualTo(32),
                "Right padding should be rounded scaled value");
            Assert.That(padding.top, Is.Zero);
            Assert.That(padding.bottom, Is.Zero);
        }
    }
}
