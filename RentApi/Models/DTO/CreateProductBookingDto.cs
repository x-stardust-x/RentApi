namespace RentApi.Models.DTO
{
    public class CreateProductBookingDto
    {
        public int ProductId { get; set; }

        // tool / skill
        public string BookingType { get; set; } = "tool";

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        // 工具：面交自取 / 物流寄送
        // 技能：線上 / 實體
        public string? Method { get; set; }

        public string? Message { get; set; }

        public string? ExtraNote { get; set; }

        public int MatchScore { get; set; } = 85;

        public string? ShippingAddress { get; set; }

        public string? MeetingUrl { get; set; }

        public string? MeetingLocation { get; set; }
    }
}
