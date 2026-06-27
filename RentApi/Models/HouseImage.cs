using System.ComponentModel.DataAnnotations.Schema;


namespace RentApi.Models
{
    [Table("House_Image")]
    public class HouseImage
    {
        public int Id { get; set; }


        public int HouseId { get; set; }

        public string Url { get; set; }

        public string? Description { get; set; }

        public bool IsCover { get; set; }
    }
}
