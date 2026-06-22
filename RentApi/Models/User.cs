namespace RentApi.Models {
    public class User {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public int? DistrictId { get; set; }

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

        public Account Account { get; set; }
        //public string? Interests { get; set; }
    }
}
