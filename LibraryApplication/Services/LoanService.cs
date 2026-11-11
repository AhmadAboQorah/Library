using LibraryApplication.DTOs.CreateLoanDtos;
using LibraryApplication.Exceptions;
using LibraryApplication.Interfaces;
using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;

namespace LibraryApplication.Services;

public class LoanService : ILoanService
{
    private const int LoanDays = 14;
    private const int MaxActiveLoans = 3;

    private readonly ILoanRepository _loans;
    private readonly IBookRepository _books;
    private readonly IUnitOfWork _uow;

    public LoanService(ILoanRepository loans, IBookRepository books, IUnitOfWork uow)
    {
        _loans = loans;
        _books = books;
        _uow = uow;
    }

    public async Task<LoanDto> BorrowAsync(int userId, int bookId)
    {
        var book = await _books.GetByIdAsync(bookId, track: true)
            ?? throw new NotFoundException("Book not found.");

        // قاعدة 1: لا نسخ متاحة
        if (book.AvailableCopies <= 0)
            throw new BusinessRuleException("This book is not available right now (no copies left).");

        // قاعدة 2: الحد الأقصى 3 إعارات نشطة
        if (await _loans.CountActiveByUserAsync(userId) >= MaxActiveLoans)
            throw new BusinessRuleException(
                $"You cannot borrow more than {MaxActiveLoans} books at the same time.");

        // قاعدة إضافية: لا يستعير نفس الكتاب مرتين وهو لم يُرجعه
        if (await _loans.HasActiveLoanForBookAsync(userId, bookId))
            throw new BusinessRuleException("You already have an active loan for this book.");

        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = bookId,
            UserId = userId,
            LoanDate = now,
            DueDate = now.AddDays(LoanDays),   // قاعدة 3: 14 يوماً
            Book = book
        };
        book.AvailableCopies--;

        await _loans.AddAsync(loan);
        await _uow.SaveChangesAsync();   // الإعارة وإنقاص النسخ يُحفظان معاً
        return ToDto(loan, now);
    }

    public async Task<LoanDto> ReturnAsync(int userId, int loanId)
    {
        var loan = await _loans.GetByIdAsync(loanId)
            ?? throw new NotFoundException("Loan not found.");

        if (loan.UserId != userId)
            throw new ForbiddenException("You can only return your own loans.");

        if (loan.ReturnDate != null)
            throw new BusinessRuleException("This book was already returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Book.AvailableCopies++;

        await _uow.SaveChangesAsync();
        return ToDto(loan, now);
    }

    public async Task<List<LoanDto>> GetMyLoansAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var list = await _loans.GetByUserAsync(userId);
        return list.Select(l => ToDto(l, now)).ToList();
    }

    public async Task<List<LoanDto>> GetOverdueAsync()
    {
        var now = DateTime.UtcNow;
        var list = await _loans.GetOverdueAsync(now);
        return list.Select(l => ToDto(l, now)).ToList();
    }

    private static LoanDto ToDto(Loan l, DateTime now)
    {
        // متأخرة = لم تُرجع وتجاوزت تاريخ الاستحقاق
        var overdue = l.ReturnDate == null && now > l.DueDate;
        return new LoanDto
        {
            Id = l.Id,
            BookId = l.BookId,
            BookTitle = l.Book?.Title ?? string.Empty,
            UserId = l.UserId,
            UserName = l.User?.FullName ?? string.Empty,
            LoanDate = l.LoanDate,
            DueDate = l.DueDate,
            ReturnDate = l.ReturnDate,
            IsOverdue = overdue,
            Status = l.ReturnDate != null ? "Returned" : overdue ? "Overdue" : "Active"
        };
    }
}