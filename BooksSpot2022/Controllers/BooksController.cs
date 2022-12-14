using BooksSpot2022.Auth;
using BooksSpot2022.Data;
using BooksSpot2022.DTOs;
using BooksSpot2022.Models;
using BooksSpot2022.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BooksSpot2022.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{bookId}")]
        public async Task<IActionResult> Get(Guid bookId)
        {
            var book = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);

            if (book == null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Book not found." });

            return Ok(book);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var books = _context.Books.ToList();

            if (books.Count < 1)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "No books found." });
            
            return Ok(books);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Book book)
        {
            await _context.Books.AddAsync(book);

            if (await _context.SaveChangesAsync() < 1)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Book creation failed." });

            return Ok(new Response { Status = "Success", Message = "Book created successfully!" });
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Book book)
        {
            var bookToUpdate = _context.Books.Find(book.Id);

            if (bookToUpdate == null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Book not found." });

            _context.Entry(bookToUpdate).CurrentValues.SetValues(book);

            if (await _context.SaveChangesAsync() < 1)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Book updating failed." });

            return Ok(new Response { Status = "Success", Message = "Book updated successfully!" });

        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{bookId}")]
        public async Task<IActionResult> Delete(Guid bookId)
        {
            var book = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);

            if (book == null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Book not found." });

            _context.Books.Remove(book);

            if (await _context.SaveChangesAsync() < 1)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Book deletion failed." });

            return Ok(new Response { Status = "Success", Message = "Book deleted successfully!" });
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] BooksSearchParamsDTO searchParamsDTO)
        {
            var books = _context.Books.AsQueryable();

            if (searchParamsDTO.Title != null)
                books = books.Where(book => book.Title.ToLower().Contains(searchParamsDTO.Title.ToLower()));

            if (searchParamsDTO.Author != null)
                books = books.Where(book => book.Author.ToLower().Contains(searchParamsDTO.Author.ToLower()));

            if (searchParamsDTO.Publisher != null)
                books = books.Where(book => book.Publisher.ToLower().Contains(searchParamsDTO.Publisher.ToLower()));

            if (searchParamsDTO.PublishingYear != null)
                books = books.Where(book => book.PublishingDate.Year == searchParamsDTO.PublishingYear);

            if (searchParamsDTO.Genre != null)
                books = books.Where(book => book.Genre.ToLower().Contains(searchParamsDTO.Genre.ToLower()));

            if (searchParamsDTO.ISBN != null)
                books = books.Where(book => book.ISBN.Contains(searchParamsDTO.ISBN));

            if (searchParamsDTO.Status != null)
                books = books.Where(book => book.Status == searchParamsDTO.Status);

            var searchResults = books.ToList();

            if (searchResults.Count < 1)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "No books found." });

            return Ok(books);
        }

        [Authorize]
        [HttpPut("{bookId}/reserve")]
        public async Task<IActionResult> Reserve(Guid bookId)
        {
            var userId = Guid.Parse(User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value);
            var book = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);

            if (book == null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Book not found." });

            if (book.Status != BookStatus.Available)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Book is not available." });

            var reservation = new BookReservation { UserId = userId, BookId = bookId };
            _context.BookReservations.Add(reservation);

            book.Status = BookStatus.Reserved;

            if (await _context.SaveChangesAsync() < 1)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Book reservation failed." });

            return Ok(new Response { Status = "Success", Message = "Book reserved successfully!" });
        }

        [Authorize]
        [HttpPut("{bookId}/borrow")]
        public async Task<IActionResult> Borrow(Guid bookId)
        {
            var userId = Guid.Parse(User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value);
            var book = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);

            if (book == null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Book not found." });

            // Check book status

            var previousBookStatus = book.Status;

            if (previousBookStatus == BookStatus.Borrowed)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Book is already borrowed." });

            var reservation = _context.BookReservations.Where(br => br.UserId == userId && br.BookId == book.Id).FirstOrDefault();

            if (previousBookStatus == BookStatus.Reserved && reservation == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Book is already reserved by different user." });
            
            // Borrow the book

            var borrowing = new BookBorrowing { UserId = userId, BookId = bookId };
            _context.BookBorrowings.Add(borrowing);

            book.Status = BookStatus.Borrowed;

            if (previousBookStatus == BookStatus.Reserved && reservation != null)
                 _context.BookReservations.Remove(reservation);

            // Save changes

            if (await _context.SaveChangesAsync() < 1)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Book borrowing failed." });

            return Ok(new Response { Status = "Success", Message = "Book borrowed successfully!" });
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{bookId}/return")]
        public async Task<IActionResult> Return(Guid bookId)
        {
            var book = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);

            if (book == null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Book not found." });

            // Check book status

            if (book.Status == BookStatus.Available)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Book is already available." });

            // Return the book

            if (book.Status == BookStatus.Reserved)
            {
                var reservation = _context.BookReservations.Where(br => br.BookId == book.Id).FirstOrDefault();

                if (reservation != null)
                    _context.BookReservations.Remove(reservation);
            }
            else if (book.Status == BookStatus.Borrowed)
            {
                var borrowing = _context.BookBorrowings.Where(bb => bb.BookId == book.Id).FirstOrDefault();

                if (borrowing != null)
                    _context.BookBorrowings.Remove(borrowing);
            }

            book.Status = BookStatus.Available;

            // Save changes

            if (await _context.SaveChangesAsync() < 1)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Book returning failed." });

            return Ok(new Response { Status = "Success", Message = "Book returned successfully!" });
        }
    }
}
