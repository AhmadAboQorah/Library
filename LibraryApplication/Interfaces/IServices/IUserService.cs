using LibraryApplication.DTOs;
using LibraryApplication.DTOs.UserDtos;

namespace LibraryApplication.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
}