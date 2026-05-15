namespace RentApi.Models {
    public class System_Log {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
        public string IpAddress { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
