namespace RentApi.Models {
    public class Account {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Pwd { get; set; }
        public Identity Identity { get; set; }
        public bool is_Delete { get; set; }
    }
    public enum Identity {
        young = 0,
        old = 1,
    }
}
