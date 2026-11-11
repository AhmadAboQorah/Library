using LibraryApplication.DTOs.AuthorDtos;
using LibraryApplication.Exceptions;
using LibraryApplication.Interfaces;
using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;

namespace LibraryApplication.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authors;
    private readonly IUnitOfWork _uow;

    public AuthorService(IAuthorRepository authors, IUnitOfWork uow)
    {
        _authors = authors;
        _uow = uow;
    }

    public async Task<List<AuthorDto>> GetAllAsync()
    {
        var list = await _authors.GetAllAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<AuthorDto> GetByIdAsync(int id)
    {
        var author = await _authors.GetByIdAsync(id)
            ?? throw new NotFoundException("Author not found.");
        return ToDto(author);
    }

    public async Task<AuthorDto> CreateAsync(SaveAuthorDto dto)
    {
        var author = new Author { Name = dto.Name.Trim(), Bio = dto.Bio?.Trim() };
        await _authors.AddAsync(author);
        await _uow.SaveChangesAsync();
        return ToDto(author);
    }

    public async Task<AuthorDto> UpdateAsync(int id, SaveAuthorDto dto)
    {
        var author = await _authors.GetByIdAsync(id)
            ?? throw new NotFoundException("Author not found.");

        author.Name = dto.Name.Trim();
        author.Bio = dto.Bio?.Trim();
        await _uow.SaveChangesAsync();
        return ToDto(author);
    }

    public async Task DeleteAsync(int id)
    {
        var author = await _authors.GetByIdAsync(id)
            ?? throw new NotFoundException("Author not found.");

        if (await _authors.HasBooksAsync(id))
            throw new BusinessRuleException("This author has books. Remove or change those books first.");

        _authors.Remove(author);
        await _uow.SaveChangesAsync();
    }

    private static AuthorDto ToDto(Author a) => new() { Id = a.Id, Name = a.Name, Bio = a.Bio };
}