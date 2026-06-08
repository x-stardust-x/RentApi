using System.ComponentModel.DataAnnotations;

namespace RentApi.Models.DTO
{
    public class ProposeRescheduleDto
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public DateTime ProposedStartTime { get; set; }
        public DateTime? ProposedEndTime { get; set; }
        
        //public DateTime RescheduleProposedTime { get; set; }
        //public DateTime? RescheduleEndTime { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
    }
}