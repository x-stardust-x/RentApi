using System.Text.Json.Serialization;

namespace RentApi.Models.DTO
{

    public class CreateViewingOrderDto
    {
        public int HouseId { get; set; }

        public int? ViewingSlotId { get; set; }

        public DateTime? ViewingTime { get; set; }

        public DateTime? ExpectedMoveIn { get; set; }

        public string? ExpectedMoveInText { get; set; }

        public List<string>? PreferredTimeSlots { get; set; }

        public List<LesseeProfileTagDto>? LesseeProfileTags { get; set; }

        public string? Message { get; set; }

        public int MatchScore { get; set; }
    }
}
