using System;
using System.Collections.Generic;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Interface for quiz item presenters.
    /// Represents the presentation layer for a single quiz item.
    /// </summary>
    public interface IQuizItemPresenter
    {
        /// <summary>
        /// Binds the presenter to a specific quiz item and the answer draft store.
        /// </summary>
        void Bind(QuizItemDto item, QuizAnswerDraftStore draftStore);
    }

    /// <summary>
    /// Manages a registry of presenters for different quiz item types,
    /// providing factory-based creation of presenter instances.
    /// </summary>
    public class QuizItemPresenterRegistry
    {
        private readonly Dictionary<string, Func<IQuizItemPresenter>> _factories = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers a factory function for a specific quiz item type.
        /// </summary>
        public void Register(string itemType, Func<IQuizItemPresenter> factory)
        {
            if (string.IsNullOrEmpty(itemType))
                throw new ArgumentException("Item type cannot be null or empty.", nameof(itemType));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _factories[itemType] = factory;
        }

        /// <summary>
        /// Unregisters the factory for a specific quiz item type.
        /// </summary>
        public bool Unregister(string itemType)
        {
            if (string.IsNullOrEmpty(itemType))
                return false;

            return _factories.Remove(itemType);
        }

        /// <summary>
        /// Checks if a presenter factory is registered for the specified item type.
        /// </summary>
        public bool HasPresenter(string itemType)
        {
            if (string.IsNullOrEmpty(itemType))
                return false;

            return _factories.ContainsKey(itemType);
        }

        /// <summary>
        /// Creates a presenter instance for the specified item type.
        /// </summary>
        public IQuizItemPresenter CreatePresenter(string itemType)
        {
            if (string.IsNullOrEmpty(itemType))
                throw new ArgumentException("Item type cannot be null or empty.", nameof(itemType));

            if (_factories.TryGetValue(itemType, out var factory))
            {
                return factory();
            }

            throw new KeyNotFoundException($"No presenter factory registered for quiz item type '{itemType}'.");
        }

        /// <summary>
        /// Clears all registered presenter factories.
        /// </summary>
        public void Clear()
        {
            _factories.Clear();
        }
    }
}
