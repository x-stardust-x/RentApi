namespace RentApi.Models.DTO
{
    public class MatchResult
    {
        public int HouseId { get; set; }
        public int Score { get; set; }
        public string Basis { get; set; }       
        public string Risk { get; set; }        
        public string Suggestion { get; set; }
    }
}
