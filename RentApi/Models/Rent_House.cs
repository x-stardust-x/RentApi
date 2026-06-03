using RentApi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
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

        [ForeignKey("HouseId")]
        public virtual ICollection<HouseImage> HouseImages { get; set; } = new List<HouseImage>();

        // 明確告訴 EF Core：這個集合，對應到 HouseViewing 類別裡的 "RentHouse" 屬性
        [InverseProperty("RentHouse")]
        public virtual ICollection<HouseViewing> HouseViewings { get; set; } = new List<HouseViewing>();

    }
}