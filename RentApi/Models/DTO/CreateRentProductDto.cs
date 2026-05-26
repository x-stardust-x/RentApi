namespace RentApi.Models.DTO
{
    public class CreateRentProductDto
    {
        public int AccountId { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public int? Price { get; set; }
        public string? PriceUnit { get; set; }
        public int? Deposit { get; set; }
        public bool? IsOnline { get; set; }
        public int? Quantity { get; set; }
        public string? OwnTool { get; set; }
        public string? RequiredKnowledge { get; set; }
        public string? Address { get; set; }
    }
}
