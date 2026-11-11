using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LibraryApplication.DTOs.CreateLoanDtos
{
    public class CreateLoanDto
    {
        [Range(1, int.MaxValue)]
        public int BookId { get; set; }
    }
}
