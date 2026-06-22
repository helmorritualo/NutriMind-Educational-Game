using System;
using System.Collections.Generic;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Global service for validating draft quiz answers.
    /// </summary>
    public class QuizValidationService
    {
        /// <summary>
        /// Validates that all items in the quiz detail have been answered in the draft store.
        /// </summary>
        /// <param name="quizDetail">The quiz definition and its items.</param>
        /// <param name="draftStore">The store holding current answer drafts.</param>
        /// <param name="unansweredItemIds">Output list of item IDs that have not been answered.</param>
        /// <returns>True if all items are successfully answered; otherwise, false.</returns>
        public bool ValidateDraft(QuizDetailDto quizDetail, QuizAnswerDraftStore draftStore, out List<string> unansweredItemIds)
        {
            unansweredItemIds = new List<string>();

            if (quizDetail == null)
            {
                // If there's no quiz detail, we can't validate, so return false.
                return false;
            }

            if (quizDetail.Items == null)
            {
                // If there are no items, there is nothing to validate/unanswered.
                return true;
            }

            foreach (var item in quizDetail.Items)
            {
                if (item == null || string.IsNullOrEmpty(item.Id))
                {
                    continue;
                }

                bool isAnswered = false;

                if (draftStore != null && draftStore.Answers != null)
                {
                    if (draftStore.Answers.TryGetValue(item.Id, out var answer) && answer != null)
                    {
                        if (answer is string str)
                        {
                            isAnswered = !string.IsNullOrWhiteSpace(str);
                        }
                        else if (answer is System.Collections.ICollection collection)
                        {
                            isAnswered = collection.Count > 0;
                        }
                        else
                        {
                            isAnswered = true;
                        }
                    }
                }

                if (!isAnswered)
                {
                    unansweredItemIds.Add(item.Id);
                }
            }

            return unansweredItemIds.Count == 0;
        }
    }
}
