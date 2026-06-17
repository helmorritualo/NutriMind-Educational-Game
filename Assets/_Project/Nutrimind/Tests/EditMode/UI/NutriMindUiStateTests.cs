using NUnit.Framework;
using UnityEngine.UIElements;
using NutriMind.Runtime.UI;

namespace NutriMind.Tests.EditMode.UI
{
    /// <summary>
    /// EditMode tests for <see cref="NutriMindUiState"/> — the UI Toolkit state-class
    /// helper that applies / removes USS pseudo-state classes on a VisualElement.
    /// </summary>
    [TestFixture]
    public class NutriMindUiStateTests
    {
        private VisualElement _element;

        [SetUp]
        public void SetUp()
        {
            _element = new VisualElement();
        }

        [TearDown]
        public void TearDown()
        {
            _element = null;
        }

        // ---------------------------------------------------------------
        // Individual state-class application
        // ---------------------------------------------------------------

        [Test]
        public void ApplyState_WithNormal_AddsNormalClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Normal);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassNormal), Is.True);
        }

        [Test]
        public void ApplyState_WithFocused_AddsFocusedClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Focused);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassFocused), Is.True);
        }

        [Test]
        public void ApplyState_WithPressed_AddsPressedClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Pressed);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassPressed), Is.True);
        }

        [Test]
        public void ApplyState_WithSelected_AddsSelectedClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Selected);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassSelected), Is.True);
        }

        [Test]
        public void ApplyState_WithDisabled_AddsDisabledClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Disabled);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassDisabled), Is.True);
        }

        [Test]
        public void ApplyState_WithDisabled_DisablesElementInternally()
        {
            Assert.That(_element.enabledSelf, Is.True, "Precondition: element starts enabled");

            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Disabled);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassDisabled), Is.True,
                "Disabled class should be present");
            Assert.That(_element.enabledSelf, Is.False,
                "Element should be internally disabled");
        }

        [Test]
        public void ApplyState_WithLoading_AddsLoadingClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Loading);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassLoading), Is.True);
        }

        [Test]
        public void ApplyState_WithError_AddsErrorClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Error);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassError), Is.True);
        }

        [Test]
        public void ApplyState_WithSuccess_AddsSuccessClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Success);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassSuccess), Is.True);
        }

        [Test]
        public void ApplyState_WithLocked_AddsLockedClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Locked);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassLocked), Is.True);
        }

        [Test]
        public void ApplyState_WithCompleted_AddsCompletedClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Completed);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassCompleted), Is.True);
        }

        // ---------------------------------------------------------------
        // State transition — applying a new state removes the previous one
        // ---------------------------------------------------------------

        [Test]
        public void ApplyState_WhenTransitioningFromLoadingToSuccess_RemovesLoadingClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Loading);
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Success);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassLoading), Is.False,
                "Previous state class should be removed");
            Assert.That(_element.ClassListContains(NutriMindUiState.ClassSuccess), Is.True,
                "New state class should be added");
        }

        [Test]
        public void ApplyState_WhenTransitioningFromErrorToNormal_RemovesErrorClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Error);
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Normal);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassError), Is.False);
            Assert.That(_element.ClassListContains(NutriMindUiState.ClassNormal), Is.True);
        }

        [Test]
        public void ApplyState_WhenTransitioningFromDisabledToNormal_ReenablesElement()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Disabled);
            Assert.That(_element.enabledSelf, Is.False, "Precondition: element is disabled");

            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Normal);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassDisabled), Is.False,
                "Disabled class should be removed");
            Assert.That(_element.ClassListContains(NutriMindUiState.ClassNormal), Is.True,
                "Normal class should be added");
            Assert.That(_element.enabledSelf, Is.True,
                "Element should be re-enabled");
        }

        // ---------------------------------------------------------------
        // ClearState removes all state classes
        // ---------------------------------------------------------------

        [Test]
        public void ClearState_WhenMultipleStatesApplied_RemovesAllStateClasses()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Loading);
            _element.AddToClassList("some-other-class");

            NutriMindUiState.ClearState(_element);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassNormal), Is.False);
            Assert.That(_element.ClassListContains(NutriMindUiState.ClassLoading), Is.False);
            Assert.That(_element.ClassListContains(NutriMindUiState.ClassFocused), Is.False);
            // Non-state classes are not touched
            Assert.That(_element.ClassListContains("some-other-class"), Is.True);
        }

        // ---------------------------------------------------------------
        // Convenience setter — SetLoading(bool)
        // ---------------------------------------------------------------

        [Test]
        public void SetLoading_WithTrue_AddsLoadingClassAndClearsOtherState()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Normal);

            NutriMindUiState.SetLoading(_element, true);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassLoading), Is.True);
            Assert.That(_element.ClassListContains(NutriMindUiState.ClassNormal), Is.False,
                "Normal class should be removed when loading is set");
        }

        [Test]
        public void SetLoading_WithFalse_RemovesLoadingClass()
        {
            NutriMindUiState.ApplyState(_element, NutriMindUiStateType.Loading);

            NutriMindUiState.SetLoading(_element, false);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassLoading), Is.False);
        }

        // ---------------------------------------------------------------
        // Convenience setter — SetDisabled(bool)
        // ---------------------------------------------------------------

        [Test]
        public void SetDisabled_WithTrue_AddsDisabledClass()
        {
            NutriMindUiState.SetDisabled(_element, true);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassDisabled), Is.True);
        }

        [Test]
        public void SetDisabled_WithFalse_RemovesDisabledClass()
        {
            NutriMindUiState.SetDisabled(_element, true);
            NutriMindUiState.SetDisabled(_element, false);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassDisabled), Is.False);
        }

        // ---------------------------------------------------------------
        // SetDisabled functional — also disables the element internally
        // ---------------------------------------------------------------

        [Test]
        public void SetDisabled_WithTrue_DisablesElementInternally()
        {
            Assert.That(_element.enabledSelf, Is.True, "Precondition: element starts enabled");

            NutriMindUiState.SetDisabled(_element, true);

            Assert.That(_element.enabledSelf, Is.False,
                "Element should be internally disabled");
        }

        [Test]
        public void SetDisabled_WithFalse_ReenablesElement()
        {
            NutriMindUiState.SetDisabled(_element, true);
            Assert.That(_element.enabledSelf, Is.False, "Precondition: element is disabled");

            NutriMindUiState.SetDisabled(_element, false);

            Assert.That(_element.enabledSelf, Is.True,
                "Element should be re-enabled");
        }

        [Test]
        public void SetDisabled_WithTrue_AddsDisabledClassAndDisablesInternally()
        {
            NutriMindUiState.SetDisabled(_element, true);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassDisabled), Is.True,
                "Disabled class should be present");
            Assert.That(_element.enabledSelf, Is.False,
                "Element should be internally disabled");
        }

        [Test]
        public void SetDisabled_WithFalse_RemovesDisabledClassAndReenables()
        {
            NutriMindUiState.SetDisabled(_element, true);
            NutriMindUiState.SetDisabled(_element, false);

            Assert.That(_element.ClassListContains(NutriMindUiState.ClassDisabled), Is.False,
                "Disabled class should be removed");
            Assert.That(_element.enabledSelf, Is.True,
                "Element should be re-enabled");
        }

        // ---------------------------------------------------------------
        // Edge cases — null element
        // ---------------------------------------------------------------

        [Test]
        public void ApplyState_WithNullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => NutriMindUiState.ApplyState(null, NutriMindUiStateType.Normal));
        }

        [Test]
        public void ClearState_WithNullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => NutriMindUiState.ClearState(null));
        }

        [Test]
        public void SetLoading_WithNullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => NutriMindUiState.SetLoading(null, true));
        }

        [Test]
        public void SetDisabled_WithNullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => NutriMindUiState.SetDisabled(null, true));
        }
    }
}
