using LibraryApplication.DTOs;
using LibraryApplication.DTOs.BookDtos;

namespace LibraryApplication.Interfaces;

public interface IBookService
{
    Task<PagedResult<BookDto>> SearchAsync(string? search, int? categoryId, int page, int pageSize);
    Task<BookDto> GetByIdAsync(int id);
    Task<BookDto> CreateAsync(CreateBookDto dto);
    Task<BookDto> UpdateAsync(int id, UpdateBookDto dto);
    Task DeleteAsync(int id);
}