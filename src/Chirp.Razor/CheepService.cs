using DefaultNamespace;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps();
    public List<CheepViewModel> GetCheepsFromAuthor(string author);
}

public class CheepService : ICheepService
{
       private readonly DBFacade _db;

        public CheepService(DBFacade db)
        {
            _db = db;
        }

        public List<CheepViewModel> GetCheeps()
        {
            return _db.GetCheeps();
        }

        public List<CheepViewModel> GetCheepsFromAuthor(string author)
        {
            return _db.GetCheepsFromAuthor(author);
        }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

}
