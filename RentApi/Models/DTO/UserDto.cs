namespace RentApi.Models.DTO {
    public class UserDto {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public string? RealName { get; set; }

        public string? EnglishName { get; set; }

        public string? Avatar { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }
        public string? LineId { get; set; }

        public string? Bio { get; set; }

        public decimal? Rating { get; set; }

        public int? ReviewCount { get; set; }

        public DateTime? CreateAt { get; set; }

        // Account
        public int Age { get; set; }

        public Identity Identity { get; set; }

        public bool Status { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? LastLoginAt { get; set; }
    }
    public class UpdateEmailDto {
        public string Email { get; set; } = string.Empty;
    }
    public class UpdatePwdDto {
        public string Pwd { get; set; } = string.Empty;
    }
}
