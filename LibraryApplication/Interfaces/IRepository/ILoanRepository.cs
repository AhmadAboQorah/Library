using LibraryDomain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApplication.Interfaces.IRepository
{
    public interface ILoanRepository
    {
        Task<Loan?> GetByIdAsync(int id);
        Task<List<Loan>> GetByUserAsync(int userId);
        Task<List<Loan>> GetOverdueAsync(DateTime nowUtc);
        Task<int> CountActiveByUserAsync(int userId);
        Task<bool> HasActiveLoanForBookAsync(int userId, int bookId);
        Task AddAsync(Loan loan);
    }
}
