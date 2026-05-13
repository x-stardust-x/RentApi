using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoLiving.models
{
    [Table("Rent_House")]
    public class Rent_House
    {


        [Key]
        public int Id { get; set; }

        
        public int? AccountId { get; set; }
        public int? DistrictId { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        public string? Description { get; set; }

        
        public int RentPrice { get; set; }

        
        public bool IncludeUtilities { get; set; }
        public bool IncludeWifi { get; set; }
        public bool IncludeManagementFee { get; set; }

        
        public decimal? AreaSize { get; set; }

        public int? LeaseTerm { get; set; }
        public string? FloorInfo { get; set; }
        public string? HouseType { get; set; }
        public int? ViewCount { get; set; }

        public int Status { get; set; }

    }
}