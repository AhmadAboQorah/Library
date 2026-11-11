using LibraryDomain.Entities;

namespace LibraryApplication.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}