using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApplication.DTOs.AuthorDtos;

public class AuthorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
}
