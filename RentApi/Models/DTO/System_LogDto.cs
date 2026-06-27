namespace RentApi.Models.DTO {
    public class System_LogDto {
        public int UserId { get; set; }
        public string Action { get; set; }
        public string IpAddress { get; set; }
    }
    public class GetSystem_LogDto {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Action { get; set; }
        public string IpAddress { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
