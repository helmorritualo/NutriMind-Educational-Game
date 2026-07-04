namespace NutriMind.Runtime.App
{
    /// <summary>Subject filter keys for Available Quiz List.</summary>
    public static class QuizSubjectFilters
    {
        public const string All = "all";
        public const string LiteraQuest = "literaquest";
        public const string PeHealth = "healthquest";
        public const string Science = "sciencequest";

        public static bool Matches(string? subjectSlug, string filterKey)
        {
            if (filterKey == All) return true;
            if (string.IsNullOrWhiteSpace(subjectSlug)) return false;
            return string.Equals(subjectSlug, filterKey, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
