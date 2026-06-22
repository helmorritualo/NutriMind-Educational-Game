using System.Collections.Generic;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Typed store that holds unsubmitted student answers as a key-value dictionary.
    /// Maps question ID to answer object.
    /// </summary>
    public class QuizAnswerDraftStore
    {
        /// <summary>
        /// Key-value dictionary mapping question ID to answer object.
        /// </summary>
        public Dictionary<string, object> Answers { get; set; } = new();

        /// <summary>
        /// Sets/adds an answer for a given question ID.
        /// </summary>
        public void SetAnswer(string questionId, object answer)
        {
            Answers[questionId] = answer;
        }

        /// <summary>
        /// Gets an answer for a given question ID, if present.
        /// </summary>
        public object? GetAnswer(string questionId)
        {
            return Answers.TryGetValue(questionId, out var answer) ? answer : null;
        }

        /// <summary>
        /// Removes an answer for a given question ID.
        /// </summary>
        public bool RemoveAnswer(string questionId)
        {
            return Answers.Remove(questionId);
        }

        /// <summary>
        /// Checks if an answer exists for a given question ID.
        /// </summary>
        public bool HasAnswer(string questionId)
        {
            return Answers.ContainsKey(questionId);
        }

        /// <summary>Resets all state to defaults.</summary>
        public void Reset()
        {
            Answers.Clear();
        }
    }
}
