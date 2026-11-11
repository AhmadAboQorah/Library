using LibraryApplication.DTOs.CategoryDtos;
using LibraryApplication.Exceptions;
using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;

namespace LibraryApplication.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public CategoryService(ICategoryRepository categories, IUnitOfWork uow)
    {
        _categories = categories;
        _uow = uow;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var list = await _categories.GetAllAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _categories.GetByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");
        return ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(SaveCategoryDto dto)
    {
        var name = dto.Name.Trim();
        if (await _categories.NameExistsAsync(name))
            throw new BusinessRuleException("A category with this name already exists.");

        var category = new Category { Name = name };
        await _categories.AddAsync(category);
        await _uow.SaveChangesAsync();
        return ToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, SaveCategoryDto dto)
    {
        var category = await _categories.GetByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");

        var name = dto.Name.Trim();
        if (await _categories.NameExistsAsync(name, id))
            throw new BusinessRuleException("A category with this name already exists.");

        category.Name = name;
        await _uow.SaveChangesAsync();
        return ToDto(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categories.GetByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");

        if (await _categories.HasBooksAsync(id))
            throw new BusinessRuleException("This category contains books and cannot be deleted.");

        _categories.Remove(category);
        await _uow.SaveChangesAsync();
    }

    // تحويل Entity إلى DTO: لا نُرجع الـ Entity للخارج
    private static CategoryDto ToDto(Category c) => new() { Id = c.Id, Name = c.Name };
}