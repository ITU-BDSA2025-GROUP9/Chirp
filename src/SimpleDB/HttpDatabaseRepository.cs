using System.Net.Http;
using System.Net.Http.Json;
using Chirp.Shared;

namespace SimpleDB
{
    public class HttpDatabaseRepository : IDatabaseRepository<Cheep>
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public HttpDatabaseRepository(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
        }

        public void Add(Cheep item)
        {
            var response = _httpClient.PostAsJsonAsync($"{_baseUrl}/cheep", item).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public IEnumerable<Cheep> GetAll()
        {
            var response = _httpClient.GetFromJsonAsync<List<Cheep>>($"{_baseUrl}/cheeps")
                .GetAwaiter()
                .GetResult();
            return response ?? new List<Cheep>();
        }

        public Cheep? FindById(int id)
        {
            return GetAll().FirstOrDefault(c => c.Timestamp == id);
        }

        public bool Remove(int id)
        {
            // Not implemented on the API
            return false;
        }
    }
}