using Chirp.Shared;

namespace SimpleDB
{
    public static class DatabaseFactory
    {
        public static IDatabaseRepository<Cheep> Create(string source)
        {
            // force HTTP usage:
            return new HttpDatabaseRepository("https://bdsagroup9chirpremotedb-hdhbcsgjhqanaxgy.norwayeast-01.azurewebsites.net");
        }
        
    }
}
