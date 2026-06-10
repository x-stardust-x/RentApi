using System.ComponentModel.DataAnnotations;

namespace RentApi.Models.DTO
{
    public class UpdateReservationStatusDto
    {
        [Required]
        public int ReservationId { get; set; } // 對應前端的 id

        [Required]
        [RegularExpression("^(pending|confirmed|rejected)$", ErrorMessage = "狀態格式錯誤")]
        public string Status { get; set; } = string.Empty; // 前端傳來的 'confirmed' 或 'rejected'
        public string? RejectReason { get; set; }
    }
}
