using System.ComponentModel.DataAnnotations;

namespace RentApi.Models.DTO
{
    public class ConfirmMatchDto
    {
        [Required]
        public int ReservationId { get; set; }
        public bool MarkHouseAsMatched { get; set; } = true;
        public bool CloseOtherReservations { get; set; } = true;
        public string? MatchNote { get; set; }
    }
}
