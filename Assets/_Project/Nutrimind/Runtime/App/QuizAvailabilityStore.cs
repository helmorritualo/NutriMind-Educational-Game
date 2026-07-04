using System;
using System.Collections.Generic;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Typed store for the global assigned-quiz catalog and list UI preservation state.
    /// Populated by <see cref="QuizListCatalogLoader"/> and cleared on logout.
    /// </summary>
    public class QuizAvailabilityStore
    {
        /// <summary>The merged list of assigned quizzes across subjects/terms.</summary>
        public List<QuizDto> Quizzes { get; set; } = new();

        /// <summary>Whether the catalog has been loaded at least once this session.</summary>
        public bool IsLoaded { get; set; }

        /// <summary>UTC timestamp of the last successful catalog load.</summary>
        public DateTime? LoadedAt { get; set; }

        /// <summary>Active subject filter key: all, literaquest, healthquest, sciencequest.</summary>
        public string SelectedSubjectFilter { get; set; } = QuizSubjectFilters.All;

        /// <summary>Normalized vertical scroll position for the list ScrollRect.</summary>
        public float ScrollPosition { get; set; } = 1f;

        /// <summary>Current pagination page (1-based).</summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>Last row-selected quiz id (for upcoming View modal).</summary>
        public string? SelectedQuizId { get; set; }

        /// <summary>Resets all state to defaults.</summary>
        public void Reset()
        {
            Quizzes.Clear();
            IsLoaded = false;
            LoadedAt = null;
            SelectedSubjectFilter = QuizSubjectFilters.All;
            ScrollPosition = 1f;
            CurrentPage = 1;
            SelectedQuizId = null;
        }
    }
}
