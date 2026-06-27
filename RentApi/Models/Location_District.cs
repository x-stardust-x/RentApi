// Models/District.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("Location_District")]
    public class Location_District
    {
        [Key]
        public int DistrictId { get; set; }

        [StringLength(50)]
        public string CityName { get; set; } = null!;

        [StringLength(50)]
        public string DistrictName { get; set; } = null!;

        public int ZipCode { get; set; }
    }
}
