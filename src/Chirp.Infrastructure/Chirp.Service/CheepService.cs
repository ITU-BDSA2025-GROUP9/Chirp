using Chirp.Core.DTO;
using Chirp.Core.Interfaces;
using System.Globalization;

namespace Chirp.Infrastructure.Service;

public class CheepService : ICheepService
{
    private readonly IRepository _repository;

    public CheepService(IRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<CheepDTO> GetCheeps(int pageNumber, int pageSize)
        => _repository.GetAllCheeps(pageNumber, pageSize)
            .Select(c => new CheepDTO
            {
                AuthorName = c.Author.Name,
                AuthorEmail = c.Author.Email,
                Text = c.Text,
                TimeStamp = c.TimeStamp
            });

    public IEnumerable<CheepDTO> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize)
        => _repository.GetCheepsByAuthor(authorName, pageNumber, pageSize)
            .Select(c => new CheepDTO
            {
                AuthorName = c.Author.Name,
                AuthorEmail = c.Author.Email,
                Text = c.Text,
                TimeStamp = c.TimeStamp
            });

    public void AddCheep(string authorName, string authorEmail, string text)
        => _repository.AddCheep(authorName, authorEmail, text);
}