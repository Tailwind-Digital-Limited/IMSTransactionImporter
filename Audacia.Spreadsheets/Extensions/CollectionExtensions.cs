using System;
using System.Collections.Generic;

namespace Audacia.Spreadsheets.Extensions
{
     /// <summary>
    /// Extension methods for the type <see cref="ICollection{T}"/>.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds <paramref name="item"/> to <paramref name="collection"/> if it doesn't already exist within it.
        /// </summary>
        /// <typeparam name="T">The type of the collection <paramref name="collection"/> and item <paramref name="item"/>.</typeparam>
        /// <param name="collection">A collection of type <typeparamref name="T"/>.</param>
        /// <param name="item">Item to add to <paramref name="collection"/>.</param>
        public static void AddIfNotContains<T>(this ICollection<T> collection, T item)
        {
            if (collection?.Contains(item) == false)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of the specified enumerable to the end of the collection.
        /// </summary>
        /// <typeparam name="T">The type of the collections <paramref name="destination"/> and <paramref name="source"/>.</typeparam>
        /// <param name="destination">A collection of type <typeparamref name="T"/>.</param>
        /// <param name="source">A collection of type <typeparamref name="T"/> to add to <paramref name="destination"/>.</param>
        /// <exception cref="InvalidOperationException">Throws if collection is readonly.</exception>
        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            if (source == null || destination == null)
            {
                return;
            }

            // Prevent add range on arrays
            if (destination.IsReadOnly)
            {
                throw new InvalidOperationException("Collection is read-only.");
            }

            if (destination is List<T> list)
            {
                list.AddRange(source);
                return;
            }

            foreach (var item in source)
            {
                destination.Add(item);
            }
        }

        /// <summary>
        /// Removes the elements of the specified enumerable from the collection.
        /// </summary>
        /// <typeparam name="T">The type of the collections <paramref name="destination"/> and <paramref name="source"/>.</typeparam>
        /// <param name="destination">A collection of type <typeparamref name="T"/>.</param>
        /// <param name="source">A collection of type <typeparamref name="T"/> to remove from <paramref name="destination"/>.</param>
        /// <exception cref="InvalidOperationException">Throws if collection is readonly.</exception>
        public static void RemoveARange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            if (source == null || destination == null)
            {
                return;
            }

            // Prevent remove range on arrays
            if (destination.IsReadOnly)
            {
                throw new InvalidOperationException("Collection is read-only.");
            }

            if (ReferenceEquals(destination, source))
            {
                destination.Clear();
                return;
            }

            foreach (var item in source)
            {
                destination.Remove(item);
            }
        }
    }
}