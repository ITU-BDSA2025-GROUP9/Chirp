using Chirp.CLI;
using DocoptNet;
using Chirp.Shared;
using SimpleDB;

/// <summary>
/// Entry point for the Chirp Command Line Interface (CLI).
/// Allows users to read and post "cheeps" (short messages)
/// against the Azure-hosted Chirp database service.
/// </summary>
public class Program
{
    /// <summary>
    /// Command-line usage instructions parsed by Docopt.
    /// </summary>
    const string usage = @"Chirp CLI version.
    Usage:
      Chirp read [<limit>]
      Chirp cheep <message>
      Chirp (-h | --help)
      Chirp --version";

    /// <summary>
    /// Database instance used to store and retrieve cheeps.
    /// </summary>
    private static IDatabaseRepository<Cheep> _db = null!;

    /// <summary>
    /// Cached list of cheeps loaded from the database.
    /// </summary>
    private static IEnumerable<Cheep> _messages = [];

    /// <summary>
    /// Application entry point.
    /// Parses arguments, connects to the database service,
    /// and executes the requested action (read or cheep).
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var arguments = new Docopt().Apply(usage, args, version: "1.0", exit: true)!;

        var serviceUrl = "https://bdsagroup9chirpremotedb-hdhbcsgjhqanaxgy.norwayeast-01.azurewebsites.net";
        _db = DatabaseFactory.Create(serviceUrl);

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
