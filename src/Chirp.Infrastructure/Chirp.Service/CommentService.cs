using System.Globalization;
using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;

namespace Chirp.Infrastructure.Chirp.Service;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _repo;

    public CommentService(ICommentRepository repo) => _repo = repo;
    
    public async Task<List<CommentDTO>> GetCommentsForCheep(int cheepId)
    {
        var comments = await _repo.GetCommentsForCheep(cheepId);
        return CommentDTO.ToDtos(comments);
    }

    public async Task<List<CommentDTO>> GetCommentsByAuthor(string authorName, int pageNumber, int pageSize)
    {
        var comments = await _repo.GetCommentsByAuthor(authorName, pageNumber, pageSize);
        return CommentDTO.ToDtos(comments);
    }
    
    public async Task<int> AddComment(Cheep cheep, Author author, string text)
    {
        return await _repo.AddComment(cheep, author, text);
    }
    
    public Task<bool> DeleteComment(int commentId)
        => _repo.DeleteComment(commentId);
  
}