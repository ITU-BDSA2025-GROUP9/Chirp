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
        if (string.IsNullOrWhiteSpace(authorName)) throw new ArgumentException("Author is required", nameof(authorName));
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException($"Pagenumber must be greater than 0. Invalid pagenumber: {pageNumber}");

        var comments = await _repo.GetCommentsByAuthor(authorName, pageNumber, pageSize);
        return CommentDTO.ToDtos(comments);
    }
    
    public async Task<int> AddComment(int cheepId, int authorId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Cheep text is required and cannot be null or empty", nameof(text));
        if (text.Length > 160) throw new ArgumentException("Cheep text cannot exceed 160 characters.", nameof(text));
        
        return await _repo.AddComment(cheepId, authorId, text);
    }
    
    public Task<bool> DeleteComment(int commentId)
        => _repo.DeleteComment(commentId);
  
}