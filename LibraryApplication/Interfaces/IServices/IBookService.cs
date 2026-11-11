using LibraryApplication.DTOs.BookDtos;

namespace LibraryApplication.Interfaces;

public interface IBookService
{
    Task<List<BookDto>> SearchAsync(string? title, int? categoryId);
    Task<BookDto> GetByIdAsync(int id);
    Task<BookDto> CreateAsync(CreateBookDto dto);
    Task<BookDto> UpdateAsync(int id, UpdateBookDto dto);
    Task DeleteAsync(int id);
}