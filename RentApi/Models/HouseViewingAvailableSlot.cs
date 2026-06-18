using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("House_Viewing_Available_Slot")]
    public class HouseViewingAvailableSlot
    {
        [Key]
        public int Id { get; set; }

        public int HouseId { get; set; }

        public int LessorId { get; set; }

        public DateOnly? AvailableDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [ForeignKey("HouseId")]
        public virtual Rent_House? RentHouse { get; set; }

        [ForeignKey("LessorId")]
        public virtual User? Lessor { get; set; }
    }
}




//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace RentApi.Models
//{
//    public class HouseViewingAvailableSlot
//    {
//        [Table("House_Viewing_Available_Slot")]
//        public class HouseViewingAvailableSlot
//        {
//            [Key]
//            public int Id { get; set; }

//            public int HouseId { get; set; }

//            public int LessorId { get; set; }

//            public DateOnly AvailableDate { get; set; }

//            public TimeOnly StartTime { get; set; }

//            public TimeOnly EndTime { get; set; }
//            s
//            public bool IsEnabled { get; set; }

//            public DateTime CreatedAt { get; set; }

//            public DateTime UpdatedAt { get; set; }

//            [ForeignKey("HouseId")]
//            public virtual Rent_House? RentHouse { get; set; }
//        }
//    }
//}
