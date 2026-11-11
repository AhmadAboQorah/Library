using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;
using LibraryInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryInfrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbcontext _db;
    public BookRepository(LibraryDbcontext db) => _db = db;

    private IQueryable<Book> WithDetails() =>
        _db.Books
           .Include(b => b.Category)
           .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author);

    public Task<List<Book>> SearchAsync(string? title, int? categoryId)
    {
        var query = WithDetails().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(b => b.Title.Contains(title.Trim()));

        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId.Value);

        return query.OrderBy(b => b.Title).ToListAsync();
    }

    public Task<Book?> GetByIdAsync(int id, bool track = false) =>
        (track ? WithDetails() : WithDetails().AsNoTracking())
            .FirstOrDefaultAsync(b => b.Id == id);

    public Task<bool> IsbnExistsAsync(string isbn, int? excludeBookId = null) =>
        _db.Books.AnyAsync(b => b.ISBN == isbn && (excludeBookId == null || b.Id != excludeBookId));

    public Task<bool> HasLoansAsync(int bookId) =>
        _db.Loans.AnyAsync(l => l.BookId == bookId);

    public async Task AddAsync(Book book) => await _db.Books.AddAsync(book);

    public void Remove(Book book) => _db.Books.Remove(book);
}