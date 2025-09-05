using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleDB
{
    /// <summary>
    /// A simple CSV-based database implementation that stores and retrieves records of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects stored in the database.</typeparam>
    public sealed class CSVDatabase<T> : IDatabaseRepository<T>
    {
        private readonly string _filePath;
        private readonly Func<string, T> _fromLine;
        private readonly Func<T, string> _toLine;
        private readonly Func<T, int> _getId;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSVDatabase{T}"/> class.
        /// </summary>
        /// <param name="filePath">Path to the CSV file used for persistence.</param>
        /// <param name="fromLine">Function that converts a CSV line into an object of type <typeparamref name="T"/>.</param>
        /// <param name="toLine">Function that converts an object of type <typeparamref name="T"/> into a CSV line.</param>
        /// <param name="getId">Function that extracts the unique identifier from an object of type <typeparamref name="T"/>.</param>
        public CSVDatabase(string filePath, Func<string, T> fromLine, Func<T, string> toLine, Func<T, int> getId) 
        {
            _filePath = filePath;
            _fromLine = fromLine;
            _toLine = toLine;
            _getId = getId;

            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose();
            }
        }

        /// <summary>
        /// Adds a new item to the database.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            File.AppendAllLines(_filePath, new[] { _toLine(item) });
        }

        /// <summary>
        /// Retrieves all items from the database.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all stored items.</returns>
        public IEnumerable<T> GetAll()
        {
            if (!File.Exists(_filePath)) yield break;
            foreach (var line in File.ReadAllLines(_filePath))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    yield return _fromLine(line);
            }
        }

        /// <summary>
        /// Finds an item in the database by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the item.</param>
        /// <returns>The item if found; otherwise <c>null</c>.</returns>
        public T? FindById(int id)
        {
            return GetAll().FirstOrDefault(e => _getId(e) == id);
        }

        /// <summary>
        /// Removes an item from the database by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the item to remove.</param>
        /// <returns><c>true</c> if an item was removed; otherwise <c>false</c>.</returns>
        public bool Remove(int id)
        {
            var items = GetAll().ToList();
            var kept = items.Where(e => _getId(e) != id).ToList();
            if (kept.Count == items.Count) return false;
            File.WriteAllLines(_filePath, kept.Select(_toLine));
            return true;
        }
    }
}
