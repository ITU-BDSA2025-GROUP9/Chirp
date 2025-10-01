using Chirp.Shared;

namespace SimpleDB
{
    public static class DatabaseFactory
    {
        /// <summary>
        /// Creates a repository that talks to a remote HTTP service.
        /// </summary>
        public static IDatabaseRepository<Cheep> CreateHttp(string baseUrl)
        {
            return new HttpDatabaseRepository(baseUrl);
        }

        /// <summary>
        /// Creates a repository that persists data in a local CSV file.
        /// </summary>
        public static IDatabaseRepository<Cheep> CreateCsv(string filePath)
        {
            return CSVDatabase<Cheep>.GetInstance(
                filePath,
                CheepCsv.FromCsvLine,
                CheepCsv.ToCsvLine,
                c => (int)c.Timestamp
            );
        }
    }
}