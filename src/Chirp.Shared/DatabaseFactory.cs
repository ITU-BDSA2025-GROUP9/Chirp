using SimpleDB;

namespace Chirp.Shared;

public static class DatabaseFactory
{
    public static CSVDatabase<Cheep> Create(string filePath)
    {
        return CSVDatabase<Cheep>.GetInstance(
            filePath: filePath,
            fromLine: CheepCsv.FromCsvLine,
            toLine: CheepCsv.ToCsvLine,
            getId: c => HashCode.Combine(c.Author, c.Message, c.Timestamp) // safer than timestamp
        );
    }
}