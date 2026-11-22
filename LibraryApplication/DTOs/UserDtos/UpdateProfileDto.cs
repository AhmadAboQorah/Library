using System.ComponentModel.DataAnnotations;

namespace LibraryApplication.DTOs.UserDtos;

public class UpdateProfileDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required"),
        RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
    [Phone(ErrorMessage = "Invalid phone number"),MaxLength(10),MinLength(10)]
    public string? PhoneNumber { get; set; }
}