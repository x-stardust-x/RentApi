using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("Product_Image")]
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        
        public int ProductId { get; set; }

        public string? Url { get; set; }

        public string? Description { get; set; }

        
        public bool IsCover { get; set; }
    }
}
