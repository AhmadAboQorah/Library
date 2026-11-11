using LibraryDomain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApplication.Interfaces.IRepository
{
    public interface IBookRepository
    {
        Task<List<Book>> SearchAsync(string? title, int? categoryId);
        Task<Book?> GetByIdAsync(int id, bool track = false);
        Task<bool> IsbnExistsAsync(string isbn, int? excludeBookId = null);
        Task<bool> HasLoansAsync(int bookId);
        Task AddAsync(Book book);
        void Remove(Book book);
    }
}
