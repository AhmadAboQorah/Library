using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryDomain.Entities
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? Nationality { get; set; }
        public DateTime? BirthDate { get; set; }
        public ICollection<BookAuthor>? BookAuthors { get; set; }

    }
}
