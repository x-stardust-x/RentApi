namespace RentApi.Models {
    public class User_Habit {
        public int Id { get; set; }
        public int UserId { get; set; }
        public TimeOnly? SleepTime { get; set; }
        public TimeOnly? WakeTime { get; set; }
        public int? CleanLevel { get; set; }
        public int? NoiseTolerance { get; set; }
        public bool? Pet { get; set; }
        public bool? Smoke { get; set; }
        public string? Interests { get; set; }
        public User User { get; set; }
    }
}
