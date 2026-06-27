namespace RentApi.Models.DTO
{
    public class HouseMatchResultDto
    {
        public int HouseId { get; set; }
        public string Name { get; set; }
        public int RentPrice { get; set; }
        public string HouseType { get; set; }
        public int Score { get; set; }
        public string Basis { get; set; }
        public string Risk { get; set; }
        public string Suggestion { get; set; }
    }
}
