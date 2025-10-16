namespace Chirp.Razor.DTO
{
    public class CheepDTO {
        public string Author { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;

        public CheepDTO(string Author, string Message, string Timestamp)
        {
			if (string.IsNullOrWhiteSpace(Author))
                throw new ArgumentException($"Author cannot be null or empty. Invalid author: '{Author}'");

            if (string.IsNullOrWhiteSpace(Message))
                throw new ArgumentException($"Message cannot be null or empty. Invalid message: '{Message}'");
            
            this.Author = Author;
            this.Message = Message;
            this.Timestamp = Timestamp;
        }
    }
}