namespace RentApi.Models.DTO {
    public class UserProfileDto {
        public int Id { get; set; }

        public string? RealName { get; set; }
        public string? EnglishName { get; set; }

        public byte[]? Avatar { get; set; }
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Bio { get; set; }

        public decimal? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public int? DistrictId { get; set; }
        public string? CityName { get; set; }
        public string? DistrictName { get; set; }
        public int? ZipCode { get; set; }
        public int? SleepTime { get; set; }
        public int? WakeTime { get; set; }
        public int? CleanLevel { get; set; }
        public int? NoiseTolerance { get; set; }
        public bool? Pet { get; set; }
        public bool? Smoke { get; set; }
        public string? Interests { get; set; }
    }
    public class UpdateProfileDto {
        public int Id { get; set; }

        public string? RealName { get; set; }
        public string? EnglishName { get; set; }

        public byte[]? Avatar { get; set; }
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Bio { get; set; }

        public decimal? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public int? DistrictId { get; set; }
        public int? SleepTime { get; set; }
        public int? WakeTime { get; set; }
        public int? CleanLevel { get; set; }
        public int? NoiseTolerance { get; set; }
        public bool? Pet { get; set; }
        public bool? Smoke { get; set; }
        public string? Interests { get; set; }
    }
}
