using System.ComponentModel.DataAnnotations;

namespace RentApi.Models.DTO
{
    public class UpdateProductBookingStatusDto
    {
        [Required]
        public int ReservationId { get; set; }

        // tool / skill
        [Required]
        public string BookingKind { get; set; } = "tool";

        [Required]
        [RegularExpression("^(pending|confirmed|rejected|rescheduled|closed)$", ErrorMessage = "狀態格式錯誤")]
        public string Status { get; set; } = string.Empty;

        public string? RejectReason { get; set; }
    }
}
