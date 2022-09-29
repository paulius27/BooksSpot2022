using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BooksSpot2022.Models;

namespace BooksSpot2022.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<BookReservation> BookReservations { get; set; }
        public DbSet<BookBorrowing> BookBorrowings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BookReservation>().HasKey(bookReservation => new { bookReservation.UserId, bookReservation.BookId });
            builder.Entity<BookBorrowing>().HasKey(bookBorrowing => new { bookBorrowing.UserId, bookBorrowing.BookId });
        }
    }
}