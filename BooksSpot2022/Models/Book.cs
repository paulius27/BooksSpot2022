using System.ComponentModel.DataAnnotations;

namespace BooksSpot2022.Models
{
    public enum BookStatus
    {
        Available = 0,
        Reserved,
        Borrowed
    }

    public class Book
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public DateTime PublishingDate { get; set; }
        public string Genre { get; set; }
        public string ISBN { get; set; }
        public BookStatus Status { get; set; }
    }
}
