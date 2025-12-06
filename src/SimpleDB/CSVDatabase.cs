namespace SimpleDB
{
    /// <summary>
    /// A simple CSV-based database implementation that stores and retrieves records of type <typeparamref name="T"/>.
    /// Singleton: only one instance of this class can exist.
    /// </summary>
    /// <typeparam name="T">The type of objects stored in the database.</typeparam>
    public sealed class CSVDatabase<T> : IDatabaseRepository<T>
    {
        private readonly string _filePath;
        private readonly Func<string, T> _fromLine;
        private readonly Func<T, string> _toLine;
        private readonly Func<T, int> _getId;

        // The single instance of the database
        private static CSVDatabase<T>? _instance;
        
        /// <summary>
        /// Private constructor to prevent external instantiation.
        /// </summary>
        private CSVDatabase(string filePath, Func<string, T> fromLine, Func<T, string> toLine, Func<T, int> getId)
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
        /// Gets the single instance of the CSV database.
        /// If it does not exist yet, it will be created.
        /// </summary>
        public static CSVDatabase<T> GetInstance(string filePath, Func<string, T> fromLine, Func<T, string> toLine, Func<T, int> getId)
        {
            if (_instance == null)
            {
                _instance = new CSVDatabase<T>(filePath, fromLine, toLine, getId);
            }
            return _instance;
        }
        
        /// <summary>
        /// Appends given item to the underlying CSV file.
        /// </summary>
        /// <param name="item">The item to store.</param>
        public void Add(T item)
        {
            File.AppendAllLines(_filePath, new[] { _toLine(item) });
        }

        /// <summary>
        /// Reads all CSV lines from the file and returns them as objects.
        /// </summary>
        /// <returns>
        /// An enumerable collection of all stored objects.  
        /// </returns>
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
        /// Finds and returns the first object whose ID matches the provided value.
        /// </summary>
        /// <param name="id">The ID of the item to find.</param>
        /// <returns>
        /// The matching item.
        /// </returns>
        public T? FindById(int id)
        {
            return GetAll().FirstOrDefault(e => _getId(e) == id);
        }
        
        /// <summary>
        /// Removes an item with the specified ID from the CSV file.
        /// </summary>
        /// <param name="id">The ID of the item to remove.</param>
        /// <returns>
        /// true if an item was removed.  
        /// false if no item with the specified ID was found.
        /// </returns>
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
