using LibraryApplication.Interfaces.IRepository;
using LibraryDomain.Entities;
using LibraryInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryInfrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LibraryDbcontext _db;
    public UserRepository(LibraryDbcontext db) => _db = db;

    public Task<User?> GetByIdAsync(int id) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<bool> EmailExistsAsync(string email) =>
        _db.Users.AnyAsync(u => u.Email == email);

    public async Task AddAsync(User user) => await _db.Users.AddAsync(user);
}