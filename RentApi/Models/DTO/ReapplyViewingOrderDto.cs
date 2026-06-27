using System.ComponentModel.DataAnnotations;

namespace RentApi.Models.DTO
{
    public class ReapplyViewingOrderDto
    {
        [Required]
        public int ReservationId { get; set; }
        public int? ViewingSlotId { get; set; }

        [Required]
        public DateTime ViewingTime { get; set; }
        public DateTime? ExpectedMoveIn { get; set; }
        public string? ExpectedMoveInText { get; set; }
        public List<string> PreferredTimeSlots { get; set; } = new();
        public List<LesseeProfileTagDto> LesseeProfileTags { get; set; } = new();
        public string? Message { get; set; }
        public int? MatchScore { get; set; }
    }
}
