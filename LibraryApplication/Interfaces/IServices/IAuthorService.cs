using LibraryApplication.DTOs.AuthorDtos;

namespace LibraryApplication.Interfaces;

public interface IAuthorService
{
    Task<List<AuthorDto>> GetAllAsync();
    Task<AuthorDto> GetByIdAsync(int id);
    Task<AuthorDto> CreateAsync(SaveAuthorDto dto);
    Task<AuthorDto> UpdateAsync(int id, SaveAuthorDto dto);
    Task DeleteAsync(int id);
}