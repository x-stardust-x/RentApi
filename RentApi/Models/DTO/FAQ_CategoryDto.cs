namespace RentApi.Models.DTO {
    public class FAQ_CategoryDto {
        public string Name { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}
