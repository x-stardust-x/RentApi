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

    public class LesseeProfileTagDto
    {
        public string Label { get; set; } = string.Empty;

        public string Source { get; set; } = "habit";
    }




    //public class CreateViewingOrderDto
    //{
    //    //[JsonPropertyName("houseId")]


    //    public int HouseId { get; set; }
    //    public int LesseeId { get; set; }
    //    public int LessorId { get; set; }
    //    public DateTime ViewingTime { get; set; }
    //    public string? ExpectedMoveInText { get; set; }
    //    public List<string>? PreferredTimeSlots { get; set; }
    //    public List<string>? TenantProfiles { get; set; }
    //    public string? Message { get; set; }
    //    public int MatchScore { get; set; }


    //    //public int HouseId { get; set; }
    //    //public int LesseeId { get; set; }
    //    //public int LessorId { get; set; }
    //    //public DateTime ViewingTime { get; set; }
    //    //public DateTime ExpectedMoveIn { get; set; }
    //    //public string Message { get; set; } = string.Empty;
    //    //public int MatchScore { get; set; }
    //}
}
