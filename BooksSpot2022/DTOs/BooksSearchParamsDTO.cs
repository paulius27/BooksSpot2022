using BooksSpot2022.Models;

namespace BooksSpot2022.DTOs
{
    public class BooksSearchParamsDTO
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public int? PublishingYear { get; set; }
        public string? Genre { get; set; }
        public string? ISBN { get; set; }
        public BookStatus? Status { get; set; }
    }
}
