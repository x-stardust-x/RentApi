using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("Product_Booking_Order")]
    public class ProductBookingOrder
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string ReservationNo { get; set; } = string.Empty;

        public int ProductId { get; set; }

        // 申請人 User.Id
        public int LesseeId { get; set; }

        // 工具 / 技能提供者 User.Id
        public int ProviderId { get; set; }

        // tool / skill
        [StringLength(20)]
        public string BookingType { get; set; } = "tool";

        // 工具：借用開始時間；技能：預約開始時間
        public DateTime? StartTime { get; set; }

        // 工具：借用結束時間；技能：預約結束時間
        public DateTime? EndTime { get; set; }

        // 工具：面交自取 / 其他；技能：線上視訊 / 實體面授
        [StringLength(100)]
        public string Method { get; set; } = string.Empty;

        public string? Message { get; set; }

        public string? ExtraNote { get; set; }

        public int? MatchScore { get; set; }

        // 0 待審核、1 已確認、2 已婉拒、3 提議改期、4 已關閉
        public int Status { get; set; } = 0;

        public string? RejectReason { get; set; }

        public DateTime? RescheduleProposedStartTime { get; set; }

        public DateTime? RescheduleProposedEndTime { get; set; }

        public string? RescheduleMessage { get; set; }

        public int RescheduleCount { get; set; } = 0;

        public DateTime? LastRescheduleAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ProductId")]
        public virtual RentProduct? Product { get; set; }
    }
}
