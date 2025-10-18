using System.Net.Http;
using System.Net.Http.Json;
using Chirp.Shared;
using System.Collections.Generic;

namespace SimpleDB
{
    /// <summary>
    /// Repository implementation that communicates with the remote Chirp
    /// web service using HTTP requests. Provides methods for adding and
    /// retrieving <see cref="Cheep"/> objects.
    /// </summary>
    public class HttpDatabaseRepository : IDatabaseRepository<Cheep>
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes a new instance of <see cref="HttpDatabaseRepository"/>.
        /// The base URL is fixed to the deployed Azure Chirp service.
        /// </summary>
        /// <param name="baseUrl">The base URL of the Chirp service (ignored; hardcoded to Azure URL).</param>
        public HttpDatabaseRepository(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));

            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
        }




        /// <summary>
        /// Sends a POST request to add a new cheep to the remote database.
        /// </summary>
        /// <param name="item">The <see cref="Cheep"/> to be stored.</param>
        public void Add(Cheep item)
        {
            var response = _httpClient.PostAsJsonAsync($"{_baseUrl}/cheep", item).Result;
            Console.WriteLine($"DEBUG: Posting to {_baseUrl}/cheep");

            response.EnsureSuccessStatusCode();

            response.EnsureSuccessStatusCode();
        }


        /// <summary>
        /// Sends a GET request to retrieve all cheeps from the remote database.
        /// </summary>
        /// <returns>A collection of <see cref="Cheep"/> objects.</returns>
        public IEnumerable<Cheep> GetAll()
        {
            var response = _httpClient.GetFromJsonAsync<List<Cheep>>($"{_baseUrl}/cheeps").Result;
            return response ?? new List<Cheep>();
        }
    }
}