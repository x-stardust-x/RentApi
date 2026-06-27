namespace RentApi.Models.DTO {
    public class FAQDto {
        public int CategoryId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public int SortOrder { get; set; }
        public int Status { get; set; }
    }
}
