using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("House_Facility")]
    public class HouseFacility
    {
        public int Id { get; set; }
        public int HouseId { get; set; }
        public int FacilityId { get; set; }
        //public DateTime? PurchasedDate { get; set; }
        //public string? FacilityImageUrl { get; set; }
    }
}
