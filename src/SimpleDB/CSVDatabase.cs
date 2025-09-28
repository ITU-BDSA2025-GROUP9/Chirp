using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleDB
{
    public class CSVDatabase<T> : IDatabaseRepository<T>
    {
        private readonly string _filePath;
        private readonly Func<string, T> _fromCsv;
        private readonly Func<T, string> _toCsv;
        private readonly Func<T, int> _getId;

        private static CSVDatabase<T>? _instance;

        private CSVDatabase(string filePath, Func<string, T> fromCsv, Func<T, string> toCsv, Func<T, int> getId)
        {
            _filePath = filePath;
            _fromCsv = fromCsv;
            _toCsv = toCsv;
            _getId = getId;

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "");
            }
        }

        /// <summary>
        /// Singleton-like factory method to create or reuse a CSVDatabase instance.
        /// </summary>
        public static CSVDatabase<T> GetInstance(
            string filePath,
            Func<string, T> fromCsv,
            Func<T, string> toCsv,
            Func<T, int> getId)
        {
            _instance ??= new CSVDatabase<T>(filePath, fromCsv, toCsv, getId);
            return _instance;
        }

        public void Add(T item)
        {
            File.AppendAllText(_filePath, _toCsv(item) + Environment.NewLine);
        }

        public IEnumerable<T> GetAll()
        {
            if (!File.Exists(_filePath))
                return Enumerable.Empty<T>();

            var lines = File.ReadAllLines(_filePath)
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .ToList();

            if (lines.Count > 0 && lines[0].Contains("Author", StringComparison.OrdinalIgnoreCase))
                lines.RemoveAt(0);

            return lines.Select(_fromCsv);
        }

        public T? FindById(int id)
        {
            return GetAll().FirstOrDefault(item => _getId(item) == id);
        }

        public bool Remove(int id)
        {
            var allItems = GetAll().ToList();
            var itemToRemove = allItems.FirstOrDefault(i => _getId(i) == id);

            if (itemToRemove == null)
                return false;

            allItems.Remove(itemToRemove);

            var header = "Author,Message,Timestamp";
            var lines = new List<string> { header };
            lines.AddRange(allItems.Select(_toCsv));

            File.WriteAllLines(_filePath, lines);
            return true;
        }
    }
}
