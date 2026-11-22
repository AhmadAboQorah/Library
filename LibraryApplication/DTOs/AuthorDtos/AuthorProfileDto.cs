namespace LibraryApplication.DTOs.AuthorDtos;

public class AuthorProfileDto : AuthorDto
{
    public int BooksCount { get; set; }
    public List<AuthorBookDto> Books { get; set; } = new();
}

public class AuthorBookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishYear { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int AvailableCopies { get; set; }
    public int TotalCopies { get; set; }
}