namespace RentApi.Models.DTO
{
    public class CreateRentProductsDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public int Price { get; set; }
        public string PriceUnit { get; set; } = "日";
    }
}
