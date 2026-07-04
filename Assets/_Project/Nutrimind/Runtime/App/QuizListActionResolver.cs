using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App
{
    public enum QuizListRowActionKind
    {
        Start,
        View,
        ViewResult,
        Disabled
    }

    /// <summary>Pure helpers for quiz list row display and action routing.</summary>
    public static class QuizListActionResolver
    {
        public const int PageSize = 5;

        public static QuizListRowActionKind Resolve(QuizDto? quiz)
        {
            if (quiz == null) return QuizListRowActionKind.Disabled;

            string state = quiz.State?.ToLowerInvariant() ?? string.Empty;

            if (state == "completed")
                return QuizListRowActionKind.ViewResult;

            if (state == "unlocked" && quiz.IsAvailable != false)
                return QuizListRowActionKind.Start;

            if (state == "locked" || quiz.IsAvailable == false)
                return QuizListRowActionKind.View;

            return QuizListRowActionKind.Disabled;
        }

        public static string GetActionLabel(QuizListRowActionKind kind) => kind switch
        {
            QuizListRowActionKind.Start => "Start",
            QuizListRowActionKind.View => "View",
            QuizListRowActionKind.ViewResult => "View Result",
            QuizListRowActionKind.Disabled => "—",
            _ => "—"
        };

        public static string FormatSubjectLabel(string? slug) => slug?.ToLowerInvariant() switch
        {
            QuizSubjectFilters.LiteraQuest => "LiteraQuest",
            QuizSubjectFilters.PeHealth => "PE/Health",
            QuizSubjectFilters.Science => "Science",
            _ => slug ?? "—"
        };

        public static string FormatStatusLabel(string? state) => state?.ToLowerInvariant() switch
        {
            "unlocked" => "Available",
            "locked" => "Locked",
            "completed" => "Completed",
            _ => state ?? "—"
        };

        public static bool IsAvailableStatus(string? state) =>
            string.Equals(state, "unlocked", System.StringComparison.OrdinalIgnoreCase);

        public static bool IsLockedStatus(string? state) =>
            string.Equals(state, "locked", System.StringComparison.OrdinalIgnoreCase);

        public static int GetTotalPages(int itemCount) =>
            itemCount <= 0 ? 1 : (itemCount + PageSize - 1) / PageSize;

        public static int ClampPage(int page, int itemCount)
        {
            int totalPages = GetTotalPages(itemCount);
            if (page < 1) return 1;
            if (page > totalPages) return totalPages;
            return page;
        }

        public static (int startIndex, int endIndexExclusive) GetPageSlice(int page, int itemCount)
        {
            if (itemCount <= 0) return (0, 0);
            int clamped = ClampPage(page, itemCount);
            int start = (clamped - 1) * PageSize;
            int end = System.Math.Min(start + PageSize, itemCount);
            return (start, end);
        }

        public static string FormatPageSummary(int page, int itemCount)
        {
            if (itemCount <= 0) return "Showing 0 of 0 quizzes";

            var (start, endEx) = GetPageSlice(page, itemCount);
            int startDisplay = start + 1;
            int endDisplay = endEx;
            return $"Showing {startDisplay}-{endDisplay} of {itemCount} quizzes";
        }
    }
}
