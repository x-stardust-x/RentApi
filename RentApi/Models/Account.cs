using System.ComponentModel.DataAnnotations;

namespace RentApi.Models {
    public class Account {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Pwd { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; }
        public int Age { get; set; }
        public Identity Identity { get; set; }
        public bool Status { get; set; } = true;
        public bool IsDelete { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }

        public int SubscriptionTier { get; set; } = 1;

        public DateTime? VipExpireAt { get; set; }
        public string NotificationSetting { get; set; } = "{}";
        public DateTime? PasswordChangedAt { get; set; } = DateTime.Now;
    }
    public enum Identity {
        young = 0,
        old = 1,
    }
}
