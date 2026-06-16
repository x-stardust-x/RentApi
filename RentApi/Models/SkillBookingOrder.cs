using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("Skill_Booking_Order")]

    public class SkillBookingOrder
    {
        [Key]
        public int Id { get; set; }

        public string BookingNo { get; set; } = string.Empty;

        public int SkillId { get; set; }

        public int? SessionId { get; set; }

        public int LearnerId { get; set; }

        public int MentorId { get; set; }

        public int BookingType { get; set; }

        public DateOnly? BookingDate { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public int TotalFee { get; set; }

        public decimal TotalHours { get; set; }

        public int MeetingMethod { get; set; }

        public string? MeetingUrl { get; set; }

        public string? MeetingLocation { get; set; }

        public string? CurrentStatus { get; set; }

        public int Status { get; set; }

        public string? RejectReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
