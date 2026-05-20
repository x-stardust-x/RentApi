using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("Contact_Us")]
    public class ContactUs
    {
        [Key]
        public int Id { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Category { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string? AttachmentUrl { get; set; }

        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
