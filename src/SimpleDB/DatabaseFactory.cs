using Chirp.Shared;

namespace SimpleDB
{
    public static class DatabaseFactory
    {
        public static IDatabaseRepository<Cheep> Create(string source)
        {
            if (source.StartsWith("http"))
            {
                return new HttpDatabaseRepository(source);
            }

            // Local CSV database
            return CSVDatabase<Cheep>.GetInstance(
                source,
                CheepCsv.FromCsvLine,   // parse line -> Cheep
                CheepCsv.ToCsvLine,     // Cheep -> CSV line
                c => (int)c.Timestamp
            );
        }
    }
}
