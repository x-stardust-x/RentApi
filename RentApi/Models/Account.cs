using System.ComponentModel.DataAnnotations;

namespace RentApi.Models {
    public class Account {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Pwd { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime Birthday { get; set; }
        public int Age { get; set; }
        public Identity Identity { get; set; }
        public bool Status { get; set; } = false;
        public bool IsDelete { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
    }
    public enum Identity {
        young = 0,
        old = 1,
    }
}
