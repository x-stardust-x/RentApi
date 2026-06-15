using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("Tool_Booking_Order")]
    public class ToolBookingOrder
    {
        [Key]
        public int Id { get; set; }

        public string ReservationNo { get; set; } = string.Empty;

        public int ToolId { get; set; }

        public int BorrowerId { get; set; }

        public int LenderId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? ActualReturnDate { get; set; }

        public int TotalFee { get; set; }

        public int DepositFee { get; set; }

        public int TotalDays { get; set; }

        public int DeliveryMethod { get; set; }

        public string? ShippingAddress { get; set; }

        public string? TrackingNumber { get; set; }

        public string? Purpose { get; set; }

        public int Status { get; set; }

        public string? RejectReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
