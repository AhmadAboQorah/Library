using LibraryApplication.DTOs.CreateLoanDtos;

namespace LibraryApplication.Interfaces;

public interface ILoanService
{
    Task<LoanDto> BorrowAsync(int userId, int bookId);
    Task<LoanDto> ReturnAsync(int userId, int loanId);
    Task<List<LoanDto>> GetMyLoansAsync(int userId);
    Task<List<LoanDto>> GetOverdueAsync();
}