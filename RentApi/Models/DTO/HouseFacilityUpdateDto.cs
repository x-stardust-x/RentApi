namespace RentApi.Models.DTO
{
    public class HouseFacilityUpdateDto
    {
        public int HouseId { get; set; }
        public List<int> SelectedFacilityIds { get; set; } = new();
    }
}
