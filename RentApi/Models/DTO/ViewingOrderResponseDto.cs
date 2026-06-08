namespace RentApi.Models.DTO
{
    public class ViewingOrderResponseDto
    {
        public string Id { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public string RoomName { get; set; }
        public string RoomAddress { get; set; } = string.Empty;
        public ApplicantDetailDto Applicant { get; set; }
        //public List<string> Profiles { get; set; }
        public string ViewingDateTime { get; set; }
        public string ViewingDate { get; set; } = string.Empty;
        public List<string> PreferredTimeSlots { get; set; } = new();
        public string Message { get; set; }
        public int MatchScore { get; set; }
        public RescheduleInfoDto? RescheduleInfo { get; set; }
    }
}
