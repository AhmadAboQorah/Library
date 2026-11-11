using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LibraryApplication.DTOs.BookDtos
{
    public class CreateBookDto
    {
        [Required, StringLength(250)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(20, MinimumLength = 10)]
        public string ISBN { get; set; } = string.Empty;

        [Range(1000, 2100)]
        public int PublishYear { get; set; }

        [Range(1, 10000)]
        public int TotalCopies { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Required, MinLength(1, ErrorMessage = "A book needs at least one author")]
        public List<int> AuthorIds { get; set; } = new();
    }

}
