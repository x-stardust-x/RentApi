namespace RentApi.Models.DTO
{
    public class ViewingOrderResponseDto
    {
        public string Id { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public string RoomName { get; set; }
        public ApplicantDetailDto Applicant { get; set; }
        //public List<string> Profiles { get; set; }
        public string ViewingDateTime { get; set; }
        public string Message { get; set; }
        public int MatchScore { get; set; }
    }
}
