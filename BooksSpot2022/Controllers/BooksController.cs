using BooksSpot2022.Auth;
using BooksSpot2022.Data;
using BooksSpot2022.Models;
using BooksSpot2022.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }
}
