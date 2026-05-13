namespace RentApi.Models.DTO {
    public class AccountDto {
        public string Username { get; set; } = "";
        public string Pwd { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime Birthday { get; set; }
        public int Age { get; set; }
        public int Identity { get; set; } = 0;
    }
}
