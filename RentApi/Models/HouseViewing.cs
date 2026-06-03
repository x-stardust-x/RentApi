using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("House_Viewing_Order")]
    public class HouseViewing
    {
        [Key]
        public int Id { get; set; }

        //[Required]
        [StringLength(50)]
        public string ReservationNo { get; set; } // 補上欄位

        public int HouseId { get; set; }

        public int LesseeId { get; set; }  // 配合妳 DB 的拼法：LesseId (少一個 e)
        
        public int LessorId { get; set; }

        public DateTime? ViewingTime { get; set; } // 補上欄位

        public DateTime? ExpectedMoveIn { get; set; } // 配合 DB 拼法

        public string? Message { get; set; }

        public int? MatchScore { get; set; }

        public int? Status { get; set; } // 2. 修正為 int！解決 CS0029 轉型錯誤

        public string? RejectReason { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; } // 補上欄位

        [ForeignKey("HouseId")]
        public virtual Rent_House RentHouse { get; set; }
    }
}