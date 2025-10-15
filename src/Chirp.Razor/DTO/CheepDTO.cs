namespace Chirp.Razor.DTO
{
    public class CheepDTO {
        public string Author { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;

        public CheepDTO(string Author, string Message, string Timestamp)
        {
            this.Author = Author;
            this.Message = Message;
            this.Timestamp = Timestamp;
        }
    }
}