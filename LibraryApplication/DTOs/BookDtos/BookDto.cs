using LibraryApplication.DTOs.AuthorDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApplication.DTOs.BookDtos
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int PublishYear { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<AuthorDto> Authors { get; set; } = new();
    }

}
