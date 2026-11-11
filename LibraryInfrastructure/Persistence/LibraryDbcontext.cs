using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LibraryDomain.Entities;
using LibraryApplication.Interfaces.IRepository;


namespace LibraryInfrastructure.Persistence
{
    public class LibraryDbcontext : DbContext , IUnitOfWork
    {
        public LibraryDbcontext(DbContextOptions<LibraryDbcontext> options) : base(options)
        {
        }
        public DbSet<LibraryDomain.Entities.Book> Books { get; set; }
        public DbSet<LibraryDomain.Entities.Author> Authors { get; set; }
        public DbSet<LibraryDomain.Entities.Category> Categories { get; set; }
        public DbSet<LibraryDomain.Entities.Loan> Loans { get; set; }
        public DbSet<LibraryDomain.Entities.User> Users { get; set; }
        public DbSet<LibraryDomain.Entities.BookAuthor> BookAuthors { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });


            modelBuilder.Entity<BookAuthor>()
    .HasOne(ba => ba.Book).WithMany(b => b.BookAuthors).HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author).WithMany(a => a.BookAuthors).HasForeignKey(ba => ba.AuthorId);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category).WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Book).WithMany(b => b.Loans)
                .HasForeignKey(l => l.BookId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.User).WithMany(u => u.Loans)
                .HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>(e =>
            {
                e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                e.Property(u => u.Email).IsRequired().HasMaxLength(150);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);
                e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
                e.Property(u => u.PhoneNumber).HasMaxLength(30);
            });

            modelBuilder.Entity<Category>(e =>
            {
                e.Property(c => c.Name).IsRequired().HasMaxLength(100);
                e.HasIndex(c => c.Name).IsUnique();
            });

            modelBuilder.Entity<Author>(e =>
            {
                e.Property(a => a.Name).IsRequired().HasMaxLength(150);
                e.Property(a => a.Bio).HasMaxLength(1000);
            });

            modelBuilder.Entity<Book>(e =>
            {
                e.Property(b => b.Title).IsRequired().HasMaxLength(250);
                e.Property(b => b.ISBN).IsRequired().HasMaxLength(20);
                e.HasIndex(b => b.ISBN).IsUnique();
            });

        }

    }
}
