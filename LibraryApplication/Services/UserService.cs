using LibraryApplication.DTOs;
using LibraryApplication.DTOs.UserDtos;
using LibraryApplication.Exceptions;
using LibraryApplication.Interfaces;
using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;

namespace LibraryApplication.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly ILoanRepository _loans;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;

    public UserService(IUserRepository users, ILoanRepository loans,
                       IUnitOfWork uow, IPasswordHasher hasher)
    {
        _users = users;
        _loans = loans;
        _uow = uow;
        _hasher = hasher;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found.");

        var now = DateTime.UtcNow;
        var all = (await _loans.GetByUserAsync(userId))
            .OrderByDescending(l => l.LoanDate)
            .Select(l => new ProfileLoanDto
            {
                Id = l.Id,
                BookId = l.BookId,
                BookTitle = l.Book?.Title ?? string.Empty,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                Status = l.ReturnDate != null ? "Returned"
                       : now > l.DueDate ? "Overdue" : "Active"
            }).ToList();

        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            PhoneNumber = user.PhoneNumber,
            TotalLoans = all.Count,
            ActiveLoans = all.Count(x => x.Status == "Active"),
            OverdueLoans = all.Count(x => x.Status == "Overdue"),
            ReturnedLoans = all.Count(x => x.Status == "Returned"),
            RecentLoans = all.Take(5).ToList()
        };
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found.");

        user.FullName = dto.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
        await _uow.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found.");

        if (!_hasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new BusinessRuleException("Current password is incorrect.");

        if (dto.CurrentPassword == dto.NewPassword)
            throw new BusinessRuleException("The new password must be different from the current one.");

        user.PasswordHash = _hasher.Hash(dto.NewPassword);
        await _uow.SaveChangesAsync();
    }
}