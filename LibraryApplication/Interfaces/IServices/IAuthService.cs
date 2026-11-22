using LibraryApplication.DTOs.AuthDtos;
using LibraryApplication.DTOs.UserDtos;

namespace LibraryApplication.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}