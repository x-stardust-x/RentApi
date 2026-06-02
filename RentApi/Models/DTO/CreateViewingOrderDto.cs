using System.Text.Json.Serialization;

namespace RentApi.Models.DTO
{
    public class CreateViewingOrderDto
    {
        //[JsonPropertyName("houseId")]
        public int HouseId { get; set; }
        public int LesseeId { get; set; }
        public int LessorId { get; set; }
        public DateTime ViewingTime { get; set; }
        public DateTime ExpectedMoveIn { get; set; }
        public string Message { get; set; } = string.Empty;
        public int MatchScore { get; set; }
    }
}
