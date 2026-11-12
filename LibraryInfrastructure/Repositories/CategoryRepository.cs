using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;
using LibraryInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryInfrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly LibraryDbcontext _db;

        public CategoryRepository(LibraryDbcontext db)
        {
            _db = db;
        }
        public async Task AddAsync(Category category) => await _db.Categories.AddAsync(category);



     
        public async Task<List<Category>> GetAllAsync() => await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

        public async Task<Category?> GetByIdAsync(int id) => await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        
            
        

        public async Task<bool> HasBooksAsync(int categoryId) => await _db.Books.AnyAsync(b => b.CategoryId == categoryId);


        public Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
       _db.Categories.AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId));



        public void Remove(Category category) => _db.Categories.Remove(category);

    }
}
