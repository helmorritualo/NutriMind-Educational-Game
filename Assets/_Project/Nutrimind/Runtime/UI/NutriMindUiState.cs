using UnityEngine.UIElements;

namespace NutriMind.Runtime.UI
{
    public enum NutriMindUiStateType
    {
        Normal,
        Focused,
        Pressed,
        Selected,
        Disabled,
        Loading,
        Error,
        Success,
        Locked,
        Completed
    }

    public static class NutriMindUiState
    {
        public const string ClassNormal    = "nm-state-normal";
        public const string ClassFocused   = "nm-state-focused";
        public const string ClassPressed   = "nm-state-pressed";
        public const string ClassSelected  = "nm-state-selected";
        public const string ClassDisabled  = "nm-state-disabled";
        public const string ClassLoading   = "nm-state-loading";
        public const string ClassError     = "nm-state-error";
        public const string ClassSuccess   = "nm-state-success";
        public const string ClassLocked    = "nm-state-locked";
        public const string ClassCompleted = "nm-state-completed";

        private static readonly string[] AllStateClasses =
        {
            ClassNormal, ClassFocused, ClassPressed, ClassSelected,
            ClassDisabled, ClassLoading, ClassError, ClassSuccess,
            ClassLocked, ClassCompleted
        };

        public static void ApplyState(VisualElement element, NutriMindUiStateType state)
        {
            if (element == null) return;
            ClearState(element);
            element.AddToClassList(StateToClass(state));
            element.SetEnabled(state != NutriMindUiStateType.Disabled);
        }

        public static void ClearState(VisualElement element)
        {
            if (element == null) return;
            foreach (string cls in AllStateClasses)
            {
                if (element.ClassListContains(cls))
                    element.RemoveFromClassList(cls);
            }
        }

        public static void SetLoading(VisualElement element, bool isLoading)
        {
            if (element == null) return;
            if (isLoading)
                ApplyState(element, NutriMindUiStateType.Loading);
            else if (element.ClassListContains(ClassLoading))
                element.RemoveFromClassList(ClassLoading);
        }

        public static void SetDisabled(VisualElement element, bool isDisabled)
        {
            if (element == null) return;
            element.SetEnabled(!isDisabled);
            if (isDisabled)
                element.AddToClassList(ClassDisabled);
            else if (element.ClassListContains(ClassDisabled))
                element.RemoveFromClassList(ClassDisabled);
        }

        private static string StateToClass(NutriMindUiStateType state)
        {
            return state switch
            {
                NutriMindUiStateType.Normal    => ClassNormal,
                NutriMindUiStateType.Focused   => ClassFocused,
                NutriMindUiStateType.Pressed   => ClassPressed,
                NutriMindUiStateType.Selected  => ClassSelected,
                NutriMindUiStateType.Disabled  => ClassDisabled,
                NutriMindUiStateType.Loading   => ClassLoading,
                NutriMindUiStateType.Error     => ClassError,
                NutriMindUiStateType.Success   => ClassSuccess,
                NutriMindUiStateType.Locked    => ClassLocked,
                NutriMindUiStateType.Completed => ClassCompleted,
                _                              => ClassNormal
            };
        }
    }
}

