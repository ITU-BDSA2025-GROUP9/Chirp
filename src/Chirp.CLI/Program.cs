using Chirp.CLI;
using DocoptNet;
using Chirp.Shared;   // for Cheep
using SimpleDB;       // for DatabaseFactory



public class Program
{
    const string usage = @"Chirp CLI version.
    Usage:
      Chirp read [<limit>]
      Chirp cheep <message>
      Chirp (-h | --help)
      Chirp --version";

private static IDatabaseRepository<Cheep> _db = null!;
    private static IEnumerable<Cheep> _messages = [];

    public static void Main(string[] args)
    {
        var arguments = new Docopt().Apply(usage, args, version: "1.0", exit: true)!;

        var csvPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data", "chirp_cli_db.csv"));
        _db = DatabaseFactory.Create(csvPath);

        if (arguments["read"].IsTrue)
        {
            _messages = _db.GetAll().ToList();
            var limit = !arguments["<limit>"].IsNullOrEmpty
                ? int.Parse(arguments["<limit>"].ToString())
                : _messages.Count();
            UserInterface.PrintCheeps(_messages, limit);
        }
        else if (arguments["cheep"].IsTrue)
        {
            var message = arguments["<message>"].ToString();
            var cheep = new Cheep
            {
                Author = Environment.UserName,
                Message = message,
                Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
            };
            _db.Add(cheep);
            _messages = _db.GetAll().ToList();
            UserInterface.PrintCheeps(_messages, _messages.Count());
        }
    }
}