using Chirp.Razor.Database;
using Chirp.Razor.Models;
using Chirp.Razor.DTO;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Chirp.Razor.Repositories;

public class CheepRepository : ICheepRepository
{
    private readonly ChirpDbContext _db;

    public CheepRepository(ChirpDbContext db)
    {
        _db = db; 
    }

	public async Task<int> CreateCheep(CheepDTO newCheep)
    {		
		var author = await _db.Authors.FirstOrDefaultAsync(a => a.Name == newCheep.Author);
        
		if (author == null)
            throw new Exception($"Author '{newCheep.Author}' not found.");
			// should actually probably create a new author

		var cheep = DTOToCheep(newCheep, author);

		var entry = await _db.Cheeps.AddAsync(cheep);
        await _db.SaveChangesAsync();

        return entry.Entity.CheepId; 
    }

	public async Task<List<CheepDTO>> ReadAllCheeps()
    {		
         var cheeps = await _db.Cheeps
            .Include(c => c.Author)
            .OrderByDescending(c => c.TimeStamp)
            .ToListAsync();

        return cheeps.Select(CheepToDTO).ToList();
    }

 	public async Task<List<CheepDTO>> ReadCheepsByAuthor(string authorName)
     {
          var cheeps = await _db.Cheeps
            .Include(c => c.Author)
            .Where(c => c.Author.Name == authorName)
            .OrderByDescending(c => c.TimeStamp)
            .ToListAsync();

        return cheeps.Select(CheepToDTO).ToList();
     }

	public static CheepDTO CheepToDTO(Cheep c) => new(
        c.Author.Name,
        c.Text,
        c.TimeStamp.ToLocalTime().ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture)
    );
    
	public static Cheep DTOToCheep(CheepDTO dto, Author author) => new Cheep
	{
    	Text = dto.Message,
    	Author = author,
    	TimeStamp = DateTime.ParseExact(dto.Timestamp, "MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture)
	};
}