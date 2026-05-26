namespace RentApi.Models 
{
    public class AddProductImageDto
    {
        public int ProductId { get; set; } 
        public string? Url { get; set; }   
        public string? Description { get; set; }
        public bool IsCover { get; set; }  
    }
}