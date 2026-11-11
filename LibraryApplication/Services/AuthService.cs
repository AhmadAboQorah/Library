using LibraryApplication.DTOs;
using LibraryApplication.DTOs.AuthDtos;
using LibraryApplication.Exceptions;
using LibraryApplication.Interfaces;
using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;
using LibraryDomain.Enums;

namespace LibraryApplication.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, IUnitOfWork uow,
                       IPasswordHasher hasher, ITokenService tokens)
    {
        _users = users;
        _uow = uow;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _users.EmailExistsAsync(email))
            throw new BusinessRuleException("This email is already registered.");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PasswordHash = _hasher.Hash(dto.Password),   // لا نخزن كلمة المرور الصريحة
            Role = UserRole.User                          // التسجيل العام ينشئ User فقط
        };

        await _users.AddAsync(user);
        await _uow.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());

        // نفس الرسالة في الحالتين حتى لا نكشف أي الإيميلات موجودة
        if (user == null || !_hasher.Verify(dto.Password, user.PasswordHash))
            throw new AuthenticationFailedException("Invalid email or password.");

        var (token, expires) = _tokens.CreateToken(user);
        return new AuthResponseDto { Token = token, ExpiresAt = expires, User = ToDto(user) };
    }

    private static UserDto ToDto(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Role = u.Role.ToString()
    };
}