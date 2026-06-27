namespace RentApi.Models.DTO
{
    public class UpsertViewingSlotDto
    {
        //public string AvailableDate { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
    }
}
