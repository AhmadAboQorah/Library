using LibraryDomain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApplication.Interfaces.IRepository
{
    public interface IAuthorRepository
    {
        Task<List<Author>> GetAllAsync();
        Task<Author?> GetByIdAsync(int id);
        Task<List<Author>> GetByIdsAsync(IEnumerable<int> ids);
        Task<bool> HasBooksAsync(int authorId);
        Task AddAsync(Author author);
        void Remove(Author author);
    }
}
