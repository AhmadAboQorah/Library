using LibraryApplication.DTOs.CategoryDtos;

namespace LibraryApplication.Interfaces.IRepository;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(SaveCategoryDto dto);
    Task<CategoryDto> UpdateAsync(int id, SaveCategoryDto dto);
    Task DeleteAsync(int id);
}