namespace RentApi.Models
{
    public class AddHouseImageDto
    {
        public int HouseId { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }

        public bool IsCover { get; set; }
    }
}
