namespace RentApi.Models.DTO
{
    public class RescheduleProductBookingDto
    {
        public int ReservationId { get; set; }

        public DateTime ProposedStartTime { get; set; }

        public DateTime? ProposedEndTime { get; set; }

        public string? Message { get; set; }
    }
}
