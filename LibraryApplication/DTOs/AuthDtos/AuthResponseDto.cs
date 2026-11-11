using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApplication.DTOs.AuthDtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = null!;
    }
}
