using Chirp.Shared;

namespace SimpleDB
{
    /// <summary>
    /// Factory that returns either a local CSV-based database or an HTTP-based one.
    /// </summary>
    public static class DatabaseFactory
    {
        public static IDatabaseRepository<Cheep> Create(string source)
        {
            if (source.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // Remote HTTP service
                return new HttpDatabaseRepository(source);
            }

            // Local CSV file
            return CSVDatabase<Cheep>.GetInstance(
                source,
                CheepCsv.FromCsvLine,   // parse line -> Cheep
                CheepCsv.ToCsvLine,     // Cheep -> CSV line
                c => (int)c.Timestamp
            );
        }
    }
}