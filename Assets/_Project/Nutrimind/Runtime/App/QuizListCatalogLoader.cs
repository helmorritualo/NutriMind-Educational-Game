using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Loads all assigned quizzes by multi-fetching subject/term scoped provider calls.
    /// ponytail: avoids IGameDataProvider signature change until global GET /quizzes lands.
    /// </summary>
    public static class QuizListCatalogLoader
    {
        public static async Task<DataResult<bool>> LoadAllAsync(
            IGameDataProvider provider,
            QuizAvailabilityStore store,
            CancellationToken ct = default)
        {
            if (provider == null)
                return DataResult<bool>.Fail(new DataProviderError("VALIDATION_ERROR", "Data provider is required."));

            if (store == null)
                return DataResult<bool>.Fail(new DataProviderError("VALIDATION_ERROR", "Quiz availability store is required."));

            var subjectsResult = await provider.GetSubjectsAsync(ct).ConfigureAwait(false);
            if (subjectsResult == null || !subjectsResult.Success || subjectsResult.Data == null)
            {
                string message = subjectsResult?.Error?.Message ?? "Could not load subjects.";
                return DataResult<bool>.Fail(subjectsResult?.Error ?? new DataProviderError("NETWORK_ERROR", message));
            }

            var merged = new Dictionary<string, QuizDto>(StringComparer.Ordinal);

            foreach (var subject in subjectsResult.Data)
            {
                ct.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(subject?.Slug)) continue;

                var termsResult = await provider.GetTermsAsync(subject.Slug, ct).ConfigureAwait(false);
                if (termsResult == null || !termsResult.Success || termsResult.Data == null)
                    continue;

                foreach (var term in termsResult.Data)
                {
                    ct.ThrowIfCancellationRequested();
                    if (term?.TermNumber == null) continue;

                    var quizzesResult = await provider
                        .GetQuizzesAsync(subject.Slug, term.TermNumber.Value, ct)
                        .ConfigureAwait(false);

                    if (quizzesResult == null || !quizzesResult.Success || quizzesResult.Data?.Quizzes == null)
                        continue;

                    foreach (var quiz in quizzesResult.Data.Quizzes)
                    {
                        if (quiz?.Id == null) continue;
                        merged[quiz.Id] = quiz;
                    }
                }
            }

            store.Quizzes.Clear();
            store.Quizzes.AddRange(merged.Values);
            store.IsLoaded = true;
            store.LoadedAt = DateTime.UtcNow;
            return DataResult<bool>.Ok(true);
        }
    }
}
