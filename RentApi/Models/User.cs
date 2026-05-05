namespace RentApi.Models {
    public class User {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public Account Account { get; set; }
    }
}
