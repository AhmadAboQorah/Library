using LibraryApplication.DTOs;
using LibraryApplication.DTOs.AuthDtos;

namespace LibraryApplication.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}