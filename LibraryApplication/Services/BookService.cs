using LibraryApplication.DTOs.AuthorDtos;
using LibraryApplication.DTOs.BookDtos;
using LibraryApplication.Exceptions;
using LibraryApplication.Interfaces;
using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;

namespace LibraryApplication.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _books;
    private readonly IAuthorRepository _authors;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public BookService(IBookRepository books, IAuthorRepository authors,
                       ICategoryRepository categories, IUnitOfWork uow)
    {
        _books = books;
        _authors = authors;
        _categories = categories;
        _uow = uow;
    }

    public async Task<List<BookDto>> SearchAsync(string? title, int? categoryId)
    {
        var list = await _books.SearchAsync(title, categoryId);
        return list.Select(ToDto).ToList();
    }

    public async Task<BookDto> GetByIdAsync(int id)
    {
        var book = await _books.GetByIdAsync(id)
            ?? throw new NotFoundException("Book not found.");
        return ToDto(book);
    }

    public async Task<BookDto> CreateAsync(CreateBookDto dto)
    {
        var authorIds = dto.AuthorIds.Distinct().ToList();
        await ValidateAsync(dto, authorIds, null);

        var book = new Book
        {
            Title = dto.Title.Trim(),
            ISBN = dto.ISBN.Trim(),
            PublishYear = dto.PublishYear,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies,  
            CategoryId = dto.CategoryId
        };
        foreach (var authorId in authorIds)
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });

        await _books.AddAsync(book);
        await _uow.SaveChangesAsync();
        return await GetByIdAsync(book.Id);   
    }

    public async Task<BookDto> UpdateAsync(int id, UpdateBookDto dto)
    {
        var book = await _books.GetByIdAsync(id, track: true)
            ?? throw new NotFoundException("Book not found.");

        var authorIds = dto.AuthorIds.Distinct().ToList();
        await ValidateAsync(dto, authorIds, id);

        var onLoan = book.TotalCopies - book.AvailableCopies;
        if (dto.TotalCopies < onLoan)
            throw new BusinessRuleException(
                $"TotalCopies cannot be less than the {onLoan} copies currently on loan.");

        book.Title = dto.Title.Trim();
        book.ISBN = dto.ISBN.Trim();
        book.PublishYear = dto.PublishYear;
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = dto.TotalCopies - onLoan;
        book.CategoryId = dto.CategoryId;

        var toRemove = book.BookAuthors.Where(x => !authorIds.Contains(x.AuthorId)).ToList();
        foreach (var link in toRemove)
            book.BookAuthors.Remove(link);

        foreach (var authorId in authorIds.Where(a => book.BookAuthors.All(x => x.AuthorId != a)))
            book.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var book = await _books.GetByIdAsync(id, track: true)
            ?? throw new NotFoundException("Book not found.");

        if (await _books.HasLoansAsync(id))
            throw new BusinessRuleException("This book has loan history and cannot be deleted.");

        _books.Remove(book);
        await _uow.SaveChangesAsync();
    }

    private async Task ValidateAsync(CreateBookDto dto, List<int> authorIds, int? excludeBookId)
    {
        if (await _categories.GetByIdAsync(dto.CategoryId) == null)
            throw new BusinessRuleException("The selected category does not exist.");

        var found = await _authors.GetByIdsAsync(authorIds);
        if (found.Count != authorIds.Count)
            throw new BusinessRuleException("One or more selected authors do not exist.");

        if (await _books.IsbnExistsAsync(dto.ISBN.Trim(), excludeBookId))
            throw new BusinessRuleException("A book with this ISBN already exists.");
    }

    private static BookDto ToDto(Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        ISBN = b.ISBN,
        PublishYear = b.PublishYear,
        TotalCopies = b.TotalCopies,
        AvailableCopies = b.AvailableCopies,
        CategoryId = b.CategoryId,
        CategoryName = b.Category?.Name ?? string.Empty,
        Authors = b.BookAuthors
            .Where(x => x.Author != null)
            .Select(x => new AuthorDto { Id = x.Author.Id, Name = x.Author.Name, Bio = x.Author.Bio })
            .ToList()
    };
}