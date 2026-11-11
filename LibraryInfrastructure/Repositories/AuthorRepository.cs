using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;
using LibraryInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryInfrastructure.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly LibraryDbcontext _db;
    public AuthorRepository(LibraryDbcontext db) => _db = db;

    public Task<List<Author>> GetAllAsync() =>
        _db.Authors.AsNoTracking().OrderBy(a => a.Name).ToListAsync();

    public Task<Author?> GetByIdAsync(int id) =>
        _db.Authors.FirstOrDefaultAsync(a => a.Id == id);

    public Task<List<Author>> GetByIdsAsync(IEnumerable<int> ids) =>
        _db.Authors.Where(a => ids.Contains(a.Id)).ToListAsync();

    public Task<bool> HasBooksAsync(int authorId) =>
        _db.BookAuthors.AnyAsync(x => x.AuthorId == authorId);

    public async Task AddAsync(Author author) => await _db.Authors.AddAsync(author);

    public void Remove(Author author) => _db.Authors.Remove(author);
}