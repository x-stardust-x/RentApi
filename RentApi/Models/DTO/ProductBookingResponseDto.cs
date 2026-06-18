namespace RentApi.Models.DTO
{
    public class ProductBookingResponseDto
    {
        public string Id { get; set; } = string.Empty;

        public string OrderNumber { get; set; } = string.Empty;

        public string Status { get; set; } = "pending";

        // tool / skill
        public string Type { get; set; } = "tool";

        public int ProductId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string PriceInfo { get; set; } = string.Empty;

        public string BookingPeriod { get; set; } = string.Empty;

        public string Method { get; set; } = string.Empty;

        public string ExtraNote { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public int MatchScore { get; set; }

        public ProductBookingApplicantDto Applicant { get; set; } = new();

        public ProductBookingProviderDto Provider { get; set; } = new();

        public DateTime CreatedAt { get; set; }

        public string CoverUrl { get; set; } = string.Empty;
    }

    public class ProductBookingApplicantDto
    {
        public string Name { get; set; } = string.Empty;

        public string Avatar { get; set; } = "images/mr_chen.jpg";

        public List<string> Profiles { get; set; } = new();

        public string Phone { get; set; } = string.Empty;

        public string LineId { get; set; } = string.Empty;
    }

    public class ProductBookingProviderDto
    {
        public string Name { get; set; } = string.Empty;

        public string Avatar { get; set; } = "images/mr_chen.jpg";

        public string Phone { get; set; } = string.Empty;

        public string LineId { get; set; } = string.Empty;
    }
}

