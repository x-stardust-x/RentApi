namespace RentApi.Models.DTO
{
    public class LesseeViewingApplicationDto
    {
        public string Id { get; set; } = string.Empty;
        public int HouseId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;
        public string RoomAddress { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public int RentPrice { get; set; }

        public int LessorId { get; set; }
        public int? LessorAccountId { get; set; }
        public int LessorProfileId { get; set; }

        public string LessorName { get; set; } = string.Empty;
        public string LessorAvatar { get; set; } = string.Empty;
        public string LessorPhone { get; set; } = string.Empty;
        public string LessorLineId { get; set; } = string.Empty;

        public string ViewingDate { get; set; } = string.Empty;
        public string ViewingDateTime { get; set; } = string.Empty;
        public List<string> PreferredTimeSlots { get; set; } = new();

        public string ExpectedMoveInText { get; set; } = string.Empty;
        public List<string> LesseeProfileTags { get; set; } = new();

        public string Message { get; set; } = string.Empty;
        public int MatchScore { get; set; }
        public string RejectReason { get; set; } = string.Empty;

        public RescheduleInfoDto? RescheduleInfo { get; set; }

        public DateTime? MatchedAt { get; set; }
        public string MatchNote { get; set; } = string.Empty;
        public string ClosedReason { get; set; } = string.Empty;

        public string ApplicationFlowType { get; set; } = "new";
        public int AttemptNo { get; set; } = 1;
        public int MaxAttemptCount { get; set; } = 3;

    }
}
