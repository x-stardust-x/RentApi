namespace RentApi.Models {
    public class User_Habit {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SleepTime { get; set; }
        public int WakeTime { get; set; }
        public decimal CleanLevel { get; set; }
        public decimal NoiseTolerance { get; set; }
        public bool Pet { get; set; }
        public bool Smoke { get; set; }
        public string Interests { get; set; }
    }
}
