namespace LibraryApplication.DTOs.UserDtos;

public class UserProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public int OverdueLoans { get; set; }
    public int ReturnedLoans { get; set; }
    public List<ProfileLoanDto> RecentLoans { get; set; } = new();
}

