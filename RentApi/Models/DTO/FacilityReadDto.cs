namespace RentApi.Models.DTO
{
    public class FacilityReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string IconClass { get; set; } = null!;
    }
}
