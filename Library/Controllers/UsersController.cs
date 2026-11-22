using Library.Extensions;
using LibraryApplication.DTOs;
using LibraryApplication.DTOs.UserDtos;
using LibraryApplication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

[ApiController]
[Route("api/users/me")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    public UsersController(IUserService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetProfile() =>
        Ok(await _service.GetProfileAsync(User.GetUserId()));

    [HttpPut]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileDto dto) =>
        Ok(await _service.UpdateProfileAsync(User.GetUserId(), dto));

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        await _service.ChangePasswordAsync(User.GetUserId(), dto);
        return NoContent();
    }
}