using System.Net.Http;
using System.Net.Http.Json;
using Chirp.Shared;
using System.Collections.Generic;

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
            var response = _httpClient.PostAsJsonAsync($"{_baseUrl}/cheep", item).Result;
            response.EnsureSuccessStatusCode();
        }

        public IEnumerable<Cheep> GetAll()
        {
            var response = _httpClient.GetFromJsonAsync<List<Cheep>>($"{_baseUrl}/cheeps").Result;
            return response ?? new List<Cheep>();
        }

        public Cheep? FindById(int id)
        {
            var all = GetAll();
            return all.FirstOrDefault(c => c.Timestamp == id);
        }

        public bool Remove(int id)
        {
            return false;
        }
    }
}