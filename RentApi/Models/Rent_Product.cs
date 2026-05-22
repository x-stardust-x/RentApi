using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RentApi.Models
{
    [Table("Rent_Products")]
    public class Rent_Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        public string? Description { get; set; }

        public int? Price { get; set; }

        [StringLength(50)]
        public string? PriceUnit { get; set; }
    }
}
