using LibraryDomain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApplication.Interfaces.IRepository
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<bool> HasBooksAsync(int categoryId);
        Task AddAsync(Category category);
        void Remove(Category category);
    }
}
