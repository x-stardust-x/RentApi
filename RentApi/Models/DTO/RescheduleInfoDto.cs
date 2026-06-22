namespace RentApi.Models.DTO
{
    public class RescheduleInfoDto
    {
        public string ProposedViewingDateTime { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
