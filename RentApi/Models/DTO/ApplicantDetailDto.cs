namespace RentApi.Models.DTO
{
    public class ApplicantDetailDto
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
        public List<string> Profiles { get; set; }
        public string MoveInDate { get; set; }
        public string Phone { get; set; }
        public string LineId { get; set; }
    }
}
