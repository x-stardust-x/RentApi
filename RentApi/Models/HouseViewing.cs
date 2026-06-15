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

        public int LesseeId { get; set; }
        
        public int LessorId { get; set; }

        public DateTime? ViewingTime { get; set; } // 補上欄位

        public DateTime? ExpectedMoveIn { get; set; } // 配合 DB 拼法

        public string? Message { get; set; }

        public int? MatchScore { get; set; }

        public int? Status { get; set; } // 2. 修正為 int！解決 CS0029 轉型錯誤

        public string? RejectReason { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; } // 補上欄位

        public int? ViewingSlotId { get; set; }

        public string? ExpectedMoveInText { get; set; }

        public string? PreferredTimeSlotsJson { get; set; }

        public string? LesseeProfileTagsJson { get; set; }



        public DateTime? RescheduleProposedTime { get; set; }

        public DateTime? RescheduleEndTime { get; set; }

        public string? RescheduleMessage { get; set; }

        public int RescheduleCount { get; set; }

        public DateTime? LastRescheduleAt { get; set; }



        [ForeignKey("HouseId")]
        public virtual Rent_House RentHouse { get; set; }

        [ForeignKey("ViewingSlotId")]
        public virtual HouseViewingAvailableSlot? ViewingSlot { get; set; }

        public string ApplicationFlowType { get; set; } = "new";

        public int AttemptNo { get; set; } = 1;

        public int MaxAttemptCount { get; set; } = 3;

        public DateTime? MatchedAt { get; set; }

        public int? MatchedByUserId { get; set; }

        public string? MatchNote { get; set; }

        public string? ClosedReason { get; set; }
    }
}