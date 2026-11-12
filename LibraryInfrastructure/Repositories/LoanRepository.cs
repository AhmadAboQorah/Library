using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;
using LibraryInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryInfrastructure.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly LibraryDbcontext _db;

        public LoanRepository(LibraryDbcontext db)
        {
            _db = db;
        }

    // ... باقي الدوال

        public async Task AddAsync(Loan loan) => await _db.Loans.AddAsync(loan);


        public async Task<int> CountActiveByUserAsync(int userId) => await _db.Loans.CountAsync(l => l.UserId == userId && l.ReturnDate == null);


        public Task<Loan?> GetByIdAsync(int id) => _db.Loans.Include(l => l.Book).Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id);


        public Task<List<Loan>> GetByUserAsync(int userId) => _db.Loans.Include(l => l.Book).Where(l => l.UserId == userId).ToListAsync();


        public Task<List<Loan>> GetOverdueAsync(DateTime nowUtc) => _db.Loans.Include(l => l.Book).Include(l => l.User).Where(l => l.DueDate < nowUtc && l.ReturnDate == null).ToListAsync();


        public Task<bool> HasActiveLoanForBookAsync(int userId, int bookId) => _db.Loans.AnyAsync(l =>l.UserId == userId && l.BookId == bookId && l.ReturnDate == null);
        
    }
}
