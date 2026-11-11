using LibraryApplication.Interfaces;
using LibraryDomain.Entities;
using LibraryDomain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryInfrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(LibraryDbcontext db, IPasswordHasher hasher)
    {
        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new User
            {
                FullName = "Library Admin",
                Email = "admin@library.com",
                PasswordHash = hasher.Hash("Admin@123"),
                Role = UserRole.Admin
            });
            db.Users.Add(new User
            {
                FullName = "Demo Member",
                Email = "user@library.com",
                PasswordHash = hasher.Hash("User@123"),
                Role = UserRole.User
            });
            await db.SaveChangesAsync();
        }

        if (await db.Books.AnyAsync()) return;

        var programming = new Category { Name = "Programming" };
        var fiction = new Category { Name = "Fiction" };
        var martin = new Author { Name = "Robert C. Martin", Bio = "Author of Clean Code." };
        var orwell = new Author { Name = "George Orwell", Bio = "English novelist." };

        var cleanCode = new Book
        {
            Title = "Clean Code",
            ISBN = "9780132350884",
            PublishYear = 2008,
            TotalCopies = 3,
            AvailableCopies = 3,
            Category = programming
        };
        cleanCode.BookAuthors.Add(new BookAuthor { Author = martin });

        var nineteen = new Book
        {
            Title = "1984",
            ISBN = "9780451524935",
            PublishYear = 1949,
            TotalCopies = 2,
            AvailableCopies = 2,
            Category = fiction
        };
        nineteen.BookAuthors.Add(new BookAuthor { Author = orwell });

        db.Books.AddRange(cleanCode, nineteen);
        await db.SaveChangesAsync();
    }
}